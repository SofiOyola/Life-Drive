using System;
using System.IO;
using System.Linq;
using LifeDrive.Mapas;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LifeDrive.Tests
{
    public class PersistenciaAparienciaPilotoV2Tests
    {
        string carpeta, contenido, datos;
        CatalogoMapas catalogo;
        [SetUp] public void Preparar()
        {
            carpeta = Path.Combine(Application.temporaryCachePath, "LifeDrive-AparienciaTests", Guid.NewGuid().ToString("N"));
            contenido = Path.Combine(carpeta, "Contenido"); datos = Path.Combine(carpeta, "Datos");
            Directory.CreateDirectory(contenido);
            foreach (var archivo in new[] { "ruta.json", "ruta-v2-candidata.json" })
                File.Copy(Path.Combine(Application.streamingAssetsPath, "Contenido", archivo), Path.Combine(contenido, archivo));
            catalogo = AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset");
        }
        [TearDown] public void Limpiar() { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
        ServicioJuego Abrir(int personaje = 3)
        {
            var juego = new ServicioJuego(contenido, datos, catalogo);
            juego.Usuarios.RegistrarJugador("Apariencia Ficticia", "990036", personaje); return juego;
        }
        string JsonActual() => File.ReadAllText(Path.Combine(datos, "lifedrive-db.json"));

        [TestCase(1, 0, 0, 0, 0, 0)] [TestCase(2, 1, 1, 2, 3, 4)]
        [TestCase(3, 0, 4, 3, 2, 1)] [TestCase(4, 1, 2, 4, 1, 3)]
        public void GuardaVariantesYRecuperaSnapshotSinCambiarPersonajeNiEvaluacion(int ficha, int estilo, int piel, int ojos, int cabello, int traje)
        {
            var juego = Abrir(ficha); var servicio = new PersistenciaAparienciaPilotoV2(juego);
            var apariencia = VariantesAparienciaPilotoV1.Crear(ficha, estilo, piel, ojos, cabello, traje);
            servicio.GuardarPerfilActual(apariencia); servicio.GuardarPerfilActual(apariencia);
            var p = juego.RecorridosV2.IniciarPartida();
            servicio.FijarPartida(p.id); servicio.FijarPartida(p.id);
            Assert.That(juego.BaseDatos.Eventos.Count(e => e.tipo == PersistenciaAparienciaPilotoV2.EventoPerfil), Is.EqualTo(1));
            Assert.That(juego.BaseDatos.Eventos.Count(e => e.tipo == PersistenciaAparienciaPilotoV2.EventoPartida), Is.EqualTo(1));
            var reabierto = Abrir(ficha); servicio = new PersistenciaAparienciaPilotoV2(reabierto);
            Assert.That(JsonUtility.ToJson(servicio.LeerPerfilActual()), Is.EqualTo(JsonUtility.ToJson(apariencia)));
            Assert.That(JsonUtility.ToJson(servicio.LeerPartida(p.id)), Is.EqualTo(JsonUtility.ToJson(apariencia)));
            servicio.GuardarPerfilActual(VariantesAparienciaPilotoV1.Crear(ficha, estilo == 1 ? 0 : 1, 4, 4, 4, 4));
            Assert.That(JsonUtility.ToJson(servicio.FijarPartida(p.id)), Is.EqualTo(JsonUtility.ToJson(apariencia)), "Una partida conserva su apariencia inicial.");
            Assert.That(reabierto.BaseDatos.Jugadores.Single().personaje_id, Is.EqualTo(ficha));
            Assert.That(reabierto.BaseDatos.Partidas.Single().puntos, Is.Zero); Assert.That(reabierto.BaseDatos.Decisiones, Is.Empty);
        }

        [Test] public void JugadorAntiguoSinAtributosYV1SonValidosSinGuardarDefaults()
        {
            var juego = Abrir(); var servicio = new PersistenciaAparienciaPilotoV2(juego);
            var p = juego.Partidas.IniciarPartida(); string antes = JsonActual();
            Assert.That(p.contenido_version, Is.EqualTo(1));
            Assert.That(servicio.LeerPerfilActual(), Is.Null); Assert.That(servicio.FijarPartida(p.id), Is.Null);
            Assert.That(servicio.LeerPartida(p.id), Is.Null); Assert.That(JsonActual(), Is.EqualTo(antes));
            juego.Usuarios.RegistrarJugador("Antiguo V2 Ficticio", "99003602", 4);
            var v2 = juego.RecorridosV2.IniciarPartida(); antes = JsonActual();
            Assert.That(servicio.LeerPerfilActual(), Is.Null); Assert.That(servicio.FijarPartida(v2.id), Is.Null);
            Assert.That(servicio.LeerPartida(v2.id), Is.Null); Assert.That(JsonActual(), Is.EqualTo(antes));
        }

        [Test] public void RechazaCambioDeIdVersionYVarianteSinSobrescribir()
        {
            var juego = Abrir(); var servicio = new PersistenciaAparienciaPilotoV2(juego);
            var valida = VariantesAparienciaPilotoV1.Crear(3, 1, 2, 3, 4, 2); servicio.GuardarPerfilActual(valida);
            string antes = JsonActual();
            Assert.Throws<ErrorJuego>(() => servicio.GuardarPerfilActual(VariantesAparienciaPilotoV1.Crear(4, 0, 0, 0, 0, 0)));
            var invalida = VariantesAparienciaPilotoV1.Crear(3, 0, 0, 0, 0, 0); invalida.version_formato = 9;
            Assert.Throws<ErrorJuego>(() => servicio.GuardarPerfilActual(invalida));
            invalida.version_formato = 1; invalida.traje = "variante-inexistente";
            Assert.Throws<ErrorJuego>(() => servicio.GuardarPerfilActual(invalida));
            Assert.That(JsonActual(), Is.EqualTo(antes));
        }

        [Test] public void FormatoFuturoNoSeReemplazaPorDefaults()
        {
            var juego = Abrir(); var servicio = new PersistenciaAparienciaPilotoV2(juego);
            var futura = VariantesAparienciaPilotoV1.Crear(3, 1, 2, 3, 4, 2); futura.version_formato = 9;
            juego.BaseDatos.Ejecutar(() => juego.Eventos.Registrar(PersistenciaAparienciaPilotoV2.EventoPerfil,
                juego.Usuarios.Actual, detalle: JsonUtility.ToJson(futura)));
            string antes = JsonActual();
            Assert.Throws<ErrorJuego>(() => servicio.LeerPerfilActual());
            Assert.Throws<ErrorJuego>(() => servicio.GuardarPerfilActual(VariantesAparienciaPilotoV1.Crear(3, 0, 0, 0, 0, 0)));
            Assert.That(JsonActual(), Is.EqualTo(antes));
        }

        [Test] public void FalloDeEscrituraConservaPerfilYArchivoPrevios()
        {
            var juego = Abrir(); var servicio = new PersistenciaAparienciaPilotoV2(juego);
            var original = VariantesAparienciaPilotoV1.Crear(3, 1, 2, 3, 4, 2); servicio.GuardarPerfilActual(original);
            var partida = juego.RecorridosV2.IniciarPartida();
            string antes = JsonActual(); int eventos = juego.BaseDatos.Eventos.Count;
            Directory.CreateDirectory(Path.Combine(datos, "lifedrive-db.json.tmp"));
            Assert.Throws<ErrorJuego>(() => servicio.GuardarPerfilActual(VariantesAparienciaPilotoV1.Crear(3, 0, 0, 0, 0, 0)));
            Assert.Throws<ErrorJuego>(() => servicio.FijarPartida(partida.id));
            Assert.That(JsonActual(), Is.EqualTo(antes)); Assert.That(juego.BaseDatos.Eventos.Count, Is.EqualTo(eventos));
            Assert.That(JsonUtility.ToJson(servicio.LeerPerfilActual()), Is.EqualTo(JsonUtility.ToJson(original)));
            Directory.Delete(Path.Combine(datos, "lifedrive-db.json.tmp"));
            Assert.That(JsonUtility.ToJson(servicio.FijarPartida(partida.id)), Is.EqualTo(JsonUtility.ToJson(original)));
        }

        [Test] public void JugadorNoPuedeLeerNiFijarAparienciaDeOtraPartida()
        {
            var juego = Abrir(); var servicio = new PersistenciaAparienciaPilotoV2(juego);
            servicio.GuardarPerfilActual(VariantesAparienciaPilotoV1.Crear(3, 1, 2, 3, 4, 2));
            var propia = juego.RecorridosV2.IniciarPartida(); servicio.FijarPartida(propia.id);
            juego.Usuarios.RegistrarJugador("Otro Ficticio", "99003603", 1);
            Assert.Throws<ErrorJuego>(() => servicio.LeerPartida(propia.id));
            Assert.Throws<ErrorJuego>(() => servicio.FijarPartida(propia.id));
            Assert.That(juego.BaseDatos.Decisiones, Is.Empty); Assert.That(juego.BaseDatos.Partidas.Single().puntos, Is.Zero);
        }
    }
}
