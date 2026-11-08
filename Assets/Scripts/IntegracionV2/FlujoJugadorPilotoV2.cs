using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LifeDrive.Mapas;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.IntegracionV2
{
    // Created only by the explicit editor command or tests. No auto-activation, production session or schema changes.
    public sealed class FlujoJugadorPilotoV2 : MonoBehaviour
    {
        public ServicioJuego Juego { get; private set; }
        public int PartidaId { get; private set; }
        public int PersonajeId { get; private set; }
        public string CarpetaDatos { get; private set; }
        Action<string> cargar;
        bool navegando;
        readonly Dictionary<int, Apariencia> apariencias = new Dictionary<int, Apariencia>();
        readonly Dictionary<TMP_FontAsset, TMP_FontAsset> fuentes = new Dictionary<TMP_FontAsset, TMP_FontAsset>();
        sealed class Apariencia { public int estilo; public Color piel, ojos, cabello, traje; }

        public static FlujoJugadorPilotoV2 Activar(CatalogoMapas catalogo, string sesion, Action<string> navegar)
        {
            if (FindAnyObjectByType<FlujoJugadorPilotoV2>() != null) throw new InvalidOperationException("El piloto ya está activo.");
            if (string.IsNullOrEmpty(sesion) || sesion.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
                throw new ArgumentException("Identificador de sesión inválido.");
            var carpeta = Path.Combine(Application.temporaryCachePath, "LifeDrive-FlujoJugadorV2", sesion);
            var juego = new ServicioJuego(Path.Combine(Application.streamingAssetsPath, "Contenido"), carpeta, catalogo);
            var piloto = new GameObject("Flujo jugador PILOTO V2").AddComponent<FlujoJugadorPilotoV2>();
            DontDestroyOnLoad(piloto.gameObject);
            piloto.Juego = juego;
            piloto.CarpetaDatos = carpeta;
            piloto.cargar = navegar ?? throw new ArgumentNullException(nameof(navegar));
            SceneManager.sceneLoaded += piloto.AlCargar;
            piloto.Navegar("Inicio");
            return piloto;
        }

        public void Navegar(string escena)
        {
            if (navegando) return;
            navegando = true;
            cargar(escena);
        }

        void AlCargar(Scene escena, LoadSceneMode modo)
        {
            navegando = false;
            foreach (var raiz in escena.GetRootGameObjects())
            {
                // Disable demonstration and legacy result logic before Start.
                foreach (var resultado in raiz.GetComponentsInChildren<PrevisualizacionResultado>(true)) resultado.enabled = false;
                foreach (var resultado in raiz.GetComponentsInChildren<LifeDrive.UI.PantallaResultado>(true)) resultado.enabled = false;
                foreach (var texto in raiz.GetComponentsInChildren<TMP_Text>(true)) texto.font = Fuente(texto.font);
                foreach (var campo in raiz.GetComponentsInChildren<TMP_InputField>(true)) campo.fontAsset = Fuente(campo.fontAsset);
            }
            var partida = FindAnyObjectByType<PruebaIntegracionV2>();
            if (partida != null) partida.ConectarJugador(Juego, id => { PartidaId = id; Navegar("Resultado"); });
            StartCoroutine(Enlazar(escena));
        }

        TMP_FontAsset Fuente(TMP_FontAsset origen)
        {
            if (origen == null) return null;
            if (!fuentes.TryGetValue(origen, out var copia))
            {
                copia = TMP_FontAsset.CreateFontAsset(origen.sourceFontFile, Mathf.RoundToInt(origen.faceInfo.pointSize),
                    origen.atlasPadding, origen.atlasRenderMode, origen.atlasWidth, origen.atlasHeight);
                fuentes.Add(origen, copia);
            }
            return copia;
        }

        IEnumerator Enlazar(Scene escena)
        {
            yield return null; // Visual Start wires its preview listeners first.
            if (SceneManager.GetActiveScene() != escena) yield break;
            var vista = FindAnyObjectByType<PersonalizacionRegistroUI>();
            if (vista != null)
            {
                if (!vista.registro)
                {
                    vista.jugar.onClick.RemoveAllListeners();
                    vista.jugar.onClick.AddListener(() => Navegar("Registro"));
                    vista.mensaje.text = "PILOTO V2 · Datos temporales · Usa solo datos ficticios";
                    foreach (var t in escena.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TMP_Text>(true)))
                        if (t.name == "Previsualizacion") t.text = "PILOTO V2 EXPLÍCITO · CONTENIDO CANDIDATO";
                }
                else Registro(vista);
            }
            if (escena.name == "Resultado") Resultado();
        }

        void Registro(PersonalizacionRegistroUI v)
        {
            PersonajeId = 0; // Never infer a content ID from silhouette or colors.
            v.volver.onClick.RemoveAllListeners();
            v.volver.onClick.AddListener(() => Navegar("Inicio"));
            v.comprobar.onClick.RemoveAllListeners();
            v.comprobar.GetComponentInChildren<TMP_Text>().text = "INICIAR / RETOMAR V2";
            v.mensaje.text = "PILOTO · Elige una ficha compatible. La apariencia solo dura esta sesión de Play.";
            var privacidad = v.nombre.transform.parent.GetComponentsInChildren<TMP_Text>().First(t => t.name == "Privacidad");
            privacidad.text = "Solo datos ficticios.\nGuardado temporal de nombre, cédula y ficha.\nColores fuera del motor.";
            privacidad.fontSize = 20;
            privacidad.rectTransform.sizeDelta = new Vector2(474, 100);
            var ids = Juego.Contenido.personajes.Select(p => p.id).OrderBy(id => id).ToArray();
            for (int i = 0; i < ids.Length; i++)
            {
                int id = ids[i];
                var b = Instantiate(v.volver, privacidad.transform.parent);
                b.name = "Ficha piloto " + id;
                var r = (RectTransform)b.transform;
                r.anchorMin = r.anchorMax = new Vector2(0, 1);
                r.pivot = new Vector2(0, 1);
                r.anchoredPosition = new Vector2(32 + i * 120, -590);
                r.sizeDelta = new Vector2(114, 54);
                b.onClick.RemoveAllListeners();
                var etiqueta = b.GetComponentInChildren<TMP_Text>();
                etiqueta.text = "Ficha " + id;
                etiqueta.fontSize = 21; etiqueta.enableAutoSizing = false;
                etiqueta.textWrappingMode = TextWrappingModes.NoWrap;
                etiqueta.rectTransform.anchorMin = etiqueta.rectTransform.anchorMax = new Vector2(0, 1);
                etiqueta.rectTransform.pivot = new Vector2(.5f, .5f);
                etiqueta.rectTransform.sizeDelta = new Vector2(106, 46);
                etiqueta.rectTransform.anchoredPosition = new Vector2(57, -27);
                b.onClick.AddListener(() =>
                {
                    PersonajeId = id;
                    foreach (var ficha in privacidad.transform.parent.GetComponentsInChildren<Button>())
                        if (ficha.name.StartsWith("Ficha piloto ")) ficha.GetComponent<RespuestaBotonUI>().marcado = ficha == b;
                    v.mensaje.text = "Ficha " + id + " seleccionada. No define tu nombre, género ni colores.";
                });
            }
            v.comprobar.onClick.AddListener(() =>
            {
                if (navegando) return;
                try
                {
                    var jugador = Juego.Usuarios.RegistrarJugador(v.nombre.text, v.documento.text, PersonajeId);
                    // Existing registrations preserve the original ID, per the service contract.
                    PersonajeId = jugador.personaje_id;
                    apariencias[jugador.id] = new Apariencia { estilo = v.Estilo,
                        piel = v.avatar.piel, ojos = v.avatar.ojos, cabello = v.avatar.cabello, traje = v.avatar.traje };
                    Navegar("PruebaIntegracionV2");
                }
                catch (ErrorJuego e) { v.mensaje.text = e.Message; }
            });
        }

        void Resultado()
        {
            var v = FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
            foreach (var texto in SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TMP_Text>(true)))
            {
                if (texto.text.Contains("RESULTADOS DEMO")) texto.text = "LIFE DRIVE / RESULTADO PILOTO V2";
                else if (texto.text.Contains("PUNTOS SIMULADOS")) texto.text = "SEGURIDAD · PUNTOS GUARDADOS";
                else if (texto.text.Contains("NO ES UN REGISTRO REAL")) texto.text = "DECISIONES Y CONSECUENCIAS GUARDADAS";
                else if (texto.text.Contains("Datos escritos para esta maqueta")) texto.text = "Historial consultado de la partida del piloto. Las consecuencias y puntos fueron guardados por el motor.";
            }
            v.volver.onClick.RemoveAllListeners();
            v.volver.onClick.AddListener(() => { Juego.Usuarios.CerrarSesion(); Navegar("Inicio"); });
            try
            {
                if (!Juego.RecorridosV2.Consultar(PartidaId).CierrePresentado) throw new ErrorJuego("La entrega todavía no se ha presentado.");
                var r = Juego.Desempeno.Resumen(PartidaId);
                v.titulo.text = "Resumen de la operación";
                v.acento.color = new Color32(223, 193, 105, 255);
                v.jugador.text = r.Jugador.nombre;
                v.personaje.text = "Ficha " + r.Jugador.personaje_id + " · apariencia de sesión";
                v.puntos.text = r.Partida.puntos + " pts de seguridad";
                v.resumen.text = r.Historial.Count + " decisiones guardadas · " + r.Partida.retraso_min + " min de retraso";
                v.educativo.text = "Recorrido: " + string.Join(" → ", Juego.RecorridosV2.Consultar(PartidaId).Historial);
                v.entrega.text = "ENTREGA COMPLETADA · DELIVERY";
                v.progreso.text = "RECORRIDO COMPLETADO · 100%";
                v.conteoInsignias.text = "RECONOCIMIENTOS";
                foreach (var texto in new[] { v.puntos, v.personaje, v.jugador, v.conteoInsignias })
                {
                    texto.fontSizeMax = texto.fontSize;
                    texto.fontSizeMin = 18;
                    texto.enableAutoSizing = true;
                }
                v.aviso.text = "PILOTO V2 · Datos guardados del motor. Contenido candidato. Sin clasificación ni KPI calculados.";
                for (int i = 0; i < v.estadosInsignias.Length; i++)
                {
                    v.estadosInsignias[i].text = "PENDIENTE DE REGLAS APROBADAS";
                    v.nombresInsignias[i].text = v.definiciones.reconocimientos[i].nombre;
                    v.iconosInsignias[i].gameObject.SetActive(false);
                }
                v.retrato.enabled = false;
                if (apariencias.TryGetValue(r.Jugador.id, out var a))
                {
                    var retrato = new GameObject("Apariencia piloto", typeof(RectTransform));
                    retrato.transform.SetParent(v.retrato.transform, false);
                    var rect = (RectTransform)retrato.transform;
                    rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                    var avatar = retrato.AddComponent<AvatarPersonalizableUI>();
                    avatar.mujer = a.estilo == 1;
                    // Copy the approved avatar colors retained at registration, without touching its database model.
                    avatar.piel = a.piel; avatar.ojos = a.ojos;
                    avatar.cabello = a.cabello; avatar.traje = a.traje;
                }
                int pagina = 0;
                Action pintar = () =>
                {
                    for (int i = 0; i < v.filasDecisiones.Length; i++)
                    {
                        int indice = pagina * v.filasDecisiones.Length + i;
                        v.filasDecisiones[i].text = indice >= r.Historial.Count ? "" :
                            (indice + 1) + ". " + r.Historial[indice].Decision.alternativa + "\n" +
                            r.Historial[indice].Decision.puntos_aplicados + " pts · " + r.Historial[indice].Decision.consecuencia;
                    }
                    v.selectores[0].interactable = pagina > 0;
                    v.selectores[1].interactable = (pagina + 1) * v.filasDecisiones.Length < r.Historial.Count;
                };
                for (int i = 0; i < v.selectores.Length; i++)
                {
                    v.selectores[i].onClick.RemoveAllListeners();
                    v.selectores[i].gameObject.SetActive(i < 2);
                    if (i < 2) v.selectores[i].GetComponentInChildren<TMP_Text>().text = i == 0 ? "Decisiones anteriores" : "Decisiones siguientes";
                }
                v.selectores[0].onClick.AddListener(() => { pagina--; pintar(); });
                v.selectores[1].onClick.AddListener(() => { pagina++; pintar(); });
                v.panelDecisiones.SetActive(false);
                v.revisar.onClick.RemoveAllListeners();
                v.revisar.GetComponentInChildren<TMP_Text>().text = "Ver / ocultar decisiones";
                v.revisar.onClick.AddListener(() => v.panelDecisiones.SetActive(!v.panelDecisiones.activeSelf));
                pintar();
            }
            catch (ErrorJuego e) { v.aviso.text = e.Message; v.revisar.interactable = false; }
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= AlCargar;
            foreach (var f in fuentes.Values)
            {
                foreach (var atlas in f.atlasTextures) Destroy(atlas);
                Destroy(f.material); Destroy(f);
            }
        }
    }
}
