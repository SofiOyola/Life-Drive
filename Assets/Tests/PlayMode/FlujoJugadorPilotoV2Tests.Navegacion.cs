#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LifeDrive.Datos;
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
        IEnumerator CapturaNavegacion(string nombre)
        {
            yield return new WaitForEndOfFrame();
            Assert.That(Screen.width, Is.EqualTo(ancho)); Assert.That(Screen.height, Is.EqualTo(alto));
            var imagen = ScreenCapture.CaptureScreenshotAsTexture();
            var destino = Path.GetFullPath(Path.Combine(Application.dataPath, "../TestEvidence/NavegacionV2"));
            Directory.CreateDirectory(destino);
            File.WriteAllBytes(Path.Combine(destino, nombre + "-" + ancho + "x" + alto + ".png"), imagen.EncodeToPNG());
            Object.Destroy(imagen);
        }

        IEnumerator VolverYRetomarDesdeMenu(bool cancelarPrimero = false)
        {
            var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            var mapa = prueba.MapaVisible;
            var fase = prueba.Estado;
            var partida = piloto.Juego.BaseDatos.Partidas.Last();
            var guardada = JsonUtility.ToJson(partida);
            var apariencia = JsonUtility.ToJson(piloto.AparienciaPartidaActual);
            var decisiones = piloto.Juego.BaseDatos.Decisiones.Select(d => d.id).ToArray();
            int eventosPuntos = piloto.Juego.BaseDatos.Eventos.Count(e => e.tipo == TipoEvento.PuntosActualizados);
            int hitos = partida.ultima_decision_presentada_id;
            Click(Boton("Menú del jugador"));
            float posicion = prueba.ProgresoMapa;
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(prueba.ProgresoMapa, Is.EqualTo(posicion).Within(.00001f), "El diálogo suspende solo la presentación.");
            var mensaje = Object.FindObjectsByType<TMP_Text>().Single(t => t.name == "Datos conservados");
            mensaje.ForceMeshUpdate(); Assert.That(mensaje.isTextTruncated, Is.False);
            var panel = (RectTransform)mensaje.transform.parent;
            var esquinas = new Vector3[4]; panel.GetWorldCorners(esquinas);
            Assert.That(esquinas[0].x, Is.GreaterThan(prueba.GetComponent<LifeDrive.Mapas.Demo.DemoRecorrido>().AreaVisualExterna.xMax),
                "El mapa OnGUI no debe cubrir la confirmación.");
            var confirmar = Boton("Confirmar Inicio");
            var impactos = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current)
                { position = RectTransformUtility.WorldToScreenPoint(null, confirmar.transform.position) }, impactos);
            Assert.That(impactos.First().gameObject, Is.EqualTo(confirmar.gameObject), "La confirmación recibe el clic delante de los controles de partida.");
            if (fase == Fase.Situacion)
            {
                Boton("Respuesta 1").onClick.Invoke();
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Situacion), "Una activación residual detrás del diálogo no responde.");
                Assert.That(piloto.Juego.BaseDatos.Decisiones.Select(d => d.id), Is.EqualTo(decisiones));
            }
            yield return CapturaNavegacion("Menu-" + mapa + "-" + fase);
            if (cancelarPrimero)
            {
                int eventos = piloto.Juego.BaseDatos.Eventos.Count;
                Click(Boton("Seguir en la operación"));
                Assert.That(prueba.Estado, Is.EqualTo(fase));
                Assert.That(piloto.Juego.BaseDatos.Eventos.Count, Is.EqualTo(eventos));
                Click(Boton("Menú del jugador"));
            }
            Click(Boton("Confirmar Inicio"));
            yield return Pantalla("Inicio");
            Assert.That(piloto.Juego.Usuarios.Actual.Rol, Is.EqualTo(Rol.Ninguno));
            Assert.That(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().mensaje.text, Does.Contain("conservada"));
            yield return CapturaNavegacion("Inicio-tras-regreso");
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar); yield return Pantalla("Registro");
            var registro = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
            registro.nombre.text = "Piloto Ficticio"; registro.documento.text = "990033";
            Assert.That(registro.comprobar.GetComponentInChildren<TMP_Text>().text,
                Is.EqualTo("COMENZAR RECORRIDO"));
            Assert.That(registro.mensaje.text, Does.Contain(fase == Fase.Entregada ? "última entrega" : "retomarás"));
            Click(Boton("Recuperar apariencia"));
            Assert.That(registro.Estilo, Is.EqualTo(1)); Assert.That(registro.Selecciones, Is.EqualTo(new[] { 2, 3, 4, 2 }));
            yield return CapturaNavegacion("Registro-" + (fase == Fase.Entregada ? "entrega" : "retomar"));
            Click(registro.comprobar); yield return Pantalla("PruebaIntegracionV2");
            prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            Assert.That(prueba.MapaVisible, Is.EqualTo(mapa));
            Assert.That(piloto.Juego.BaseDatos.Partidas.Count, Is.EqualTo(1));
            Assert.That(JsonUtility.ToJson(piloto.Juego.BaseDatos.Partidas.Single()), Is.EqualTo(guardada));
            Assert.That(JsonUtility.ToJson(piloto.AparienciaPartidaActual), Is.EqualTo(apariencia));
            Assert.That(piloto.Juego.BaseDatos.Decisiones.Select(d => d.id), Is.EqualTo(decisiones));
            Assert.That(piloto.Juego.BaseDatos.Eventos.Count(e => e.tipo == TipoEvento.PuntosActualizados), Is.EqualTo(eventosPuntos));
            Assert.That(prueba.Gamificacion.Puntos, Is.EqualTo(partida.puntos));
            Assert.That(piloto.Juego.BaseDatos.Partidas.Single().ultima_decision_presentada_id, Is.EqualTo(hitos));
            Assert.That(prueba.ProgresoRecorrido, Is.InRange(0f, 1f));
            if (fase == Fase.Transicion || fase == Fase.Consecuencia)
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
            else if (fase == Fase.Entregada)
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Entregada));
        }

        [UnityTest]
        public IEnumerator NavegacionMenuReanudaMapasTransicionYDelivery([ValueSource(nameof(ResolucionesApariencia))] int resolucion)
        {
            Resolucion(resolucion == 1920); Activar(); yield return Registrar(7);
            var visitados = new HashSet<string>(); bool transicionProbada = false;
            for (int paso = 0; paso < 12; paso++)
            {
                var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
                if (prueba.Estado == Fase.Entregada) break;
                yield return Esperar(prueba, Fase.Situacion);
                if (visitados.Add(prueba.MapaVisible))
                {
                    yield return VolverYRetomarDesdeMenu(visitados.Count == 1);
                    prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>(); yield return Esperar(prueba, Fase.Situacion);
                }
                Click(Boton("Respuesta 1"));
                if (paso == 0)
                {
                    yield return VolverYRetomarDesdeMenu();
                    prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
                }
                Click(Boton("Continuar"));
                if (prueba.Estado == Fase.Transicion && !transicionProbada)
                {
                    transicionProbada = true;
                    yield return VolverYRetomarDesdeMenu(); Click(Boton("Continuar"));
                }
            }
            CollectionAssert.AreEquivalent(new[] { "S0", "R1A", "R2A", "R3A", "DELIVERY" }, visitados);
            Assert.That(transicionProbada, Is.True);
            Assert.That(Object.FindAnyObjectByType<PruebaIntegracionV2>().Estado, Is.EqualTo(Fase.Entregada));
            yield return VolverYRetomarDesdeMenu();
            Assert.That(piloto.Juego.BaseDatos.Eventos.Count(e => e.tipo == TipoEvento.PartidaFinalizada), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator NavegacionNuevaOperacionTrasResultadoSinPerderAnterior([ValueSource(nameof(ResolucionesApariencia))] int resolucion)
        {
            Resolucion(resolucion == 1920); Activar(); yield return Registrar(7);
            yield return Terminar(Object.FindAnyObjectByType<PruebaIntegracionV2>());
            var anterior = JsonUtility.ToJson(piloto.Juego.BaseDatos.Partidas.Single());
            var decisiones = piloto.Juego.BaseDatos.Decisiones.Select(d => JsonUtility.ToJson(d)).ToArray();
            Click(Boton("Nueva prueba")); yield return Pantalla("Resultado");
            var resultado = Object.FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
            var nueva = resultado.selectores[2];
            Assert.That(nueva.gameObject.activeInHierarchy && nueva.interactable, Is.True);
            var etiqueta = nueva.GetComponentInChildren<TMP_Text>(); etiqueta.ForceMeshUpdate(); Assert.That(etiqueta.isTextTruncated, Is.False);
            yield return CapturaNavegacion("Resultado-nueva-operacion");
            nueva.onClick.Invoke(); nueva.onClick.Invoke(); // Repeated callback must also be harmless.
            yield return Pantalla("PruebaIntegracionV2");
            var partidas = piloto.Juego.BaseDatos.Partidas.ToArray();
            Assert.That(partidas.Length, Is.EqualTo(2));
            Assert.That(JsonUtility.ToJson(partidas[0]), Is.EqualTo(anterior));
            Assert.That(piloto.Juego.BaseDatos.Decisiones.Select(d => JsonUtility.ToJson(d)), Is.EqualTo(decisiones));
            Assert.That(partidas[1].estado, Is.EqualTo(EstadoPartida.EnCurso)); Assert.That(partidas[1].puntos, Is.Zero);
            Assert.That(partidas[1].contenido_version, Is.EqualTo(2));
            Assert.That(piloto.Juego.BaseDatos.Partidas.Count(p => p.estado == EstadoPartida.EnCurso), Is.EqualTo(1));
            var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            Assert.That(prueba.MapaVisible, Is.EqualTo("S0")); Assert.That(prueba.Gamificacion.Puntos, Is.Zero);
            Assert.That(piloto.AparienciaPartidaActual.personaje_id, Is.EqualTo(3)); Assert.That(piloto.AparienciaPartidaActual.presentacion, Is.EqualTo("mujer"));
            yield return Terminar(prueba);
            Assert.That(JsonUtility.ToJson(piloto.Juego.BaseDatos.Partidas.First()), Is.EqualTo(anterior));
            Assert.That(piloto.Juego.BaseDatos.Eventos.Count(e => e.tipo == TipoEvento.PartidaFinalizada), Is.EqualTo(2));
            Click(Boton("Nueva prueba")); yield return Pantalla("Resultado");
            Click(Object.FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include).volver);
            yield return Pantalla("Inicio"); Assert.That(piloto.Juego.Usuarios.Actual.Rol, Is.EqualTo(Rol.Ninguno));
        }

        [UnityTest]
        public IEnumerator NavegacionV1ActivaExplicaLimiteSinReemplazarPartida()
        {
            Activar(); yield return Pantalla("Inicio");
            var jugador = piloto.Juego.Usuarios.RegistrarJugador("Legado de prueba", "99003801", 2);
            var partida = piloto.Juego.Partidas.IniciarPartida(); var guardada = JsonUtility.ToJson(partida);
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar); yield return Pantalla("Registro");
            var registro = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
            registro.nombre.text = jugador.nombre; registro.documento.text = jugador.cedula;
            Assert.That(registro.mensaje.text, Does.Contain("flujo original"));
            Click(Boton("Ficha piloto 4"));
            Assert.That(registro.mensaje.text, Does.Contain("flujo original"));
            Click(registro.comprobar);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Registro"));
            Assert.That(registro.mensaje.text, Does.Contain("flujo original"));
            Assert.That(piloto.Juego.BaseDatos.Partidas.Count, Is.EqualTo(1));
            Assert.That(JsonUtility.ToJson(piloto.Juego.BaseDatos.Partidas.Single()), Is.EqualTo(guardada));
            Assert.That(piloto.Juego.BaseDatos.Jugadores.Single().personaje_id, Is.EqualTo(2));
            Assert.That(piloto.Apariencias.LeerPerfilActual(), Is.Null);
            Click(registro.volver); yield return Pantalla("Inicio");
        }

        [UnityTest]
        public IEnumerator NavegacionV1FinalizadaPermiteV2SinModificarHistorico()
        {
            Activar(); yield return Pantalla("Inicio");
            var jugador = piloto.Juego.Usuarios.RegistrarJugador("Legado finalizado", "99003802", 2);
            var partida = piloto.Juego.Partidas.IniciarPartida();
            for (int paso = 0; paso < 32 && partida.estado != EstadoPartida.Finalizada; paso++)
            {
                var situacion = piloto.Juego.Partidas.SituacionActual(partida);
                piloto.Juego.Partidas.Decidir(partida.id, situacion.id, situacion.alternativas[0].id);
                partida = piloto.Juego.BaseDatos.Partidas.Single();
            }
            Assert.That(partida.estado, Is.EqualTo(EstadoPartida.Finalizada));
            var historico = JsonUtility.ToJson(partida);
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar); yield return Pantalla("Registro");
            var registro = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
            registro.nombre.text = jugador.nombre; registro.documento.text = jugador.cedula;
            Assert.That(registro.comprobar.GetComponentInChildren<TMP_Text>().text, Is.EqualTo("COMENZAR RECORRIDO"));
            Click(Boton("Ficha piloto 4")); Click(registro.comprobar); yield return Pantalla("PruebaIntegracionV2");
            Assert.That(piloto.Juego.BaseDatos.Partidas.Count, Is.EqualTo(2));
            Assert.That(JsonUtility.ToJson(piloto.Juego.BaseDatos.Partidas.First()), Is.EqualTo(historico));
            Assert.That(piloto.Juego.BaseDatos.Partidas.Last().contenido_version, Is.EqualTo(2));
            Assert.That(Object.FindAnyObjectByType<PruebaIntegracionV2>().MapaVisible, Is.EqualTo("S0"));
            Assert.That(piloto.Juego.BaseDatos.Jugadores.Single().personaje_id, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator NavegacionCancelarMenuRespetaPausaManual()
        {
            Activar(); yield return Registrar(7);
            var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            yield return Esperar(prueba, Fase.Situacion); Click(Boton("Respuesta 1")); Click(Boton("Continuar"));
            Assert.That(prueba.Estado, Is.EqualTo(Fase.EnMarcha)); Click(Boton("Configuración")); Click(Boton("Pausa"));
            Assert.That(prueba.Pausada, Is.True);
            int eventos = piloto.Juego.BaseDatos.Eventos.Count;
            float posicion = prueba.ProgresoMapa;
            Click(Boton("Menú del jugador")); yield return new WaitForSecondsRealtime(.15f);
            Click(Boton("Seguir en la operación")); yield return new WaitForSecondsRealtime(.15f);
            Assert.That(prueba.Pausada, Is.True); Assert.That(prueba.ProgresoMapa, Is.EqualTo(posicion));
            Assert.That(piloto.Juego.BaseDatos.Eventos.Count, Is.EqualTo(eventos));
            Click(Boton("Configuración")); Click(Boton("Pausa")); Assert.That(prueba.Pausada, Is.False);
        }
    }
}
#endif
