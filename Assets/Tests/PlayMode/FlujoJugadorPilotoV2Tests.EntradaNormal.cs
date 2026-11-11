#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.IntegracionV2;
using LifeDrive.Mapas;
using LifeDrive.Mapas.Demo;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Fase = LifeDrive.IntegracionV2.PruebaIntegracionV2.Fase;

namespace LifeDrive.Tests
{
    public partial class FlujoJugadorPilotoV2Tests
    {
        static readonly int[] OperacionesV1 = {0,1,2,3,4,5};
        [UnityTearDown] public IEnumerator LimpiarEntrada()
        {
            EntradaJuego.CarpetaPruebas=null;
            EntradaJuego.ClasicoEnPruebas = false;
            yield return null;
        }
        IEnumerator AbrirInicioSinLanzador(int resolucion, bool clasico = false)
        {
            Resolucion(resolucion==1920);
            if (clasico) EntradaJuego.ClasicoEnPruebas = true;
            carpeta = Path.Combine(Application.temporaryCachePath,"LifeDrive-EntradaNormalTests",sesion);
            EntradaJuego.CarpetaPruebas = carpeta;
            Cargar("Inicio"); yield return Pantalla("Inicio");
            piloto=Object.FindAnyObjectByType<FlujoJugadorPilotoV2>();
            Assert.That(piloto,Is.Not.Null,"El Inicio normal debe instalar su flujo sin un lanzador del Editor.");
            Assert.That(piloto.EsPilotoV2,Is.EqualTo(!clasico));
            Assert.That(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().mensaje.text,
                Is.EqualTo(clasico ? "Recorrido clásico activo." : "Piloto de carreteras activo."));
            Assert.That(Path.GetFullPath(piloto.CarpetaDatos),Is.EqualTo(Path.GetFullPath(carpeta)));
            Assert.That(Object.FindObjectsByType<TMP_Text>().Any(t=>t.text.Contains("SIN CONEXIÓN AL MOTOR")),Is.False);
            Assert.That(Object.FindObjectsByType<Button>().Length, Is.EqualTo(2));
        }
        IEnumerator RegistroEntrada()
        {
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar); yield return Pantalla("Registro");
            var v=Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
            Assert.That(v.comprobar.GetComponentInChildren<TMP_Text>().text,Is.EqualTo("COMENZAR RECORRIDO"));
            v.nombre.text=""; v.documento.text="abc"; Click(v.comprobar); yield return null;
            Assert.That(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,Is.EqualTo("Registro"));
            Assert.That(piloto.Juego.BaseDatos.Jugadores,Is.Empty);
            Assert.That(piloto.Juego.BaseDatos.Partidas,Is.Empty);
            v.nombre.text="Jugador de ensayo";v.documento.text="99004001";
            Click(v.estilos[1]);Click(v.piel[2]);Click(v.ojos[3]);Click(v.cabello[4]);Click(v.traje[2]);
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)v.comprobar.transform;
            var puntero = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
            };
            var impactos = new List<RaycastResult>();
            EventSystem.current.RaycastAll(puntero, impactos);
            Assert.That(impactos.First().gameObject.GetComponentInParent<Button>(), Is.SameAs(v.comprobar));
            yield return CapturaEntrada("Registro");
            Click(v.comprobar);Click(v.comprobar);yield return Pantalla("PruebaIntegracionV2");
        }
        IEnumerator CapturaEntrada(string nombre, string carpetaEvidencia = "PulidoJugador")
        {
            yield return new WaitForEndOfFrame();
            Assert.That(Screen.width,Is.EqualTo(ancho));Assert.That(Screen.height,Is.EqualTo(alto));
            var imagen=ScreenCapture.CaptureScreenshotAsTexture();
            var ruta=Path.GetFullPath(Path.Combine(Application.dataPath,"../TestEvidence",carpetaEvidencia));Directory.CreateDirectory(ruta);
            File.WriteAllBytes(Path.Combine(ruta,nombre+"-"+ancho+"x"+alto+".png"),imagen.EncodeToPNG());Object.Destroy(imagen);
        }
        [UnityTest] public IEnumerator InicioNormalV1CompletaTresRutas([ValueSource(nameof(OperacionesV1))] int caso)
        {
            yield return AbrirInicioSinLanzador(caso<3?1920:1280, clasico: true);yield return CapturaEntrada("Inicio-V1");
            Time.timeScale = 1;
            yield return RegistroEntrada();
            Assert.That(piloto.Juego.Usuarios.Actual.Nombre,Is.EqualTo("Jugador de ensayo"));
            Assert.That(piloto.Juego.BaseDatos.Jugadores.Single().personaje_id,Is.EqualTo(1));
            Assert.That(piloto.Juego.BaseDatos.Partidas.Single().contenido_version,Is.EqualTo(1));
            // V1 también guarda la apariencia en el modelo: perfil del jugador y copia fijada en la partida.
            Assert.That(piloto.Juego.BaseDatos.Jugadores.Single().apariencia,Is.Not.Empty);
            Assert.That(piloto.Juego.BaseDatos.Partidas.Single().apariencia,Is.EqualTo(piloto.Juego.BaseDatos.Jugadores.Single().apariencia));
            var prueba=Object.FindAnyObjectByType<PruebaIntegracionV2>();int respuestas=0;int rama=caso%3;
            var volante = prueba.GetComponentInChildren<VolantePixelUI>();
            Assert.That(volante, Is.Not.Null);
            var orientacion = volante.transform.localRotation;
            // Two samples on opposite sides of the sinusoid's peak can coincide.
            float limiteObservacion = Time.realtimeSinceStartup + .35f;
            float giro = 0f;
            do
            {
                yield return null;
                giro = Quaternion.Angle(orientacion, volante.transform.localRotation);
            }
            while (giro <= .01f && Time.realtimeSinceStartup < limiteObservacion);
            Assert.That(giro, Is.GreaterThan(.01f));
            Assert.That(Boton("Iniciar V2").gameObject.activeInHierarchy, Is.False);
            Assert.That(Boton("Reinicio visual").gameObject.activeInHierarchy, Is.False);
            yield return CapturaEntrada("Espera-V1");
            Time.timeScale = 28;
            while(prueba.Estado!=Fase.Entregada)
            {
                yield return Esperar(prueba,Fase.Situacion);
                int id=prueba.SituacionVisibleId;
                int indice=id==2?(rama==2?1:0):id==4?(rama==1?2:1):0;
                Click(Boton("Respuesta "+(indice+1)));Click(Boton("Respuesta "+(indice+1)));respuestas++;
                Assert.That(piloto.Juego.BaseDatos.Decisiones.Count,Is.EqualTo(respuestas));
                if(respuestas==1)
                {
                    var p=JsonUtility.ToJson(piloto.Juego.BaseDatos.Partidas.Single());
                    Click(Boton("Menú del jugador"));Click(Boton("Confirmar Inicio"));yield return Pantalla("Inicio");
                    Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar);yield return Pantalla("Registro");
                    var v=Object.FindAnyObjectByType<PersonalizacionRegistroUI>();v.nombre.text="Jugador de ensayo";v.documento.text="99004001";
                    Click(Boton("Ficha piloto 4"));Click(v.comprobar);yield return Pantalla("PruebaIntegracionV2");
                    prueba=Object.FindAnyObjectByType<PruebaIntegracionV2>();
                    Assert.That(prueba.Estado,Is.EqualTo(Fase.Consecuencia));
                    Assert.That(JsonUtility.ToJson(piloto.Juego.BaseDatos.Partidas.Single()),Is.EqualTo(p));
                }
                Click(Boton("Continuar"));
                if(piloto.Juego.BaseDatos.Partidas.Single().estado==EstadoPartida.Finalizada)yield return Esperar(prueba,Fase.Entregada);
            }
            Assert.That(prueba.MapaVisible,Is.EqualTo("DELIVERY"));
            var entrega = prueba.GetComponentInChildren<AvatarPersonalizableUI>();
            Assert.That(entrega, Is.Not.Null);
            Assert.That(entrega.mujer, Is.True);
            Assert.That(prueba.GetComponentsInChildren<RectTransform>().Any(t => t.name == "Paquete de envío"), Is.True);
            Assert.That(prueba.GetComponentsInChildren<RectTransform>().Any(t => t.name == "Confirmación de entrega"), Is.True);
            var coloresEntrega = (entrega.piel, entrega.ojos, entrega.cabello, entrega.traje);
            var datos=piloto.Juego.BaseDatos;Assert.That(datos.Partidas.Single().puntos,Is.EqualTo(datos.Decisiones.Sum(d=>d.puntos_aplicados)));
            Assert.That(datos.Decisiones.All(d=>d.partida_id==datos.Partidas.Single().id),Is.True);
            yield return CapturaEntrada("Entrega-V1");Click(Boton("Nueva prueba"));yield return Pantalla("Resultado");
            var resultado=Object.FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
            Assert.That(resultado.puntos.text,Is.EqualTo(datos.Partidas.Single().puntos+" pts de seguridad"));
            Assert.That(resultado.personaje.text,Does.Contain("Ficha 1"));
            var retrato = resultado.retrato.GetComponentInChildren<AvatarPersonalizableUI>();
            Assert.That(retrato.mujer,Is.True);
            Assert.That((retrato.piel, retrato.ojos, retrato.cabello, retrato.traje), Is.EqualTo(coloresEntrega));
            Assert.That(resultado.selectores.Take(2).All(b => !b.gameObject.activeInHierarchy), Is.True);
            var etiquetaMetricas = resultado.revisar.GetComponentInChildren<TMP_Text>();
            Assert.That(resultado.revisar.targetGraphic, Is.TypeOf<FormaRedondeadaUI>());
            Assert.That(TecladoPilotoV2.Contraste(etiquetaMetricas.color, resultado.revisar.targetGraphic.color), Is.GreaterThan(4.5f));
            yield return CapturaEntrada("Resultado-V1");
            Click(resultado.revisar);
            Assert.That(resultado.revisar.GetComponentInChildren<TMP_Text>().text, Is.EqualTo("Ocultar métricas"));
            Assert.That(resultado.filasDecisiones[0].text, Is.EqualTo(resultado.puntos.text));
            Click(resultado.selectores[1]);
            Assert.That(resultado.filasDecisiones[0].text, Does.EndWith(" min"));
            yield return CapturaEntrada("Metricas-V1");
            Click(resultado.selectores[0]);
            Assert.That(resultado.filasDecisiones[0].text, Is.EqualTo(resultado.puntos.text));
            Click(resultado.revisar);
            Assert.That(resultado.revisar.GetComponentInChildren<TMP_Text>().text, Is.EqualTo("Ver métricas"));
            Assert.That(resultado.panelDecisiones.activeSelf, Is.False);
            Click(resultado.volver);yield return Pantalla("Inicio");
        }
        [UnityTest] public IEnumerator InicioNormalDeDesarrolloOchoRutasSinSelector([ValueSource(nameof(Rutas))] int ruta)
        {
            yield return AbrirInicioSinLanzador(ruta % 2 == 0 ? 1920 : 1280);
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar);
            yield return Pantalla("Registro");
            var v=Object.FindAnyObjectByType<PersonalizacionRegistroUI>();v.nombre.text="Ensayo candidato";v.documento.text="99004002";
            Assert.That(v.comprobar.GetComponentInChildren<TMP_Text>().text, Is.EqualTo("COMENZAR RECORRIDO"));
            Click(Boton("Ficha piloto 3"));Click(v.estilos[1]);Click(v.cabello[3]);Click(v.comprobar);
            yield return Pantalla("PruebaIntegracionV2");
            var jugador = piloto.Juego.BaseDatos.Jugadores.Single();
            Assert.That((jugador.nombre, jugador.cedula, jugador.personaje_id), Is.EqualTo(("Ensayo candidato", "99004002", 3)));
            Assert.That(piloto.Juego.BaseDatos.Partidas.Single().contenido_version, Is.EqualTo(2));
            var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            var reproductor = prueba.GetComponent<DemoRecorrido>();
            var fondos = new HashSet<Texture2D>();
            string[] mapas = { "S0", ruta < 4 ? "R1A" : "R1B", "R2" + (char)('A' + ruta / 2), Hojas[ruta], "DELIVERY" };
            int[][] situaciones =
            {
                new[] { 1, 101 }, new[] { ruta < 4 ? 102 : 103 },
                ruta < 2 ? new[] { 3, 104 } : new[] { ruta < 4 ? 105 : ruta < 6 ? 5 : 106 },
                new[] { 6 }, new[] { 7 }
            };
            int partidaId = piloto.PartidaId;
            int total = 0;
            var vistas = new HashSet<int>();
            for (int tramo = 0; tramo < mapas.Length; tramo++)
            {
                for (int turno = 0; turno < situaciones[tramo].Length; turno++)
                {
                    yield return Esperar(prueba, Fase.Situacion);
                    Assert.That(prueba.MapaVisible, Is.EqualTo(mapas[tramo]));
                    if (turno == 0)
                    {
                        Assert.That(reproductor.MapaActual.fondo, Is.Not.Null);
                        Assert.That(fondos.Add(reproductor.MapaActual.fondo), Is.True,
                            $"El nuevo mapa debe cargar otro fondo. Esperado {mapas[tramo]}, MapaActual {reproductor.MapaActual.Id} (fondo {reproductor.MapaActual.fondo.name}), ya vistos {fondos.Count}.");
                    }
                    int id = prueba.SituacionVisibleId;
                    Assert.That(id, Is.EqualTo(situaciones[tramo][turno]));
                    Assert.That(vistas.Add(id), Is.True, "Una situación ya respondida volvió a aparecer.");
                    if (turno == 0 && (ruta == 0 || ruta == 7))
                        yield return CapturaEntrada(Hojas[ruta] + "-" + mapas[tramo], "ProgresionVisualV2");

                    string destino = tramo < 4 && turno == situaciones[tramo].Length - 1 ? mapas[tramo + 1] : null;
                    int indice = destino != null && tramo < 3 ? (ruta >> (2 - tramo)) & 1 : 0;
                    var alternativa = piloto.Juego.Partidas.SituacionActual(piloto.Juego.BaseDatos.Partidas.Single()).alternativas[indice];
                    bool bifurcacion = destino != null && tramo < 3;
                    if (bifurcacion)
                    {
                        Assert.That(reproductor.BifurcacionVisible, Is.True, "Se deben ver los dos caminos antes de elegir.");
                        var opciones = piloto.Juego.RecorridosV2.CaminosDisponibles(partidaId);
                        Assert.That(opciones.Length, Is.EqualTo(2));
                        Assert.That(opciones.Single(o => o.Numero == indice + 1).Destino, Is.EqualTo(destino));
                        if (ruta == 0 || ruta == 7)
                            yield return CapturaEntrada(Hojas[ruta] + "-" + mapas[tramo] + "-bifurcacion", "BifurcacionesNicolas");
                    }
                    var boton = Boton("Respuesta " + (indice + 1));
                    Click(boton);
                    boton.onClick.Invoke(); // Incluso un callback repetido debe ser inocuo.
                    var disco = new BaseDatos(carpeta);
                    Assert.That(disco.Decisiones.Count, Is.EqualTo(++total));
                    var decision = disco.Decisiones.Last();
                    Assert.That(decision.alternativa_id, Is.EqualTo(alternativa.id));
                    Assert.That(decision.tramo_destino_id, Is.EqualTo(alternativa.tramo_destino_id == 0
                        ? decision.tramo_origen_id : alternativa.tramo_destino_id));
                    Assert.That(disco.Partidas.Single().puntos, Is.EqualTo(disco.Decisiones.Sum(d => d.puntos_aplicados)));
                    var estado = piloto.Juego.RecorridosV2.Consultar(partidaId);
                    Assert.That(estado.DestinoVisualConfirmado, Is.EqualTo(destino));
                    Assert.That(estado.PresentacionPendiente, Is.True);
                    Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
                    Assert.That(prueba.MapaVisible, Is.EqualTo(mapas[tramo]));

                    Click(Boton("Continuar"));
                    if (destino == null)
                    {
                        Assert.That(prueba.Estado, Is.Not.EqualTo(Fase.Transicion));
                        Assert.That(prueba.MapaVisible, Is.EqualTo(mapas[tramo]));
                        continue;
                    }
                    Assert.That(prueba.Estado, Is.EqualTo(Fase.Transicion));
                    Assert.That(prueba.MapaVisible, Is.EqualTo(mapas[tramo]));
                    if (bifurcacion)
                    {
                        Assert.That(reproductor.UsandoDesvio, Is.EqualTo(!alternativa.es_acertada), "El camión debe circular por el ramal elegido.");
                        if (ruta == 0 || ruta == 7)
                        {
                            // Observe actual movement on the road, not a teleported preview.
                            float salida = reproductor.Distancia;
                            float limiteMovimiento = Time.realtimeSinceStartup + 4;
                            while (reproductor.Distancia < salida + 45 && prueba.MapaVisible == mapas[tramo] && Time.realtimeSinceStartup < limiteMovimiento)
                                yield return null;
                            Assert.That(prueba.MapaVisible, Is.EqualTo(mapas[tramo]));
                            Assert.That(reproductor.Distancia, Is.GreaterThan(salida + 40));
                            yield return CapturaEntrada(Hojas[ruta] + "-" + mapas[tramo] + "-camino-elegido", "BifurcacionesNicolas");
                        }
                    }
                    yield return EsperarEntradaConFundido(prueba, destino);
                    if (tramo == 0 && (ruta == 0 || ruta == 7))
                    {
                        yield return ReabrirDesdeInicioDuranteFundido(estado, disco);
                        prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
                        reproductor = prueba.GetComponent<DemoRecorrido>(); // la escena se reabrió: el anterior quedó obsoleto
                        Click(Boton("Continuar"));
                        yield return EsperarEntradaConFundido(prueba, destino);
                    }
                }
            }
            yield return Esperar(prueba, Fase.Entregada);
            var cierre = piloto.Juego.RecorridosV2.Consultar(partidaId);
            Assert.That(cierre.Historial, Is.EqualTo(mapas));
            Assert.That(cierre.CierrePresentado, Is.True);
            Assert.That(prueba.MapaVisible, Is.EqualTo("DELIVERY"));
            Assert.That(piloto.Juego.BaseDatos.Decisiones.Count, Is.EqualTo(vistas.Count));
            Assert.That(piloto.Juego.BaseDatos.Eventos.Count(e => e.tipo == TipoEvento.PuntosActualizados), Is.EqualTo(total));
            Assert.That(piloto.Juego.BaseDatos.Partidas.Single().contenido_version,Is.EqualTo(2));
            Assert.That(piloto.Apariencias.LeerPartida(piloto.Juego.BaseDatos.Partidas.Single().id),Is.Not.Null);
            var entrega = prueba.GetComponentInChildren<AvatarPersonalizableUI>();
            Assert.That(entrega.mujer, Is.True);
            Color cabello = entrega.cabello;
            Click(Boton("Nueva prueba"));yield return Pantalla("Resultado");
            var resultado=Object.FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
            Assert.That(resultado.educativo.text,Does.Contain(Hojas[ruta]));
            Assert.That(resultado.retrato.GetComponentInChildren<AvatarPersonalizableUI>().cabello, Is.EqualTo(cabello));
            Click(resultado.volver);
            yield return Pantalla("Inicio");
        }

        static IEnumerator EsperarEntradaConFundido(PruebaIntegracionV2 prueba, string destino)
        {
            var reproductor = prueba.GetComponent<DemoRecorrido>();
            float limite = Time.realtimeSinceStartup + 12;
            while (prueba.MapaVisible != destino)
            {
                Assert.That(prueba.Estado, Is.EqualTo(Fase.Transicion));
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(limite), destino);
                yield return null;
            }
            Assert.That(reproductor.EnTransicion, Is.True);
            Assert.That(reproductor.OpacidadFundido, Is.EqualTo(1).Within(.001f));
            Assert.That(prueba.ProgresoMapa, Is.Zero, "El nuevo mapa comienza en su entrada.");
        }

        IEnumerator ReabrirDesdeInicioDuranteFundido(EstadoRecorridoV2 estado, BaseDatos guardada)
        {
            string apariencia = JsonUtility.ToJson(piloto.AparienciaPartidaActual);
            Click(Boton("Menú del jugador"));
            Click(Boton("Confirmar Inicio"));
            yield return Pantalla("Inicio");
            Object.Destroy(piloto.gameObject);
            yield return null;
            Cargar("Inicio");
            yield return Pantalla("Inicio");
            piloto = Object.FindAnyObjectByType<FlujoJugadorPilotoV2>();
            Assert.That(piloto.EsPilotoV2, Is.True);
            Click(Object.FindAnyObjectByType<PersonalizacionRegistroUI>().jugar);
            yield return Pantalla("Registro");
            var registro = Object.FindAnyObjectByType<PersonalizacionRegistroUI>();
            registro.nombre.text = "Ensayo candidato";
            registro.documento.text = "99004002";
            Click(registro.comprobar);
            yield return Pantalla("PruebaIntegracionV2");
            var prueba = Object.FindAnyObjectByType<PruebaIntegracionV2>();
            Assert.That(prueba.MapaVisible, Is.EqualTo(estado.MapaActual));
            Assert.That(prueba.Estado, Is.EqualTo(Fase.Consecuencia));
            Assert.That(JsonUtility.ToJson(piloto.Juego.BaseDatos.Partidas.Single()), Is.EqualTo(JsonUtility.ToJson(guardada.Partidas.Single())));
            Assert.That(piloto.Juego.BaseDatos.Decisiones.Select(d => d.id), Is.EqualTo(guardada.Decisiones.Select(d => d.id)));
            Assert.That(JsonUtility.ToJson(piloto.AparienciaPartidaActual), Is.EqualTo(apariencia));
        }
    }
}
#endif
