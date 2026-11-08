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
        bool vigilarProgreso;
        float ultimoProgreso;

        [SetUp]
        public void Preparar()
        {
            escalaPrevia = Time.timeScale;
            vigilarProgreso = false; ultimoProgreso = 0;
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
            vigilarProgreso = true;
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
                Assert.That(prueba.Gamificacion.Puntos, Is.EqualTo(antes.Partidas.Single().puntos));
                Assert.That(prueba.Gamificacion.SituacionesResueltas, Is.EqualTo(respuestas));
                Assert.That(Texto("Variación guardada"), Does.Contain(decision.puntos_aplicados.ToString("+0;-0;0")));
                Assert.That(Texto("Situaciones resueltas"), Does.Contain("Situaciones resueltas: " + respuestas));
                Assert.That(prueba.Gamificacion.BuenasPracticas.Length,
                    Is.EqualTo(antes.Decisiones.Count(d => d.es_buena_practica)));

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
            Assert.That(prueba.ProgresoRecorrido, Is.EqualTo(1).Within(.0001f));
            Assert.That(Texto("Situaciones resueltas"), Does.Contain("Entrega presentada"));
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
                if (vigilarProgreso)
                {
                    Assert.That(prueba.ProgresoRecorrido + .0001f, Is.GreaterThanOrEqualTo(ultimoProgreso), "El cambio de mapa no debe hacer retroceder la barra.");
                    ultimoProgreso = prueba.ProgresoRecorrido;
                }
                Assert.That(prueba.Estado, Is.Not.EqualTo(Fase.Error), Texto("Detalle"));
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(limite), $"Se esperaba {fase}, se obtuvo {prueba.Estado}.");
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator ReconocimientoRealSinInsigniaYPausaDePanel()
        {
            yield return AbrirEscena();
            Click("Reconocimientos");
            Assert.That(prueba.ReconocimientosAbiertos, Is.True);
            float posicion = prueba.ProgresoMapa;
            yield return null; yield return null;
            Assert.That(prueba.ProgresoMapa, Is.EqualTo(posicion));
            Click("Cerrar reconocimientos");
            yield return Esperar(Fase.Situacion);
            Click("Respuesta 1");
            Assert.That(prueba.Gamificacion.Puntos, Is.EqualTo(20));
            Assert.That(prueba.Gamificacion.BuenasPracticas.Length, Is.EqualTo(1));
            Assert.That(Texto("Detalle"), Does.Contain("Buena práctica registrada"));
            Assert.That(Texto("Variación guardada"), Does.Contain("Ganancia"));
            Click("Reconocimientos");
            Assert.That(Texto("Buenas prácticas registradas"), Does.Contain("registradas: 1"));
            Assert.That(Texto("Buenas prácticas registradas"), Does.Contain("Insignias otorgadas: ninguna"));
            Click("Cerrar reconocimientos");
            var guardada = LeerDatos(); int cantidad = guardada.Eventos.Count;
            yield return AbrirEscena();
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
            Assert.That(prueba.Gamificacion.Puntos, Is.EqualTo(20));
            Assert.That(prueba.Gamificacion.BuenasPracticas.Length, Is.EqualTo(1));
            Assert.That(LeerDatos().Eventos.Count(e => e.tipo == TipoEvento.BuenaPractica), Is.EqualTo(1));
            Click("Reinicio visual");
            Assert.That(prueba.Gamificacion.BuenasPracticas.Length, Is.EqualTo(1));
            Assert.That(LeerDatos().Decisiones.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CambiosNeutroYNegativoSinInsigniaAplicable()
        {
            yield return AbrirEscena(); yield return Esperar(Fase.Situacion);
            Click("Respuesta 2");
            Assert.That(Texto("Variación guardada"), Is.EqualTo("0 pts · Sin cambio"));
            Assert.That(prueba.Gamificacion.BuenasPracticas, Is.Empty);
            Click("Continuar"); yield return Esperar(Fase.Situacion);
            Click("Respuesta 2");
            Assert.That(Texto("Variación guardada"), Is.EqualTo("-20 pts · Pérdida"));
            Assert.That(prueba.Gamificacion.Puntos, Is.EqualTo(-20));
            Assert.That(prueba.Gamificacion.BuenasPracticas, Is.Empty);
            Click("Reconocimientos");
            Assert.That(Texto("Buenas prácticas registradas"), Does.Contain("no hay evidencias"));
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
