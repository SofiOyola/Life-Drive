using System;
using System.Linq;
using LifeDrive.Mapas.InterfazVisual;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LifeDrive.IntegracionV2
{
    public sealed partial class PruebaIntegracionV2
    {
        Func<AparienciaJugadorPilotoV1> consultarApariencia;
        GameObject panelEspera, panelEntrega;
        RectTransform volante;
        AvatarPersonalizableUI retratoEntrega;

        void ConstruirEstadosJugador(Transform bitacora)
        {
            panelEspera = Caja("Espera del recorrido", bitacora, 24, 132, 434, 382, Bosque).gameObject;
            Caja("Base del volante", panelEspera.transform, 77, 24, 280, 280, Musgo);
            volante = Rectangulo("Volante en ruta", panelEspera.transform, 100, 47, 234, 234);
            volante.gameObject.AddComponent<VolantePixelUI>().raycastTarget = false;
            Texto(panelEspera.transform, "Estado de espera", 22, 322, 390, 38, "Rumbo a la próxima parada", 23)
                .alignment = TextAlignmentOptions.Center;
            panelEspera.SetActive(false);

            panelEntrega = Caja("Composición de entrega", bitacora, 24, 132, 434, 382, Bosque).gameObject;
            var padre = panelEntrega.transform;
            var verde = new Color32(161, 216, 114, 255);
            var fondoCheck = Caja("Confirmación de entrega", padre, 174, 24, 86, 80, new Color32(51, 90, 53, 255));
            var origenCheck = fondoCheck.transform;
            Pixel("Check 1", origenCheck, 19, 39, 10, 10, verde);
            Pixel("Check 2", origenCheck, 29, 49, 10, 10, verde);
            Pixel("Check 3", origenCheck, 39, 39, 10, 10, verde);
            Pixel("Check 4", origenCheck, 49, 29, 10, 10, verde);
            Pixel("Check 5", origenCheck, 59, 19, 10, 10, verde);

            Caja("Base del paquete", padre, 20, 129, 181, 178, Panel);
            Caja("Base del perfil", padre, 233, 129, 181, 178, Panel);
            var paquete = Rectangulo("Paquete de envío", padre, 48, 158, 126, 112);
            Pixel("Contorno", paquete, 0, 8, 126, 104, new Color32(53, 39, 26, 255));
            Pixel("Tapa", paquete, 8, 0, 110, 34, new Color32(230, 177, 102, 255));
            Pixel("Frente", paquete, 8, 34, 81, 70, new Color32(190, 124, 62, 255));
            Pixel("Costado", paquete, 89, 34, 29, 70, new Color32(146, 87, 45, 255));
            Pixel("Cinta", paquete, 47, 0, 24, 60, new Color32(250, 215, 147, 255));
            Pixel("Etiqueta", paquete, 20, 73, 28, 17, Crema);
            Pixel("Marca de la etiqueta", paquete, 24, 78, 19, 4, Musgo);
            retratoEntrega = Rectangulo("Retrato de entrega", padre, 251, 139, 145, 158).gameObject
                .AddComponent<AvatarPersonalizableUI>();
            retratoEntrega.raycastTarget = false;
            Texto(padre, "Mensaje de entrega", 22, 324, 390, 35, "Paquete entregado", 24, true, verde)
                .alignment = TextAlignmentOptions.Center;
            panelEntrega.SetActive(false);
        }

        static void Pixel(string nombre, Transform padre, float x, float y, float ancho, float alto, Color color)
        {
            var bloque = Rectangulo(nombre, padre, x, y, ancho, alto).gameObject.AddComponent<Image>();
            bloque.color = color;
            bloque.raycastTarget = false;
        }

        void PresentarEstadoJugador()
        {
            bool esperando = Estado == Fase.EnMarcha || Estado == Fase.Transicion;
            bool entregada = Estado == Fase.Entregada;
            panelEspera.SetActive(esperando);
            lecturaFicha.gameObject.SetActive(Estado == Fase.Situacion || Estado == Fase.Consecuencia || Estado == Fase.Error);
            panelEntrega.SetActive(entregada);
            if (Estado == Fase.Situacion) titulo.text = "Tu decisión";
            if (Estado == Fase.Transicion) titulo.text = pausa ? "Recorrido en pausa" : "En camino";
            if (Estado == Fase.Preparada) titulo.text = "Preparando el recorrido";
            if (!entregada) return;

            var apariencia = consultarApariencia?.Invoke();
            if (apariencia == null)
            {
                int id = juego.BaseDatos.Jugadores.Single(j => j.id == juego.Usuarios.Actual.JugadorId).personaje_id;
                apariencia = VariantesAparienciaPilotoV1.Crear(id, 0, 0, 0, 0, 0);
            }
            VariantesAparienciaPilotoV1.Aplicar(retratoEntrega, apariencia);
        }

        void AnimarVolante()
        {
            if (volante == null || !panelEspera.activeInHierarchy || pausa || salidaAbierta) return;
            float angulo = 9 * Mathf.Sin(Time.unscaledTime * 2.1f);
            volante.localRotation = Quaternion.Euler(0, 0, angulo);
        }
    }
}
