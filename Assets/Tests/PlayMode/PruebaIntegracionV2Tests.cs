#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.IntegracionV2;
using LifeDrive.Motor;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Fase = LifeDrive.IntegracionV2.PruebaIntegracionV2.Fase;

namespace LifeDrive.Tests
{
    public class PruebaIntegracionV2Tests
    {
        const string Escena = "Assets/Scenes/PruebaIntegracionV2.unity";
        static readonly int[] Rutas = { 0, 1, 2, 3, 4, 5, 6, 7 };
        static readonly string[][] Caminos =
        {
            new[] { "S0", "R1A", "R2A", "R3A", "DELIVERY" },
            new[] { "S0", "R1A", "R2A", "R3B", "DELIVERY" },
            new[] { "S0", "R1A", "R2B", "R3C", "DELIVERY" },
            new[] { "S0", "R1A", "R2B", "R3D", "DELIVERY" },
            new[] { "S0", "R1B", "R2C", "R3E", "DELIVERY" },
            new[] { "S0", "R1B", "R2C", "R3F", "DELIVERY" },
            new[] { "S0", "R1B", "R2D", "R3G", "DELIVERY" },
            new[] { "S0", "R1B", "R2D", "R3H", "DELIVERY" }
        };
        Guid sesion;
        string carpeta;
        PruebaIntegracionV2 prueba;
        float escalaPrevia;

        [SetUp]
        public void Preparar()
        {
            escalaPrevia = Time.timeScale;
            Time.timeScale = 28;
            sesion = Guid.NewGuid();
            carpeta = Path.Combine(Application.temporaryCachePath, "LifeDrive-PruebaIntegracionV2", sesion.ToString("N"));
        }

        [TearDown]
        public void Limpiar()
        {
            Time.timeScale = escalaPrevia;
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true);
        }

        IEnumerator AbrirEscena()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(Escena, new LoadSceneParameters(LoadSceneMode.Single));
            prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            Assert.That(prueba, Is.Not.Null);
            prueba.AislarSesion(sesion);
            yield return Esperar(Fase.Preparada);
            Click("Iniciar V2");
        }

        [UnityTest]
        public IEnumerator OchoRutasConBotonesReales([ValueSource(nameof(Rutas))] int ruta)
        {
            yield return AbrirEscena();
            int tramo = 0;
            int respuestas = 0;
            while (respuestas < 8 && prueba.Estado != Fase.Entregada)
            {
                yield return Esperar(Fase.Situacion);
                string origen = prueba.MapaVisible;
                int id = prueba.SituacionVisibleId;
                bool bifurca = id == 5 || id >= 101;
                int indice = bifurca ? (ruta >> (2 - tramo)) & 1 : 0;

                var boton = BuscarBoton("Respuesta " + (indice + 1));
                Assert.That(boton.GetComponentInChildren<TMP_Text>().text, Is.Not.Empty);
                Click(boton);
                Click(boton);
                respuestas++;
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
                Assert.That(prueba.MapaVisible, Is.EqualTo(origen));
                Assert.That(boton.interactable, Is.False);
                var antes = LeerDatos();
                var decision = antes.Decisiones.Last();
                Assert.That(decision.situacion_id, Is.EqualTo(id));
                Assert.That(Texto("Detalle"), Does.Contain(decision.consecuencia));
                Assert.That(antes.Decisiones.Count, Is.EqualTo(respuestas));

                Click("Continuar");
                if (bifurca || id == 6)
                {
                    Assert.That(prueba.Estado, Is.EqualTo(Fase.Transicion));
                    tramo++;
                    yield return Esperar(Fase.Situacion);
                    Assert.That(prueba.MapaVisible, Is.EqualTo(Caminos[ruta][tramo]));
                }
            }
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Entregada));
            Assert.That(prueba.MapaVisible, Is.EqualTo("DELIVERY"));
            Assert.That(Texto("Título"), Is.EqualTo("Entrega completada"));
            Assert.That(tramo, Is.EqualTo(4));
            var datos = LeerDatos();
            Assert.That(datos.Partidas.Single().estado, Is.EqualTo(EstadoPartida.Finalizada));
            Assert.That(datos.Partidas.Single().puntos, Is.EqualTo(datos.Decisiones.Sum(d => d.puntos_aplicados)));
            Assert.That(datos.Partidas.Single().retraso_min, Is.EqualTo(datos.Decisiones.Sum(d => d.retraso_aplicado)));
            Assert.That(datos.Eventos.Count(e => e.tipo == TipoEvento.PuntosActualizados), Is.EqualTo(respuestas));
            Assert.That(datos.Eventos.Count(e => e.tipo == TipoEvento.PartidaFinalizada), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator RestauraHitosYPuedePausarYReiniciarLaPresentacion()
        {
            yield return AbrirEscena();
            var idPartida = LeerDatos().Partidas.Single().id;
            Click("Pausa");
            float progreso = prueba.ProgresoMapa;
            yield return null;
            Assert.That(prueba.Pausada, Is.True);
            Assert.That(prueba.ProgresoMapa, Is.EqualTo(progreso));
            Click("Pausa");
            yield return AbrirEscena();
            Assert.That(prueba.MapaVisible, Is.EqualTo("S0"));
            yield return ResponderYContinuar(1);

            int[] orden = { 101, 102, 105, 6 };
            for (int i = 0; i < orden.Length; i++)
            {
                yield return Esperar(Fase.Situacion);
                Assert.That(prueba.SituacionVisibleId, Is.EqualTo(orden[i]));
                Click("Respuesta " + (i == 1 ? 2 : 1));
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
                int confirmadas = LeerDatos().Decisiones.Count;
                Click("Continuar");
                if (i == 0)
                {
                    Assert.That(prueba.Estado, Is.EqualTo(Fase.Transicion));
                    yield return null;
                    Click("Pausa");
                    Click("Reinicio visual");
                    Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
                    Click("Continuar");
                    yield return AbrirEscena();
                    Assert.That(prueba.MapaVisible, Is.EqualTo("S0"));
                    Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
                    Assert.That(LeerDatos().Decisiones.Count, Is.EqualTo(confirmadas));
                    Click("Continuar");
                }
                yield return Esperar(Fase.Situacion);
                string mapa = prueba.MapaVisible;
                yield return AbrirEscena();
                Assert.That(prueba.MapaVisible, Is.EqualTo(mapa));
                Assert.That(LeerDatos().Decisiones.Count, Is.EqualTo(confirmadas));
            }

            yield return ResponderYContinuar(7);
            yield return Esperar(Fase.Entregada);
            yield return AbrirEscena();
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Entregada));
            Assert.That(prueba.MapaVisible, Is.EqualTo("DELIVERY"));
            var datos = LeerDatos();
            Assert.That(datos.Partidas.Single().id, Is.EqualTo(idPartida));
            Assert.That(datos.Decisiones.Count, Is.EqualTo(6));
            Assert.That(datos.Eventos.Count(e => e.tipo == TipoEvento.PuntosActualizados), Is.EqualTo(6));
            Assert.That(datos.Eventos.Count(e => e.tipo == TipoEvento.PartidaFinalizada), Is.EqualTo(1));
        }

        IEnumerator ResponderYContinuar(int id)
        {
            yield return Esperar(Fase.Situacion);
            Assert.That(prueba.SituacionVisibleId, Is.EqualTo(id));
            Click("Respuesta 1");
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
            Click("Continuar");
        }

        IEnumerator Esperar(Fase fase)
        {
            float limite = Time.realtimeSinceStartup + 12;
            while (prueba.Estado != fase)
            {
                Assert.That(prueba.Estado, Is.Not.EqualTo(Fase.Error), Texto("Detalle"));
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(limite), $"Se esperaba {fase}, se obtuvo {prueba.Estado}.");
                yield return null;
            }
        }

        Button BuscarBoton(string nombre) => prueba.GetComponentsInChildren<Button>(true)
            .Single(b => b.name == nombre);
        void Click(string nombre) => Click(BuscarBoton(nombre));
        static void Click(Button boton) => ExecuteEvents.Execute(boton.gameObject,
            new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left },
            ExecuteEvents.pointerClickHandler);
        string Texto(string nombre) => prueba.GetComponentsInChildren<TMP_Text>(true)
            .Single(t => t.name == nombre).text;
        BaseDatos LeerDatos() => new BaseDatos(carpeta);
    }
}
#endif
