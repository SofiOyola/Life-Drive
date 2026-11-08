using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LifeDrive.Datos;
using LifeDrive.Mapas;
using LifeDrive.Motor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LifeDrive.Tests
{
    public class ReanudacionRecorridosV2Tests
    {
        static readonly string Contenido = Path.Combine(Application.streamingAssetsPath, "Contenido");
        static readonly string[] Mapas = { "S0", "R1B", "R2C", "R3E", "DELIVERY" };
        static readonly int[] Situaciones = { 101, 103, 5, 6, 7 };
        static readonly int[] Alternativas = { 102, 105, 12, 15, 18 };
        string carpeta;
        CatalogoMapas catalogo;

        [SetUp]
        public void Preparar()
        {
            carpeta = Path.Combine(Path.GetTempPath(), "lifedrive-reanudacion-" + Guid.NewGuid());
            catalogo = AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset");
        }

        [TearDown]
        public void Limpiar()
        {
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true);
        }

        ServicioJuego Abrir(bool v2 = true)
        {
            var juego = new ServicioJuego(Contenido, carpeta, v2 ? catalogo : null);
            juego.Usuarios.RegistrarJugador("Ana de prueba", "123456789", 1);
            return juego;
        }

        static ResultadoDecision ResponderYConfirmar(ServicioJuego juego, int partidaId, int situacion, int alternativa)
        {
            var resultado = juego.Partidas.Decidir(partidaId, situacion, alternativa);
            juego.RecorridosV2.ConfirmarPresentacion(partidaId, resultado.Decision.id);
            return resultado;
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void ReabreV2EnCadaHitoConfirmado(int avance)
        {
            var juego = Abrir();
            var partida = juego.RecorridosV2.IniciarPartida();
            Assert.That(partida.contenido_version, Is.EqualTo(2));
            if (avance > 0)
                ResponderYConfirmar(juego, partida.id, 1, 1);
            for (var i = 0; i < avance; i++)
                ResponderYConfirmar(juego, partida.id, Situaciones[i], Alternativas[i]);

            var reabierto = Abrir();
            var eventos = reabierto.BaseDatos.Eventos.Count;
            var decisiones = reabierto.BaseDatos.Decisiones.Count;
            var retomada = reabierto.Partidas.IniciarPartida();
            var estado = reabierto.RecorridosV2.Consultar(retomada.id);
            Assert.That(retomada.id, Is.EqualTo(partida.id));
            Assert.That(estado.VersionContenido, Is.EqualTo(2));
            Assert.That(estado.MapaActual, Is.EqualTo(Mapas[avance]));
            CollectionAssert.AreEqual(Mapas.Take(avance + 1), estado.Historial);
            Assert.That(estado.SituacionPendienteId, Is.EqualTo(avance == 0 ? 1 : Situaciones[avance]));
            Assert.That(estado.PresentacionPendiente, Is.False);
            Assert.That(retomada.puntos, Is.EqualTo(partida.puntos));
            Assert.That(reabierto.BaseDatos.Eventos.Count, Is.EqualTo(eventos));
            Assert.That(reabierto.BaseDatos.Decisiones.Count, Is.EqualTo(decisiones));
            var tablas = JsonUtility.FromJson<TablasLocales>(File.ReadAllText(Path.Combine(carpeta, "lifedrive-db.json")));
            Assert.That(tablas.version, Is.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void ReproducePendienteSinReevaluarNiDuplicar(int paso)
        {
            var juego = Abrir();
            var partida = juego.RecorridosV2.IniciarPartida();
            ResponderYConfirmar(juego, partida.id, 1, 1);
            for (var i = 0; i < paso; i++)
                ResponderYConfirmar(juego, partida.id, Situaciones[i], Alternativas[i]);

            var respuesta = juego.Partidas.Decidir(partida.id, Situaciones[paso], Alternativas[paso]);
            var destino = paso < 4 ? Mapas[paso + 1] : null;
            if (paso == 2)
                CollectionAssert.AreEqual(new[] { 112, 116 }, respuesta.TramosAlcanzados);
            if (paso == 3)
                Assert.That(respuesta.CambioRuta, Is.False);

            var reabierto = Abrir();
            var eventos = reabierto.BaseDatos.Eventos.Count;
            var decisiones = reabierto.BaseDatos.Decisiones.Count;
            var retomada = reabierto.RecorridosV2.IniciarPartida();
            Assert.That(retomada.id, Is.EqualTo(partida.id));
            var pendiente = reabierto.RecorridosV2.Consultar(partida.id);
            Assert.That(pendiente.MapaActual, Is.EqualTo(Mapas[paso]));
            Assert.That(pendiente.PresentacionPendiente, Is.True);
            Assert.That(pendiente.DestinoVisualConfirmado, Is.EqualTo(destino));
            Assert.That(pendiente.DecisionConfirmada.id, Is.EqualTo(respuesta.Decision.id));
            Assert.That(pendiente.DecisionConfirmada.consecuencia, Is.EqualTo(respuesta.Decision.consecuencia));
            Assert.That(pendiente.CierrePresentado, Is.False);
            if (paso < 4)
                Assert.Throws<ErrorJuego>(() =>
                    reabierto.Partidas.Decidir(partida.id, Situaciones[paso + 1], Alternativas[paso + 1]));
            var repetida = reabierto.Partidas.Decidir(partida.id, Situaciones[paso], -1);
            Assert.That(repetida.Repetida, Is.True);
            Assert.That(repetida.Antes, Is.Null);
            Assert.That(repetida.Decision.id, Is.EqualTo(respuesta.Decision.id));
            Assert.That(repetida.TramosAlcanzados, Is.Empty);

            var confirmada = reabierto.RecorridosV2.ConfirmarPresentacion(partida.id, repetida.Decision.id);
            reabierto.RecorridosV2.ConfirmarPresentacion(partida.id, repetida.Decision.id);
            Assert.That(confirmada.MapaActual, Is.EqualTo(destino ?? "DELIVERY"));
            Assert.That(confirmada.PresentacionPendiente, Is.False);
            Assert.That(confirmada.CierrePresentado, Is.EqualTo(paso == 4));
            Assert.That(retomada.puntos, Is.EqualTo(partida.puntos));
            Assert.That(reabierto.BaseDatos.Decisiones.Count, Is.EqualTo(decisiones));
            Assert.That(reabierto.BaseDatos.Eventos.Count, Is.EqualTo(eventos));
            Assert.That(Abrir().RecorridosV2.Consultar(partida.id).CierrePresentado, Is.EqualTo(paso == 4));
        }

        [Test]
        public void ReanudaArchivoV1SinCamposNuevosYRegistraElAvanceImplicito()
        {
            var anterior = Abrir(false);
            var partida = anterior.Partidas.IniciarPartida();
            anterior.Partidas.Decidir(partida.id, 1, 1);
            anterior.Partidas.Decidir(partida.id, 2, 4);
            anterior.Partidas.Decidir(partida.id, 3, 7);
            var archivo = Path.Combine(carpeta, "lifedrive-db.json");
            var legado = Regex.Replace(File.ReadAllText(archivo),
                @"""(contenido_version|ultima_decision_presentada_id)""\s*:\s*\d+,\s*", "");
            File.WriteAllText(archivo, legado);

            var juego = Abrir();
            var retomada = juego.RecorridosV2.IniciarPartida();
            Assert.That(retomada.id, Is.EqualTo(partida.id));
            Assert.That(retomada.contenido_version, Is.EqualTo(0));
            Assert.That(juego.Partidas.SituacionActual(retomada).id, Is.EqualTo(4));
            Assert.That(juego.BaseDatos.Decisiones.Count, Is.EqualTo(3));
            Assert.That(juego.BaseDatos.Eventos.Count(e => e.tipo == TipoEvento.PartidaRetomada), Is.EqualTo(1));
            Assert.Throws<ErrorJuego>(() => juego.RecorridosV2.Consultar(retomada.id));

            var resultado = juego.Partidas.Decidir(retomada.id, 4, 10);
            Assert.That(resultado.Antes.Value.TramoId, Is.EqualTo(2));
            Assert.That(resultado.Antes.Value.SituacionId, Is.EqualTo(4));
            Assert.That(resultado.Decision.tramo_destino_id, Is.EqualTo(2));
            Assert.That(resultado.CambioRuta, Is.False);
            CollectionAssert.AreEqual(new[] { 4 }, resultado.TramosAlcanzados);
            Assert.That(resultado.Despues.TramoId, Is.EqualTo(4));
            Assert.That(resultado.Despues.SituacionId, Is.EqualTo(6));
            var resumen = juego.Desempeno.Resumen(retomada.id);
            Assert.That(resumen.Historial.Last().TramoDestino, Is.EqualTo("Vía principal"));
            Assert.That(JsonUtility.FromJson<TablasLocales>(File.ReadAllText(archivo)).version, Is.EqualTo(1));
        }

        [Test]
        public void ConservaAmbasVersionesYRechazaRetomarV2SinPaquete()
        {
            var juego = Abrir();
            var v2 = juego.RecorridosV2.IniciarPartida();
            ResponderYConfirmar(juego, v2.id, 1, 1);
            ResponderYConfirmar(juego, v2.id, 101, 101);
            Assert.That(juego.Desempeno.Resumen(v2.id).Historial.Last().TramoDestino, Is.EqualTo("Salida preparada"));

            juego.Usuarios.RegistrarJugador("Otro jugador", "222222222", 1);
            var v1 = juego.Partidas.IniciarPartida();
            Assert.That(v1.contenido_version, Is.EqualTo(1));
            Assert.Throws<ErrorJuego>(() => juego.RecorridosV2.ConfirmarPresentacion(v2.id, 1));
            Assert.That(juego.RecorridosV2.IniciarPartida().id, Is.EqualTo(v1.id));

            var sinPaquete = Abrir(false);
            Assert.Throws<ErrorJuego>(() => sinPaquete.Partidas.IniciarPartida());
            Assert.That(sinPaquete.BaseDatos.Partidas.Count, Is.EqualTo(2));
            Assert.That(sinPaquete.BaseDatos.Partidas.Single(p => p.id == v2.id).tramo_actual_id, Is.EqualTo(102));
        }

        [Test]
        public void RechazaUnPaqueteDistintoConElMismoNumeroDeVersion()
        {
            var original = Abrir();
            var partida = original.RecorridosV2.IniciarPartida();
            var copia = Path.Combine(carpeta, "contenido-prueba");
            Directory.CreateDirectory(copia);
            File.Copy(Path.Combine(Contenido, "ruta.json"), Path.Combine(copia, "ruta.json"));
            var json = File.ReadAllText(Path.Combine(Contenido, "ruta-v2-candidata.json"));
            File.WriteAllText(Path.Combine(copia, "ruta-v2-candidata.json"),
                json.Replace("Patio de salida", "Patio de salida modificado"));

            var reabierto = new ServicioJuego(copia, carpeta, catalogo);
            reabierto.Usuarios.RegistrarJugador("Ana de prueba", "123456789", 1);
            var eventos = reabierto.BaseDatos.Eventos.Count;
            Assert.Throws<ErrorJuego>(() => reabierto.RecorridosV2.IniciarPartida());
            Assert.Throws<ErrorJuego>(() => reabierto.RecorridosV2.Consultar(partida.id));
            Assert.That(reabierto.BaseDatos.Partidas.Single().id, Is.EqualTo(partida.id));
            Assert.That(reabierto.BaseDatos.Eventos.Count, Is.EqualTo(eventos));
        }

        [Test]
        public void UnFalloDeEscrituraNoConfirmaRespuestaNiHito()
        {
            var juego = Abrir();
            var partida = juego.RecorridosV2.IniciarPartida();
            var temporal = Path.Combine(carpeta, "lifedrive-db.json.tmp");
            Directory.CreateDirectory(temporal);
            Assert.Throws<ErrorJuego>(() => juego.Partidas.Decidir(partida.id, 1, 1));
            Assert.That(juego.RecorridosV2.Consultar(partida.id).DecisionConfirmada, Is.Null);
            Assert.That(juego.BaseDatos.Partidas.Single().puntos, Is.Zero);
            Directory.Delete(temporal);

            var respuesta = juego.Partidas.Decidir(partida.id, 1, 1);
            Directory.CreateDirectory(temporal);
            Assert.Throws<ErrorJuego>(() => juego.RecorridosV2.ConfirmarPresentacion(partida.id, respuesta.Decision.id));
            var recuperada = AbrirSinIngresar();
            Assert.That(recuperada.BaseDatos.Partidas.Single().ultima_decision_presentada_id, Is.Zero);
            Assert.That(recuperada.BaseDatos.Decisiones.Single().id, Is.EqualTo(respuesta.Decision.id));
            Directory.Delete(temporal);
            Assert.That(juego.RecorridosV2.ConfirmarPresentacion(partida.id, respuesta.Decision.id)
                .PresentacionPendiente, Is.False);
        }

        ServicioJuego AbrirSinIngresar() => new ServicioJuego(Contenido, carpeta, catalogo);
    }
}
