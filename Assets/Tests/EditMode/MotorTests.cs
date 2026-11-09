// Pruebas del contenido, las estrategias, los roles, el ciclo de la partida y el registro de eventos.
// Usan la ruta.json real y guardan la base de datos en una carpeta temporal que se borra al final.
using System;
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Motor;
using NUnit.Framework;
using UnityEngine;

namespace LifeDrive.Tests
{
    public class MotorTests
    {
        static readonly string ContenidoReal = Path.Combine(Application.streamingAssetsPath, "Contenido");
        string carpetaDatos, carpetaContenido;

        [SetUp]
        public void Preparar()
        {
            carpetaDatos = Path.Combine(Path.GetTempPath(), "lifedrive-prueba-" + Guid.NewGuid());
            // Copia del contenido con un administrador de prueba, sin tocar el administradores.json real.
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

        static int Contar(ServicioJuego juego, string tipo) => juego.BaseDatos.Eventos.Count(e => e.tipo == tipo);

        // ---------- Contenido y estrategias ----------

        [Test]
        public void ElContenidoRealNoTieneErroresNiRutasSinSalida()
        {
            var contenido = new Almacen(ContenidoReal).CargarContenido();
            CollectionAssert.IsEmpty(ValidadorRuta.Revisar(contenido));
        }

        [Test]
        public void ElValidadorDetectaUnCicloSinSalida()
        {
            var c = new Almacen(ContenidoReal).CargarContenido();
            c.tramos.First(t => t.id == 2).siguiente_tramo_id = 3;
            c.tramos.First(t => t.id == 3).siguiente_tramo_id = 2;
            var problemas = ValidadorRuta.Revisar(c);
            Assert.IsTrue(problemas.Any(p => p.Contains("Vía principal")), string.Join("\n", problemas));
        }

        [Test]
        public void LasEstrategiasSeEligenPorTipo()
        {
            var evaluador = new EvaluadorSituaciones();
            var a = new Alternativa { puntos = 15, retraso_min = 30, tramo_destino_id = 7, es_acertada = true, explicacion = "x" };
            var pregunta = evaluador.Evaluar(new Situacion { tipo = TipoSituacion.Pregunta }, a);
            Assert.AreEqual(0, pregunta.SiguienteTramoId);
            Assert.AreEqual(0, pregunta.RetrasoMin);
            var ruta = evaluador.Evaluar(new Situacion { tipo = TipoSituacion.DecisionRuta }, a);
            Assert.AreEqual(7, ruta.SiguienteTramoId);
            Assert.AreEqual(30, ruta.RetrasoMin);
            Assert.IsInstanceOf<EvaluacionRiesgoEmergencia>(evaluador.SeleccionarEstrategia(TipoSituacion.Emergencia));
        }

        // ---------- Partidas ----------

        [Test]
        public void TodosLosCaminosLleganAlDestinoYLosPuntosCuadran()
        {
            var estrategias = new Func<Alternativa[], Alternativa>[] { a => a[0], a => a[a.Length - 1], a => a.Length > 1 ? a[1] : a[0] };
            var juego = NuevoJuego();
            for (var i = 0; i < estrategias.Length; i++)
            {
                juego.Usuarios.RegistrarJugador("Jugador " + i, "1000" + i + "0000", 1);
                var p = Jugar(juego, estrategias[i]);
                Assert.AreEqual(EstadoPartida.Finalizada, p.estado);
                Assert.IsTrue(juego.Partidas.Tramo(p.tramo_actual_id).es_destino);
                Assert.AreEqual(juego.BaseDatos.Decisiones.Where(d => d.partida_id == p.id).Sum(d => d.puntos_aplicados), p.puntos);
                var r = juego.Desempeno.Resumen(p.id);
                Assert.AreEqual(p.puntos, r.Historial.Last().PuntosAcumulados);
                Assert.AreEqual("Destino final", r.RutaRecorrida.Last());
            }
        }

        [Test]
        public void LaPartidaSobreviveAlCierreDelJuego()
        {
            var juego = NuevoJuego();
            juego.Usuarios.RegistrarJugador("Ana Pérez", "123456789", 2);
            var p = juego.Partidas.IniciarPartida();
            var s = juego.Partidas.SituacionActual(p);
            juego.Partidas.Decidir(p.id, s.id, s.alternativas[0].id);

            var reabierto = NuevoJuego();
            var mismo = reabierto.Usuarios.RegistrarJugador("ana  pérez", "123456789", 2);
            var recuperada = reabierto.Partidas.IniciarPartida();
            Assert.AreEqual(p.id, recuperada.id);
            Assert.AreEqual(1, reabierto.BaseDatos.Decisiones.Count(d => d.partida_id == p.id));
            Assert.AreEqual(1, mismo.id);
            Assert.Throws<ErrorJuego>(() => reabierto.Usuarios.RegistrarJugador("Otra Persona", "123456789", 2));
        }

        [Test]
        public void UnaDecisionRepetidaNoSumaDosVecesYLasInvalidasSeRechazan()
        {
            var juego = NuevoJuego();
            juego.Usuarios.RegistrarJugador("Luis", "55555555", 1);
            var p = juego.Partidas.IniciarPartida();
            var s = juego.Partidas.SituacionActual(p);

            Assert.Throws<ErrorJuego>(() => juego.Partidas.Decidir(p.id, s.id, 999));
            Assert.Throws<ErrorJuego>(() => juego.Partidas.Decidir(p.id, s.id + 100, s.alternativas[0].id));

            var r1 = juego.Partidas.Decidir(p.id, s.id, s.alternativas[0].id);
            var r2 = juego.Partidas.Decidir(p.id, s.id, s.alternativas[1].id);
            Assert.IsFalse(r1.Repetida);
            Assert.IsTrue(r2.Repetida);
            Assert.AreEqual(r1.Decision.alternativa_id, r2.Decision.alternativa_id);
            Assert.AreEqual(r1.Decision.puntos_aplicados, r2.Partida.puntos);
        }

        // ---------- Usuarios y roles ----------

        [Test]
        public void LaTablaDePermisosSeparaLosRoles()
        {
            foreach (Accion a in Enum.GetValues(typeof(Accion)))
            {
                Assert.IsFalse(Permisos.Permite(Rol.Ninguno, a), $"{a} sin sesión");
                Assert.IsTrue(Permisos.Permite(Rol.Jugador, a) != Permisos.Permite(Rol.Administrador, a), $"{a} debe ser de un solo rol");
            }
        }

        [Test]
        public void CadaRolSoloHaceLoSuyoYLosRechazosQuedanRegistrados()
        {
            var juego = NuevoJuego();
            Assert.Throws<ErrorJuego>(() => juego.Desempeno.ListarJugadores());           // sin sesión
            juego.Usuarios.RegistrarJugador("Marta", "66666666", 1);
            var partidaMarta = juego.Partidas.IniciarPartida();
            Assert.Throws<ErrorJuego>(() => juego.Desempeno.ListarJugadores());           // un jugador no administra
            Assert.Throws<ErrorJuego>(() => juego.Desempeno.Eventos());

            juego.Usuarios.RegistrarJugador("Pedro", "77777777", 1);
            Assert.Throws<ErrorJuego>(() => juego.Desempeno.Resumen(partidaMarta.id));    // partida ajena
            Assert.Throws<ErrorJuego>(() => juego.Partidas.Decidir(partidaMarta.id, 1, 1));

            Assert.Throws<ErrorJuego>(() => juego.Usuarios.IniciarSesionAdministrador("admin", "incorrecta"));
            juego.Usuarios.IniciarSesionAdministrador("admin", "ClaveDePrueba123");
            Assert.AreEqual(Rol.Administrador, juego.Usuarios.Actual.Rol);
            Assert.Throws<ErrorJuego>(() => juego.Partidas.IniciarPartida());              // el administrador no juega

            Assert.AreEqual(6, Contar(juego, TipoEvento.AccesoDenegado));
            Assert.AreEqual(1, Contar(juego, TipoEvento.SesionFallida));
            Assert.IsFalse(File.ReadAllText(Path.Combine(carpetaDatos, "lifedrive-db.json")).Contains("ClaveDePrueba123"));

            juego.Usuarios.CerrarSesion();
            Assert.AreEqual(Rol.Ninguno, juego.Usuarios.Actual.Rol);
            Assert.AreEqual(1, Contar(juego, TipoEvento.SesionCerrada));
        }

        // ---------- Registro de eventos ----------

        [Test]
        public void UnaPartidaDejaSusEventosEnOrden()
        {
            var juego = NuevoJuego();
            var j = juego.Usuarios.RegistrarJugador("Sofía", "12121212", 4);
            var p = Jugar(juego, a => a[a.Length - 1]);
            var decisiones = juego.BaseDatos.Decisiones.Count(d => d.partida_id == p.id);

            var tipos = juego.BaseDatos.Eventos.Where(e => e.partida_id == p.id).OrderBy(e => e.id).Select(e => e.tipo).ToList();
            Assert.AreEqual(TipoEvento.PartidaIniciada, tipos.First());
            Assert.AreEqual(TipoEvento.PartidaFinalizada, tipos.Last());
            Assert.AreEqual(decisiones, tipos.Count(t => t == TipoEvento.PuntosActualizados));
            Assert.AreEqual(decisiones, tipos.Count(t => t == TipoEvento.PreguntaRespondida || t == TipoEvento.RutaDecidida
                                                         || t == TipoEvento.RiesgoAtendido || t == TipoEvento.EmergenciaAtendida));
            Assert.IsTrue(tipos.Contains(TipoEvento.TramoAlcanzado));
            Assert.AreEqual(1, Contar(juego, TipoEvento.JugadorRegistrado));
            Assert.IsTrue(juego.BaseDatos.Eventos.Where(e => e.partida_id == p.id).All(e => e.jugador_id == j.id && e.usuario == "12121212"));
        }

        [Test]
        public void ElAdministradorConsultaJugadoresEventosYGeneraElPdf()
        {
            var juego = NuevoJuego();
            juego.Usuarios.RegistrarJugador("Carlos Reporte", "88888888", 3);
            var terminada = Jugar(juego, a => a[0]);
            var enCurso = juego.Partidas.IniciarPartida();

            juego.Usuarios.IniciarSesionAdministrador("admin", "ClaveDePrueba123");
            var filas = juego.Desempeno.ListarJugadores("8888");
            Assert.AreEqual(1, filas.Count);
            Assert.AreEqual(1, filas[0].PartidasFinalizadas);
            Assert.AreEqual(2, juego.Desempeno.PartidasDe(filas[0].Jugador.id).Count);

            var pdf = juego.Desempeno.ReportePdf(terminada.id);
            Assert.AreEqual("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
            Assert.Throws<ErrorJuego>(() => juego.Desempeno.ReportePdf(enCurso.id));

            var eventos = juego.Desempeno.Eventos(partidaId: terminada.id);
            Assert.AreEqual(TipoEvento.ReporteGenerado, eventos.First().tipo);   // el más reciente primero
            Assert.IsTrue(juego.Desempeno.Eventos(jugadorId: filas[0].Jugador.id).Count > 0);
        }

        [Test]
        public void ElPdfConNombreLargoConservaLaCedulaYAjustaElNombre()
        {
            var juego = NuevoJuego();
            var nombre = string.Join(" ", System.Linq.Enumerable.Repeat("Maria Fernanda Rodriguez", 5));
            juego.Usuarios.RegistrarJugador(nombre, "77777777", 3);
            var terminada = Jugar(juego, a => a[0]);
            juego.Usuarios.IniciarSesionAdministrador("admin", "ClaveDePrueba123");

            var texto = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(juego.Desempeno.ReportePdf(terminada.id));
            StringAssert.Contains("(Cédula: 77777777) Tj", texto);          // cédula en su propia línea
            StringAssert.DoesNotContain("Jugador: " + nombre + ")", texto);      // el nombre se partió en varias líneas
        }
    }
}
