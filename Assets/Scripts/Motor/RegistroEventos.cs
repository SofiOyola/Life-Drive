// Registro de eventos (modelo de eventos del S2-06): cada hecho del sistema queda en la tabla
// "evento" dentro de la misma transacción que el cambio que lo produjo, así nunca hay un evento
// sin su cambio ni un cambio sin su evento.
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LifeDrive.Datos;

namespace LifeDrive.Motor
{
    public static class TipoEvento
    {
        // Usuarios y acceso
        public const string JugadorRegistrado = "JUGADOR_REGISTRADO";
        public const string IngresoJugador = "INGRESO_JUGADOR";
        public const string SesionIniciada = "SESION_INICIADA";
        public const string SesionFallida = "SESION_FALLIDA";
        public const string SesionCerrada = "SESION_CERRADA";
        public const string AccesoDenegado = "ACCESO_DENEGADO";
        // Partida
        public const string PartidaIniciada = "PARTIDA_INICIADA";
        public const string PartidaRetomada = "PARTIDA_RETOMADA";
        public const string PreguntaRespondida = "PREGUNTA_RESPONDIDA";
        public const string RutaDecidida = "RUTA_DECIDIDA";
        public const string RiesgoAtendido = "RIESGO_ATENDIDO";
        public const string EmergenciaAtendida = "EMERGENCIA_ATENDIDA";
        public const string BuenaPractica = "BUENA_PRACTICA";
        public const string PuntosActualizados = "PUNTOS_ACTUALIZADOS";
        public const string TramoAlcanzado = "TRAMO_ALCANZADO";
        public const string PartidaFinalizada = "PARTIDA_FINALIZADA";
        // Consulta
        public const string ReporteGenerado = "REPORTE_GENERADO";

        public static string DeSituacion(string tipoSituacion) => tipoSituacion switch
        {
            TipoSituacion.Pregunta => PreguntaRespondida,
            TipoSituacion.DecisionRuta => RutaDecidida,
            TipoSituacion.Riesgo => RiesgoAtendido,
            _ => EmergenciaAtendida,
        };
    }

    public class RegistroEventos
    {
        readonly BaseDatos db;

        public RegistroEventos(BaseDatos db) => this.db = db;

        public static string Ahora() => System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        // Se llama dentro de una transacción de BaseDatos.
        public Evento Registrar(string tipo, Usuario quien, int partidaId = 0, int situacionId = 0, string detalle = null) =>
            db.Insertar(new Evento
            {
                fecha_hora = Ahora(), tipo = tipo, rol = quien.RolTexto, usuario = quien.Identificador,
                jugador_id = quien.Rol == Rol.Jugador ? quien.JugadorId : 0,
                partida_id = partidaId, situacion_id = situacionId, detalle = detalle,
            });

        // Eventos más recientes primero; filtros opcionales por partida, jugador y tipo.
        public List<Evento> Consultar(int partidaId = 0, int jugadorId = 0, string tipo = null) =>
            db.Eventos
                .Where(e => (partidaId == 0 || e.partida_id == partidaId)
                            && (jugadorId == 0 || e.jugador_id == jugadorId)
                            && (tipo == null || e.tipo == tipo))
                .OrderByDescending(e => e.id)
                .ToList();
    }
}
