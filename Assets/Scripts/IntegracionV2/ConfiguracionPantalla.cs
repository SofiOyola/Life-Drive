using System.Linq;
using LifeDrive.Mapas.InterfazVisual;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LifeDrive.IntegracionV2
{
    // Botón «Configuración» de Inicio, Registro y Resultado, con un panel que contiene el control de la música.
    // No se rehacen las escenas aprobadas: se CLONA el botón secundario de cada pantalla (Administradores, Volver, Volver a Inicio),
    // así que hereda su forma, color, fuente y efecto al pasar el ratón. El recorrido tiene su propio panel (PruebaIntegracionV2).
    public sealed class ConfiguracionPantalla : MonoBehaviour
    {
        static readonly Color Bosque = new Color32(24, 41, 28, 255), Panel = new Color32(43, 64, 44, 255),
            Dorado = new Color32(223, 193, 105, 255), DoradoClaro = new Color32(242, 217, 139, 255), Suave = new Color32(194, 201, 168, 255);

        GameObject panel;
        Button abrir, cerrar;
        TMP_Text musica;

        // true cuando la pantalla ya tiene su botón (o no necesita uno); false si su interfaz aún no existe y hay que reintentar.
        public static bool Inyectar()
        {
            if (FindAnyObjectByType<ConfiguracionPantalla>() != null) return true;
            Button plantilla;
            Vector2 centro; // posición en el lienzo de referencia de 1920 x 1080, arriba a la derecha
            var registro = FindAnyObjectByType<PersonalizacionRegistroUI>();
            var resultado = FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
            if (registro != null)
            {
                plantilla = registro.registro ? registro.volver : registro.administrador;
                centro = registro.registro ? new Vector2(1444, 100) : new Vector2(1735, 60); // en Registro, a la izquierda de «Volver»
            }
            else if (resultado != null) { plantilla = resultado.volver; centro = new Vector2(1700, 70); }
            else return false;
            if (plantilla == null || !plantilla.gameObject.activeInHierarchy) return false;

            var lienzo = plantilla.GetComponentInParent<Canvas>().rootCanvas.transform;
            var raiz = new GameObject("Configuración", typeof(RectTransform), typeof(ConfiguracionPantalla));
            var r = (RectTransform)raiz.transform; r.SetParent(lienzo, false);
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            raiz.GetComponent<ConfiguracionPantalla>().Construir(plantilla, centro);
            return true;
        }

        void Construir(Button plantilla, Vector2 centro)
        {
            var pixel = plantilla.GetComponentInChildren<TMP_Text>().font;
            // En ejecución las fuentes son copias temporales sin nombre: la de lectura es la de cualquier texto que no use la de los botones.
            var lectura = FindObjectsByType<TMP_Text>().Select(t => t.font).FirstOrDefault(f => f != null && f != pixel) ?? pixel;

            abrir = Clonar(plantilla, transform, "Configuración", "Configuración", centro, new Vector2(300, 62), 28);
            abrir.onClick.AddListener(Abrir);

            var fondo = new GameObject("Configuración (fondo)", typeof(RectTransform), typeof(Image));
            var fr = (RectTransform)fondo.transform; fr.SetParent(transform, false);
            fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.offsetMin = fr.offsetMax = Vector2.zero;
            var velo = fondo.GetComponent<Image>(); velo.color = new Color(Bosque.r, Bosque.g, Bosque.b, .94f); velo.raycastTarget = true;
            panel = fondo;

            var cuadro = new GameObject("Panel de configuración", typeof(RectTransform), typeof(FormaRedondeadaUI));
            var cr = (RectTransform)cuadro.transform; cr.SetParent(fr, false);
            cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(.5f, .5f); cr.sizeDelta = new Vector2(800, 560); cr.anchoredPosition = Vector2.zero;
            var forma = cuadro.GetComponent<FormaRedondeadaUI>(); forma.color = Panel; forma.raycastTarget = false;

            Texto(cr, "Título de configuración", "Configuración", pixel, 46, Dorado, new Vector2(400, 70), new Vector2(700, 70));
            var botonMusica = Clonar(plantilla, cr, "Música del juego", "", new Vector2(400, 210), new Vector2(680, 84), 34);
            botonMusica.onClick.AddListener(AlternarMusica);
            musica = botonMusica.GetComponentInChildren<TMP_Text>();
            Texto(cr, "Ayuda de configuración", "La música también se silencia con la tecla M. Tu elección se recuerda la próxima vez que juegues.",
                lectura, 27, Suave, new Vector2(400, 335), new Vector2(680, 110));
            cerrar = Clonar(plantilla, cr, "Cerrar configuración", "Volver", new Vector2(400, 470), new Vector2(680, 84), 34);
            cerrar.onClick.AddListener(Cerrar);
            Principal(cerrar);

            panel.SetActive(false);
        }

        // Copia del botón de la pantalla: misma forma, color, fuente y efecto; solo cambian texto, tamaño y posición.
        static Button Clonar(Button plantilla, Transform padre, string nombre, string texto, Vector2 centro, Vector2 tamano, float fuente)
        {
            var b = Instantiate(plantilla, padre);
            b.name = nombre; b.onClick.RemoveAllListeners();
            for (int i = b.transform.childCount - 1; i >= 0; i--)
                if (b.transform.GetChild(i).name == "Foco accesible") DestroyImmediate(b.transform.GetChild(i).gameObject); // el teclado crea el suyo
            var r = (RectTransform)b.transform;
            r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(.5f, .5f);
            r.anchoredPosition = new Vector2(centro.x, -centro.y); r.sizeDelta = tamano; r.localScale = Vector3.one;
            var marca = b.GetComponent<RespuestaBotonUI>();
            if (marca != null) marca.marcado = false;
            var etiqueta = b.GetComponentInChildren<TMP_Text>();
            etiqueta.text = texto; etiqueta.enableAutoSizing = false; etiqueta.fontSize = fuente;
            etiqueta.rectTransform.anchorMin = Vector2.zero; etiqueta.rectTransform.anchorMax = Vector2.one;
            etiqueta.rectTransform.offsetMin = new Vector2(12, 4); etiqueta.rectTransform.offsetMax = new Vector2(-12, -4);
            return b;
        }

        // Acción principal, como «Volver al recorrido» del panel del recorrido: dorado con texto oscuro.
        static void Principal(Button b)
        {
            var marca = b.GetComponent<RespuestaBotonUI>();
            if (marca != null) { marca.normal = Dorado; marca.hover = DoradoClaro; }
            b.targetGraphic.color = Dorado;
            b.GetComponentInChildren<TMP_Text>().color = Bosque;
        }

        static void Texto(Transform padre, string nombre, string texto, TMP_FontAsset fuente, float tamano, Color color, Vector2 centro, Vector2 dimension)
        {
            var t = new GameObject(nombre, typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            var r = t.rectTransform; r.SetParent(padre, false);
            r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(.5f, .5f);
            r.anchoredPosition = new Vector2(centro.x, -centro.y); r.sizeDelta = dimension;
            t.font = fuente; t.text = texto; t.fontSize = tamano; t.color = color; t.richText = false; t.raycastTarget = false;
            t.alignment = TextAlignmentOptions.Center; t.textWrappingMode = TextWrappingModes.Normal;
        }

        void Abrir()
        {
            panel.SetActive(true);
            transform.SetAsLastSibling(); // el piloto añade botones a la pantalla después: el velo debe quedar por encima de todos
            panel.transform.SetAsLastSibling();
            ActualizarEtiqueta();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(cerrar.gameObject);
        }

        void Cerrar()
        {
            panel.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(abrir.gameObject);
        }

        void AlternarMusica()
        {
            var gestor = GestorAudio.Instancia;
            if (gestor != null) gestor.Silenciado = !gestor.Silenciado;
            ActualizarEtiqueta();
        }

        void ActualizarEtiqueta() =>
            musica.text = GestorAudio.Instancia != null && GestorAudio.Instancia.Silenciado ? "Música: silenciada" : "Música: activa";

        void Update() { if (panel != null && panel.activeSelf) ActualizarEtiqueta(); } // la tecla M puede cambiarla con el panel abierto
    }
}
