#if UNITY_EDITOR
using System.Collections;
using System.IO;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LifeDrive.Tests
{
    public partial class AdministracionPilotoV2Tests
    {
        [UnityTest]
        public IEnumerator AccesibilidadAdministracionTecladoPermisosYLectura([ValueSource(nameof(Resoluciones))] int resolucion)
        {
            using (var teclas = new TeclasPilotoPruebas())
            {
                Resolucion(resolucion); Activar(); CrearDatos(); yield return Pantalla("Inicio");
                yield return teclas.Pulsar(Key.Tab); yield return teclas.Pulsar(Key.Enter); yield return Pantalla("AdminLogin");
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Vista().usuario.gameObject));
                TeclasPilotoPruebas.Escribir(usuario); yield return teclas.Pulsar(Key.Tab);
                TeclasPilotoPruebas.Escribir("Incorrecta de prueba"); yield return teclas.Pulsar(Key.Enter);
                Assert.That(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, Is.EqualTo("AdminLogin"));
                yield return teclas.Pulsar(Key.Enter); yield return null;
                Assert.That(piloto.Juego.Usuarios.Actual.Rol, Is.EqualTo(Rol.Ninguno)); Assert.That(Vista().mensaje.text, Does.Contain("incorrectos"));
                yield return teclas.Pulsar(Key.Tab, true); TeclasPilotoPruebas.Escribir(clave);
                yield return teclas.Pulsar(Key.Enter);
                Assert.That(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, Is.EqualTo("AdminLogin"), "Enter en contraseña no envía sin pasar al botón.");
                yield return teclas.Pulsar(Key.Enter); yield return Pantalla("Admin");
                Assert.That(piloto.Juego.Usuarios.Actual.Rol, Is.EqualTo(Rol.Administrador));
                TeclasPilotoPruebas.Escribir("Alfa"); yield return teclas.Pulsar(Key.Tab); yield return teclas.Pulsar(Key.Space);
                Assert.That(Vista().busqueda.text, Is.EqualTo("Alfa"));
                Assert.That(Vista().busqueda.textComponent.text, Does.Contain("Alfa"));
                yield return teclas.IrA(Vista().jugadores[0]); yield return teclas.Pulsar(Key.Enter);
                Assert.That(Panel().JugadorSeleccionado, Is.EqualTo(alfa.id));
                Assert.That(Object.FindAnyObjectByType<VolantePixelUI>().gameObject.activeInHierarchy, Is.True);
                yield return Captura("Teclado-seleccion");
                yield return teclas.IrA(Boton("Partida piloto 1")); yield return teclas.Pulsar(Key.Space);
                yield return teclas.IrA(Vista().ver); yield return teclas.Pulsar(Key.Enter); ValidarResumen(segunda);
                yield return null;
                var scroll = Object.FindAnyObjectByType<ScrollRect>();
                yield return teclas.IrA(scroll.GetComponent<Selectable>()); yield return teclas.Pulsar(Key.PageDown);
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(scroll.gameObject));
                Assert.That(scroll.transform.Find("Foco accesible"), Is.Null, "Sin recuadro amarillo de foco.");
                Assert.That(scroll.verticalNormalizedPosition, Is.LessThan(.999f));
                yield return Captura("Teclado-desempeno");
                yield return teclas.IrA(Vista().pdf); yield return teclas.Pulsar(Key.Space);
                Assert.That(File.Exists(Panel().UltimoPdf), Is.True);
                yield return teclas.Pulsar(Key.Tab); Assert.That(EventSystem.current.currentSelectedGameObject.name, Is.EqualTo("Abrir PDF piloto"));
                // OS viewer is deliberately not launched. Focus and generated file are verified.
                yield return teclas.IrA(Vista().volver); yield return teclas.Pulsar(Key.Enter); yield return Pantalla("Inicio");
                Assert.That(piloto.Juego.Usuarios.Actual.Rol, Is.EqualTo(Rol.Ninguno));
            }
        }
    }
}
#endif
