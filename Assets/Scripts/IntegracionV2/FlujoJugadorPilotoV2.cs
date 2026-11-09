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
    // Shared screen adapter. EntradaJuego selects the original V1 service, or the explicit isolated V2 pilot.
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static FlujoJugadorPilotoV2 Activar(CatalogoMapas catalogo, string sesion, Action<string> navegar,
            bool abrirInicio = true, string carpetaEnsayo = null)
        {
            if (catalogo == null) throw new ArgumentNullException(nameof(catalogo));
            if (FindAnyObjectByType<FlujoJugadorPilotoV2>() != null) throw new InvalidOperationException("El piloto ya está activo.");
            var contenidoPiloto = string.IsNullOrEmpty(carpetaEnsayo)
                ? PrepararContenidoPiloto(sesion)
                : PrepararContenidoAislado(Path.GetFullPath(carpetaEnsayo));
            var carpeta = Path.GetDirectoryName(contenidoPiloto);
            var juego = new ServicioJuego(contenidoPiloto, carpeta, catalogo);
            var piloto = new GameObject("Flujo jugador PILOTO V2").AddComponent<FlujoJugadorPilotoV2>();
            DontDestroyOnLoad(piloto.gameObject);
            piloto.Juego = juego;
            piloto.Apariencias = new PersistenciaAparienciaPilotoV2(juego);
            piloto.CarpetaDatos = carpeta;
            piloto.cargar = navegar ?? throw new ArgumentNullException(nameof(navegar));
            piloto.gameObject.AddComponent<TecladoPilotoV2>();
            SceneManager.sceneLoaded += piloto.AlCargar;
            if (abrirInicio) piloto.Navegar("Inicio");
            return piloto;
        }

        // Same engine and database. Only the content/account source is snapshotted into the isolated environment.
        // Never copy production administrator accounts. Provision pilot hashes through the existing editor mechanism.
        public static string PrepararContenidoPiloto(string sesion)
        {
            if (string.IsNullOrEmpty(sesion) || sesion.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
                throw new ArgumentException("Identificador de sesión inválido.");
            return PrepararContenidoAislado(Path.Combine(Application.temporaryCachePath, "LifeDrive-FlujoJugadorV2", sesion));
        }

        static string PrepararContenidoAislado(string carpeta)
        {
            var contenido = Path.Combine(carpeta, "Contenido");
            Directory.CreateDirectory(contenido);
            foreach (var nombre in new[] { "ruta.json", "ruta-v2-candidata.json" })
            {
                var destino = Path.Combine(contenido, nombre);
                if (!File.Exists(destino)) File.Copy(Path.Combine(Application.streamingAssetsPath, "Contenido", nombre), destino);
            }
            return contenido;
        }
#endif

        public void Navegar(string escena)
        {
            if (navegando) return;
            navegando = true;
            cargar(escena);
        }

        void AlCargar(Scene escena, LoadSceneMode modo)
        {
            if (escenaEnlazada == escena) return;
            escenaEnlazada = escena;
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
                id => { PartidaId = id; Sesion.PartidaId = id; AparienciaPartidaActual = Apariencias.FijarPartida(id); },
                VolverAlInicioDesdePartida, !EsPilotoV2, () => AparienciaPartidaActual);
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
                    vista.mensaje.text = "Inicia o retoma tu recorrido.";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    vista.mensaje.text = EsPilotoV2 ? "Piloto de carreteras activo." : "Recorrido clásico activo.";
#endif
                    if (regresoDePartida) vista.mensaje.text = "Operación conservada. Pulsa Jugar e ingresa los mismos datos para retomarla.";
                    foreach (var t in escena.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TMP_Text>(true)))
                        if (t.name == "Previsualizacion") t.gameObject.SetActive(false);
                }
                else Registro(vista);
            }
            if (escena.name == "Resultado") Resultado();
        }

        void Registro(PersonalizacionRegistroUI v)
        {
            var ids = Juego.Contenido.personajes.Select(p => p.id).OrderBy(id => id).ToArray();
            // An initial valid ID lets the player start with the displayed avatar as-is.
            // Silhouette and colors are independent from the content ID.
            PersonajeId = ids.FirstOrDefault();
            v.volver.onClick.RemoveAllListeners();
            v.volver.onClick.AddListener(() => Navegar("Inicio"));
            v.comprobar.onClick.RemoveAllListeners();
            v.comprobar.GetComponentInChildren<TMP_Text>().text = "COMENZAR RECORRIDO";
            v.mensaje.text = "Completa tus datos y personaliza tu personaje.";
            var privacidad = v.nombre.transform.parent.GetComponentsInChildren<TMP_Text>().First(t => t.name == "Privacidad");
            privacidad.text = "Apariencia guardada con tu partida.\nLa ficha original se conserva.";
            privacidad.fontSize = 20;
            privacidad.rectTransform.sizeDelta = new Vector2(474, 100);
            for (int i = 0; i < ids.Length; i++)
            {
                int id = ids[i];
                var b = Instantiate(v.volver, privacidad.transform.parent);
                b.name = "Ficha piloto " + id;
                b.GetComponent<RespuestaBotonUI>().marcado = id == PersonajeId;
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
                    ActualizarMensajeRegistro(v, "Ficha " + id + " seleccionada. Puedes comenzar el recorrido.");
                });
            }
            var conservarApariencia = PrepararPersistenciaRegistro(v);
            v.documento.onValueChanged.AddListener(_ => ActualizarMensajeRegistro(v, "Completa tus datos y personaliza tu personaje."));
            v.comprobar.onClick.AddListener(() =>
            {
                if (navegando) return;
                try
                {
                    var jugador = Juego.Usuarios.RegistrarJugador(v.nombre.text, v.documento.text, PersonajeId);
                    // Existing registrations preserve the original ID, per the service contract.
                    PersonajeId = jugador.personaje_id;
                    ComprobarVersionActiva(jugador.id);
                    conservarApariencia(jugador);
                    Navegar("PruebaIntegracionV2");
                }
                catch (Exception e) when (e is ErrorJuego || e is IOException || e is UnauthorizedAccessException)
                { v.mensaje.text = e.Message; }
            });
        }

        void Resultado()
        {
            var v = FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
            foreach (var texto in SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TMP_Text>(true)))
            {
                if (texto.name == "Modo") texto.text = "LIFE DRIVE / RESULTADO";
                else if (texto.name == "Etiqueta seguridad") texto.text = "PUNTOS DE SEGURIDAD";
            }
            v.volver.onClick.RemoveAllListeners();
            v.volver.onClick.AddListener(() => { Juego.Usuarios.CerrarSesion(); Navegar("Inicio"); });
            try
            {
                if (!CierreDisponible()) throw new ErrorJuego("La entrega todavía no se ha presentado.");
                var r = Juego.Desempeno.Resumen(PartidaId);
                var gamificacion = EsPilotoV2 ? GamificacionPartidaV2.Consultar(Juego, PartidaId) : null;
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
                v.resumen.text += "\nBuenas prácticas registradas: " + (gamificacion?.BuenasPracticas.Length ?? r.BuenasPracticas);
                v.resumen.fontSizeMax = v.resumen.fontSize; v.resumen.fontSizeMin = 18; v.resumen.enableAutoSizing = true;
                v.educativo.text = "Recorrido: " + (EsPilotoV2 ? string.Join(" → ", Juego.RecorridosV2.Consultar(PartidaId).Historial) : string.Join(" → ", r.RutaRecorrida));
                v.entrega.text = "ENTREGA COMPLETADA";
                v.progreso.text = "RECORRIDO COMPLETADO · 100%";
                v.conteoInsignias.text = "RECONOCIMIENTOS";
                foreach (var texto in new[] { v.puntos, v.personaje, v.jugador, v.conteoInsignias })
                {
                    texto.fontSizeMax = texto.fontSize;
                    texto.fontSizeMin = 18;
                    texto.enableAutoSizing = true;
                }
                v.aviso.text = "Balance de tu recorrido";
                var tonosInsignias = new[] { new Color32(241, 200, 108, 255), new Color32(176, 213, 127, 255), new Color32(245, 170, 133, 255) };
                for (int i = 0; i < v.estadosInsignias.Length; i++)
                {
                    v.estadosInsignias[i].text = "Aún no disponible";
                    v.nombresInsignias[i].text = v.definiciones.reconocimientos[i].nombre;
                    v.iconosInsignias[i].gameObject.SetActive(true);
                    v.iconosInsignias[i].texture = iconosResultado.Obtener(v.definiciones.reconocimientos[i].icono);
                    DarColorInsignia(v.iconosInsignias[i], tonosInsignias[i % tonosInsignias.Length]);
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
                PrepararMetricas(v, r);
                PrepararNuevaOperacion(v);
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
