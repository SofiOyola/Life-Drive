// Contenido del recorrido (antes tablas personaje, tramo_ruta, situacion y alternativa del S2-06).
// Se edita en StreamingAssets/Contenido/ruta.json; los nombres en snake_case coinciden con ese JSON.
// Un id 0 significa "ninguno" (JsonUtility no admite null en enteros).
using System;

namespace LifeDrive.Datos
{
    [Serializable]
    public class Contenido
    {
        public Personaje[] personajes;
        public Tramo[] tramos;
        public Situacion[] situaciones;
    }

    [Serializable] public class Personaje { public int id; public string nombre; public string descripcion; }

    [Serializable]
    public class Tramo
    {
        public int id;
        public string nombre;
        public string descripcion;
        public bool es_salida;
        public bool es_destino;
        public int siguiente_tramo_id;   // camino por defecto; 0 solo en el destino
    }

    public static class TipoSituacion
    {
        public const string Pregunta = "PREGUNTA";
        public const string DecisionRuta = "DECISION_RUTA";
        public const string Riesgo = "RIESGO";
        public const string Emergencia = "EMERGENCIA";
    }

    [Serializable]
    public class Situacion
    {
        public int id;
        public int tramo_id;
        public int orden;                 // orden de aparición dentro del tramo
        public string tipo;               // TipoSituacion
        public string tema;               // SEGURIDAD_VIAL, MANEJO_CARGA, FATIGA_PAUSAS…
        public string enunciado;
        public bool validada;             // revisada con la organización (1.3.4)
        public string fecha_validacion;
        public Alternativa[] alternativas;
    }

    [Serializable]
    public class Alternativa
    {
        public int id;
        public string descripcion;
        public bool es_acertada;          // respuesta correcta o acción segura
        public bool es_buena_practica;    // decisión preventiva con reconocimiento
        public int puntos;                // suma (+) o resta (−)
        public int retraso_min;
        public string explicacion;        // retroalimentación que ve el jugador
        public int tramo_destino_id;      // desvío; 0 = sigue en el mismo camino
    }

    // Cuentas creadas con Life Drive > Administradores (Editor). Solo se guarda el hash.
    [Serializable] public class Administrador { public string usuario; public string sal; public string hash; public int iteraciones; }
    [Serializable] public class ListaAdministradores { public Administrador[] administradores = new Administrador[0]; }
}
