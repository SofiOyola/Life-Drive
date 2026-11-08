// Tablas de la base de datos local (modelo S2-06): jugador, partida, decision y evento.
// Los nombres en snake_case coinciden con las columnas del modelo y con el archivo guardado.
// Un id 0 significa "ninguno" (JsonUtility no admite null en enteros).
using System;
using System.Collections.Generic;

namespace LifeDrive.Datos
{
    // Contenido completo del archivo lifedrive-db.json.
    [Serializable]
    public class TablasLocales
    {
        public int version = 1;
        public Secuencias secuencias = new Secuencias();
        public List<Jugador> jugador = new List<Jugador>();
        public List<Partida> partida = new List<Partida>();
        public List<Decision> decision = new List<Decision>();
        public List<Evento> evento = new List<Evento>();
    }

    // Último id asignado en cada tabla (equivale a SERIAL): un id borrado nunca se reutiliza.
    [Serializable] public class Secuencias { public int jugador; public int partida; public int decision; public int evento; }

    [Serializable]
    public class Jugador
    {
        public int id;
        public string nombre;
        public string cedula;
        public int personaje_id;
        public string fecha_registro;
    }

    public static class EstadoPartida
    {
        public const string EnCurso = "EN_CURSO";
        public const string Finalizada = "FINALIZADA";
    }

    [Serializable]
    public class Partida
    {
        public int id;
        public int jugador_id;
        public int contenido_version; // 0 en registros antiguos equivale a V1
        public string contenido_huella; // identidad del paquete V2 al iniciar
        public int ultima_decision_presentada_id; // 0 representa el hito inicial de V2
        public string estado = EstadoPartida.EnCurso;
        public int tramo_actual_id;
        public int puntos;
        public int retraso_min;
        public string fecha_inicio;
        public string fecha_fin;
    }

    // Evento principal del juego: cada respuesta, decisión de ruta o acción ante un riesgo.
    // Guarda una copia del resultado para que el reporte no cambie si después se edita el contenido.
    [Serializable]
    public class Decision
    {
        public int id;
        public int partida_id;
        public int situacion_id;
        public int alternativa_id;
        public string fecha_hora;
        public int tramo_origen_id;
        public int tramo_destino_id;
        public string tipo;
        public string alternativa;
        public bool es_acertada;
        public bool es_buena_practica;
        public int puntos_aplicados;
        public int retraso_aplicado;
        public string consecuencia;
    }

    // Registro de eventos: todo lo que pasa en el sistema, en orden (ver Motor/RegistroEventos.cs).
    [Serializable]
    public class Evento
    {
        public int id;
        public string fecha_hora;
        public string tipo;          // TipoEvento
        public string rol;           // JUGADOR, ADMINISTRADOR o NINGUNO
        public string usuario;       // cédula del jugador o usuario del administrador
        public int jugador_id;
        public int partida_id;
        public int situacion_id;
        public string detalle;
    }
}
