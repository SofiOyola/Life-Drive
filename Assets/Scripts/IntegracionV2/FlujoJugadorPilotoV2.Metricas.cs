using System.Linq;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LifeDrive.IntegracionV2
{
    public sealed partial class FlujoJugadorPilotoV2
    {
        static readonly Color OroMetricas = new Color32(223, 193, 105, 255);
        static readonly Color BosqueMetricas = new Color32(24, 41, 28, 255);

        void PrepararMetricas(PrevisualizacionResultado vista, Resumen resumen)
        {
            var panel = (RectTransform)vista.panelDecisiones.transform;
            var lienzo = panel.parent;
            var anterior = vista.selectores[0];
            var siguiente = vista.selectores[1];
            var nuevaOperacion = vista.selectores[2];
            foreach (var boton in vista.selectores) boton.onClick.RemoveAllListeners();

            // The arrows belong to the card. All actions outside it remain on the lower row.
            PrepararFlecha(anterior, panel, 64, "←", vista.aviso.font);
            PrepararFlecha(siguiente, panel, 1584, "→", vista.aviso.font);
            Colocar((RectTransform)nuevaOperacion.transform, 694, 910, 500, 72);
            Colocar((RectTransform)vista.revisar.transform, 80, 910, 510, 72);
            Colocar((RectTransform)vista.volver.transform, 1380, 910, 460, 72);
            Colocar(panel, 80, 250, 1760, 602);
            panel.GetComponent<Image>().enabled = false;
            var tarjetaVisual = new GameObject("Superficie de la tarjeta", typeof(RectTransform));
            var rectTarjeta = (RectTransform)tarjetaVisual.transform;
            rectTarjeta.SetParent(panel, false);
            rectTarjeta.SetAsFirstSibling();
            Colocar(rectTarjeta, 0, 0, 1760, 602);
            var superficie = tarjetaVisual.AddComponent<FormaRedondeadaUI>();
            superficie.color = new Color32(43, 64, 44, 255);
            superficie.radio = 28;
            superficie.raycastTarget = false;
            vista.acento.rectTransform.anchoredPosition += new Vector2(0, 64);

            var elementosResumen = lienzo.Cast<Transform>().Where(t => t.gameObject.activeSelf &&
                t != panel && t != vista.revisar.transform && t != vista.volver.transform &&
                t != vista.acento.transform && t != nuevaOperacion.transform &&
                t != vista.titulo.transform && t != vista.aviso.transform &&
                t.name != "Fondo" && t.name != "Modo" && t.name != "Ayuda de teclado").ToArray();
            foreach (RectTransform elemento in elementosResumen)
                elemento.anchoredPosition += new Vector2(0, 64);

            var categoria = panel.Find("Titulo decisiones").GetComponent<TMP_Text>();
            var valor = vista.filasDecisiones[0];
            var explicacion = vista.filasDecisiones[1];
            var pagina = panel.Find("Nota decisiones").GetComponent<TMP_Text>();
            vista.filasDecisiones[2].gameObject.SetActive(false);
            EstiloTarjeta(categoria, 80, 52, 1600, 54, 32);
            EstiloTarjeta(valor, 120, 127, 1520, 120, 55);
            EstiloTarjeta(explicacion, 175, 258, 1410, 206, 33);
            EstiloTarjeta(pagina, 240, 497, 1280, 66, 24);
            categoria.color = OroMetricas;

            var tarjetas = new[]
            {
                ("SEGURIDAD", vista.puntos.text, "Balance de puntos de las respuestas guardadas."),
                ("TIEMPO", resumen.Partida.retraso_min + " min", "Retraso registrado durante la operación."),
                ("DECISIONES", resumen.Historial.Count + " respuestas",
                    $"Preguntas correctas: {resumen.PreguntasCorrectas} · Incorrectas: {resumen.PreguntasIncorrectas}\n" +
                    $"Decisiones seguras: {resumen.DecisionesSeguras} · Inseguras: {resumen.DecisionesInseguras}"),
                ("BUENAS PRÁCTICAS", resumen.BuenasPracticas.ToString(), "Acciones preventivas registradas durante el recorrido."),
                ("RECORRIDO", "Entrega completada", vista.educativo.text)
            }.Concat(resumen.Historial.Select((paso, i) =>
                ("DECISIÓN " + (i + 1), paso.Decision.alternativa,
                    $"{paso.Decision.puntos_aplicados:+0;-0;0} pts · {paso.Decision.retraso_aplicado} min\n\n" + paso.Decision.consecuencia)))
                .ToArray();

            var etiqueta = vista.revisar.GetComponentInChildren<TMP_Text>();
            var respuesta = vista.revisar.GetComponent<RespuestaBotonUI>();
            var rellenoBoton = vista.revisar.GetComponentInChildren<FormaRedondeadaUI>();
            if (rellenoBoton != null && rellenoBoton != vista.revisar.targetGraphic)
            {
                vista.revisar.targetGraphic.enabled = false;
                rellenoBoton.raycastTarget = true;
                vista.revisar.targetGraphic = rellenoBoton;
            }
            vista.revisar.transition = Selectable.Transition.None;
            respuesta.normal = OroMetricas;
            respuesta.hover = new Color32(242, 217, 139, 255);
            vista.revisar.targetGraphic.color = respuesta.normal;
            etiqueta.color = BosqueMetricas;
            int indice = 0;
            void Pintar()
            {
                var tarjeta = tarjetas[indice];
                categoria.text = tarjeta.Item1;
                valor.text = tarjeta.Item2;
                explicacion.text = tarjeta.Item3;
                pagina.text = $"Tarjeta {indice + 1} de {tarjetas.Length}";
                anterior.interactable = panel.gameObject.activeSelf && indice > 0;
                siguiente.interactable = panel.gameObject.activeSelf && indice < tarjetas.Length - 1;
            }

            anterior.onClick.AddListener(() => { indice = Mathf.Max(0, indice - 1); Pintar(); });
            siguiente.onClick.AddListener(() => { indice = Mathf.Min(tarjetas.Length - 1, indice + 1); Pintar(); });
            vista.revisar.onClick.RemoveAllListeners();
            vista.revisar.onClick.AddListener(() =>
            {
                bool mostrar = !panel.gameObject.activeSelf;
                foreach (var elemento in elementosResumen) elemento.gameObject.SetActive(!mostrar);
                nuevaOperacion.gameObject.SetActive(!mostrar);
                panel.gameObject.SetActive(mostrar);
                etiqueta.text = mostrar ? "Ocultar métricas" : "Ver métricas";
                vista.titulo.text = mostrar ? "Métricas del recorrido" : "Resumen de la operación";
                indice = 0;
                Pintar();
            });
            panel.gameObject.SetActive(false);
            etiqueta.text = "Ver métricas";
            Pintar();
        }

        static void PrepararFlecha(Button boton, RectTransform panel, float x, string texto, TMP_FontAsset fuente)
        {
            var rect = (RectTransform)boton.transform;
            rect.SetParent(panel, false);
            Colocar(rect, x, 490, 112, 80);
            var etiqueta = boton.GetComponentInChildren<TMP_Text>();
            etiqueta.text = texto;
            etiqueta.font = fuente;
            etiqueta.fontSize = 44;
            boton.gameObject.SetActive(true);
        }

        static void EstiloTarjeta(TMP_Text texto, float x, float y, float ancho, float alto, float tamano)
        {
            Colocar(texto.rectTransform, x, y, ancho, alto);
            texto.alignment = TextAlignmentOptions.Center;
            texto.fontSize = texto.fontSizeMax = tamano;
            texto.fontSizeMin = 22;
            texto.enableAutoSizing = true;
        }

        static void Colocar(RectTransform rect, float x, float y, float ancho, float alto)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(ancho, alto);
        }

        static void DarColorInsignia(RawImage icono, Color tono)
        {
            icono.color = tono;
            var rect = icono.rectTransform;
            var baseIcono = new GameObject("Base de insignia", typeof(RectTransform));
            var baseRect = (RectTransform)baseIcono.transform;
            baseRect.SetParent(rect.parent, false);
            baseRect.SetSiblingIndex(rect.GetSiblingIndex());
            baseRect.anchorMin = rect.anchorMin;
            baseRect.anchorMax = rect.anchorMax;
            baseRect.pivot = rect.pivot;
            baseRect.sizeDelta = rect.sizeDelta + new Vector2(20, 20);
            baseRect.anchoredPosition = rect.anchoredPosition + new Vector2(-10, 10);
            var forma = baseIcono.AddComponent<FormaRedondeadaUI>();
            forma.color = BosqueMetricas;
            forma.raycastTarget = false;
        }
    }
}
