using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LifeDrive.IntegracionV2
{
    public sealed partial class PruebaIntegracionV2
    {
        Action alInicioJugador;
        GameObject panelSalida;
        bool salidaAbierta;
        Button menuJugador, seguirSalida;

        void ConstruirNavegacionJugador(Transform cabecera)
        {
            if (alInicioJugador == null) return; // Explicit player pilot only. Standalone and V1 keep their UI.
            var pausaRect = (RectTransform)pausaBoton.transform;
            pausaRect.anchoredPosition = new Vector2(1425, -80);
            pausaRect.sizeDelta = new Vector2(190, 42);
            menuJugador = Boton(cabecera, "Menú del jugador", "Volver a Inicio", 1330, 8, 190, 42, AbrirSalida, out _);

            var fondo = Caja("Confirmar regreso a Inicio", marco, 0, 0, 1600, 900, new Color(24/255f, 41/255f, 28/255f, .92f));
            fondo.raycastTarget = true;
            panelSalida = fondo.gameObject;
            // DemoRecorrido draws the map through OnGUI above the canvas. Keep this prompt in the existing
            // side panel, outside its drawing area, without modifying the renderer, map or truck.
            var panel = Caja("Panel de regreso", fondo.transform, 1094, 144, 482, 637, Panel);
            Texto(panel.transform, "Título de regreso", 24, 24, 434, 60, "¿Volver a Inicio?", 31, true, Dorado);
            Texto(panel.transform, "Datos conservados", 24, 104, 434, 302,
                "Tus respuestas, puntos y apariencia ya registrados se conservan.\n\nAl retomar volverás al último punto confirmado. Si hay una transición pendiente, verás de nuevo su consecuencia sin volver a evaluarla.\n\nDesde Inicio pulsa Jugar e ingresa los mismos datos.", 25);
            seguirSalida = Boton(panel.transform, "Seguir en la operación", "Seguir jugando", 24, 446, 434, 65, CerrarSalida, out _, true);
            Boton(panel.transform, "Confirmar Inicio", "Volver a Inicio", 24, 535, 434, 65, () =>
            {
                if (!salidaAbierta) return;
                CerrarSalida();
                Ejecutar(alInicioJugador);
            }, out _);
            panelSalida.SetActive(false);
        }

        void AbrirSalida()
        {
            if (salidaAbierta) return;
            salidaAbierta = true;
            panelSalida.SetActive(true);
            panelSalida.transform.SetAsLastSibling();
            EventSystem.current.SetSelectedGameObject(seguirSalida.gameObject);
        }

        void CerrarSalida()
        {
            salidaAbierta = false;
            panelSalida.SetActive(false);
            EventSystem.current.SetSelectedGameObject(menuJugador.gameObject);
            // No save, decision, event or checkpoint confirmation. Manual pause remains unchanged.
        }
    }
}
