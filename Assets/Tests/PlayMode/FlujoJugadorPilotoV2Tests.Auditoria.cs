#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using LifeDrive.IntegracionV2;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Fase = LifeDrive.IntegracionV2.PruebaIntegracionV2.Fase;

namespace LifeDrive.Tests
{
    public partial class FlujoJugadorPilotoV2Tests
    {
        IEnumerator CapturaAuditoria(string pantalla)
        {
            yield return new WaitForEndOfFrame();
            Assert.That(Screen.width, Is.EqualTo(ancho)); Assert.That(Screen.height, Is.EqualTo(alto));
            var captura = ScreenCapture.CaptureScreenshotAsTexture();
            var destino = Path.GetFullPath(Path.Combine(Application.dataPath, "../TestEvidence/AuditoriaV2"));
            Directory.CreateDirectory(destino);
            File.WriteAllBytes(Path.Combine(destino, pantalla + "-" + ancho + "x" + alto + ".png"), captura.EncodeToPNG());
            Object.Destroy(captura);
        }

        [UnityTest]
        public IEnumerator AuditoriaNombreLargoSinRecortes([ValueSource(nameof(ResolucionesApariencia))] int resolucion)
        {
            Resolucion(resolucion == 1920); Activar(); yield return Pantalla("Inicio");
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar); yield return Pantalla("Registro");
            var v = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
            Click(Boton("Ficha piloto 3")); Click(v.comprobar);
            Assert.That(v.mensaje.text, Does.Contain("nombre"));
            Assert.That(piloto.Juego.BaseDatos.Jugadores, Is.Empty);
            v.nombre.text = new string('W', 60); v.documento.text = "1234"; Click(v.comprobar);
            Assert.That(v.mensaje.text, Does.Contain("5 y 15"));
            Assert.That(piloto.Juego.BaseDatos.Jugadores, Is.Empty);
            v.documento.text = "99003701";
            Click(v.estilos[1]); Click(v.piel[4]); Click(v.ojos[2]); Click(v.cabello[3]); Click(v.traje[1]);
            yield return CapturaAuditoria("Registro-validado");
            Click(v.comprobar); yield return Pantalla("PruebaIntegracionV2");
            var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>(); yield return Terminar(prueba);
            Click(Boton("Nueva prueba")); yield return Pantalla("Resultado");
            var resultado = Object.FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
            Canvas.ForceUpdateCanvases(); resultado.jugador.ForceMeshUpdate();
            yield return CapturaAuditoria("Resultado-nombre-largo");
            Assert.That(resultado.jugador.text, Is.EqualTo(new string('W', 60)));
            Assert.That(resultado.jugador.isTextTruncated, Is.False, "Un nombre permitido por Registro debe poder leerse completo.");
            Assert.That(resultado.jugador.textInfo.characterInfo.Take(resultado.jugador.textInfo.characterCount).Count(c => c.isVisible), Is.EqualTo(60));
            Assert.That(resultado.personaje.rectTransform.rect.height, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator AuditoriaNombreSeMuestraLiteralSinInterpretarEtiquetas()
        {
            Resolucion(false); Activar(); yield return Pantalla("Inicio");
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar); yield return Pantalla("Registro");
            var v = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
            const string nombre = "<color=red>Auditora</color>";
            v.nombre.text = nombre; v.documento.text = "99003702"; Click(Boton("Ficha piloto 3")); Click(v.comprobar);
            yield return Pantalla("PruebaIntegracionV2");
            yield return Terminar(Object.FindAnyObjectByType<PruebaIntegracionV2>());
            Click(Boton("Nueva prueba")); yield return Pantalla("Resultado");
            var resultado = Object.FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
            resultado.jugador.ForceMeshUpdate();
            yield return CapturaAuditoria("Nombre-literal");
            Assert.That(resultado.jugador.GetParsedText(), Is.EqualTo(nombre), "El nombre no debe controlar el formato de la interfaz.");
        }

        [UnityTest]
        public IEnumerator AuditoriaErrorDeHuellaNoFallaOtraVezAlPresentarlo()
        {
            Resolucion(false); Activar(); yield return Registrar(7);
            var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            yield return Esperar(prueba, Fase.Situacion); Click(Boton("Respuesta 1"));
            var partida = piloto.Juego.BaseDatos.Partidas.Single(); string original = partida.contenido_huella;
            int puntos = partida.puntos, decisiones = piloto.Juego.BaseDatos.Decisiones.Count;
            piloto.Juego.BaseDatos.Ejecutar(() => piloto.Juego.BaseDatos.Partidas.Single().contenido_huella = "huella-alterada-solo-prueba");
            Click(Boton("Reinicio visual")); yield return null;
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Error));
            var detalle = prueba.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "Detalle");
            Assert.That(detalle.text, Does.Contain("no coincide"));
            yield return CapturaAuditoria("Error-controlado");
            LogAssert.NoUnexpectedReceived();
            piloto.Juego.BaseDatos.Ejecutar(() => piloto.Juego.BaseDatos.Partidas.Single().contenido_huella = original);
            Click(Boton("Reinicio visual"));
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
            Assert.That(prueba.Gamificacion.Puntos, Is.EqualTo(puntos));
            Assert.That(piloto.Juego.BaseDatos.Decisiones.Count, Is.EqualTo(decisiones));
        }

        [UnityTest]
        public IEnumerator AuditoriaHoverYDeshabilitadoSonDistinguibles()
        {
            Resolucion(false); Activar(); yield return Pantalla("Inicio");
            var v = Object.FindAnyObjectByType<PersonalizacionRegistroUI>(); var b = v.jugar;
            var feedback = b.GetComponent<RespuestaBotonUI>();
            yield return new WaitForSecondsRealtime(.3f);
            var normal = b.targetGraphic.color;
            ExecuteEvents.Execute(b.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(b.transform.localScale.x, Is.GreaterThan(1.02f));
            Assert.That(Vector4.Distance(b.targetGraphic.color, normal), Is.GreaterThan(.01f));
            ExecuteEvents.Execute(b.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
            b.interactable = false; yield return new WaitForSecondsRealtime(.3f);
            yield return CapturaAuditoria("Control-deshabilitado");
            Assert.That(b.transform.localScale.x, Is.EqualTo(1).Within(.001f));
            Assert.That(Vector4.Distance(b.targetGraphic.color, normal), Is.GreaterThan(.05f), "Un botón deshabilitado no debe verse idéntico al disponible.");
            Click(b); Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Inicio"));
            b.interactable = true; yield return new WaitForSecondsRealtime(.3f);
            Assert.That(Vector4.Distance(b.targetGraphic.color, feedback.normal), Is.LessThan(.01f));
        }
    }
}
#endif
