// Pruebas de la base de datos local: reglas de integridad, transacciones y recuperación del archivo.
using System;
using System.IO;
using LifeDrive.Datos;
using NUnit.Framework;

namespace LifeDrive.Tests
{
    public class BaseDatosTests
    {
        string carpeta;

        [SetUp] public void Preparar() => carpeta = Path.Combine(Path.GetTempPath(), "lifedrive-bd-" + Guid.NewGuid());
        [TearDown] public void Limpiar() => Directory.Delete(carpeta, true);

        static Jugador Ana() => new Jugador { nombre = "Ana", cedula = "123456", personaje_id = 1, fecha_registro = "2026-10-31 10:00:00" };
        static Partida PartidaDe(int jugadorId) => new Partida { jugador_id = jugadorId, tramo_actual_id = 1, fecha_inicio = "2026-10-31 10:00:00" };

        [Test]
        public void LosCambiosSeGuardanYSeLeenAlReabrir()
        {
            var db = new BaseDatos(carpeta);
            db.Transaccion(() => { var j = db.Insertar(Ana()); db.Insertar(PartidaDe(j.id)); });

            var otra = new BaseDatos(carpeta);
            Assert.AreEqual(1, otra.Jugadores.Count);
            Assert.AreEqual("123456", otra.Jugadores[0].cedula);
            Assert.AreEqual(1, otra.Partidas.Count);
        }

        [Test]
        public void NoSePuedeCambiarFueraDeUnaTransaccion()
        {
            var db = new BaseDatos(carpeta);
            Assert.Throws<InvalidOperationException>(() => db.Insertar(Ana()));
        }

        [Test]
        public void UnaReglaRotaRevierteTodaLaTransaccion()
        {
            var db = new BaseDatos(carpeta);
            db.Transaccion(() => db.Insertar(Ana()));

            // La segunda Ana repite la cédula: también se descarta la partida insertada antes que ella.
            Assert.Throws<ErrorIntegridad>(() => db.Transaccion(() =>
            {
                db.Insertar(PartidaDe(1));
                db.Insertar(Ana());
            }));
            Assert.AreEqual(1, db.Jugadores.Count);
            Assert.AreEqual(0, db.Partidas.Count);
            Assert.AreEqual(0, new BaseDatos(carpeta).Partidas.Count);
        }

        [TestCase("12a45")]
        [TestCase("1234")]
        public void LaCedulaInvalidaSeRechaza(string cedula)
        {
            var db = new BaseDatos(carpeta);
            var j = Ana();
            j.cedula = cedula;
            Assert.Throws<ErrorIntegridad>(() => db.Transaccion(() => db.Insertar(j)));
        }

        [Test]
        public void LasReglasDePartidasYDecisionesSeCumplen()
        {
            var db = new BaseDatos(carpeta);
            db.Transaccion(() => { db.Insertar(Ana()); db.Insertar(PartidaDe(1)); });

            Assert.Throws<ErrorIntegridad>(() => db.Transaccion(() => db.Insertar(PartidaDe(1))));      // dos en curso
            Assert.Throws<ErrorIntegridad>(() => db.Transaccion(() => db.Insertar(PartidaDe(99))));     // jugador inexistente
            Assert.Throws<ErrorIntegridad>(() => db.Transaccion(() => db.Partidas[0].estado = EstadoPartida.Finalizada)); // sin fecha_fin
            Assert.AreEqual(EstadoPartida.EnCurso, db.Partidas[0].estado);

            var d = new Decision { partida_id = 1, situacion_id = 5, alternativa_id = 1, tramo_origen_id = 1, tramo_destino_id = 1 };
            db.Transaccion(() => db.Insertar(d));
            var repetida = new Decision { partida_id = 1, situacion_id = 5, alternativa_id = 2, tramo_origen_id = 1, tramo_destino_id = 1 };
            Assert.Throws<ErrorIntegridad>(() => db.Transaccion(() => db.Insertar(repetida)));
            Assert.Throws<ErrorIntegridad>(() => db.Transaccion(() => db.Insertar(new Evento { tipo = "X", partida_id = 42 })));
        }

        [Test]
        public void LosIdsNoSeReutilizan()
        {
            var db = new BaseDatos(carpeta);
            Assert.Throws<ErrorIntegridad>(() => db.Transaccion(() => { db.Insertar(Ana()); db.Insertar(Ana()); }));
            var j = db.Transaccion(() => db.Insertar(Ana()));
            Assert.AreEqual(1, j.id);   // la transacción fallida no consumió ids
            var e1 = db.Transaccion(() => db.Insertar(new Evento { tipo = "A" }));
            var e2 = db.Transaccion(() => db.Insertar(new Evento { tipo = "B" }));
            Assert.AreEqual(e1.id + 1, e2.id);
        }

        [Test]
        public void UnArchivoDanadoSeApartaYSeUsaLaCopia()
        {
            var db = new BaseDatos(carpeta);
            db.Transaccion(() => db.Insertar(Ana()));
            db.Transaccion(() => db.Insertar(new Evento { tipo = "A" }));   // ahora existe el .bak con Ana

            File.WriteAllText(Path.Combine(carpeta, "lifedrive-db.json"), "{ esto no es json");
            var recuperada = new BaseDatos(carpeta);
            Assert.AreEqual(1, recuperada.Jugadores.Count);
            Assert.AreEqual(1, Directory.GetFiles(carpeta, "lifedrive-db.json.danado-*").Length);
        }
    }
}
