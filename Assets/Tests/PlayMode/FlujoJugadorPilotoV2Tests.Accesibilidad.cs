#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using LifeDrive.IntegracionV2;
using LifeDrive.Mapas.InterfazVisual;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Fase = LifeDrive.IntegracionV2.PruebaIntegracionV2.Fase;

namespace LifeDrive.Tests
{
    public partial class FlujoJugadorPilotoV2Tests
    {
        IEnumerator CapturaAccesibilidad(string nombre)
        {
            yield return new WaitForEndOfFrame();
            Assert.That(Screen.width, Is.EqualTo(ancho)); Assert.That(Screen.height, Is.EqualTo(alto));
            var imagen = ScreenCapture.CaptureScreenshotAsTexture();
            var destino = Path.GetFullPath(Path.Combine(Application.dataPath, "../TestEvidence/AccesibilidadV2"));
            Directory.CreateDirectory(destino);
            File.WriteAllBytes(Path.Combine(destino, nombre + "-" + ancho + "x" + alto + ".png"), imagen.EncodeToPNG());
            Object.Destroy(imagen);
        }
        static TMP_Text TextoFicha(string nombre) => Object.FindObjectsByType<TMP_Text>().Single(t => t.name == nombre);

        [UnityTest]
        public IEnumerator AccesibilidadJugadorTecladoRegistroEntregaYReanudacion([ValueSource(nameof(ResolucionesApariencia))] int resolucion)
        {
            using (var teclas = new TeclasPilotoPruebas())
            {
                Resolucion(resolucion == 1920); Activar(); yield return Pantalla("Inicio");
                var vista = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
                yield return EsperarConfiguracion("Inicio");
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(vista.jugar.gameObject));
                Assert.That(vista.jugar.transform.Find("Foco accesible").gameObject.activeSelf, Is.True);
                yield return teclas.Pulsar(Key.Tab, true);
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Boton("Configuración").gameObject),
                    "«Configuración» es el último elemento de Inicio: Mayús+Tab desde Jugar da la vuelta hasta él.");
                yield return teclas.Pulsar(Key.Tab); yield return CapturaAccesibilidad("Inicio-foco");
                yield return teclas.Pulsar(Key.Enter); yield return Pantalla("Registro");
                vista = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
                TeclasPilotoPruebas.Escribir("Piloto Teclado"); yield return teclas.Pulsar(Key.Enter);
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(vista.documento.gameObject));
                TeclasPilotoPruebas.Escribir("99003901");
                yield return teclas.IrA(Boton("Ficha piloto 3")); yield return teclas.Pulsar(Key.Space);
                yield return teclas.IrA(vista.estilos[1]); yield return teclas.Pulsar(Key.Enter);
                yield return teclas.IrA(vista.piel[4]); yield return teclas.Pulsar(Key.Space);
                yield return teclas.IrA(vista.ojos[2]); yield return teclas.Pulsar(Key.Enter);
                yield return teclas.IrA(vista.cabello[3]); yield return teclas.Pulsar(Key.Space);
                yield return teclas.IrA(vista.traje[1]); yield return teclas.Pulsar(Key.Enter);
                Assert.That(vista.Selecciones, Is.EqualTo(new[] { 4,2,3,1 }));
                yield return CapturaAccesibilidad("Registro-foco");
                yield return teclas.IrA(vista.comprobar); yield return teclas.Pulsar(Key.Enter);
                yield return Pantalla("PruebaIntegracionV2");
                var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
                yield return Esperar(prueba, Fase.Situacion);
                yield return null; // Let the late UI focus/layout pass finish before inspecting its rendered state.
                var situacion = piloto.Juego.Partidas.SituacionActual(piloto.Juego.BaseDatos.Partidas.Single());
                Assert.That(TextoFicha("Detalle").text, Is.EqualTo(situacion.enunciado));
                Assert.That(TextoFicha("Sección").text, Does.Contain(situacion.tipo.Replace('_',' ')));
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Boton("Respuesta 1").gameObject));
                yield return CapturaAccesibilidad("Ficha-enunciado");
                yield return teclas.MantenerEnter();
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
                Assert.That(piloto.Juego.BaseDatos.Decisiones.Count, Is.EqualTo(1));
                yield return teclas.Soltar();
                var decision = piloto.Juego.BaseDatos.Decisiones.Single();
                Assert.That(TextoFicha("Detalle").text, Does.Contain(decision.alternativa));
                Assert.That(TextoFicha("Detalle").text, Does.Contain(decision.consecuencia));
                yield return CapturaAccesibilidad("Ficha-consecuencia");

                yield return teclas.IrA(Boton("Menú del jugador")); yield return teclas.Pulsar(Key.Space);
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Boton("Seguir en la operación").gameObject));
                yield return teclas.Pulsar(Key.Tab);
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Boton("Confirmar Inicio").gameObject));
                yield return teclas.Pulsar(Key.Tab);
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Boton("Seguir en la operación").gameObject), "Foco confinado al diálogo");
                yield return teclas.Pulsar(Key.Escape);
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Boton("Menú del jugador").gameObject));
                yield return teclas.Pulsar(Key.Enter); yield return teclas.Pulsar(Key.Tab); yield return teclas.Pulsar(Key.Enter);
                yield return Pantalla("Inicio"); yield return teclas.Pulsar(Key.Enter); yield return Pantalla("Registro");
                vista = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
                TeclasPilotoPruebas.Escribir("Piloto Teclado"); yield return teclas.Pulsar(Key.Tab);
                TeclasPilotoPruebas.Escribir("99003901");
                yield return teclas.IrA(Boton("Recuperar apariencia")); yield return teclas.Pulsar(Key.Space);
                Assert.That(vista.Selecciones, Is.EqualTo(new[] { 4,2,3,1 }));
                yield return teclas.IrA(vista.comprobar); yield return teclas.Pulsar(Key.Enter); yield return Pantalla("PruebaIntegracionV2");
                prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia)); Assert.That(piloto.Juego.BaseDatos.Decisiones.Count, Is.EqualTo(1));
                int respuestas = 1;
                for (int paso = 0; paso < 12 && prueba.Estado != Fase.Entregada; paso++)
                {
                    if (prueba.Estado != Fase.Consecuencia)
                    {
                        yield return Esperar(prueba, Fase.Situacion);
                        int alternativa = resolucion == 1280 && (prueba.SituacionVisibleId >= 101 || prueba.SituacionVisibleId == 5) ? 2 : 1;
                        yield return teclas.IrA(Boton("Respuesta " + alternativa)); yield return teclas.Pulsar(Key.Space); respuestas++;
                    }
                    Assert.That(piloto.Juego.BaseDatos.Decisiones.Count, Is.EqualTo(respuestas));
                    yield return teclas.IrA(Boton("Continuar")); yield return teclas.Pulsar(Key.Enter);
                }
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Entregada)); Assert.That(prueba.MapaVisible, Is.EqualTo("DELIVERY"));
                Assert.That(piloto.Juego.BaseDatos.Partidas.Single().puntos, Is.EqualTo(piloto.Juego.BaseDatos.Decisiones.Sum(d => d.puntos_aplicados)));
                yield return teclas.IrA(Boton("Nueva prueba")); yield return teclas.Pulsar(Key.Enter); yield return Pantalla("Resultado");
                var resultado = Object.FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(resultado.revisar.gameObject));
                Assert.That(resultado.selectores[1].interactable, Is.False, "Historial cerrado no debe paginar a ciegas");
                yield return teclas.Pulsar(Key.Space); Assert.That(resultado.panelDecisiones.activeSelf, Is.True);
                Assert.That(resultado.revisar.GetComponentInChildren<TMP_Text>().text, Is.EqualTo("Ocultar métricas"));
                string seguridad = resultado.filasDecisiones[0].text;
                yield return teclas.Pulsar(Key.RightArrow);
                Assert.That(resultado.filasDecisiones[0].text, Does.EndWith(" min"));
                yield return teclas.Pulsar(Key.LeftArrow);
                Assert.That(resultado.filasDecisiones[0].text, Is.EqualTo(seguridad));
                yield return CapturaAccesibilidad("Resultado-foco");
                yield return EsperarConfiguracion("Resultado");
                Click(Boton("Configuración")); yield return null;
                Assert.That(Boton("Música del juego").gameObject.activeInHierarchy, Is.True);
                yield return CapturaAccesibilidad("Resultado-configuracion");
                yield return teclas.Pulsar(Key.Escape);
                Assert.That(Object.FindObjectsByType<Button>().Any(b => b.name == "Cerrar configuración"), Is.False, "Escape cierra el panel.");
                yield return teclas.IrA(resultado.volver); yield return teclas.Pulsar(Key.Enter); yield return Pantalla("Inicio");
            }
        }

        static IEnumerator EsperarConfiguracion(string donde = "")
        {
            // El botón lo añade el gestor de audio cuando la interfaz de la pantalla ya existe.
            for (int i = 0; i < 60 && Object.FindObjectsByType<Button>().All(b => b.name != "Configuración"); i++) yield return null;
            Assert.That(Object.FindObjectsByType<Button>().Any(b => b.name == "Configuración"), Is.True, "Debe haber un botón Configuración (" + donde + ").");
        }

        [UnityTest]
        public IEnumerator AccesibilidadConfiguracionEnInicioYRegistroConElEstiloDeCadaPantalla()
        {
            using (var teclas = new TeclasPilotoPruebas())
            {
                Resolucion(true); Activar(); yield return Pantalla("Inicio");
                yield return EsperarConfiguracion();
                var gestor = GestorAudio.Instancia; bool antes = gestor.Silenciado;
                try
                {
                    var inicio = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
                    var abrir = Boton("Configuración");
                    // Mismo tipo de botón que los de la pantalla: misma fuente y el mismo componente de efecto.
                    Assert.That(abrir.GetComponentInChildren<TMP_Text>().font, Is.SameAs(inicio.administrador.GetComponentInChildren<TMP_Text>().font));
                    Assert.That(abrir.GetComponent<RespuestaBotonUI>(), Is.Not.Null);
                    Assert.That(abrir.transform.parent.GetComponentInParent<Canvas>().rootCanvas, Is.SameAs(inicio.jugar.GetComponentInParent<Canvas>().rootCanvas));
                    Click(abrir); yield return null;
                    Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Boton("Cerrar configuración").gameObject), "El foco entra al panel.");
                    yield return CapturaAccesibilidad("Inicio-configuracion");
                    Click(Boton("Música del juego"));
                    Assert.That(gestor.Silenciado, Is.Not.EqualTo(antes));
                    Assert.That(Boton("Música del juego").GetComponentInChildren<TMP_Text>().text,
                        Is.EqualTo(gestor.Silenciado ? "Música: silenciada" : "Música: activa"));
                    Click(Boton("Música del juego"));
                    Assert.That(gestor.Silenciado, Is.EqualTo(antes));
                    yield return teclas.Pulsar(Key.Escape);
                    Assert.That(Object.FindObjectsByType<Button>().Any(b => b.name == "Cerrar configuración"), Is.False, "Escape cierra el panel.");
                    Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Boton("Configuración").gameObject));

                    Click(inicio.jugar); yield return Pantalla("Registro");
                    yield return EsperarConfiguracion();
                    var registro = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
                    Assert.That(Boton("Configuración").GetComponentInChildren<TMP_Text>().font, Is.SameAs(registro.volver.GetComponentInChildren<TMP_Text>().font));
                    yield return CapturaAccesibilidad("Registro-configuracion-cerrada");
                    Click(Boton("Configuración")); yield return null;
                    yield return CapturaAccesibilidad("Registro-configuracion");
                    Click(Boton("Cerrar configuración"));
                    Assert.That(Object.FindObjectsByType<Button>().Any(b => b.name == "Cerrar configuración"), Is.False);
                }
                finally { gestor.Silenciado = antes; }
            }
        }

        [UnityTest]
        public IEnumerator AccesibilidadConfiguracionConTecladoMusicaYEscape()
        {
            using (var teclas = new TeclasPilotoPruebas())
            {
                Resolucion(true); Activar(); yield return Registrar(7);
                var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
                yield return Esperar(prueba, Fase.Situacion);
                var abrir = Boton("Configuración");
                yield return teclas.IrA(abrir); yield return teclas.Pulsar(Key.Enter);
                Assert.That(prueba.ConfiguracionAbierta, Is.True);
                yield return null;
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Boton("Cerrar configuración").gameObject), "El foco entra al panel.");
                yield return CapturaAccesibilidad("Configuracion-abierta");

                var gestor = GestorAudio.Instancia; bool antes = gestor.Silenciado;
                try
                {
                    yield return teclas.IrA(Boton("Música del juego")); yield return teclas.Pulsar(Key.Enter);
                    Assert.That(gestor.Silenciado, Is.Not.EqualTo(antes), "Enter sobre «Música» la silencia o la reactiva.");
                }
                finally { gestor.Silenciado = antes; }

                yield return teclas.Pulsar(Key.Escape);
                Assert.That(prueba.ConfiguracionAbierta, Is.False, "Escape cierra el panel.");
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(abrir.gameObject), "El foco vuelve al botón Configuración.");
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Situacion), "Abrir y cerrar los ajustes no toca la partida.");
            }
        }

        [UnityTest]
        public IEnumerator AccesibilidadTextoLargoDesplazableSinEvaluar([ValueSource(nameof(ResolucionesApariencia))] int resolucion)
        {
            using (var teclas = new TeclasPilotoPruebas())
            {
                Resolucion(resolucion == 1920); Activar(); yield return Registrar(7);
                var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>(); yield return Esperar(prueba, Fase.Situacion);
                var antes = JsonUtility.ToJson(piloto.Juego.BaseDatos.Partidas.Single());
                // Synthetic layout fixture only. No educational package, decision or persisted explanation is changed.
                var texto = TextoFicha("Detalle"); texto.text = string.Concat(Enumerable.Repeat("Texto sintético para probar la lectura y el desplazamiento de un panel largo. ", 35)) + "FIN DE LA PRUEBA VISUAL";
                yield return null; Canvas.ForceUpdateCanvases();
                var scroll = Object.FindObjectsByType<ScrollRect>().Single(s => s.name == "Lectura de ficha");
                LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
                Assert.That(texto.fontSize, Is.EqualTo(25)); Assert.That(texto.isTextTruncated, Is.False);
                Assert.That(scroll.content.rect.height, Is.GreaterThan(scroll.viewport.rect.height));
                yield return teclas.IrA(scroll.GetComponent<Selectable>());
                Assert.That(scroll.transform.Find("Foco accesible").gameObject.activeSelf, Is.True);
                Assert.That(scroll.transform.Find("Foco accesible").GetComponent<RectTransform>().offsetMin.x, Is.GreaterThanOrEqualTo(0));
                for (int i = 0; i < 50 && scroll.verticalNormalizedPosition > .001f; i++) yield return teclas.Pulsar(Key.PageDown);
                Assert.That(scroll.verticalNormalizedPosition, Is.LessThan(.001f));
                yield return CapturaAccesibilidad("Texto-sintetico-final");
                for (int i = 0; i < 50 && scroll.verticalNormalizedPosition < .999f; i++) yield return teclas.Pulsar(Key.PageUp);
                Assert.That(scroll.verticalNormalizedPosition, Is.GreaterThan(.999f));
                Assert.That(JsonUtility.ToJson(piloto.Juego.BaseDatos.Partidas.Single()), Is.EqualTo(antes));
                Assert.That(piloto.Juego.BaseDatos.Decisiones, Is.Empty);
            }
        }

        [UnityTest]
        public IEnumerator AccesibilidadContrasteHoverYFocoVisible()
        {
            Resolucion(false); Activar(); yield return Pantalla("Inicio");
            var vista = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
            EventSystem.current.SetSelectedGameObject(vista.administrador.gameObject);
            yield return new WaitForSecondsRealtime(.4f);
            var texto = vista.administrador.GetComponentInChildren<TMP_Text>();
            Assert.That(TecladoPilotoV2.Contraste(texto.color, vista.administrador.targetGraphic.color), Is.GreaterThanOrEqualTo(4.5f));
            Assert.That(vista.administrador.transform.Find("Foco accesible").gameObject.activeSelf, Is.True);
            yield return CapturaAccesibilidad("Contraste-foco");
        }
    }
}
#endif
