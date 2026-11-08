using System;
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Mapas;
using LifeDrive.Motor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LifeDrive.Tests
{
    public class PaqueteRecorridosTests
    {
        static readonly string Carpeta = Path.Combine(Application.streamingAssetsPath, "Contenido");
        PaqueteRecorridos paquete;
        CatalogoMapas catalogo;

        [SetUp]
        public void Cargar()
        {
            paquete = PaqueteRecorridos.Cargar(Carpeta, 2);
            catalogo = AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset");
        }

        [Test]
        public void ValidaSieteBifurcacionesConDosRespuestasExplicitas()
        {
            Assert.DoesNotThrow(() => paquete.Validar(catalogo));
            CollectionAssert.AreEquivalent(new[] { "S0", "R1A", "R1B", "R2A", "R2B", "R2C", "R2D" },
                paquete.asociaciones.Select(a => a.mapa_origen).Distinct());
            Assert.That(paquete.asociaciones.Length, Is.EqualTo(14));
            Assert.That(paquete.contenido.situaciones.All(s => !s.validada && string.IsNullOrEmpty(s.fecha_validacion)));
        }

        [TestCase("situacion")]
        [TestCase("alternativa")]
        [TestCase("mapa")]
        [TestCase("salto")]
        [TestCase("duplicado")]
        [TestCase("version")]
        [TestCase("destinoLogico")]
        [TestCase("validada")]
        public void RechazaContratosInvalidos(string defecto)
        {
            var fila = paquete.asociaciones.First(a => a.mapa_origen == "S0" && a.mapa_destino == "R1A");
            var situacion = paquete.contenido.situaciones.Single(s => s.id == fila.situacion_id);
            switch (defecto)
            {
                case "situacion": fila.situacion_id = 999; break;
                case "alternativa": fila.alternativa_id = 999; break;
                case "mapa": fila.mapa_destino = "INEXISTENTE"; break;
                case "salto": fila.mapa_destino = "DELIVERY"; break;
                case "duplicado":
                    fila.alternativa_id = paquete.asociaciones.Single(a => a.mapa_destino == "R1B").alternativa_id;
                    break;
                case "version": fila.contenido_version = 1; break;
                case "destinoLogico":
                    situacion.alternativas.Single(a => a.id == fila.alternativa_id).tramo_destino_id = situacion.tramo_id;
                    break;
                case "validada": situacion.validada = true; break;
            }

            Assert.Throws<InvalidDataException>(() => paquete.Validar(catalogo));
        }

        [TestCase("R1A", "R2A", "R3A", 101, 103, 107)]
        [TestCase("R1A", "R2A", "R3B", 101, 103, 108)]
        [TestCase("R1A", "R2B", "R3C", 101, 104, 109)]
        [TestCase("R1A", "R2B", "R3D", 101, 104, 110)]
        [TestCase("R1B", "R2C", "R3E", 102, 105, 12)]
        [TestCase("R1B", "R2C", "R3F", 102, 105, 13)]
        [TestCase("R1B", "R2D", "R3G", 102, 106, 111)]
        [TestCase("R1B", "R2D", "R3H", 102, 106, 112)]
        public void JuegaHastaDelivery(string r1, string r2, string r3, int primera, int segunda, int tercera)
        {
            paquete.Validar(catalogo);
            var carpetaDatos = Path.Combine(Path.GetTempPath(), "lifedrive-v2-prueba-" + Guid.NewGuid());
            try
            {
                var db = new BaseDatos(carpetaDatos);
                var eventos = new RegistroEventos(db);
                var usuarios = new ServicioUsuarios(db, paquete.contenido, new ListaAdministradores(), eventos);
                var partidas = new ServicioPartidas(db, paquete.contenido, usuarios, eventos);
                usuarios.RegistrarJugador("Jugador de prueba", "123456789", 1);
                var partida = partidas.IniciarPartida();
                var cursor = new RecorridoVisualValidado(catalogo);
                var elecciones = new[] { primera, segunda, tercera };
                var paso = 0;
                var huboEmergencia = false;
                var huboDescargue = false;

                for (var i = 0; i < paquete.contenido.situaciones.Length; i++)
                {
                    var situacion = partidas.SituacionActual(partida);
                    if (situacion == null) break;
                    var filas = paquete.asociaciones.Where(a =>
                        a.mapa_origen == cursor.Actual && a.situacion_id == situacion.id).ToArray();
                    var fila = filas.Length == 0 ? null : filas.Single(a => a.alternativa_id == elecciones[paso]);
                    var alternativa = fila == null
                        ? situacion.alternativas.First(a => a.es_acertada)
                        : situacion.alternativas.Single(a => a.id == fila.alternativa_id);

                    if (situacion.id == 6)
                    {
                        Assert.That(cursor.Actual, Is.EqualTo(r3));
                        huboEmergencia = true;
                    }
                    if (situacion.id == 7)
                    {
                        Assert.That(cursor.Actual, Is.EqualTo("DELIVERY"));
                        huboDescargue = true;
                    }

                    var resultado = partidas.Decidir(partida.id, situacion.id, alternativa.id);
                    partida = resultado.Partida;
                    var decision = resultado.Decision;
                    Assert.That(decision.tipo, Is.EqualTo(situacion.tipo));
                    Assert.That(decision.alternativa_id, Is.EqualTo(alternativa.id));
                    Assert.That(decision.es_acertada, Is.EqualTo(alternativa.es_acertada));
                    Assert.That(decision.es_buena_practica, Is.EqualTo(alternativa.es_buena_practica));
                    Assert.That(decision.puntos_aplicados, Is.EqualTo(alternativa.puntos));
                    Assert.That(decision.retraso_aplicado, Is.EqualTo(alternativa.retraso_min));
                    Assert.That(decision.consecuencia, Is.EqualTo(alternativa.explicacion));

                    if (fila != null)
                    {
                        Assert.That(decision.tramo_destino_id,
                            Is.EqualTo(paquete.nodos.Single(n => n.mapa_id == fila.mapa_destino).tramo_id));
                        cursor.AvanzarConfirmado(fila.mapa_destino);
                        paso++;
                    }
                    if (situacion.id == 6) cursor.AvanzarConfirmado("DELIVERY");
                }

                Assert.That(paso, Is.EqualTo(3));
                Assert.That(huboEmergencia && huboDescargue && cursor.Entregado);
                CollectionAssert.AreEqual(new[] { "S0", r1, r2, r3, "DELIVERY" }, cursor.Historial);
                Assert.That(partida.estado, Is.EqualTo(EstadoPartida.Finalizada));
                var guardada = new BaseDatos(carpetaDatos);
                var decisiones = guardada.Decisiones.Where(d => d.partida_id == partida.id).ToArray();
                Assert.That(guardada.Partidas.Single().estado, Is.EqualTo(EstadoPartida.Finalizada));
                Assert.That(partida.puntos, Is.EqualTo(decisiones.Sum(d => d.puntos_aplicados)));
                CollectionAssert.AreEqual(elecciones,
                    decisiones.Where(d => d.tramo_origen_id != d.tramo_destino_id).Select(d => d.alternativa_id));
            }
            finally
            {
                if (Directory.Exists(carpetaDatos)) Directory.Delete(carpetaDatos, true);
            }
        }

        [Test]
        public void CargarCandidatoNoCambiaLaInterpretacionDeV1()
        {
            var rutaActiva = Path.Combine(Carpeta, "ruta.json");
            var original = File.ReadAllBytes(rutaActiva);
            var activo = new Almacen(Carpeta).CargarContenido();
            var legado = PaqueteRecorridos.Cargar(Carpeta, 0);
            var v1 = PaqueteRecorridos.Cargar(Carpeta, 1);
            Assert.That(legado.contenido_version, Is.EqualTo(1));
            Assert.That(v1.contenido_version, Is.EqualTo(1));
            Assert.That(JsonUtility.ToJson(legado.contenido), Is.EqualTo(JsonUtility.ToJson(activo)));
            Assert.That(JsonUtility.ToJson(v1.contenido), Is.EqualTo(JsonUtility.ToJson(activo)));
            Assert.That(paquete.contenido.tramos.All(t => activo.tramos.All(anterior => anterior.id != t.id)));
            Assert.Throws<ArgumentOutOfRangeException>(() => PaqueteRecorridos.Cargar(Carpeta, 99));
            CollectionAssert.AreEqual(original, File.ReadAllBytes(rutaActiva));

            var fatigaOriginal = activo.situaciones.Single(s => s.id == 5);
            var fatigaV2 = paquete.contenido.situaciones.Single(s => s.id == 5);
            CollectionAssert.AreEqual(new[] { 12, 13, 14 }, fatigaOriginal.alternativas.Select(a => a.id));
            CollectionAssert.AreEqual(new[] { 12, 13 }, fatigaV2.alternativas.Select(a => a.id));
            Assert.That(fatigaV2.enunciado, Is.EqualTo(fatigaOriginal.enunciado));
            Assert.That(fatigaV2.tipo, Is.EqualTo(fatigaOriginal.tipo));
            foreach (var alternativa in fatigaV2.alternativas)
            {
                var anterior = fatigaOriginal.alternativas.Single(a => a.id == alternativa.id);
                anterior.tramo_destino_id = alternativa.tramo_destino_id;
                Assert.That(JsonUtility.ToJson(alternativa), Is.EqualTo(JsonUtility.ToJson(anterior)));
            }
        }
    }
}
