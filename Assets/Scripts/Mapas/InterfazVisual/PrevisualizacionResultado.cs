using System;
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Mapas.Demo;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.Mapas.InterfazVisual
{
    public sealed class PrevisualizacionResultado : MonoBehaviour
    {
        public ResultadoEjemploDemo[] ejemplos;
        public GamificacionDemo definiciones;
        public TMP_Text titulo, jugador, personaje, puntos, resumen, educativo, entrega, progreso, conteoInsignias, aviso;
        public TMP_Text[] nombresInsignias, estadosInsignias, filasDecisiones;
        public RawImage retrato;
        public RawImage[] iconosInsignias;
        public Texture2D atlasPersonajes;
        public Image acento;
        public Button[] selectores;
        public Button volver, revisar;
        public GameObject panelDecisiones;
        readonly IconosReconocimientoDemo iconos = new IconosReconocimientoDemo();
        Personaje[] personajes;
        public int Seleccionado { get; private set; }
        public bool HistorialAbierto => panelDecisiones.activeSelf;
        public ResultadoEjemploDemo EjemploActual => ejemplos[Seleccionado];

        void Start()
        {
            volver.onClick.AddListener(VolverInicio);
            revisar.onClick.AddListener(AlternarDecisiones);
            try
            {
                personajes = new Almacen(RutasContenido.Carpeta).CargarContenido().personajes;
                if (ejemplos.Length != 3 || ejemplos.Any(e => e == null || !personajes.Any(p => p.id == e.personajeId) || e.decisiones.Length != filasDecisiones.Length || e.insignias.Any(id => !definiciones.reconocimientos.Any(d => d.id == id))))
                    throw new InvalidOperationException("Revisar los tres conjuntos de datos DEMO, personajes o insignias.");
                for (int i = 0; i < selectores.Length; i++) { int opcion = i; selectores[i].onClick.AddListener(() => Mostrar(opcion)); }
                Mostrar(0);
            }
            catch (Exception e) when (e is IOException || e is InvalidOperationException || e is ArgumentException)
            {
                aviso.text = "No se pudo cargar la presentación: " + e.Message;
                foreach (var b in selectores) b.interactable = false;
                revisar.interactable = false;
            }
        }
        public void Mostrar(int indice)
        {
            if (personajes == null || indice < 0 || indice >= ejemplos.Length) return;
            Seleccionado = indice;
            var e = EjemploActual;
            titulo.text = e.titulo;
            Color color = e.tono == TonoResultadoDemo.Seguro ? new Color32(223,193,105,255) : e.tono == TonoResultadoDemo.Complicaciones ? new Color32(255,207,106,255) : new Color32(255,145,127,255);
            titulo.color = color; acento.color = color;
            jugador.text = e.jugador;
            personaje.text = "Personaje: " + personajes.Single(p => p.id == e.personajeId).nombre;
            puntos.text = (e.puntosSimulados >= 0 ? "+" : "") + e.puntosSimulados + " pts DEMO";
            resumen.text = e.resumen; educativo.text = e.mensajeEducativo;
            entrega.text = "ENTREGA COMPLETADA · DELIVERY";
            progreso.text = "TRAYECTO COMPLETADO · 100%";
            conteoInsignias.text = "INSIGNIAS DEMO · " + e.insignias.Length + "/" + definiciones.reconocimientos.Length;
            int celda = e.personajeId - 1;
            retrato.texture = atlasPersonajes; retrato.uvRect = new Rect(celda % 2 * .5f, celda < 2 ? .5f : 0, .5f, .5f);
            for (int i = 0; i < definiciones.reconocimientos.Length; i++)
            {
                var d = definiciones.reconocimientos[i]; bool obtenida = e.insignias.Contains(d.id);
                nombresInsignias[i].text = d.nombre;
                estadosInsignias[i].text = obtenida ? "OBTENIDA EN ESTE EJEMPLO" : "NO OBTENIDA EN ESTE EJEMPLO";
                estadosInsignias[i].color = obtenida ? new Color32(223,193,105,255) : new Color32(194,201,168,255);
                iconosInsignias[i].texture = iconos.Obtener(d.icono);
                iconosInsignias[i].color = obtenida ? new Color32(255,207,106,255) : new Color(.45f,.55f,.57f);
            }
            for (int i = 0; i < filasDecisiones.Length; i++)
            {
                var d = e.decisiones[i];
                filasDecisiones[i].text = (i + 1) + ". " + d.decision + "  /  " + (d.cambioIlustrativo >= 0 ? "+" : "") + d.cambioIlustrativo + " pts DEMO\n" + d.consecuencia;
            }
            for (int i = 0; i < selectores.Length; i++) selectores[i].interactable = i != indice;
            panelDecisiones.SetActive(false); revisar.GetComponentInChildren<TMP_Text>().text = "Revisar decisiones DEMO";
            aviso.text = "DEMO · Tres ejemplos independientes. Sin clasificación automática ni datos guardados.";
        }
        public void AlternarDecisiones()
        {
            panelDecisiones.SetActive(!panelDecisiones.activeSelf);
            revisar.GetComponentInChildren<TMP_Text>().text = HistorialAbierto ? "Ocultar decisiones DEMO" : "Revisar decisiones DEMO";
        }
        public void VolverInicio()
        {
            if (PrevisualizacionPantallas.NavegarEnEditor != null && PrevisualizacionPantallas.NavegarEnEditor("Inicio")) return;
            if (SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Inicio.unity") >= 0) SceneManager.LoadScene("Inicio");
            else aviso.text = "Inicio no está en Build Settings. Previsualizar esta navegación dentro del Editor.";
        }
        void OnDestroy() => iconos.Liberar();
    }
}

