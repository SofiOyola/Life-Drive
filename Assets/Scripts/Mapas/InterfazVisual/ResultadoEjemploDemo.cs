using System;
using UnityEngine;

namespace LifeDrive.Mapas.InterfazVisual
{
    public enum TonoResultadoDemo { Seguro, Complicaciones, Critico }

    [Serializable]
    public class DecisionResumenDemo
    {
        public string decision;
        [TextArea] public string consecuencia;
        public int cambioIlustrativo;
    }

    // Authored presentation snapshots. No evaluation, thresholds or real-event IDs.
    [CreateAssetMenu(menuName = "Life Drive/Demo/Resultado visual ficticio")]
    public sealed class ResultadoEjemploDemo : ScriptableObject
    {
        public string titulo;
        public TonoResultadoDemo tono;
        public string jugador = "Conductor DEMO";
        public int personajeId = 2;
        public int puntosSimulados;
        [TextArea] public string resumen;
        [TextArea] public string mensajeEducativo;
        public string[] insignias = new string[0];
        public DecisionResumenDemo[] decisiones = new DecisionResumenDemo[0];
    }
}
