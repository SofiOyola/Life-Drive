using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Motor;
using UnityEngine;

namespace LifeDrive.Mapas
{
    public sealed class EstadoRecorridoV2
    {
        public int PartidaId { get; internal set; }
        public int VersionContenido => 2;
        public string MapaActual { get; internal set; }
        public string[] Historial { get; internal set; }
        public int SituacionPendienteId { get; internal set; }
        public int UltimaDecisionPresentadaId { get; internal set; }
        public Decision DecisionConfirmada { get; internal set; }
        public bool PresentacionPendiente { get; internal set; }
        public string DestinoVisualConfirmado { get; internal set; }
        public bool Finalizada { get; internal set; }
        public bool CierrePresentado => Finalizada && MapaActual == "DELIVERY" && !PresentacionPendiente;
    }

    // El motor confirma las respuestas. Este adaptador reconstruye y confirma hitos de presentación.
    public sealed class ReanudacionRecorridosV2
    {
        readonly BaseDatos db;
        readonly ServicioUsuarios usuarios;
        readonly ServicioPartidas partidas;
        readonly PaqueteRecorridos paquete;
        readonly CatalogoMapas catalogo;

        internal ReanudacionRecorridosV2(BaseDatos db, ServicioUsuarios usuarios, ServicioPartidas partidas,
            PaqueteRecorridos paquete, CatalogoMapas catalogo)
        {
            this.db = db;
            this.usuarios = usuarios;
            this.partidas = partidas;
            this.paquete = paquete;
            this.catalogo = catalogo;
        }

        // Una partida activa conserva su versión, incluso si se solicita iniciar V2.
        public Partida IniciarPartida() => partidas.IniciarPartida(2);

        public EstadoRecorridoV2 Consultar(int partidaId) => Reconstruir(ObtenerPartida(partidaId));

        public EstadoRecorridoV2 ConfirmarPresentacion(int partidaId, int decisionId)
        {
            var partida = ObtenerPartida(partidaId);
            var estado = Reconstruir(partida);
            if (decisionId > 0 && decisionId == estado.UltimaDecisionPresentadaId)
                return estado;
            if (!estado.PresentacionPendiente || estado.DecisionConfirmada.id != decisionId)
                throw new ErrorJuego("La decisión no es el siguiente hito de presentación.");

            db.Ejecutar(() => { partida.ultima_decision_presentada_id = decisionId; });
            return Reconstruir(partida);
        }

        Partida ObtenerPartida(int id)
        {
            var partida = db.Partidas.FirstOrDefault(p => p.id == id);
            usuarios.ExigirPartidaPropia(partida, Accion.Jugar);
            if (partida.contenido_version != 2)
                throw new ErrorJuego("La partida pertenece a la ruta clásica.");
            return partida;
        }

        EstadoRecorridoV2 Reconstruir(Partida partida)
        {
            var situacionPendiente = partidas.SituacionActual(partida);
            var cursor = new RecorridoVisualValidado(catalogo);
            var decisiones = db.Decisiones.Where(d => d.partida_id == partida.id).OrderBy(d => d.id).ToArray();
            var hito = partida.ultima_decision_presentada_id;
            var indiceHito = hito == 0 ? -1 : System.Array.FindIndex(decisiones, d => d.id == hito);
            if ((hito != 0 && indiceHito < 0) || decisiones.Length - indiceHito > 2)
                throw new ErrorJuego("El hito guardado no coincide con el historial de decisiones.");

            string destinoConfirmado = null;
            foreach (var decision in decisiones)
            {
                destinoConfirmado = ResolverDestino(cursor, decision);
                if (destinoConfirmado != null && decision.id <= hito)
                    cursor.AvanzarConfirmado(destinoConfirmado);
            }

            var ultima = decisiones.LastOrDefault();
            return new EstadoRecorridoV2
            {
                PartidaId = partida.id,
                MapaActual = cursor.Actual,
                Historial = cursor.Historial,
                SituacionPendienteId = situacionPendiente?.id ?? 0,
                UltimaDecisionPresentadaId = hito,
                DecisionConfirmada = ultima == null ? null : JsonUtility.FromJson<Decision>(JsonUtility.ToJson(ultima)),
                PresentacionPendiente = ultima != null && ultima.id != hito,
                DestinoVisualConfirmado = destinoConfirmado,
                Finalizada = partida.estado == EstadoPartida.Finalizada
            };
        }

        string ResolverDestino(RecorridoVisualValidado cursor, Decision decision)
        {
            var situacion = paquete.contenido.situaciones.SingleOrDefault(s => s.id == decision.situacion_id);
            var alternativa = situacion?.alternativas.SingleOrDefault(a => a.id == decision.alternativa_id);
            var mapaLogico = paquete.nodos.SingleOrDefault(n => n.tramo_id == decision.tramo_origen_id)?.mapa_id;
            var esAproximacion = decision.situacion_id == PaqueteRecorridos.SituacionAproximacion;
            if (alternativa == null || situacion.tramo_id != decision.tramo_origen_id ||
                (esAproximacion ? mapaLogico != "DELIVERY" || !cursor.Permitidos.Contains("DELIVERY")
                    : mapaLogico != cursor.Actual))
                throw new ErrorJuego($"La decisión {decision.id} no coincide con el recorrido guardado.");

            var destinoLogico = alternativa.tramo_destino_id != 0
                ? alternativa.tramo_destino_id : decision.tramo_origen_id;
            if (decision.tramo_destino_id != destinoLogico)
                throw new ErrorJuego($"El destino lógico de la decisión {decision.id} cambió.");

            if (esAproximacion) return "DELIVERY";
            if (destinoLogico == decision.tramo_origen_id) return null;

            var fila = paquete.asociaciones.SingleOrDefault(a => a.mapa_origen == cursor.Actual &&
                a.situacion_id == decision.situacion_id && a.alternativa_id == decision.alternativa_id);
            if (fila == null || !cursor.Permitidos.Contains(fila.mapa_destino))
                throw new ErrorJuego($"La decisión {decision.id} no tiene una transición visual válida.");
            return fila.mapa_destino;
        }
    }
}
