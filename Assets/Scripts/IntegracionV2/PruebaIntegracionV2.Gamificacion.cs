using LifeDrive.Mapas;
using LifeDrive.Mapas.Demo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LifeDrive.IntegracionV2
{
    public sealed partial class PruebaIntegracionV2
    {
        [SerializeField] GamificacionDemo catalogoReconocimientos;
        readonly IconosReconocimientoDemo iconosReconocimientos = new IconosReconocimientoDemo();
        GameObject bitacoraGamificacion, panelReconocimientos;
        TMP_Text progresoTexto, variacionTexto, situacionesTexto, reconocimientosTexto, practicasTexto;
        Button reconocimientosBoton;
        bool reconocimientosAbiertos;
        public float ProgresoRecorrido { get; private set; }
        public bool ReconocimientosAbiertos => reconocimientosAbiertos;
        public GamificacionPartidaV2 Gamificacion { get; private set; }

        void ConstruirReconocimientos(GameObject bitacora)
        {
            bitacoraGamificacion = bitacora;
            panelReconocimientos = Caja("Panel reconocimientos", marco, 1094, 144, 482, 637, Panel).gameObject;
            var p = panelReconocimientos.transform;
            Texto(p, "Título reconocimientos", 24, 15, 434, 42, "RECONOCIMIENTOS", 27, true, Dorado);
            practicasTexto = Texto(p, "Buenas prácticas registradas", 24, 64, 434, 108, "", 22);
            if (catalogoReconocimientos != null)
            {
                for (int i = 0; i < catalogoReconocimientos.reconocimientos.Length; i++)
                {
                    var d = catalogoReconocimientos.reconocimientos[i];
                    var icono = Rectangulo("Icono pendiente " + i, p, 28, 195 + i * 90, 48, 48).gameObject.AddComponent<RawImage>();
                    icono.texture = iconosReconocimientos.Obtener(d.icono); icono.color = Suave * .65f; icono.raycastTarget = false;
                    Texto(p, "Insignia pendiente " + i, 94, 186 + i * 90, 352, 40, d.nombre, 22, true);
                    Texto(p, "Regla pendiente " + i, 94, 226 + i * 90, 352, 36, "Pendiente de reglas aprobadas", 18, false, Suave);
                }
            }
            Texto(p, "Aviso de reconocimientos", 24, 472, 434, 56,
                "Las insignias de esta colección aún no están disponibles.", 18, false, Suave);
            Boton(p, "Cerrar reconocimientos", "Volver al recorrido", 24, 539, 434, 65, AlternarReconocimientos, out _, true);
            panelReconocimientos.SetActive(false);
        }

        void AlternarReconocimientos()
        {
            if (salidaAbierta) return;
            reconocimientosAbiertos = !reconocimientosAbiertos;
            panelReconocimientos.SetActive(reconocimientosAbiertos);
            bitacoraGamificacion.SetActive(!reconocimientosAbiertos);
            pausaBoton.interactable = !reconocimientosAbiertos;
            // Presentation is suspended by Update. No decision, event, reward or manual-pause change.
        }

        void ActualizarGamificacion()
        {
            if (Estado == Fase.Error)
            {
                Gamificacion = null;
                variacionTexto.text = "Datos no disponibles"; variacionTexto.color = Suave;
                situacionesTexto.text = "No se pudo consultar la partida";
                reconocimientosTexto.text = "Reconocimientos no disponibles";
                practicasTexto.text = "No se pudo consultar la partida.";
                reconocimientosBoton.interactable = false;
                return; // Do not repeat the failing service query while trying to present its error.
            }
            reconocimientosBoton.interactable = true;
            if (esV1) { ActualizarGamificacionV1(); return; }
            Gamificacion = partidaId == 0 ? null : GamificacionPartidaV2.Consultar(juego, partidaId);
            var ultima = Gamificacion?.UltimaDecision;
            variacionTexto.text = ultima == null ? "Sin respuestas guardadas" :
                ultima.puntos_aplicados.ToString("+0;-0;0") + " pts · " +
                (ultima.puntos_aplicados > 0 ? "Ganancia" : ultima.puntos_aplicados < 0 ? "Pérdida" : "Sin cambio");
            variacionTexto.color = ultima == null || ultima.puntos_aplicados == 0 ? Crema :
                ultima.puntos_aplicados > 0 ? Dorado : new Color32(239, 170, 130, 255);
            situacionesTexto.text = "Situaciones resueltas: " + (Gamificacion?.SituacionesResueltas ?? 0) + " · " +
                (estado?.CierrePresentado == true ? "Entrega presentada" : estado?.MapaActual == "DELIVERY" ? "Llegada a DELIVERY" : "Entrega pendiente");
            int buenas = Gamificacion?.BuenasPracticas.Length ?? 0;
            reconocimientosTexto.text = "Buenas prácticas: " + buenas + " / Ver reconocimientos";
            practicasTexto.text = "Buenas prácticas registradas: " + buenas + "\n" +
                (buenas == 0 ? "Todavía no hay evidencias registradas." : "Respaldadas por decisiones y eventos del motor.") +
                "\nInsignias otorgadas: ninguna.";
        }
    }
}
