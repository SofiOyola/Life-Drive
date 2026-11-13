// Pruebas de los indicadores S3-09 (TDS, IEE, PPS, PCRS): fórmulas con datos fijos,
// casos sin datos, acumulado por jugador, permisos y presencia en el PDF.
using System;
using System.IO;
using System.Linq;
using System.Text;
using LifeDrive.Datos;
using LifeDrive.Motor;
using NUnit.Framework;
using UnityEngine;

namespace LifeDrive.Tests
{
    public class IndicadoresTests
    {
        static readonly string ContenidoReal = Path.Combine(Application.streamingAssetsPath, "Contenido");
        string carpetaDatos, carpetaContenido;

        [SetUp]
        public void Preparar()
        {
            carpetaDatos = Path.Combine(Path.GetTempPath(), "lifedrive-kpi-" + Guid.NewGuid());
            carpetaContenido = Path.Combine(carpetaDatos, "contenido");
            Directory.CreateDirectory(carpetaContenido);
            File.Copy(Path.Combine(ContenidoReal, "ruta.json"), Path.Combine(carpetaContenido, "ruta.json"));
            var admins = new ListaAdministradores { administradores = new[] { Contrasenas.Crear("admin", "ClaveDePrueba123") } };
            File.WriteAllText(Path.Combine(carpetaContenido, "administradores.json"), JsonUtility.ToJson(admins));
        }

        [TearDown]
        public void Limpiar() => Directory.Delete(carpetaDatos, true);

        ServicioJuego NuevoJuego() => new ServicioJuego(carpetaContenido, carpetaDatos);

        static Partida Jugar(ServicioJuego juego, Func<Alternativa[], Alternativa> elegir)
        {
            var p = juego.Partidas.IniciarPartida();
            for (var i = 0; i < 100; i++)
            {
                var s = juego.Partidas.SituacionActual(p);
                if (s == null) return p;
                p = juego.Partidas.Decidir(p.id, s.id, elegir(s.alternativas).id).Partida;
            }
            throw new AssertionException("La partida no termina");
        }

        static Decision D(int partida, string tipo, bool acertada) => new Decision { partida_id = partida, tipo = tipo, es_acertada = acertada };

        static Partida P(int id, string estado, int puntos) => new Partida { id = id, estado = estado, puntos = puntos };

        // ---------- Fórmulas ----------

        [Test]
        public void LasFormulasUsanLosTiposQueIndicaS309()
        {
            var partidas = new[] { P(1, EstadoPartida.Finalizada, 40) };
            var decisiones = new[]
            {
                D(1, TipoSituacion.Pregunta, true), D(1, TipoSituacion.Pregunta, true), D(1, TipoSituacion.Pregunta, false),
                D(1, TipoSituacion.Riesgo, true), D(1, TipoSituacion.Emergencia, false),
                D(1, TipoSituacion.DecisionRuta, true), D(1, TipoSituacion.DecisionRuta, false), D(1, TipoSituacion.DecisionRuta, true),
                D(2, TipoSituacion.Pregunta, false),   // de otra partida: no cuenta
            };
            var k = CalculadoraIndicadores.Calcular(partidas, decisiones);

            Assert.AreEqual((2, 3), (k.IEE.Numerador, k.IEE.Denominador));             // solo preguntas
            Assert.AreEqual((3, 5), (k.TDS.Numerador, k.TDS.Denominador));             // riesgo, emergencia y ruta
            Assert.AreEqual((2, 3), (k.PCRS.Numerador, k.PCRS.Denominador));           // solo rutas
            Assert.AreEqual(60.0, k.TDS.Valor, 1e-9);
            Assert.AreEqual(40.0, k.PPS.Valor, 1e-9);
            Assert.AreEqual("66,7 % (2 de 3)", k.IEE.Texto);
            Assert.AreEqual("40,0 (40 pts en 1 partida)", k.PPS.Texto);
            CollectionAssert.AreEqual(new[] { "TDS", "IEE", "PPS", "PCRS" }, k.Todos.Select(x => x.Sigla));
        }

        [Test]
        public void SinCasosElValorEsCeroYSeIndica()
        {
            var k = CalculadoraIndicadores.Calcular(new[] { P(1, EstadoPartida.EnCurso, 25) }, new Decision[0]);
            foreach (var i in k.Todos)
            {
                Assert.IsTrue(i.SinCasos, i.Sigla);
                Assert.AreEqual(0, i.Valor, i.Sigla);
            }
            Assert.AreEqual("0 % (sin casos)", k.TDS.Texto);
            Assert.AreEqual("0 (sin partidas finalizadas)", k.PPS.Texto);   // la partida en curso no cuenta
        }

        [Test]
        public void PpsPromediaSoloPartidasFinalizadasIncluidosPuntosNegativos()
        {
            var partidas = new[] { P(1, EstadoPartida.Finalizada, 50), P(2, EstadoPartida.Finalizada, -15), P(3, EstadoPartida.EnCurso, 999) };
            var k = CalculadoraIndicadores.Calcular(partidas, new Decision[0]);
            Assert.AreEqual((35, 2), (k.PPS.Numerador, k.PPS.Denominador));
            Assert.AreEqual(17.5, k.PPS.Valor, 1e-9);
            Assert.AreEqual("17,5 (35 pts en 2 partidas)", k.PPS.Texto);
        }

        // ---------- Servicio ----------

        [Test]
        public void ElResumenTraeLosIndicadoresDeLaPartidaYCuadranConSusConteos()
        {
            var juego = NuevoJuego();
            juego.Usuarios.RegistrarJugador("Kpi Partida", "31313131", 1);
            var p = Jugar(juego, a => a[a.Length - 1]);
            var r = juego.Desempeno.Resumen(p.id);   // el propio jugador puede verlos
            var k = r.Indicadores;

            Assert.AreEqual(r.PreguntasCorrectas, k.IEE.Numerador);
            Assert.AreEqual(r.PreguntasCorrectas + r.PreguntasIncorrectas, k.IEE.Denominador);
            Assert.AreEqual(r.DecisionesSeguras, k.TDS.Numerador);
            Assert.AreEqual(r.DecisionesSeguras + r.DecisionesInseguras, k.TDS.Denominador);
            var rutas = juego.BaseDatos.Decisiones.Where(d => d.partida_id == p.id && d.tipo == TipoSituacion.DecisionRuta).ToList();
            Assert.AreEqual((rutas.Count(d => d.es_acertada), rutas.Count), (k.PCRS.Numerador, k.PCRS.Denominador));
            Assert.AreEqual(p.puntos, k.PPS.Numerador);
            Assert.AreEqual(1, k.PPS.Denominador);
        }

        [Test]
        public void ElAcumuladoDelJugadorSumaSusPartidasFinalizadasYNadaMas()
        {
            var juego = NuevoJuego();
            var ana = juego.Usuarios.RegistrarJugador("Ana Kpi", "41414141", 1);
            var p1 = Jugar(juego, a => a[0]);
            var p2 = Jugar(juego, a => a[a.Length - 1]);
            var enCurso = juego.Partidas.IniciarPartida();
            var s = juego.Partidas.SituacionActual(enCurso);
            juego.Partidas.Decidir(enCurso.id, s.id, s.alternativas[0].id);   // decisión de una partida en curso: no cuenta
            juego.Usuarios.RegistrarJugador("Otro Jugador", "51515151", 2);
            Jugar(juego, a => a[0]);                                           // partida de otro jugador: no cuenta

            juego.Usuarios.IniciarSesionAdministrador("admin", "ClaveDePrueba123");
            var k = juego.Desempeno.IndicadoresJugador(ana.id);
            var r1 = juego.Desempeno.Resumen(p1.id).Indicadores;
            var r2 = juego.Desempeno.Resumen(p2.id).Indicadores;

            Assert.AreEqual(2, k.PartidasConsideradas);
            Assert.AreEqual(p1.puntos + p2.puntos, k.PPS.Numerador);
            Assert.AreEqual((p1.puntos + p2.puntos) / 2.0, k.PPS.Valor, 1e-9);
            for (var i = 0; i < k.Todos.Count; i++)
            {
                Assert.AreEqual(r1.Todos[i].Numerador + r2.Todos[i].Numerador, k.Todos[i].Numerador, k.Todos[i].Sigla);
                Assert.AreEqual(r1.Todos[i].Denominador + r2.Todos[i].Denominador, k.Todos[i].Denominador, k.Todos[i].Sigla);
            }
        }

        [Test]
        public void SoloElAdministradorConsultaElAcumulado()
        {
            var juego = NuevoJuego();
            var j = juego.Usuarios.RegistrarJugador("Sin Permiso", "61616161", 1);
            Assert.Throws<ErrorJuego>(() => juego.Desempeno.IndicadoresJugador(j.id));
        }

        [Test]
        public void ElContenidoSinValidarMarcaLosIndicadoresComoProvisionales()
        {
            var juego = NuevoJuego();
            juego.Usuarios.RegistrarJugador("Validacion Kpi", "71717171", 1);
            var p = Jugar(juego, a => a[0]);
            var usadas = juego.BaseDatos.Decisiones.Where(d => d.partida_id == p.id).Select(d => d.situacion_id).ToList();
            var esperado = usadas.Any(id => !juego.Contenido.situaciones.First(x => x.id == id).validada);
            Assert.AreEqual(esperado, juego.Desempeno.Resumen(p.id).Indicadores.ContenidoSinValidar);

            // Con todo el contenido validado deja de ser provisional.
            foreach (var x in juego.Contenido.situaciones) x.validada = true;
            Assert.IsFalse(juego.Desempeno.Resumen(p.id).Indicadores.ContenidoSinValidar);
        }

        [Test]
        public void UnaPartidaV2UsaSuPaqueteYSiElPaqueteCambiaQuedaProvisional()
        {
            File.Copy(Path.Combine(ContenidoReal, "ruta-v2-candidata.json"), Path.Combine(carpetaContenido, "ruta-v2-candidata.json"));
            var catalogo = UnityEditor.AssetDatabase.LoadAssetAtPath<LifeDrive.Mapas.CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset");
            var juego = new ServicioJuego(carpetaContenido, carpetaDatos, catalogo);
            var j = juego.Usuarios.RegistrarJugador("Kpi V2", "91919191", 1);
            var p = juego.RecorridosV2.IniciarPartida();
            for (var i = 0; i < 50; i++)
            {
                var s = juego.Partidas.SituacionActual(p);
                if (s == null) break;
                var resultado = juego.Partidas.Decidir(p.id, s.id, s.alternativas[0].id);
                juego.RecorridosV2.ConfirmarPresentacion(p.id, resultado.Decision.id);   // V2 exige confirmar cada decisión
                p = resultado.Partida;
            }
            Assert.AreEqual(EstadoPartida.Finalizada, p.estado);

            var k = juego.Desempeno.Resumen(p.id).Indicadores;   // situaciones resueltas con el paquete V2
            Assert.AreEqual(juego.BaseDatos.Decisiones.Count(d => d.partida_id == p.id && d.tipo == TipoSituacion.DecisionRuta), k.PCRS.Denominador);
            Assert.IsTrue(k.ContenidoSinValidar);                   // el candidato V2 no está validado

            // Si el paquete instalado ya no es el de la partida, no se puede comprobar la validación: provisional.
            juego.BaseDatos.Partidas.Single(x => x.id == p.id).contenido_huella = "otro-paquete";
            juego.Usuarios.IniciarSesionAdministrador("admin", "ClaveDePrueba123");
            var acumulado = juego.Desempeno.IndicadoresJugador(j.id);
            Assert.AreEqual(1, acumulado.PartidasConsideradas);
            Assert.IsTrue(acumulado.ContenidoSinValidar);
        }

        // ---------- PDF ----------

        [Test]
        public void ElPdfIncluyeLosCuatroIndicadoresDeLaPartidaYDelAcumulado()
        {
            var juego = NuevoJuego();
            juego.Usuarios.RegistrarJugador("Pdf Kpi", "81818181", 1);
            Jugar(juego, a => a[0]);
            var p = Jugar(juego, a => a[a.Length - 1]);
            juego.Usuarios.IniciarSesionAdministrador("admin", "ClaveDePrueba123");

            var texto = Encoding.GetEncoding("iso-8859-1").GetString(juego.Desempeno.ReportePdf(p.id));
            var k = juego.Desempeno.Resumen(p.id).Indicadores;
            // los paréntesis van escapados en el PDF
            StringAssert.Contains("(Indicadores de desempeño \\(S3-09\\)) Tj", texto);
            StringAssert.Contains("Acumulado \\(2 finalizadas\\)", texto);
            foreach (var i in k.Todos) StringAssert.Contains("(" + i.Sigla + " · " + i.Nombre + ") Tj", texto);
        }
    }
}
