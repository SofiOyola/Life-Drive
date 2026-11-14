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
        GameObject panelSalida, panelConfiguracion;
        bool salidaAbierta, configuracionAbierta;
        Button menuJugador, seguirSalida, configuracionBoton, cerrarConfiguracion, reinicioConfig, musicaConfigBoton;
        TMP_Text musicaConfigTexto;
        public bool ConfiguracionAbierta => configuracionAbierta;

        // Ranura «Configuración» (donde estaba Pausar). Reúne los ajustes con el mismo modelo de botones del juego.
        void ConstruirConfiguracion(Transform cabecera)
        {
            bool jugador = alInicioJugador != null;
            configuracionBoton = jugador
                ? Boton(cabecera, "Configuración", "Configuración", 1330, 59, 190, 42, AbrirConfiguracion, out _)
                : Boton(cabecera, "Configuración", "Configuración", 1330, 24, 190, 58, AbrirConfiguracion, out _);
            var fondo = Caja("Configuración (fondo)", marco, 0, 0, 1600, 900, new Color(24 / 255f, 41 / 255f, 28 / 255f, .92f));
            fondo.raycastTarget = true;
            panelConfiguracion = fondo.gameObject;
            var panel = Caja("Panel de configuración", fondo.transform, 1094, 144, 482, 637, Panel);
            Texto(panel.transform, "Título de configuración", 24, 24, 434, 60, "Configuración", 31, true, Dorado);
            pausaBoton = Boton(panel.transform, "Pausa", "Pausar recorrido", 24, 104, 434, 65,
                () => { CerrarConfiguracion(); AlternarPausa(); }, out pausaTexto);
            var reinicioPanel = Boton(panel.transform, jugador ? "Reiniciar vista" : "Reinicio visual", "Reiniciar vista", 24, 189, 434, 65,
                () => { CerrarConfiguracion(); ReiniciarVista(); }, out _);
            if (jugador) reinicioConfig = reinicioPanel; else reinicio = reinicioPanel;
            musicaConfigBoton = Boton(panel.transform, "Música del juego", "", 24, 274, 434, 65, AlternarMusica, out musicaConfigTexto);
            var ayuda = Texto(panel.transform, "Ayuda de configuración", 24, 359, 434, 160,
                "Reiniciar vista recarga el último punto guardado. Tus respuestas, puntos y apariencia se conservan: la partida no empieza de nuevo.\n\nLa música también se silencia con la tecla M.",
                19, false, Suave);
            ayuda.alignment = TextAlignmentOptions.TopLeft;
            cerrarConfiguracion = Boton(panel.transform, "Cerrar configuración", "Volver al recorrido", 24, 539, 434, 65,
                CerrarConfiguracion, out _, true);
            panelConfiguracion.SetActive(false);
        }

        void AbrirConfiguracion()
        {
            if (configuracionAbierta || salidaAbierta || reconocimientosAbiertos) return;
            configuracionAbierta = true;
            panelConfiguracion.SetActive(true);
            panelConfiguracion.transform.SetAsLastSibling();
            ActualizarVista();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(cerrarConfiguracion.gameObject);
        }

        void CerrarConfiguracion()
        {
            if (!configuracionAbierta) return;
            configuracionAbierta = false;
            panelConfiguracion.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(configuracionBoton.gameObject);
        }

        void AlternarMusica()
        {
            var gestor = GestorAudio.Instancia;
            if (gestor != null) gestor.Silenciado = !gestor.Silenciado;
            ActualizarEtiquetaMusica();
        }

        void ActualizarEtiquetaMusica()
        {
            if (musicaConfigTexto == null) return;
            bool silenciada = GestorAudio.Instancia != null && GestorAudio.Instancia.Silenciado;
            musicaConfigTexto.text = silenciada ? "Música: silenciada" : "Música: activa";
        }

        void ConstruirNavegacionJugador(Transform cabecera)
        {
            if (alInicioJugador == null) return; // Explicit player pilot only. Standalone and V1 keep their UI.
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
            if (salidaAbierta || configuracionAbierta) return;
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
