// Pruebas de los caminos de error y de borde del motor y de la base de datos que las demás suites no ejercitan:
// reglas de integridad, traducción de errores a mensajes, recuperación del archivo, validación del contenido y PDF.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LifeDrive.Datos;
using LifeDrive.Mapas;
using LifeDrive.Motor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LifeDrive.Tests
{
    public class CoberturaMotorTests
    {
        static readonly string ContenidoReal = Path.Combine(Application.streamingAssetsPath, "Contenido");
        string carpeta, contenido;

        [SetUp]
        public void Preparar()
        {
            carpeta = Path.Combine(Path.GetTempPath(), "lifedrive-cobertura-" + Guid.NewGuid());
            contenido = Path.Combine(carpeta, "contenido");
            Directory.CreateDirectory(contenido);
            File.Copy(Path.Combine(ContenidoReal, "ruta.json"), Path.Combine(contenido, "ruta.json"));
            var admins = new ListaAdministradores { administradores = new[] { Contrasenas.Crear("admin", "ClaveDePrueba123") } };
            File.WriteAllText(Path.Combine(contenido, "administradores.json"), JsonUtility.ToJson(admins));
        }

        [TearDown] public void Limpiar() { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }

        string CarpetaDatos => Path.Combine(carpeta, "datos");
        ServicioJuego NuevoJuego() => new ServicioJuego(contenido, CarpetaDatos);

        // ---------- Traducción de errores (Transacciones.Ejecutar) ----------

        [Test]
        public void UnDatoInvalidoSeTraduceAUnErrorParaElUsuarioYNoSeGuarda()
        {
            var db = new BaseDatos(CarpetaDatos);
            var ex = Assert.Throws<ErrorJuego>(() => db.Ejecutar(() => db.Insertar(new Jugador { nombre = " ", cedula = "12345", personaje_id = 1 })));
            StringAssert.Contains("nombre", ex.Message);
            Assert.IsInstanceOf<ErrorIntegridad>(ex.InnerException);
            Assert.IsEmpty(db.Jugadores);
        }

        [Test]
        public void UnFalloDeEscrituraSeTraduceAUnMensajeParaReintentar()
        {
            var db = new BaseDatos(CarpetaDatos);
            var ex = Assert.Throws<ErrorJuego>(() => db.Ejecutar((Action)(() => throw new IOException("disco lleno"))));
            StringAssert.Contains("Intenta de nuevo", ex.Message);
        }

        // ---------- Reglas de integridad de la base de datos ----------

        static Jugador JugadorValido() => new Jugador { nombre = "Ana Prueba", cedula = "12345678", personaje_id = 1 };
        static Partida PartidaValida(int jugadorId) => new Partida { jugador_id = jugadorId, tramo_actual_id = 1, contenido_version = 1 };

        static IEnumerable Violaciones()
        {
            yield return new TestCaseData((Action<BaseDatos>)(db => db.Insertar(new Jugador { nombre = "", cedula = "12345678", personaje_id = 1 })), "nombre").SetName("Jugador sin nombre");
            yield return new TestCaseData((Action<BaseDatos>)(db => db.Insertar(new Jugador { nombre = "A", cedula = "12", personaje_id = 1 })), "cédula").SetName("Cédula inválida");
            yield return new TestCaseData((Action<BaseDatos>)(db => { db.Insertar(JugadorValido()); db.Insertar(JugadorValido()); }), "ya está registrada").SetName("Cédula repetida");
            yield return new TestCaseData((Action<BaseDatos>)(db => db.Insertar(new Jugador { nombre = "A", cedula = "12345678", personaje_id = 0 })), "personaje").SetName("Jugador sin personaje");
            yield return new TestCaseData((Action<BaseDatos>)(db => { var j = db.Insertar(JugadorValido()); var p = PartidaValida(j.id); p.estado = "OTRO"; db.Insertar(p); }), "estado").SetName("Estado de partida desconocido");
            yield return new TestCaseData((Action<BaseDatos>)(db => { var j = db.Insertar(JugadorValido()); var p = PartidaValida(j.id); p.tramo_actual_id = 0; db.Insertar(p); }), "tramo").SetName("Partida sin tramo");
            yield return new TestCaseData((Action<BaseDatos>)(db => { var j = db.Insertar(JugadorValido()); var p = PartidaValida(j.id); p.contenido_version = 3; db.Insertar(p); }), "versión de contenido").SetName("Versión de contenido desconocida");
            yield return new TestCaseData((Action<BaseDatos>)(db => { var j = db.Insertar(JugadorValido()); var p = PartidaValida(j.id); p.contenido_version = 2; db.Insertar(p); }), "huella").SetName("Contenido V2 sin huella");
            yield return new TestCaseData((Action<BaseDatos>)(db => { var j = db.Insertar(JugadorValido()); var p = PartidaValida(j.id); p.ultima_decision_presentada_id = 9; db.Insertar(p); }), "hito").SetName("Hito de presentación sin decisión propia");
            yield return new TestCaseData((Action<BaseDatos>)(db => db.Insertar(new Decision { partida_id = 99, situacion_id = 1, tramo_origen_id = 1, tramo_destino_id = 1 })), "partida").SetName("Decisión de una partida inexistente");
            yield return new TestCaseData((Action<BaseDatos>)(db => { var j = db.Insertar(JugadorValido()); var p = db.Insertar(PartidaValida(j.id)); db.Insertar(new Decision { partida_id = p.id, situacion_id = 1, tramo_origen_id = 0, tramo_destino_id = 1 }); }), "tramos").SetName("Decisión sin tramo de origen");
            yield return new TestCaseData((Action<BaseDatos>)(db => db.Insertar(new Evento { tipo = "" })), "tipo").SetName("Evento sin tipo");
            yield return new TestCaseData((Action<BaseDatos>)(db => db.Insertar(new Evento { tipo = "X", jugador_id = 99 })), "jugador").SetName("Evento de un jugador inexistente");
            yield return new TestCaseData((Action<BaseDatos>)(db => db.Insertar(new Evento { tipo = "X", partida_id = 99 })), "partida").SetName("Evento de una partida inexistente");
        }

        [TestCaseSource(nameof(Violaciones))]
        public void LaBaseRechazaLosDatosQueViolanSusReglasYNoGuardaNada(Action<BaseDatos> cambios, string fragmento)
        {
            var db = new BaseDatos(CarpetaDatos);
            var ex = Assert.Throws<ErrorIntegridad>(() => db.Transaccion(() => cambios(db)));
            StringAssert.Contains(fragmento, ex.Message);
            Assert.IsEmpty(db.Jugadores); Assert.IsEmpty(db.Partidas); Assert.IsEmpty(db.Decisiones); Assert.IsEmpty(db.Eventos);
            Assert.IsFalse(File.Exists(Path.Combine(CarpetaDatos, "lifedrive-db.json")), "Una transacción rechazada no escribe el archivo.");
        }

        [Test]
        public void UnaTransaccionAnidadaLaConfirmaLaExterior()
        {
            var db = new BaseDatos(CarpetaDatos);
            db.Transaccion(() => db.Transaccion(() => db.Insertar(JugadorValido())));
            Assert.AreEqual(1, new BaseDatos(CarpetaDatos).Jugadores.Count);
        }

        [Test]
        public void UnaBaseDeUnaVersionMasNuevaDelJuegoNoSeAbre()
        {
            Directory.CreateDirectory(CarpetaDatos);
            File.WriteAllText(Path.Combine(CarpetaDatos, "lifedrive-db.json"), "{\"version\":99,\"jugador\":[]}");
            var ex = Assert.Throws<IOException>(() => new BaseDatos(CarpetaDatos));
            StringAssert.Contains("versión más nueva", ex.Message);
        }

        [Test]
        public void UnArchivoDanadoSinCopiaDeRespaldoSeApartaYNoSeSobrescribe()
        {
            Directory.CreateDirectory(CarpetaDatos);
            var archivo = Path.Combine(CarpetaDatos, "lifedrive-db.json");
            File.WriteAllText(archivo, "esto no es json");
            var ex = Assert.Throws<IOException>(() => new BaseDatos(CarpetaDatos));
            StringAssert.Contains("dañada", ex.Message);
            Assert.IsFalse(File.Exists(archivo));
            Assert.AreEqual(1, Directory.GetFiles(CarpetaDatos, "lifedrive-db.json.danado-*").Length, "El original dañado se conserva apartado.");
        }

        [Test]
        public void UnArchivoDanadoSeRecuperaDesdeLaCopiaDeRespaldo()
        {
            var db = new BaseDatos(CarpetaDatos);
            db.Transaccion(() => db.Insertar(JugadorValido()));                       // primera versión
            db.Transaccion(() => db.Insertar(new Jugador { nombre = "Luis", cedula = "87654321", personaje_id = 2 })); // deja .bak con 1 jugador
            File.WriteAllText(Path.Combine(CarpetaDatos, "lifedrive-db.json"), "{ dañado");
            var recuperada = new BaseDatos(CarpetaDatos);
            Assert.AreEqual(1, recuperada.Jugadores.Count);
            Assert.AreEqual("Ana Prueba", recuperada.Jugadores[0].nombre);
        }

        // ---------- ServicioJuego ----------

        [Test]
        public void UnContenidoConErroresImpideAbrirElJuego()
        {
            var c = new Almacen(ContenidoReal).CargarContenido();
            c.personajes = new Personaje[0];
            File.WriteAllText(Path.Combine(contenido, "ruta.json"), JsonUtility.ToJson(c));
            var ex = Assert.Throws<ErrorJuego>(() => NuevoJuego());
            StringAssert.Contains("tiene errores", ex.Message);
            StringAssert.Contains("No hay personajes", ex.Message);
        }

        [Test]
        public void UnaBaseDeUnaVersionNuevaSeInformaComoErrorDelJuego()
        {
            Directory.CreateDirectory(CarpetaDatos);
            File.WriteAllText(Path.Combine(CarpetaDatos, "lifedrive-db.json"), "{\"version\":99,\"jugador\":[]}");
            var ex = Assert.Throws<ErrorJuego>(() => NuevoJuego());
            StringAssert.Contains("No se pudo abrir la base de datos", ex.Message);
        }

        [Test]
        public void SiFaltaElPaqueteDeRecorridosV2ElJuegoLoInformaYNoArranca()
        {
            var catalogo = AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset");
            Assert.IsNotNull(catalogo);
            var ex = Assert.Throws<ErrorJuego>(() => new ServicioJuego(contenido, CarpetaDatos, catalogo)); // la carpeta no trae ruta-v2-candidata.json
            StringAssert.Contains("recorridos V2", ex.Message);
        }

        // ---------- Validación del contenido (ValidadorRuta) ----------

        static IEnumerable Defectos()
        {
            TestCaseData Caso(string nombre, Action<Contenido> romper, string fragmento) => new TestCaseData(romper, fragmento).SetName(nombre);
            yield return Caso("Sin personajes", c => c.personajes = new Personaje[0], "No hay personajes");
            yield return Caso("Dos tramos de salida", c => c.tramos[1].es_salida = true, "exactamente un tramo de salida");
            yield return Caso("Sin destino", c => { foreach (var t in c.tramos) t.es_destino = false; }, "No hay tramo de destino");
            yield return Caso("Id de tramo repetido", c => c.tramos[1].id = c.tramos[0].id, "Id de tramo repetido");
            yield return Caso("Destino con siguiente", c => c.tramos.First(t => t.es_destino).siguiente_tramo_id = c.tramos[0].id, "no debe tener siguiente");
            yield return Caso("Siguiente inexistente", c => c.tramos[0].siguiente_tramo_id = 999, "siguiente tramo válido");
            yield return Caso("Situación en tramo inexistente", c => c.situaciones[0].tramo_id = 999, "no existe");
            yield return Caso("Tipo desconocido", c => c.situaciones[0].tipo = "OTRO", "desconocido");
            yield return Caso("Sin enunciado", c => c.situaciones[0].enunciado = " ", "sin enunciado");
            yield return Caso("Menos de dos alternativas", c => c.situaciones[0].alternativas = c.situaciones[0].alternativas.Take(1).ToArray(), "al menos dos alternativas");
            yield return Caso("Pregunta sin respuesta única", c => { foreach (var a in c.situaciones.First(s => s.tipo == TipoSituacion.Pregunta).alternativas) a.es_acertada = false; }, "exactamente una respuesta correcta");
            yield return Caso("Decisión de ruta sin desvío", c => { foreach (var a in c.situaciones.First(s => s.tipo == TipoSituacion.DecisionRuta).alternativas) a.tramo_destino_id = 0; }, "al menos un desvío");
            yield return Caso("Orden repetido", c => { c.situaciones[1].tramo_id = c.situaciones[0].tramo_id; c.situaciones[1].orden = c.situaciones[0].orden; }, "orden repetido");
            yield return Caso("Desvío a tramo inexistente", c => c.situaciones[0].alternativas[0].tramo_destino_id = 999, "no existe");
            yield return Caso("Buena práctica no acertada", c => { var a = c.situaciones[0].alternativas[0]; a.es_buena_practica = true; a.es_acertada = false; }, "buena práctica debe ser acertada");
            yield return Caso("Retraso negativo", c => c.situaciones[0].alternativas[0].retraso_min = -5, "retraso negativo");
            yield return Caso("Sin explicación", c => c.situaciones[0].alternativas[0].explicacion = "", "sin explicación");
            yield return Caso("Alternativa repetida", c => c.situaciones[1].alternativas[0].id = c.situaciones[0].alternativas[0].id, "Id de alternativa repetido");
        }

        [TestCaseSource(nameof(Defectos))]
        public void ElValidadorDescribeCadaDefectoDelContenido(Action<Contenido> romper, string fragmento)
        {
            var c = new Almacen(ContenidoReal).CargarContenido();
            romper(c);
            var problemas = ValidadorRuta.Revisar(c);
            Assert.IsTrue(problemas.Any(p => p.Contains(fragmento)), "Esperaba «" + fragmento + "» en:\n" + string.Join("\n", problemas));
        }

        [Test]
        public void ElValidadorNoFallaConContenidoVacio() => Assert.IsNotEmpty(ValidadorRuta.Revisar(null));

        // ---------- Sesión, permisos y registro ----------

        [Test]
        public void ElRegistroRechazaUnPersonajeQueNoExiste()
        {
            var ex = Assert.Throws<ErrorJuego>(() => NuevoJuego().Usuarios.RegistrarJugador("Ana Prueba", "12345678", 999));
            StringAssert.Contains("personaje", ex.Message);
        }

        [Test]
        public void ElFormularioPideUnPersonaje()
        {
            Assert.AreEqual("Elige un personaje", Validacion.ErrorRegistro("Ana", "12345678", 0));
            Assert.IsNull(Validacion.ErrorRegistro("Ana", "12345678", 1));
        }

        [Test]
        public void ElFormularioRechazaUnaCedulaInvalida()
        {
            StringAssert.Contains("cédula", Validacion.ErrorRegistro("Ana", "12", 1));
            StringAssert.Contains("cédula", Validacion.ErrorRegistro("Ana", "12A45678", 1));
            StringAssert.Contains("cédula", Validacion.ErrorRegistro("Ana", null, 1));
        }

        [Test]
        public void NoSePuedeDecidirEnUnaPartidaQueYaTermino()
        {
            var juego = NuevoJuego();
            juego.Usuarios.RegistrarJugador("Ana Prueba", "12345678", 1);
            var p = juego.Partidas.IniciarPartida();
            for (var i = 0; i < 100 && juego.Partidas.SituacionActual(p) != null; i++)
            {
                var s = juego.Partidas.SituacionActual(p);
                p = juego.Partidas.Decidir(p.id, s.id, s.alternativas[0].id).Partida;
            }
            Assert.AreEqual(EstadoPartida.Finalizada, p.estado);
            var pendiente = juego.Contenido.situaciones.First(s => !juego.BaseDatos.Decisiones.Any(d => d.partida_id == p.id && d.situacion_id == s.id));
            var ex = Assert.Throws<ErrorJuego>(() => juego.Partidas.Decidir(p.id, pendiente.id, pendiente.alternativas[0].id));
            StringAssert.Contains("ya terminó", ex.Message);
        }

        [Test]
        public void ElAdministradorNoPuedeVerElResumenDeUnaPartidaInexistente()
        {
            var juego = NuevoJuego();
            juego.Usuarios.IniciarSesionAdministrador("admin", "ClaveDePrueba123");
            var ex = Assert.Throws<ErrorJuego>(() => juego.Desempeno.Resumen(9999));
            StringAssert.Contains("no encontrada", ex.Message);
        }

        static void BloquearEscritura(string datos) => Directory.CreateDirectory(Path.Combine(datos, "lifedrive-db.json.tmp"));

        [Test]
        public void CerrarSesionNoDependeDePoderRegistrarlo()
        {
            var juego = NuevoJuego();
            juego.Usuarios.RegistrarJugador("Ana Prueba", "12345678", 1);
            BloquearEscritura(CarpetaDatos);
            Assert.DoesNotThrow(() => juego.Usuarios.CerrarSesion());
            Assert.AreEqual(Rol.Ninguno, juego.Usuarios.Actual.Rol);
        }

        [Test]
        public void UnaAccionNoPermitidaSeRechazaAunqueNoSePuedaRegistrarElIntento()
        {
            var juego = NuevoJuego();
            juego.Usuarios.RegistrarJugador("Ana Prueba", "12345678", 1);
            BloquearEscritura(CarpetaDatos);
            var ex = Assert.Throws<ErrorJuego>(() => juego.Usuarios.Exigir(Accion.GenerarReporte));
            StringAssert.Contains("No tienes permiso", ex.Message);   // y no el error de guardado
        }

        [Test]
        public void ElTipoDeEventoDependeDelTipoDeSituacion()
        {
            Assert.AreEqual(TipoEvento.PreguntaRespondida, TipoEvento.DeSituacion(TipoSituacion.Pregunta));
            Assert.AreEqual(TipoEvento.RutaDecidida, TipoEvento.DeSituacion(TipoSituacion.DecisionRuta));
            Assert.AreEqual(TipoEvento.RiesgoAtendido, TipoEvento.DeSituacion(TipoSituacion.Riesgo));
            Assert.AreEqual(TipoEvento.EmergenciaAtendida, TipoEvento.DeSituacion(TipoSituacion.Emergencia));
        }

        // ---------- PDF ----------

        static int Buscar(byte[] datos, byte[] patron)
        {
            for (var i = 0; i <= datos.Length - patron.Length; i++)
            {
                var ok = true;
                for (var k = 0; k < patron.Length && ok; k++) ok = datos[i + k] == patron[k];
                if (ok) return i;
            }
            return -1;
        }

        [Test]
        public void ElPdfConvierteLosSignosEspecialesAWinAnsiYEscapaLosParentesis()
        {
            var pdf = new DocumentoPdf();
            pdf.Texto(50, 50, "—–•“”‘’…−→ (a)\\ \u0001", 10);
            var bytes = pdf.Generar();
            // ‘—’ 151, ‘–’ 150, ‘•’ 149, “ 147, ” 148, ‘ 145, ’ 146, … 133, − se guarda como '-', → como '>'
            CollectionAssert.AreEqual(new byte[] { 151, 150, 149, 147, 148, 145, 146, 133, (byte)'-', (byte)'>' },
                bytes.Skip(Buscar(bytes, new byte[] { 151, 150 })).Take(10).ToArray());
            Assert.GreaterOrEqual(Buscar(bytes, Encoding.ASCII.GetBytes("\\(a\\)\\\\ ?")), 0, "Los paréntesis y la barra se escapan y lo no representable pasa a '?'.");
        }

        [Test]
        public void UnReporteLargoSeReparteEnVariasPaginas()
        {
            var resumen = new Resumen
            {
                Partida = new Partida { id = 1, puntos = 10, fecha_inicio = "2026-11-09 10:00", fecha_fin = "2026-11-09 10:30" },
                Jugador = new Jugador { nombre = "Ana Prueba", cedula = "12345678" },
            };
            for (var i = 1; i <= 80; i++)
                resumen.Historial.Add(new PasoHistorial
                {
                    TramoOrigen = "Vía " + i, PuntosAcumulados = i,
                    Decision = new Decision { tipo = TipoSituacion.Riesgo, alternativa = "Elección " + i, es_acertada = i % 2 == 0, consecuencia = "Consecuencia " + i, puntos_aplicados = 1 },
                });
            var texto = Encoding.GetEncoding("iso-8859-1").GetString(GeneradorReportes.GenerarPdf(resumen));
            var paginas = int.Parse(Regex.Match(texto, @"/Count (\d+)").Groups[1].Value);
            Assert.GreaterOrEqual(paginas, 2);
            StringAssert.Contains("80. RIESGO", texto);
        }
    }
}
