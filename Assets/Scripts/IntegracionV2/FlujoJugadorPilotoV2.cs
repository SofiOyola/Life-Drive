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
    public sealed partial class FlujoJugadorPilotoV2 : MonoBehaviour
    {
        public ServicioJuego Juego { get; private set; }
        public int PartidaId { get; private set; }
        public int PersonajeId { get; private set; }
        public string CarpetaDatos { get; private set; }
        Action<string> cargar;
        bool navegando;
        public PersistenciaAparienciaPilotoV2 Apariencias { get; private set; }
        public AparienciaJugadorPilotoV1 AparienciaPartidaActual { get; private set; }
        readonly Dictionary<TMP_FontAsset, TMP_FontAsset> fuentes = new Dictionary<TMP_FontAsset, TMP_FontAsset>();
        readonly LifeDrive.Mapas.Demo.IconosReconocimientoDemo iconosResultado = new LifeDrive.Mapas.Demo.IconosReconocimientoDemo();

        public static FlujoJugadorPilotoV2 Activar(CatalogoMapas catalogo, string sesion, Action<string> navegar)
        {
            if (FindAnyObjectByType<FlujoJugadorPilotoV2>() != null) throw new InvalidOperationException("El piloto ya está activo.");
            var contenidoPiloto = PrepararContenidoPiloto(sesion);
            var carpeta = Path.GetDirectoryName(contenidoPiloto);
            var juego = new ServicioJuego(contenidoPiloto, carpeta, catalogo);
            var piloto = new GameObject("Flujo jugador PILOTO V2").AddComponent<FlujoJugadorPilotoV2>();
            DontDestroyOnLoad(piloto.gameObject);
            piloto.Juego = juego;
            piloto.Apariencias = new PersistenciaAparienciaPilotoV2(juego);
            piloto.CarpetaDatos = carpeta;
            piloto.cargar = navegar ?? throw new ArgumentNullException(nameof(navegar));
            SceneManager.sceneLoaded += piloto.AlCargar;
            piloto.Navegar("Inicio");
            return piloto;
        }

        // Same engine and database. Only the content/account source is snapshotted into the isolated environment.
        // Never copy production administrator accounts. Provision pilot hashes through the existing editor mechanism.
        public static string PrepararContenidoPiloto(string sesion)
        {
            if (string.IsNullOrEmpty(sesion) || sesion.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
                throw new ArgumentException("Identificador de sesión inválido.");
            var contenido = Path.Combine(Application.temporaryCachePath, "LifeDrive-FlujoJugadorV2", sesion, "Contenido");
            Directory.CreateDirectory(contenido);
            foreach (var nombre in new[] { "ruta.json", "ruta-v2-candidata.json" })
            {
                var destino = Path.Combine(contenido, nombre);
                if (!File.Exists(destino)) File.Copy(Path.Combine(Application.streamingAssetsPath, "Contenido", nombre), destino);
            }
            return contenido;
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
                foreach (var texto in raiz.GetComponentsInChildren<TMP_Text>(true))
                {
                    texto.font = Fuente(texto.font);
                    texto.richText = false; // Player-entered names and audit details are literal data, never UI markup.
                }
                foreach (var campo in raiz.GetComponentsInChildren<TMP_InputField>(true)) campo.fontAsset = Fuente(campo.fontAsset);
            }
            var partida = FindAnyObjectByType<PruebaIntegracionV2>();
            if (partida != null) partida.ConectarJugador(Juego, id => { PartidaId = id; Navegar("Resultado"); },
                id => AparienciaPartidaActual = Apariencias.FijarPartida(id));
            AdministracionPilotoV2.Preparar(this, escena);
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
                    vista.administrador.onClick.RemoveAllListeners();
                    vista.administrador.onClick.AddListener(() => { Juego.Usuarios.CerrarSesion(); Navegar("AdminLogin"); });
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
            v.mensaje.text = "PILOTO · Personaliza o recupera tu apariencia guardada. Solo datos ficticios.";
            var privacidad = v.nombre.transform.parent.GetComponentsInChildren<TMP_Text>().First(t => t.name == "Privacidad");
            privacidad.text = "Solo datos ficticios.\nApariencia guardada en el mismo piloto.\nLa ficha original se conserva.";
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
            var conservarApariencia = PrepararPersistenciaRegistro(v);
            v.comprobar.onClick.AddListener(() =>
            {
                if (navegando) return;
                try
                {
                    var jugador = Juego.Usuarios.RegistrarJugador(v.nombre.text, v.documento.text, PersonajeId);
                    // Existing registrations preserve the original ID, per the service contract.
                    PersonajeId = jugador.personaje_id;
                    conservarApariencia(jugador);
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
                var gamificacion = GamificacionPartidaV2.Consultar(Juego, PartidaId);
                v.titulo.text = "Resumen de la operación";
                v.acento.color = new Color32(223, 193, 105, 255);
                v.jugador.text = r.Jugador.nombre;
                // Keep the approved portrait/header layout while allowing the full 60-character registration name.
                v.jugador.rectTransform.sizeDelta = new Vector2(v.jugador.rectTransform.sizeDelta.x, 96);
                v.personaje.rectTransform.anchoredPosition += new Vector2(0, -20);
                var aparienciaGuardada = Apariencias.LeerPartida(PartidaId);
                v.personaje.text = "Ficha " + r.Jugador.personaje_id + (aparienciaGuardada == null ? " · apariencia base sin guardar" : " · apariencia guardada");
                v.puntos.text = r.Partida.puntos + " pts de seguridad";
                v.resumen.text = r.Historial.Count + " decisiones guardadas · " + r.Partida.retraso_min + " min de retraso";
                v.resumen.text += "\nBuenas prácticas registradas: " + gamificacion.BuenasPracticas.Length;
                v.resumen.fontSizeMax = v.resumen.fontSize; v.resumen.fontSizeMin = 18; v.resumen.enableAutoSizing = true;
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
                    v.iconosInsignias[i].gameObject.SetActive(true);
                    v.iconosInsignias[i].texture = iconosResultado.Obtener(v.definiciones.reconocimientos[i].icono);
                    v.iconosInsignias[i].color = new Color32(125, 139, 111, 255);
                }
                v.retrato.enabled = false;
                {
                    var a = aparienciaGuardada ?? VariantesAparienciaPilotoV1.Crear(r.Jugador.personaje_id, 0, 0, 0, 0, 0);
                    var retrato = new GameObject("Apariencia piloto", typeof(RectTransform));
                    retrato.transform.SetParent(v.retrato.transform, false);
                    var rect = (RectTransform)retrato.transform;
                    rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                    var avatar = retrato.AddComponent<AvatarPersonalizableUI>();
                    VariantesAparienciaPilotoV1.Aplicar(avatar, a);
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
            iconosResultado.Liberar();
            SceneManager.sceneLoaded -= AlCargar;
            foreach (var f in fuentes.Values)
            {
                foreach (var atlas in f.atlasTextures) Destroy(atlas);
                Destroy(f.material); Destroy(f);
            }
        }
    }
}
