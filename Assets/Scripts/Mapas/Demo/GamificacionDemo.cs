using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LifeDrive.Mapas.Demo
{
    public enum IconoReconocimientoDemo { Escudo, Camino, Apoyo }

    [Serializable]
    public class ReconocimientoDemo
    {
        public string id, nombre;
        [TextArea] public string descripcion;
        [TextArea] public string condicionSimulada;
        public string senalRequerida;
        public IconoReconocimientoDemo icono;
    }

    [CreateAssetMenu(menuName = "Life Drive/Demo/Gamificación ficticia")]
    public sealed class GamificacionDemo : ScriptableObject
    {
        public ReconocimientoDemo[] reconocimientos = new ReconocimientoDemo[0];
    }

    // Presentation input only: a future adapter could supply confirmed results here.
    // This DTO does not evaluate answers and has no dependency on Motor or Datos.
    public sealed class ResultadoPresentacionDemo
    {
        public string IdSituacion;
        public int CambioPuntos;
        public string Explicacion;
        public string[] Senales = new string[0];
    }

    public sealed class EstadoGamificacionDemo
    {
        readonly ReconocimientoDemo[] definiciones;
        readonly HashSet<string> respuestas = new HashSet<string>(StringComparer.Ordinal);
        readonly HashSet<string> desbloqueadas = new HashSet<string>(StringComparer.Ordinal);
        public int Puntos { get; private set; }
        public int UltimoCambio { get; private set; }
        public string UltimaExplicacion { get; private set; }
        public int SituacionesResueltas => respuestas.Count;
        public int TotalSituaciones { get; }
        public int TotalInsignias => definiciones.Length;
        public int InsigniasDesbloqueadas => desbloqueadas.Count;
        public float ProgresoTrayecto { get; private set; }

        public EstadoGamificacionDemo(GamificacionDemo datos, int totalSituaciones)
        {
            if (datos == null || datos.reconocimientos == null || datos.reconocimientos.Length < 3 || totalSituaciones < 1)
                throw new ArgumentException("Faltan tres reconocimientos ficticios o situaciones de prueba.");
            definiciones = datos.reconocimientos;
            if (definiciones.Any(d => d == null || string.IsNullOrWhiteSpace(d.id) || string.IsNullOrWhiteSpace(d.senalRequerida)) || definiciones.Select(d => d.id).Distinct().Count() != definiciones.Length)
                throw new ArgumentException("IDs de reconocimiento vacíos/duplicados o condiciones sin señal.");
            TotalSituaciones = totalSituaciones;
        }
        public bool Desbloqueada(string id) => desbloqueadas.Contains(id);
        public bool Aplicar(ResultadoPresentacionDemo resultado, out string[] nuevas)
        {
            nuevas = new string[0];
            if (resultado == null || string.IsNullOrWhiteSpace(resultado.IdSituacion) || !respuestas.Add(resultado.IdSituacion)) return false;
            Puntos += resultado.CambioPuntos;
            UltimoCambio = resultado.CambioPuntos;
            UltimaExplicacion = resultado.Explicacion;
            var lista = new List<string>();
            foreach (var d in definiciones)
                if ((resultado.Senales ?? new string[0]).Contains(d.senalRequerida) && desbloqueadas.Add(d.id)) lista.Add(d.nombre);
            nuevas = lista.ToArray();
            return true;
        }
        // Distance/animation input can only update this property, never points or badges.
        public void ActualizarTrayecto(float valor)
        {
            if (float.IsFinite(valor)) ProgresoTrayecto = Mathf.Max(ProgresoTrayecto, Mathf.Clamp01(valor));
        }
    }
}
