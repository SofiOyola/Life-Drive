using System;
using System.Linq;
using LifeDrive.Mapas.InterfazVisual;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LifeDrive.IntegracionV2
{
    public sealed partial class PruebaIntegracionV2
    {
        [SerializeField] TMP_FontAsset fuentePixel, fuenteLectura;
        TMP_FontAsset pixelTemporal, lecturaTemporal;
        static readonly Color Bosque = new Color32(24, 41, 28, 255);
        static readonly Color Panel = new Color32(43, 64, 44, 255);
        static readonly Color Musgo = new Color32(82, 101, 59, 255);
        static readonly Color Dorado = new Color32(223, 193, 105, 255);
        static readonly Color Crema = new Color32(245, 235, 207, 255);
        static readonly Color Suave = new Color32(194, 201, 168, 255);
        RectTransform marco, relleno;
        TMP_Text titulo, detalle, mapa, puntos, pausaTexto;
        Button inicio, reinicio, continuar, pausaBoton, nuevaPrueba;
        readonly Button[] respuestas = new Button[3];
        readonly TMP_Text[] textos = new TMP_Text[3];

        void ConstruirVista()
        {
            pixelTemporal = CrearFuenteTemporal(fuentePixel);
            lecturaTemporal = CrearFuenteTemporal(fuenteLectura);
            var lienzo = new GameObject("Interfaz V2", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            lienzo.transform.SetParent(transform, false);
            lienzo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var eventos = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventos.transform.SetParent(transform, false);
            eventos.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

            var contenedor = new GameObject("Marco 16:9", typeof(RectTransform));
            marco = (RectTransform)contenedor.transform;
            marco.SetParent(lienzo.transform, false);
            marco.anchorMin = marco.anchorMax = new Vector2(.5f, .5f);
            marco.sizeDelta = new Vector2(1600, 900);
            var superior = Caja("Cabecera", marco, 24, 20, 1552, 108, Bosque);
            Texto(superior.transform, "Life Drive", 24, 10, 630, 48, "LIFE DRIVE  /  EN RUTA", 38, true);
            Texto(superior.transform, "Versión", 25, 61, 645, 35,
                "PRUEBA V2 · CONTENIDO EDUCATIVO PENDIENTE DE VALIDACIÓN", 18, false, Suave);
            Texto(superior.transform, "Progreso", 720, 11, 270, 38, "Progreso del recorrido", 21, false, Suave);
            Caja("Fondo progreso", superior.transform, 720, 58, 300, 15, Musgo);
            relleno = Caja("Progreso presentado", superior.transform, 720, 58, 300, 15, Dorado).rectTransform;
            relleno.pivot = new Vector2(0, .5f);
            relleno.anchoredPosition = new Vector2(720, -65.5f);
            puntos = Texto(superior.transform, "Puntos", 1060, 18, 245, 65, "0 pts", 29, true);
            pausaBoton = Boton(superior.transform, "Pausa", "Pausar", 1330, 24, 190, 58, AlternarPausa, out pausaTexto);

            var bitacora = Caja("Bitácora", marco, 1094, 144, 482, 637, Panel);
            Texto(bitacora.transform, "Sección", 24, 16, 425, 34, "BITÁCORA DEL RECORRIDO", 22, true, Dorado);
            titulo = Texto(bitacora.transform, "Título", 24, 53, 434, 55, "", 29, true);
            detalle = Texto(bitacora.transform, "Detalle", 24, 118, 434, 210, "", 25);
            detalle.alignment = TextAlignmentOptions.TopLeft;

            inicio = Boton(bitacora.transform, "Iniciar V2", "Iniciar / retomar V2", 24, 539, 434, 65,
                () => Abrir(false), out _, true);
            continuar = Boton(bitacora.transform, "Continuar", "Continuar recorrido", 24, 539, 434, 65,
                Continuar, out _, true);
            nuevaPrueba = Boton(bitacora.transform, "Nueva prueba", "Iniciar otra prueba", 24, 539, 434, 65,
                () => { if (alResultado != null) VerResultado(); else Abrir(true); }, out var etiquetaFinal, true);
            if (alResultado != null) etiquetaFinal.text = "Ver resultado de la operación";
            for (int i = 0; i < respuestas.Length; i++)
            {
                int indice = i;
                respuestas[i] = Boton(bitacora.transform, "Respuesta " + (i + 1), "", 24, 342 + 90 * i,
                    434, 78, () => Responder(indice), out textos[i]);
            }
            var pie = Caja("Estado", marco, 24, 800, 1552, 76, Bosque);
            mapa = Texto(pie.transform, "Mapa", 24, 10, 650, 54, "", 23, true);
            Texto(pie.transform, "Insignias", 706, 10, 380, 54, "Insignias pendientes de integración", 19, false, Suave);
            reinicio = Boton(pie.transform, "Reinicio visual", "Reiniciar vista", 1266, 10, 262, 54,
                ReiniciarVista, out _);
            AjustarVista();
        }

        // Dynamic glyphs generated in the pilot must never be written into the shared font assets.
        static TMP_FontAsset CrearFuenteTemporal(TMP_FontAsset origen) =>
            TMP_FontAsset.CreateFontAsset(origen.sourceFontFile, Mathf.RoundToInt(origen.faceInfo.pointSize), origen.atlasPadding,
                origen.atlasRenderMode, origen.atlasWidth, origen.atlasHeight);

        void OnDestroy()
        {
            LiberarFuente(pixelTemporal);
            LiberarFuente(lecturaTemporal);
        }

        static void LiberarFuente(TMP_FontAsset fuente)
        {
            if (fuente == null) return;
            foreach (var atlas in fuente.atlasTextures) Destroy(atlas);
            Destroy(fuente.material);
            Destroy(fuente);
        }

        void AjustarVista()
        {
            if (marco == null) return;
            float escala = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
            marco.localScale = Vector3.one * escala;
            reproductor.AreaVisualExterna = new Rect(
                (Screen.width - 1600 * escala) / 2 + 24 * escala,
                (Screen.height - 900 * escala) / 2 + 150 * escala,
                1052 * escala, 592 * escala);
            float progreso = estado == null ? 0 : estado.CierrePresentado ? 1 :
                (estado.Historial.Length - 1 + (MapaVisible == estado.MapaActual ? ProgresoMapa : 1)) / 5;
            relleno.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300 * Mathf.Clamp01(progreso));
        }

        void DeshabilitarRespuestas()
        {
            foreach (var respuesta in respuestas) respuesta.interactable = false;
        }

        void ActualizarVista()
        {
            if (marco == null) return;
            var partida = juego?.BaseDatos.Partidas.FirstOrDefault(p => p.id == partidaId);
            puntos.text = (partida?.puntos ?? 0) + " pts de seguridad";
            mapa.text = "MAPA " + (MapaVisible ?? "S0");
            inicio.gameObject.SetActive(Estado == Fase.Preparada || Estado == Fase.Error && partidaId == 0);
            reinicio.gameObject.SetActive(partidaId != 0);
            continuar.gameObject.SetActive(Estado == Fase.Consecuencia);
            nuevaPrueba.gameObject.SetActive(Estado == Fase.Entregada);
            pausaBoton.gameObject.SetActive(Estado == Fase.EnMarcha || Estado == Fase.Transicion);
            pausaTexto.text = pausa ? "Reanudar" : "Pausar";

            for (int i = 0; i < respuestas.Length; i++)
            {
                bool visible = Estado == Fase.Situacion && i < situacion.alternativas.Length;
                respuestas[i].gameObject.SetActive(visible);
                respuestas[i].interactable = visible;
                if (visible) textos[i].text = situacion.alternativas[i].descripcion;
            }

            titulo.text = Estado switch
            {
                Fase.Situacion => "Situación " + situacion.id,
                Fase.Consecuencia => "Respuesta registrada",
                Fase.Transicion => pausa ? "Transición en pausa" : "En transición",
                Fase.Entregada => "Entrega completada",
                Fase.Error => "No se pudo continuar",
                Fase.EnMarcha => pausa ? "Recorrido en pausa" : "En camino",
                _ => "Prueba de integración"
            };

            detalle.text = Estado switch
            {
                Fase.Situacion => situacion.enunciado,
                Fase.Consecuencia => Retroalimentacion(),
                Fase.Transicion => "El camión continúa por el trayecto confirmado.",
                Fase.Entregada => "El camión llegó a DELIVERY y la última respuesta quedó guardada.",
                Fase.Error => error,
                Fase.EnMarcha => "El camión se detendrá en el próximo punto de decisión.",
                _ => "Inicia una partida candidata o retoma la prueba guardada en este equipo."
            };
        }

        string Retroalimentacion()
        {
            var decision = estado.DecisionConfirmada;
            string veredicto = decision.tipo == "PREGUNTA"
                ? (decision.es_acertada ? "Correcta" : "Incorrecta")
                : (decision.es_acertada ? "Acción segura" : "Acción insegura");
            return $"{veredicto} · {decision.puntos_aplicados:+0;-0;0} pts · {decision.retraso_aplicado} min\n\n{decision.consecuencia}";
        }

        static RectTransform Rectangulo(string nombre, Transform padre, float x, float y, float ancho, float alto)
        {
            var rect = (RectTransform)new GameObject(nombre, typeof(RectTransform)).transform;
            rect.SetParent(padre, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(x + ancho / 2, -y - alto / 2);
            rect.sizeDelta = new Vector2(ancho, alto);
            return rect;
        }

        static FormaRedondeadaUI Caja(string nombre, Transform padre, float x, float y, float ancho, float alto, Color color)
        {
            var forma = Rectangulo(nombre, padre, x, y, ancho, alto).gameObject.AddComponent<FormaRedondeadaUI>();
            forma.color = color;
            forma.raycastTarget = false;
            return forma;
        }

        TMP_Text Texto(Transform padre, string nombre, float x, float y, float ancho, float alto,
            string texto, int tamano, bool pixel = false, Color? color = null)
        {
            var t = Rectangulo(nombre, padre, x, y, ancho, alto).gameObject.AddComponent<TextMeshProUGUI>();
            t.font = pixel ? pixelTemporal : lecturaTemporal;
            t.text = texto;
            t.fontSize = tamano;
            t.fontSizeMin = 16;
            t.fontSizeMax = tamano;
            t.enableAutoSizing = true;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.overflowMode = TextOverflowModes.Truncate;
            t.color = color ?? Crema;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;
            return t;
        }

        Button Boton(Transform padre, string nombre, string texto, float x, float y, float ancho, float alto,
            Action accion, out TMP_Text etiqueta, bool principal = false)
        {
            var forma = Caja(nombre, padre, x, y, ancho, alto, principal ? Dorado : Musgo);
            forma.raycastTarget = true;
            var boton = forma.gameObject.AddComponent<Button>();
            boton.targetGraphic = forma;
            boton.transition = Selectable.Transition.None;
            var feedback = forma.gameObject.AddComponent<RespuestaBotonUI>();
            feedback.normal = forma.color;
            feedback.hover = principal ? new Color32(242, 217, 139, 255) : new Color32(119, 134, 76, 255);
            boton.onClick.AddListener(() => accion());
            etiqueta = Texto(forma.transform, "Etiqueta", 14, 5, ancho - 28, alto - 10, texto, 23,
                principal, principal ? Bosque : Crema);
            etiqueta.alignment = TextAlignmentOptions.Center;
            return boton;
        }
    }
}
