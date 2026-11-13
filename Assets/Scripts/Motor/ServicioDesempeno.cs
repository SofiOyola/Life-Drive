// Módulo "Desempeño y reportes" (S2-03): resumen final (HU-09), consulta del administrador (HU-10),
// reporte PDF (BL-16) y consulta del registro de eventos.
using System;
using System.Collections.Generic;
using System.Linq;
using LifeDrive.Datos;

namespace LifeDrive.Motor
{
    public class PasoHistorial
    {
        public Decision Decision;
        public string TramoOrigen;
        public string TramoDestino;
        public int PuntosAcumulados;
    }

    public class Resumen
    {
        public Partida Partida;
        public Jugador Jugador;
        public int PreguntasCorrectas, PreguntasIncorrectas, DecisionesSeguras, DecisionesInseguras;
        public int RiesgosEnfrentados, BuenasPracticas, CambiosDeRuta;
        public List<string> RutaRecorrida = new List<string>();
        public List<PasoHistorial> Historial = new List<PasoHistorial>();
        public Indicadores Indicadores;   // S3-09, de esta partida
    }

    public class FilaJugador
    {
        public Jugador Jugador;
        public string Personaje;
        public int PartidasFinalizadas;
        public string UltimaPartida;
    }

    public class ServicioDesempeno
    {
        readonly BaseDatos db;
        readonly Contenido contenido;
        readonly ServicioUsuarios usuarios;
        readonly RegistroEventos eventos;
        readonly Func<Partida, int, Tramo> resolverTramo;
        readonly Func<Partida, int, Situacion> resolverSituacion;

        public ServicioDesempeno(BaseDatos db, Contenido contenido, ServicioUsuarios usuarios, RegistroEventos eventos)
            : this(db, contenido, usuarios, eventos, null) { }

        internal ServicioDesempeno(BaseDatos db, Contenido contenido, ServicioUsuarios usuarios,
            RegistroEventos eventos, Func<Partida, int, Tramo> resolverTramo, Func<Partida, int, Situacion> resolverSituacion = null)
        {
            this.db = db;
            this.contenido = contenido;
            this.usuarios = usuarios;
            this.eventos = eventos;
            this.resolverTramo = resolverTramo;
            this.resolverSituacion = resolverSituacion;
        }

        // Una situación sin validar, o que ya no se encuentra en el contenido, vuelve provisional el indicador.
        bool SituacionValidada(Partida partida, int id)
        {
            try
            {
                var s = partida.contenido_version == 2 && resolverSituacion != null
                    ? resolverSituacion(partida, id)
                    : contenido.situaciones.FirstOrDefault(x => x.id == id);
                return s != null && s.validada;
            }
            catch (ErrorJuego) { return false; }   // paquete V2 distinto del de la partida
        }

        Indicadores CalcularIndicadores(List<Partida> partidas)
        {
            var ids = new HashSet<int>(partidas.Select(p => p.id));
            var decisiones = db.Decisiones.Where(d => ids.Contains(d.partida_id)).ToList();
            var r = CalculadoraIndicadores.Calcular(partidas, decisiones);
            r.ContenidoSinValidar = decisiones.Any(d => !SituacionValidada(partidas.First(p => p.id == d.partida_id), d.situacion_id));
            return r;
        }

        // S3-09: indicadores acumulados del jugador sobre sus partidas FINALIZADAS (las en curso no cuentan).
        public Indicadores IndicadoresJugador(int jugadorId)
        {
            usuarios.Exigir(Accion.VerPartidasDeJugador);
            return CalcularIndicadores(db.Partidas.Where(p => p.jugador_id == jugadorId && p.estado == EstadoPartida.Finalizada).ToList());
        }

        string NombreTramo(Partida partida, int id) => partida.contenido_version == 2 && resolverTramo != null
            ? resolverTramo(partida, id).nombre
            : contenido.tramos.FirstOrDefault(t => t.id == id)?.nombre ?? $"Tramo {id}";

        // El jugador ve solo sus partidas; el administrador, cualquiera.
        public Resumen Resumen(int partidaId)
        {
            var p = db.Partidas.FirstOrDefault(x => x.id == partidaId);
            if (usuarios.Actual.Rol == Rol.Administrador) usuarios.Exigir(Accion.VerResumenDeCualquiera);
            else usuarios.ExigirPartidaPropia(p, Accion.VerResumenPropio);
            if (p == null) throw new ErrorJuego("Partida no encontrada");

            var r = new Resumen { Partida = p, Jugador = db.Jugadores.First(j => j.id == p.jugador_id) };
            var acumulado = 0;
            foreach (var d in db.Decisiones.Where(d => d.partida_id == p.id).OrderBy(d => d.id))
            {
                var esPregunta = d.tipo == TipoSituacion.Pregunta;
                if (esPregunta && d.es_acertada) r.PreguntasCorrectas++;
                if (esPregunta && !d.es_acertada) r.PreguntasIncorrectas++;
                if (!esPregunta && d.es_acertada) r.DecisionesSeguras++;
                if (!esPregunta && !d.es_acertada) r.DecisionesInseguras++;
                if (d.tipo == TipoSituacion.Riesgo || d.tipo == TipoSituacion.Emergencia) r.RiesgosEnfrentados++;
                if (d.es_buena_practica) r.BuenasPracticas++;
                if (d.tipo == TipoSituacion.DecisionRuta && d.tramo_destino_id != d.tramo_origen_id) r.CambiosDeRuta++;

                acumulado += d.puntos_aplicados;
                var origen = NombreTramo(p, d.tramo_origen_id);
                var destino = NombreTramo(p, d.tramo_destino_id);
                r.Historial.Add(new PasoHistorial { Decision = d, TramoOrigen = origen, TramoDestino = destino, PuntosAcumulados = acumulado });
                foreach (var t in new[] { origen, destino })
                    if (r.RutaRecorrida.Count == 0 || r.RutaRecorrida[r.RutaRecorrida.Count - 1] != t) r.RutaRecorrida.Add(t);
            }
            r.Indicadores = CalcularIndicadores(new List<Partida> { p });
            return r;
        }

        // Búsqueda por nombre (contiene) o cédula (empieza por).
        public List<FilaJugador> ListarJugadores(string busqueda = "")
        {
            usuarios.Exigir(Accion.ListarJugadores);
            busqueda = (busqueda ?? "").Trim();
            return db.Jugadores
                .Where(j => busqueda == "" || j.cedula.StartsWith(busqueda)
                            || j.nombre.IndexOf(busqueda, StringComparison.CurrentCultureIgnoreCase) >= 0)
                .OrderBy(j => j.nombre)
                .Select(j =>
                {
                    var finalizadas = db.Partidas.Where(p => p.jugador_id == j.id && p.estado == EstadoPartida.Finalizada).ToList();
                    return new FilaJugador
                    {
                        Jugador = j,
                        Personaje = contenido.personajes.FirstOrDefault(p => p.id == j.personaje_id)?.nombre,
                        PartidasFinalizadas = finalizadas.Count,
                        UltimaPartida = finalizadas.Select(p => p.fecha_fin).OrderByDescending(f => f).FirstOrDefault(),
                    };
                })
                .ToList();
        }

        public List<Partida> PartidasDe(int jugadorId)
        {
            usuarios.Exigir(Accion.VerPartidasDeJugador);
            return db.Partidas.Where(p => p.jugador_id == jugadorId).OrderByDescending(p => p.id).ToList();
        }

        // BL-16. Solo partidas finalizadas.
        public byte[] ReportePdf(int partidaId)
        {
            usuarios.Exigir(Accion.GenerarReporte);
            var r = Resumen(partidaId);
            if (r.Partida.estado != EstadoPartida.Finalizada) throw new ErrorJuego("La partida aún no ha finalizado");
            var pdf = GeneradorReportes.GenerarPdf(r, IndicadoresJugador(r.Jugador.id));
            db.Ejecutar(() => eventos.Registrar(TipoEvento.ReporteGenerado, usuarios.Actual, partidaId, detalle: $"Jugador {r.Jugador.cedula}"));
            return pdf;
        }

        // Registro de eventos para el administrador: más recientes primero, con filtros opcionales.
        public List<Evento> Eventos(int partidaId = 0, int jugadorId = 0, string tipo = null)
        {
            usuarios.Exigir(Accion.VerEventos);
            return eventos.Consultar(partidaId, jugadorId, tipo);
        }
    }
}
