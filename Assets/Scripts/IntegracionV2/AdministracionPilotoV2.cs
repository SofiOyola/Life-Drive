using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.IntegracionV2
{
    // Binds the approved scene's references, never the serialized DEMO snapshots or production Sesion.
    public sealed class AdministracionPilotoV2 : MonoBehaviour
    {
        FlujoJugadorPilotoV2 piloto;
        VistaAdministrativaDemo vista;
        SeleccionAdministrativaVisual seleccion;
        List<FilaJugador> jugadores = new List<FilaJugador>();
        List<Partida> partidas = new List<Partida>();
        readonly Button[] filasPartidas = new Button[2];
        Button anterior, siguiente, partidaAnterior, partidaSiguiente, abrirPdf;
        ScrollRect desplazamiento;
        int pagina, paginaPartidas;
        public int JugadorSeleccionado { get; private set; }
        public int PartidaSeleccionada { get; private set; }
        public Resumen ResumenVisible { get; private set; }
        public List<Evento> EventosVisibles { get; private set; } = new List<Evento>();
        public string UltimoPdf { get; private set; }
        ServicioJuego Juego => piloto.Juego;

        public static void Preparar(FlujoJugadorPilotoV2 piloto, Scene escena)
        {
            var v = escena.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<VistaAdministrativaDemo>(true)).FirstOrDefault();
            if (v == null) return;
            v.enabled = false; // Prevent the no-authentication DEMO Start, before its first frame.
            foreach (var r in escena.GetRootGameObjects())
            {
                foreach (var original in r.GetComponentsInChildren<LifeDrive.UI.PantallaAdmin>(true)) original.enabled = false;
                foreach (var original in r.GetComponentsInChildren<LifeDrive.UI.PantallaAdminLogin>(true)) original.enabled = false;
            }
            var visual = FindAnyObjectByType<SeleccionAdministrativaVisual>();
            if (visual != null) visual.enabled = false;
            var adaptador = v.gameObject.AddComponent<AdministracionPilotoV2>();
            adaptador.piloto = piloto; adaptador.vista = v; adaptador.seleccion = visual;
            if (!v.login && JuegoSinAdmin(piloto))
                foreach (var raiz in escena.GetRootGameObjects())
                    foreach (var canvas in raiz.GetComponentsInChildren<Canvas>(true)) canvas.enabled = false;
        }

        static bool JuegoSinAdmin(FlujoJugadorPilotoV2 piloto) => piloto.Juego.Usuarios.Actual.Rol != Rol.Administrador;

        void Start()
        {
            foreach (var texto in SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TMP_Text>(true)))
            {
                if (texto.name == "Aviso demo") texto.text = piloto.EsPilotoV2 ? "PILOTO V2 · Entorno temporal compartido · Solo datos de prueba" : "LIFE DRIVE · Administración del contenido vigente V1";
                if (texto.name == "Usuario etiqueta") texto.text = piloto.EsPilotoV2 ? "Usuario administrativo del piloto" : "Usuario administrativo";
                if (texto.name == "Contraseña etiqueta") texto.text = "Contraseña";
                if (texto.name == "Nota acceso") texto.text = "Utiliza una cuenta administrativa existente.\nNo se permite el autorregistro administrativo.";
            }
            vista.volver.onClick.RemoveAllListeners();
            vista.volver.onClick.AddListener(() => { Juego.Usuarios.CerrarSesion(); piloto.Navegar("Inicio"); });
            if (vista.login) { Login(); return; }
            if (!Autorizado()) return;
            Panel();
        }

        void Update()
        {
            if (!vista.login && JuegoSinAdmin(piloto))
            {
                foreach (var canvas in FindObjectsByType<Canvas>()) canvas.enabled = false;
                ResumenVisible = null; EventosVisibles.Clear(); UltimoPdf = null;
                piloto.Navegar("AdminLogin");
            }
        }

        bool Autorizado()
        {
            try { Juego.Usuarios.Exigir(Accion.ListarJugadores); return true; }
            catch (ErrorJuego) { piloto.Navegar("AdminLogin"); return false; }
        }

        void Ejecutar(Action accion)
        {
            if (!Autorizado()) return;
            try { accion(); }
            catch (Exception e) when (e is ErrorJuego || e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            { vista.mensaje.text = "No se pudo completar la consulta: " + e.Message; }
        }

        void Login()
        {
            vista.usuario.text = vista.contrasena.text = "";
            vista.usuario.placeholder.GetComponent<TMP_Text>().text = piloto.EsPilotoV2 ? "Usuario del piloto" : "Usuario";
            vista.contrasena.placeholder.GetComponent<TMP_Text>().text = "Contraseña";
            vista.contrasena.contentType = TMP_InputField.ContentType.Password;
            vista.ingresar.onClick.RemoveAllListeners();
            vista.ingresar.GetComponentInChildren<TMP_Text>().text = "Ingresar";
            vista.mensaje.text = piloto.EsPilotoV2 ? "Acceso mediante el servicio original. Cuentas aisladas configuradas desde el Editor." : "Acceso mediante la autenticación administrativa existente.";
            vista.ingresar.onClick.AddListener(() =>
            {
                try
                {
                    Juego.Usuarios.CerrarSesion(); // A failed attempt must never retain a preceding privileged session.
                    Juego.Usuarios.IniciarSesionAdministrador(vista.usuario.text, vista.contrasena.text);
                    vista.contrasena.text = "";
                    piloto.Navegar("Admin");
                }
                catch (Exception e) when (e is ErrorJuego || e is IOException || e is FormatException || e is ArgumentException)
                { vista.contrasena.text = ""; vista.mensaje.text = e.Message; }
            });
        }

        void Panel()
        {
            vista.volver.GetComponentInChildren<TMP_Text>().text = "Cerrar sesión";
            vista.ver.GetComponentInChildren<TMP_Text>().text = "Ver desempeño";
            vista.busqueda.placeholder.GetComponent<TMP_Text>().text = "Buscar por nombre o cédula";
            foreach (var b in vista.jugadores) { b.onClick.RemoveAllListeners(); b.gameObject.SetActive(false); }
            vista.buscar.onClick.RemoveAllListeners();
            vista.buscar.onClick.AddListener(() => Ejecutar(() => { pagina = 0; Buscar(); }));
            var padre = vista.busqueda.transform.parent;
            anterior = Boton(vista.buscar, padre, "Jugadores anteriores", "Anteriores", 80, 620, 240, 50,
                () => Ejecutar(() => { pagina--; PintarJugadores(); }));
            siguiente = Boton(vista.buscar, padre, "Jugadores siguientes", "Siguientes", 580, 620, 250, 50,
                () => Ejecutar(() => { pagina++; PintarJugadores(); }));
            for (int i = 0; i < filasPartidas.Length; i++)
            {
                int fila = i;
                filasPartidas[i] = Boton(vista.buscar, padre, "Partida piloto " + i, "", 80, 715 + 65 * i, 750, 58,
                    () => Ejecutar(() => ElegirPartida(partidas[paginaPartidas * 2 + fila])));
            }
            var etiqueta = Instantiate(vista.mensaje, padre);
            etiqueta.name = "Partidas del jugador"; etiqueta.text = "PARTIDAS DEL JUGADOR"; etiqueta.fontSize = 24;
            Posicion(etiqueta.rectTransform, 80, 677, 750, 35);
            partidaAnterior = Boton(vista.buscar, padre, "Partidas anteriores", "Anteriores", 80, 850, 240, 50,
                () => Ejecutar(() => { paginaPartidas--; PintarPartidas(); }));
            partidaSiguiente = Boton(vista.buscar, padre, "Partidas siguientes", "Siguientes", 580, 850, 250, 50,
                () => Ejecutar(() => { paginaPartidas++; PintarPartidas(); }));
            vista.ver.onClick.RemoveAllListeners(); vista.ver.onClick.AddListener(() => Ejecutar(MostrarResumen));
            vista.pdf.onClick.RemoveAllListeners(); vista.pdf.onClick.AddListener(() => Ejecutar(GenerarPdf));
            abrirPdf = Boton(vista.pdf, padre, "Abrir PDF piloto", "Abrir PDF", 1490, 850, 330, 58,
                () => Ejecutar(() =>
                {
                    if (UltimoPdf == null || !File.Exists(UltimoPdf)) throw new ErrorJuego("Genera primero el PDF de esta partida.");
                    Application.OpenURL(new Uri(UltimoPdf).AbsoluteUri);
                }));
            PrepararDesplazamiento();
            Ejecutar(Buscar);
        }

        void Buscar()
        {
            jugadores = Juego.Desempeno.ListarJugadores(vista.busqueda.text);
            JugadorSeleccionado = 0; PartidaSeleccionada = 0; partidas.Clear(); paginaPartidas = 0;
            LimpiarResumen(); PintarJugadores(); PintarPartidas();
            Composicion("Seleccionado: ningún jugador");
            vista.mensaje.text = jugadores.Count == 0 ? "No hay jugadores con ese nombre o cédula." : "Elige un jugador y una partida del piloto.";
        }

        void PintarJugadores()
        {
            for (int i = 0; i < vista.jugadores.Length; i++)
            {
                int indice = pagina * vista.jugadores.Length + i;
                var b = vista.jugadores[i]; b.gameObject.SetActive(indice < jugadores.Count);
                b.onClick.RemoveAllListeners(); Posicion((RectTransform)b.transform, 80, 375 + i * 80, 750, 70);
                if (indice >= jugadores.Count) continue;
                var jugador = jugadores[indice].Jugador;
                Etiqueta(b, jugador.nombre + " · " + jugador.cedula);
                b.GetComponent<RespuestaBotonUI>().marcado = jugador.id == JugadorSeleccionado;
                b.onClick.AddListener(() => Ejecutar(() => ElegirJugador(jugador)));
            }
            anterior.interactable = pagina > 0;
            siguiente.interactable = (pagina + 1) * vista.jugadores.Length < jugadores.Count;
        }

        void ElegirJugador(Jugador jugador)
        {
            JugadorSeleccionado = jugador.id; PartidaSeleccionada = 0; paginaPartidas = 0;
            partidas = Juego.Desempeno.PartidasDe(jugador.id);
            LimpiarResumen(); PintarJugadores(); PintarPartidas();
            Composicion("Seleccionado: " + jugador.nombre);
            vista.mensaje.text = partidas.Count == 0 ? "Este jugador no tiene partidas." : "Selecciona una partida específica y pulsa Ver desempeño.";
        }

        void PintarPartidas()
        {
            for (int i = 0; i < filasPartidas.Length; i++)
            {
                int indice = paginaPartidas * 2 + i;
                var b = filasPartidas[i]; b.gameObject.SetActive(indice < partidas.Count);
                if (indice >= partidas.Count) continue;
                var p = partidas[indice];
                Etiqueta(b, "Partida #" + p.id + " · " + p.estado + " · " + p.puntos + " pts");
                b.GetComponent<RespuestaBotonUI>().marcado = p.id == PartidaSeleccionada;
            }
            partidaAnterior.interactable = paginaPartidas > 0;
            partidaSiguiente.interactable = (paginaPartidas + 1) * 2 < partidas.Count;
            vista.ver.interactable = PartidaSeleccionada != 0;
        }

        void ElegirPartida(Partida partida)
        {
            PartidaSeleccionada = partida.id; LimpiarResumen(); PintarPartidas();
            Composicion("Seleccionado: " + jugadores.Single(j => j.Jugador.id == JugadorSeleccionado).Jugador.nombre);
            vista.mensaje.text = "Partida #" + partida.id + " seleccionada. Pulsa Ver desempeño para consultar.";
        }

        void LimpiarResumen()
        {
            ResumenVisible = null; EventosVisibles.Clear(); UltimoPdf = null;
            vista.pdf.interactable = false; abrirPdf.gameObject.SetActive(false);
            vista.detalle.text = "";
        }

        void Composicion(string texto)
        {
            seleccion.composicion.SetActive(true); seleccion.encabezado.text = texto;
            desplazamiento.gameObject.SetActive(false);
        }

        void MostrarResumen()
        {
            if (PartidaSeleccionada == 0) throw new ErrorJuego("Selecciona una partida.");
            var r = Juego.Desempeno.Resumen(PartidaSeleccionada);
            if (r.Jugador.id != JugadorSeleccionado) throw new ErrorJuego("La partida no corresponde al jugador seleccionado.");
            var eventos = Juego.Desempeno.Eventos(partidaId: PartidaSeleccionada);
            ResumenVisible = r; EventosVisibles = eventos;
            seleccion.composicion.SetActive(false); desplazamiento.gameObject.SetActive(true); vista.detalle.alpha = 1;
            vista.detalle.text = r.Jugador.nombre + " · Partida #" + r.Partida.id + " · " + r.Partida.estado +
                "\nPuntos de seguridad: " + r.Partida.puntos + " · Retraso: " + r.Partida.retraso_min + " min" +
                "\nRecorrido: " + string.Join(" → ", r.RutaRecorrida) +
                "\nKPI TDS, IEE, PPS y PCRS: pendientes del servicio de indicadores." +
                "\n\nDECISIONES Y CONSECUENCIAS\n" +
                (r.Historial.Count == 0 ? "Sin decisiones guardadas." : string.Join("\n\n", r.Historial.Select(h =>
                    h.Decision.tipo + " · " + h.Decision.alternativa + "\n" + h.Decision.consecuencia +
                    "\n" + h.Decision.puntos_aplicados + " pts · " + h.Decision.retraso_aplicado + " min"))) +
                "\n\nEVENTOS DE ESTA PARTIDA\n" + (eventos.Count == 0 ? "Sin eventos." : string.Join("\n", eventos.Select(e =>
                    e.fecha_hora + " · " + e.tipo + " · " + e.detalle)));
            vista.pdf.interactable = r.Partida.estado == EstadoPartida.Finalizada;
            Canvas.ForceUpdateCanvases(); desplazamiento.verticalNormalizedPosition = 1;
            vista.mensaje.text = "Datos guardados del motor. Desplaza el detalle para leer decisiones y eventos completos.";
        }

        void GenerarPdf()
        {
            if (ResumenVisible == null || ResumenVisible.Partida.id != PartidaSeleccionada) throw new ErrorJuego("Consulta primero la partida seleccionada.");
            UltimoPdf = null; abrirPdf.gameObject.SetActive(false);
            var bytes = Juego.Desempeno.ReportePdf(PartidaSeleccionada);
            var carpeta = Path.Combine(piloto.CarpetaDatos, "Reportes"); Directory.CreateDirectory(carpeta);
            var ruta = Path.Combine(carpeta, "lifedrive-partida-" + PartidaSeleccionada + ".pdf");
            File.WriteAllBytes(ruta, bytes);
            UltimoPdf = ruta; abrirPdf.gameObject.SetActive(true);
            vista.mensaje.text = "PDF generado y guardado para la partida #" + PartidaSeleccionada + ". Pulsa Abrir PDF para verlo o guardar una copia desde tu visor.";
        }

        void PrepararDesplazamiento()
        {
            // The approved wheel composition is a child of the old detail text. Keep its exact
            // coordinate frame outside the scrolling detail, so hiding the detail never hides the wheel.
            var soporte = (RectTransform)new GameObject("Soporte selección aprobada", typeof(RectTransform)).transform;
            soporte.SetParent(vista.detalle.transform.parent, false);
            var original = vista.detalle.rectTransform;
            soporte.anchorMin = original.anchorMin; soporte.anchorMax = original.anchorMax;
            soporte.pivot = original.pivot; soporte.sizeDelta = original.sizeDelta;
            soporte.anchoredPosition = original.anchoredPosition;
            seleccion.composicion.transform.SetParent(soporte, false);
            var rect = (RectTransform)new GameObject("Detalle desplazable", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect)).transform;
            rect.SetParent(vista.detalle.transform.parent, false); Posicion(rect, 920, 270, 900, 470);
            rect.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            desplazamiento = rect.GetComponent<ScrollRect>(); desplazamiento.horizontal = false;
            desplazamiento.movementType = ScrollRect.MovementType.Clamped; desplazamiento.scrollSensitivity = 42;
            desplazamiento.viewport = rect; desplazamiento.content = vista.detalle.rectTransform;
            vista.detalle.transform.SetParent(rect, false);
            vista.detalle.rectTransform.anchorMin = new Vector2(0, 1); vista.detalle.rectTransform.anchorMax = Vector2.one;
            vista.detalle.rectTransform.pivot = new Vector2(.5f, 1); vista.detalle.rectTransform.anchoredPosition = Vector2.zero;
            vista.detalle.rectTransform.sizeDelta = Vector2.zero;
            vista.detalle.fontSize = 26; vista.detalle.alignment = TextAlignmentOptions.TopLeft;
            vista.detalle.overflowMode = TextOverflowModes.Overflow; vista.detalle.raycastTarget = true;
            vista.detalle.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        static void Posicion(RectTransform rect, float x, float y, float ancho, float alto)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(x + ancho / 2, -y - alto / 2); rect.sizeDelta = new Vector2(ancho, alto);
        }
        static void Etiqueta(Button b, string texto)
        {
            var t = b.GetComponentInChildren<TMP_Text>(); t.text = texto; t.fontSize = 26;
            t.enableAutoSizing = true; t.fontSizeMin = 18; t.fontSizeMax = 26;
            Posicion(t.rectTransform, 10, 4, ((RectTransform)b.transform).rect.width - 20, ((RectTransform)b.transform).rect.height - 8);
        }
        static Button Boton(Button plantilla, Transform padre, string nombre, string texto, float x, float y, float ancho, float alto, Action accion)
        {
            var b = Instantiate(plantilla, padre); b.name = nombre; b.gameObject.SetActive(true);
            b.onClick.RemoveAllListeners(); b.onClick.AddListener(() => accion());
            Posicion((RectTransform)b.transform, x, y, ancho, alto); Etiqueta(b, texto); return b;
        }
    }
}
