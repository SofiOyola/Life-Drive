using System;
using UnityEngine;

namespace LifeDrive.Mapas.Demo
{
    [Serializable]
    public class RespuestaInterfazDemo
    {
        [TextArea] public string texto;
        [TextArea] public string retroalimentacion;
        public string consecuencia;
        public int puntosSimulados;
        public string explicacionPuntos;
        public string[] senalesSimuladas = new string[0];
        public MapaVisual hojaVisual;
    }

    [CreateAssetMenu(menuName = "Life Drive/Demo/Situación de interfaz ficticia")]
    public sealed class SituacionInterfazDemo : ScriptableObject
    {
        public string titulo = "Mensaje por confirmar";
        public string idDemo;
        public string prefijoMapa = "S0";
        [TextArea] public string enunciado;
        [Range(.05f, .9f)] public float puntoDemostracion = .35f;
        public RespuestaInterfazDemo[] respuestas = new RespuestaInterfazDemo[0];
    }
}
