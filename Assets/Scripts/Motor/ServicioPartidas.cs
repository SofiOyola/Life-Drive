// Módulos "Partidas", "Rutas y decisiones", "Preguntas y eventos" y "Puntuación" (S2-03).
// Ciclo (HU-05): iniciar → situación → decisión → consecuencia → siguiente camino → puntos → continuar.
using System;
using System.Collections.Generic;
using System.Linq;
using LifeDrive.Datos;

namespace LifeDrive.Motor
{
    public class ResultadoDecision
    {
        public Decision Decision;
        public Partida Partida;
        public bool CambioRuta;
        public string TramoDestino;
        public bool Repetida;          // la situación ya estaba resuelta (doble clic o reintento)
        public EstadoDecision? Antes;
        public EstadoDecision Despues;
        public IReadOnlyList<int> TramosAlcanzados;
    }

    public readonly struct EstadoDecision
    {
        public readonly int TramoId, SituacionId, Puntos, RetrasoMin;
        public readonly string Estado;

        internal EstadoDecision(Partida partida, int situacionId)
        {
            TramoId = partida.tramo_actual_id;
            SituacionId = situacionId;
            Puntos = partida.puntos;
            RetrasoMin = partida.retraso_min;
            Estado = partida.estado;
        }
    }

    public class ServicioPartidas
    {
        readonly BaseDatos db;
        readonly Contenido contenido;
        readonly Contenido contenidoV2;
        readonly string huellaV2;
        readonly ServicioUsuarios usuarios;
        readonly RegistroEventos eventos;
        readonly EvaluadorSituaciones evaluador = new EvaluadorSituaciones();

        public ServicioPartidas(BaseDatos db, Contenido contenido, ServicioUsuarios usuarios, RegistroEventos eventos)
            : this(db, contenido, usuarios, eventos, null, null) { }

        internal ServicioPartidas(BaseDatos db, Contenido contenido, ServicioUsuarios usuarios,
            RegistroEventos eventos, Contenido contenidoV2, string huellaV2)
        {
            this.db = db;
            this.contenido = contenido;
            this.usuarios = usuarios;
            this.eventos = eventos;
            this.contenidoV2 = contenidoV2;
            this.huellaV2 = huellaV2;
        }

        public Tramo Tramo(int id) => contenido.tramos.First(t => t.id == id);

        public Tramo Tramo(Partida partida, int id) => ContenidoDe(partida).tramos.First(t => t.id == id);

        internal Situacion Situacion(Partida partida, int id) => ContenidoDe(partida).situaciones.FirstOrDefault(s => s.id == id);

        Contenido ContenidoDe(Partida partida)
        {
            var disponible = ContenidoDe(partida.contenido_version);
            if (partida.contenido_version == 2 &&
                !string.Equals(partida.contenido_huella, huellaV2, StringComparison.Ordinal))
                throw new ErrorJuego("El paquete V2 no coincide con el contenido con que empezó la partida.");
            return disponible;
        }

        Contenido ContenidoDe(int version)
        {
            if (version == 0 || version == 1) return contenido;
            if (version == 2 && contenidoV2 != null) return contenidoV2;
            throw new ErrorJuego($"La versión de contenido {version} no está disponible.");
        }

        // Inicia una operación desde la salida o retoma la que el jugador dejó en curso.
        public Partida IniciarPartida() => IniciarPartida(1);

        internal Partida IniciarPartida(int versionNueva)
        {
            usuarios.Exigir(Accion.Jugar);
            var yo = usuarios.Actual;
            var enCurso = db.Partidas.FirstOrDefault(p => p.jugador_id == yo.JugadorId && p.estado == EstadoPartida.EnCurso);
            if (enCurso != null)
            {
                ContenidoDe(enCurso);
                if (enCurso.contenido_version != 2)
                    db.Ejecutar(() => eventos.Registrar(TipoEvento.PartidaRetomada, yo, enCurso.id));
                return enCurso;
            }
            if (versionNueva == 2)
            {
                var cierrePendiente = db.Partidas.LastOrDefault(p => p.jugador_id == yo.JugadorId &&
                    p.contenido_version == 2 && p.estado == EstadoPartida.Finalizada &&
                    db.Decisiones.Any(d => d.partida_id == p.id && d.id > p.ultima_decision_presentada_id));
                if (cierrePendiente != null)
                {
                    ContenidoDe(cierrePendiente);
                    return cierrePendiente;
                }
            }
            var contenidoNuevo = ContenidoDe(versionNueva);
            var id = db.Ejecutar(() =>
            {
                var nueva = db.Insertar(new Partida
                {
                    jugador_id = yo.JugadorId,
                    contenido_version = versionNueva,
                    contenido_huella = versionNueva == 2 ? huellaV2 : null,
                    tramo_actual_id = contenidoNuevo.tramos.Single(t => t.es_salida).id,
                    fecha_inicio = RegistroEventos.Ahora(),
                });
                eventos.Registrar(TipoEvento.PartidaIniciada, yo, nueva.id, detalle: Tramo(nueva, nueva.tramo_actual_id).nombre);
                Avanzar(nueva);
                return nueva.id;
            });
            return db.Partidas.First(p => p.id == id);
        }

        // Primera situación del tramo actual que la partida aún no ha resuelto; null si terminó.
        public Situacion SituacionActual(Partida p)
        {
            var contenidoPartida = ContenidoDe(p);
            if (p.estado != EstadoPartida.EnCurso) return null;
            return contenidoPartida.situaciones
                .Where(s => s.tramo_id == p.tramo_actual_id && !db.Decisiones.Any(d => d.partida_id == p.id && d.situacion_id == s.id))
                .OrderBy(s => s.orden)
                .FirstOrDefault();
        }

        void CambiarTramo(Partida p, int tramoId, List<int> traza)
        {
            if (p.tramo_actual_id == tramoId) return;
            p.tramo_actual_id = tramoId;
            traza?.Add(tramoId);
            eventos.Registrar(TipoEvento.TramoAlcanzado, usuarios.Actual, p.id, detalle: Tramo(p, tramoId).nombre);
        }

        // Sigue el camino por defecto hasta una situación pendiente o hasta terminar en el destino.
        void Avanzar(Partida p, List<int> traza = null)
        {
            var maximo = ContenidoDe(p).tramos.Length * 2;
            for (var saltos = 0; saltos <= maximo; saltos++)
            {
                if (SituacionActual(p) != null) return;
                var tramo = Tramo(p, p.tramo_actual_id);
                if (tramo.es_destino)
                {
                    p.estado = EstadoPartida.Finalizada;
                    p.fecha_fin = RegistroEventos.Ahora();
                    eventos.Registrar(TipoEvento.PartidaFinalizada, usuarios.Actual, p.id,
                        detalle: $"{p.puntos} puntos, {p.retraso_min} min de retraso");
                    return;
                }
                CambiarTramo(p, tramo.siguiente_tramo_id, traza);
            }
            throw new ErrorJuego("La ruta no tiene salida desde este punto; revisa ruta.json");
        }

        // CU-03: evalúa la alternativa, aplica la consecuencia y guarda decisión, partida y eventos juntos.
        public ResultadoDecision Decidir(int partidaId, int situacionId, int alternativaId)
        {
            var p = db.Partidas.FirstOrDefault(x => x.id == partidaId);
            usuarios.ExigirPartidaPropia(p, Accion.Jugar);

            var previa = db.Decisiones.FirstOrDefault(d => d.partida_id == p.id && d.situacion_id == situacionId);
            if (previa != null) return Resultado(p, previa, true);

            if (p.estado != EstadoPartida.EnCurso) throw new ErrorJuego("La partida ya terminó");
            if (p.contenido_version == 2 && db.Decisiones.Any(d => d.partida_id == p.id &&
                d.id > p.ultima_decision_presentada_id))
                throw new ErrorJuego("Confirma la presentación de la decisión anterior antes de continuar.");
            var s = SituacionActual(p);
            if (s == null || s.id != situacionId) throw new ErrorJuego("Esa situación no es la actual");
            var a = s.alternativas.FirstOrDefault(x => x.id == alternativaId)
                    ?? throw new ErrorJuego("La alternativa no pertenece a la situación");
            var r = evaluador.Evaluar(s, a);
            var yo = usuarios.Actual;
            var antes = new EstadoDecision(p, s.id);
            var traza = new List<int>();

            var decisionId = db.Ejecutar(() =>
            {
                var partida = db.Partidas.First(x => x.id == partidaId);
                var d = db.Insertar(new Decision
                {
                    partida_id = partida.id, situacion_id = s.id, alternativa_id = a.id, fecha_hora = RegistroEventos.Ahora(),
                    tramo_origen_id = partida.tramo_actual_id,
                    tramo_destino_id = r.SiguienteTramoId != 0 ? r.SiguienteTramoId : partida.tramo_actual_id,
                    tipo = s.tipo, alternativa = a.descripcion,
                    es_acertada = r.EsAcertada, es_buena_practica = r.EsBuenaPractica,
                    puntos_aplicados = r.Puntos, retraso_aplicado = r.RetrasoMin, consecuencia = r.Explicacion,
                });
                eventos.Registrar(TipoEvento.DeSituacion(s.tipo), yo, partida.id, s.id,
                    $"{(r.EsAcertada ? "Segura/correcta" : "Insegura/incorrecta")}: {a.descripcion}");
                if (r.EsBuenaPractica) eventos.Registrar(TipoEvento.BuenaPractica, yo, partida.id, s.id, a.descripcion);

                partida.puntos += r.Puntos;
                partida.retraso_min += r.RetrasoMin;
                eventos.Registrar(TipoEvento.PuntosActualizados, yo, partida.id, s.id,
                    $"{(r.Puntos >= 0 ? "+" : "")}{r.Puntos} → {partida.puntos}");
                CambiarTramo(partida, d.tramo_destino_id, traza);
                Avanzar(partida, traza);
                return d.id;
            });

            var guardada = db.Partidas.First(x => x.id == partidaId);
            var resultado = Resultado(guardada, db.Decisiones.First(d => d.id == decisionId), false);
            resultado.Antes = antes;
            resultado.TramosAlcanzados = traza.AsReadOnly();
            return resultado;
        }

        ResultadoDecision Resultado(Partida p, Decision d, bool repetida) => new ResultadoDecision
        {
            Partida = p, Decision = d, Repetida = repetida,
            CambioRuta = d.tramo_destino_id != d.tramo_origen_id,
            TramoDestino = Tramo(p, d.tramo_destino_id).nombre,
            Despues = new EstadoDecision(p, SituacionActual(p)?.id ?? 0),
            TramosAlcanzados = Array.Empty<int>(),
        };
    }
}
