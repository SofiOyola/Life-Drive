#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using LifeDrive.Datos;
using LifeDrive.IntegracionV2;
using LifeDrive.Mapas;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LifeDrive.Tests
{
    public partial class AdministracionPilotoV2Tests
    {
        static readonly int[] Resoluciones = { 1920, 1280 };
        FlujoJugadorPilotoV2 piloto;
        string sesion, carpeta, usuario, clave;
        EditorWindow ventana;
        PropertyInfo indice;
        int indiceAnterior, ancho;
        Jugador alfa, beta, sinPartidas;
        Partida primera, segunda, otra, incompleta;

        [SetUp]
        public void Preparar()
        {
            ancho = 0; ventana = null; indice = null;
            sesion = Guid.NewGuid().ToString("N"); usuario = Guid.NewGuid().ToString("N"); clave = Guid.NewGuid().ToString("N");
            var contenido = FlujoJugadorPilotoV2.PrepararContenidoPiloto(sesion);
            carpeta = Path.GetDirectoryName(contenido);
            Assert.That(File.Exists(Path.Combine(contenido, "administradores.json")), Is.False, "No copiar cuentas reales.");
            File.WriteAllText(Path.Combine(contenido, "administradores.json"), JsonUtility.ToJson(new ListaAdministradores
                { administradores = new[] { Contrasenas.Crear(usuario, clave) } }));
        }

        [UnityTearDown]
        public IEnumerator Limpiar()
        {
            if (piloto != null) Object.Destroy(piloto.gameObject);
            yield return null;
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true);
            if (ventana != null) indice.SetValue(ventana, indiceAnterior);
            clave = usuario = null;
        }

        static void Cargar(string nombre) => EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/" + nombre + ".unity",
            new LoadSceneParameters(LoadSceneMode.Single));
        void Activar() => piloto = FlujoJugadorPilotoV2.Activar(AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset"), sesion, Cargar);
        IEnumerator Pantalla(string nombre)
        {
            float limite = Time.realtimeSinceStartup + 12;
            while (SceneManager.GetActiveScene().name != nombre)
            { Assert.That(Time.realtimeSinceStartup, Is.LessThan(limite), nombre); yield return null; }
            yield return null; yield return null;
        }

        void CrearDatos()
        {
            var j = piloto.Juego;
            alfa = j.Usuarios.RegistrarJugador("Prueba Alfa", "99003401", 1);
            primera = Completar(false); segunda = Completar(true);
            incompleta = j.RecorridosV2.IniciarPartida();
            beta = j.Usuarios.RegistrarJugador("Prueba Beta", "99003402", 2);
            otra = Completar(true);
            sinPartidas = j.Usuarios.RegistrarJugador("Prueba Sin Partidas", "99003403", 3);
            j.Usuarios.CerrarSesion();
        }
        Partida Completar(bool ultima)
        {
            var j = piloto.Juego; var p = j.RecorridosV2.IniciarPartida();
            for (int paso = 0; paso < 32; paso++)
            {
                var estado = j.RecorridosV2.Consultar(p.id);
                if (estado.CierrePresentado) return p;
                if (estado.PresentacionPendiente) j.RecorridosV2.ConfirmarPresentacion(p.id, estado.DecisionConfirmada.id);
                else
                {
                    var situacion = j.Partidas.SituacionActual(p);
                    j.Partidas.Decidir(p.id, situacion.id, situacion.alternativas[ultima ? situacion.alternativas.Length - 1 : 0].id);
                }
            }
            throw new AssertionException("La preparación no alcanzó el cierre esperado.");
        }
        VistaAdministrativaDemo Vista() => Object.FindAnyObjectByType<VistaAdministrativaDemo>(FindObjectsInactive.Include);
        AdministracionPilotoV2 Panel() => Object.FindAnyObjectByType<AdministracionPilotoV2>();
        static Button Boton(string nombre) => Object.FindObjectsByType<Button>(FindObjectsInactive.Include).Single(b => b.name == nombre);
        static void Click(Button boton) => ExecuteEvents.Execute(boton.gameObject,
            new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        static void Accion(Button boton) => boton.onClick.Invoke();
        IEnumerator Login()
        {
            yield return Pantalla("Inicio");
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().administrador);
            yield return Pantalla("AdminLogin");
            if (ancho != 0) yield return Captura("AdminLogin");
            Vista().usuario.text = usuario; Vista().contrasena.text = clave; Click(Vista().ingresar);
            yield return Pantalla("Admin");
            Assert.That(piloto.Juego.Usuarios.Actual.Rol, Is.EqualTo(Rol.Administrador));
        }

        [UnityTest]
        public IEnumerator ConsultaPartidasPermisosYPdf([ValueSource(nameof(Resoluciones))] int resolucion)
        {
            Resolucion(resolucion); Activar(); CrearDatos();
            yield return Login();
            var v = Vista(); var panel = Panel();
            Assert.That(v.enabled, Is.False);
            Assert.That(v.jugadores.Count(b => b.gameObject.activeSelf), Is.EqualTo(3));
            v.busqueda.text = "Alfa"; Click(v.buscar);
            Assert.That(v.jugadores.Count(b => b.gameObject.activeSelf), Is.EqualTo(1));
            Click(v.jugadores[0]);
            Assert.That(panel.JugadorSeleccionado, Is.EqualTo(alfa.id));
            Assert.That(Object.FindAnyObjectByType<VolantePixelUI>().gameObject.activeInHierarchy, Is.True);
            Assert.That(Object.FindAnyObjectByType<SeleccionAdministrativaVisual>().enabled, Is.False);
            yield return Captura("Seleccion");
            Click(Boton("Partida piloto 0")); Click(v.ver);
            Assert.That(panel.PartidaSeleccionada, Is.EqualTo(incompleta.id));
            Assert.That(v.pdf.interactable, Is.False);
            Accion(v.pdf); // Even a programmatic callback cannot bypass the service's finalized-state restriction.
            Assert.That(panel.UltimoPdf, Is.Null);
            Assert.That(Directory.Exists(Path.Combine(carpeta, "Reportes")), Is.False);
            Assert.That(panel.ResumenVisible.Historial.Count, Is.Zero);
            Click(Boton("Partida piloto 1")); Click(v.ver);
            ValidarResumen(segunda);
            Click(v.pdf);
            Assert.That(File.Exists(panel.UltimoPdf), Is.True);
            Assert.That(Path.GetDirectoryName(panel.UltimoPdf), Is.EqualTo(Path.Combine(carpeta, "Reportes")));
            var textoPdf = Encoding.GetEncoding(1252).GetString(File.ReadAllBytes(panel.UltimoPdf));
            Assert.That(textoPdf, Does.StartWith("%PDF-1.4")); Assert.That(textoPdf, Does.Contain("%%EOF"));
            Assert.That(textoPdf, Does.Contain(alfa.nombre)); Assert.That(textoPdf, Does.Not.Contain(beta.nombre));
            Assert.That(textoPdf, Does.Contain("Partida #" + segunda.id));
            Assert.That(textoPdf, Does.Contain("Puntos de seguridad: " + segunda.puntos));
            string evidencia = Path.GetFullPath(Path.Combine(Application.dataPath, "../TestEvidence/AdministracionV2"));
            Directory.CreateDirectory(evidencia);
            File.Copy(panel.UltimoPdf, Path.Combine(evidencia, "partida-seleccionada-" + ancho + ".pdf"), true);
            Click(v.ver);
            Assert.That(panel.EventosVisibles.Any(e => e.tipo == TipoEvento.ReporteGenerado), Is.True);
            yield return Captura("Desempeno");
            var scroll = Object.FindAnyObjectByType<ScrollRect>();
            ExecuteEvents.Execute(scroll.gameObject, new PointerEventData(EventSystem.current) { scrollDelta = new Vector2(0, -10000) }, ExecuteEvents.scrollHandler);
            yield return null;
            Assert.That(scroll.verticalNormalizedPosition, Is.LessThan(.01f), "El historial y los eventos deben poder leerse hasta el final.");
            yield return Captura("Eventos");
            Click(Boton("Partidas siguientes")); Click(Boton("Partida piloto 0"));
            Assert.That(panel.ResumenVisible, Is.Null); Assert.That(panel.UltimoPdf, Is.Null);
            Click(v.ver); ValidarResumen(primera);
            Assert.That(primera.puntos, Is.Not.EqualTo(segunda.puntos));
            v.busqueda.text = beta.cedula; Click(v.buscar); Click(v.jugadores[0]); Click(Boton("Partida piloto 0")); Click(v.ver);
            ValidarResumen(otra);
            Assert.That(panel.ResumenVisible.Jugador.id, Is.EqualTo(beta.id));
            v.busqueda.text = "Sin Partidas"; Click(v.buscar); Click(v.jugadores[0]);
            Assert.That(v.mensaje.text, Does.Contain("no tiene partidas")); Assert.That(v.ver.interactable, Is.False);
            Assert.That(panel.ResumenVisible, Is.Null);
            v.busqueda.text = Guid.NewGuid().ToString(); Click(v.buscar);
            Assert.That(v.mensaje.text, Does.Contain("No hay jugadores")); Assert.That(panel.JugadorSeleccionado, Is.Zero);
            Click(v.volver); yield return Pantalla("Inicio");
            Assert.That(piloto.Juego.Usuarios.Actual.Rol, Is.EqualTo(Rol.Ninguno));
            piloto.Navegar("Admin"); yield return Pantalla("AdminLogin");
            Assert.Throws<ErrorJuego>(() => piloto.Juego.Desempeno.ListarJugadores());
        }

        void ValidarResumen(Partida p)
        {
            var panel = Panel(); var datos = new BaseDatos(carpeta);
            var gamificacion = GamificacionPartidaV2.Consultar(piloto.Juego, p.id);
            Assert.That(panel.ResumenVisible.Partida.puntos, Is.EqualTo(gamificacion.Puntos));
            Assert.That(panel.ResumenVisible.BuenasPracticas, Is.EqualTo(gamificacion.BuenasPracticas.Length));
            Assert.That(panel.ResumenVisible.Partida.id, Is.EqualTo(p.id));
            Assert.That(panel.ResumenVisible.Partida.puntos, Is.EqualTo(datos.Partidas.Single(x => x.id == p.id).puntos));
            var decisiones = datos.Decisiones.Where(d => d.partida_id == p.id).ToArray();
            Assert.That(panel.ResumenVisible.Historial.Select(h => h.Decision.id), Is.EqualTo(decisiones.Select(d => d.id)));
            foreach (var d in decisiones) Assert.That(Vista().detalle.text, Does.Contain(d.consecuencia));
            // S3-09: el panel muestra los cuatro KPI del servicio, de la partida y del acumulado del jugador.
            Assert.That(Vista().detalle.text, Does.Contain("INDICADORES (S3-09)"));
            Assert.That(Vista().detalle.text, Does.Not.Contain("pendientes del servicio de indicadores"));
            var acumulado = piloto.Juego.Desempeno.IndicadoresJugador(p.jugador_id);
            Assert.That(panel.IndicadoresJugadorVisibles.PartidasConsideradas, Is.EqualTo(acumulado.PartidasConsideradas));
            for (int i = 0; i < acumulado.Todos.Count; i++)
                Assert.That(Vista().detalle.text, Does.Contain(panel.ResumenVisible.Indicadores.Todos[i].Texto + " | " + acumulado.Todos[i].Texto));
            Assert.That(panel.EventosVisibles, Is.Not.Empty);
            Assert.That(panel.EventosVisibles.All(e => e.partida_id == p.id && (e.jugador_id == 0 || e.jugador_id == p.jugador_id)), Is.True);
        }

        [UnityTest]
        public IEnumerator CredencialesIncorrectasJugadorBloqueadoYReapertura()
        {
            Activar(); CrearDatos(); yield return Pantalla("Inicio");
            piloto.Juego.Usuarios.RegistrarJugador(alfa.nombre, alfa.cedula, 1);
            Assert.Throws<ErrorJuego>(() => piloto.Juego.Desempeno.ListarJugadores());
            Assert.Throws<ErrorJuego>(() => piloto.Juego.Desempeno.PartidasDe(alfa.id));
            Assert.Throws<ErrorJuego>(() => piloto.Juego.Desempeno.ReportePdf(primera.id));
            piloto.Navegar("Admin"); yield return Pantalla("AdminLogin");
            Vista().usuario.text = usuario; Vista().contrasena.text = Guid.NewGuid().ToString(); Click(Vista().ingresar);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("AdminLogin"));
            Assert.That(piloto.Juego.Usuarios.Actual.Rol, Is.EqualTo(Rol.Ninguno));
            Assert.That(Vista().contrasena.text, Is.Empty); Assert.That(Vista().mensaje.text, Does.Contain("incorrectos"));
            Assert.That(piloto.Juego.BaseDatos.Eventos.Any(e => e.tipo == TipoEvento.SesionFallida), Is.True);
            Object.Destroy(piloto.gameObject); yield return null;
            Activar(); yield return Login();
            Assert.That(piloto.CarpetaDatos, Is.EqualTo(carpeta));
            Assert.That(piloto.Juego.Desempeno.ListarJugadores().Count, Is.EqualTo(3));
            Assert.That(piloto.Juego.BaseDatos.Partidas.Count, Is.EqualTo(4));
            piloto.Juego.Usuarios.CerrarSesion();
            yield return Pantalla("AdminLogin");
            Assert.That(piloto.Juego.Usuarios.Actual.Rol, Is.EqualTo(Rol.Ninguno));
        }

        [UnityTest]
        public IEnumerator PanelVacioYUsuarioInexistente()
        {
            Activar(); yield return Login();
            Assert.That(Vista().mensaje.text, Does.Contain("No hay jugadores"));
            Assert.That(Vista().ver.interactable, Is.False); Assert.That(Vista().pdf.interactable, Is.False);
            Click(Vista().volver); yield return Pantalla("Inicio");
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().administrador); yield return Pantalla("AdminLogin");
            Vista().usuario.text = Guid.NewGuid().ToString(); Vista().contrasena.text = clave; Click(Vista().ingresar);
            Assert.That(piloto.Juego.Usuarios.Actual.Rol, Is.EqualTo(Rol.Ninguno));
            Assert.That(Vista().mensaje.text, Does.Contain("incorrectos"));
        }

        void Resolucion(int anchoPrueba)
        {
            ancho = anchoPrueba; int alto = ancho == 1920 ? 1080 : 720;
            var asm = typeof(UnityEditor.Editor).Assembly;
            var tipo = asm.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(tipo);
            var sizes = singleton.GetProperty("instance", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            var group = tipo.GetProperty("currentGroup").GetValue(sizes); var gt = group.GetType();
            int total = (int)gt.GetMethod("GetTotalCount").Invoke(group, null), elegido = -1;
            for (int i = 0; i < total; i++)
            {
                var size = gt.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
                if ((int)size.GetType().GetProperty("width").GetValue(size) == ancho && (int)size.GetType().GetProperty("height").GetValue(size) == alto) { elegido = i; break; }
            }
            if (elegido < 0)
            {
                var size = Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),
                    Enum.ToObject(asm.GetType("UnityEditor.GameViewSizeType"), 1), ancho, alto, "Piloto V2 " + ancho);
                gt.GetMethod("AddCustomSize").Invoke(group, new[] { size }); elegido = total;
            }
            ventana = EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));
            indice = ventana.GetType().GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            indiceAnterior = (int)indice.GetValue(ventana); indice.SetValue(ventana, elegido); ventana.Focus(); ventana.Repaint();
        }
        IEnumerator Captura(string pantalla)
        {
            yield return new WaitForEndOfFrame();
            Assert.That(Screen.width, Is.EqualTo(ancho)); Assert.That(Screen.height, Is.EqualTo(ancho == 1920 ? 1080 : 720));
            var imagen = ScreenCapture.CaptureScreenshotAsTexture();
            string destino = Path.GetFullPath(Path.Combine(Application.dataPath, "../TestEvidence/AdministracionV2")); Directory.CreateDirectory(destino);
            File.WriteAllBytes(Path.Combine(destino, pantalla + "-" + Screen.width + "x" + Screen.height + ".png"), imagen.EncodeToPNG());
            Object.Destroy(imagen);
        }
    }
}
#endif
