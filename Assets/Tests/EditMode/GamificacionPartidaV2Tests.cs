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
    public class GamificacionPartidaV2Tests
    {
        string carpeta, contenido, datos;
        CatalogoMapas catalogo;
        [SetUp] public void Preparar()
        {
            carpeta = Path.Combine(Application.temporaryCachePath, "LifeDrive-GamificacionTests", Guid.NewGuid().ToString("N"));
            contenido = Path.Combine(carpeta, "Contenido"); datos = Path.Combine(carpeta, "Datos");
            Directory.CreateDirectory(contenido);
            foreach (var archivo in new[] { "ruta.json", "ruta-v2-candidata.json" })
                File.Copy(Path.Combine(Application.streamingAssetsPath, "Contenido", archivo), Path.Combine(contenido, archivo));
            catalogo = AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset");
        }
        [TearDown] public void Limpiar() { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
        ServicioJuego Abrir()
        {
            var juego = new ServicioJuego(contenido, datos, catalogo);
            juego.Usuarios.RegistrarJugador("Gamificación ficticia", "990035", 1); return juego;
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void OchoRutasConProgresoContinuoYReconstruccionDesdeHitos(int ruta)
        {
            var juego = Abrir(); var partida = juego.RecorridosV2.IniciarPartida();
            int bifurcacion = 0, respuestas = 0; float previo = 0, local = 0;
            for (int paso = 0; paso < 8; paso++)
            {
                var estado = juego.RecorridosV2.Consultar(partida.id);
                if (estado.CierrePresentado) break;
                var situacion = juego.Partidas.SituacionActual(partida);
                float hito = GamificacionPartidaV2.HitoVisual(situacion.id);
                for (int muestra = 0; muestra <= 10; muestra++)
                {
                    float progreso = GamificacionPartidaV2.Progreso(estado, Mathf.Lerp(local, hito, muestra / 10f), false);
                    Assert.That(progreso + .00001f, Is.GreaterThanOrEqualTo(previo)); previo = progreso;
                }
                int indice = situacion.id == 5 || situacion.id >= 101 ? (ruta >> (2 - bifurcacion++)) & 1 : 0;
                var d = juego.Partidas.Decidir(partida.id, situacion.id, situacion.alternativas[indice].id).Decision; respuestas++;
                var presentacion = GamificacionPartidaV2.Consultar(juego, partida.id);
                Assert.That(presentacion.Puntos, Is.EqualTo(partida.puntos));
                Assert.That(presentacion.SituacionesResueltas, Is.EqualTo(respuestas));
                int eventos = juego.BaseDatos.Eventos.Count;
                Assert.That(juego.Partidas.Decidir(partida.id, situacion.id, d.alternativa_id).Repetida, Is.True);
                Assert.That(juego.BaseDatos.Eventos.Count, Is.EqualTo(eventos));
                var reabierto = Abrir(); var estadoReabierto = reabierto.RecorridosV2.Consultar(partida.id);
                int eventosAlReabrir = reabierto.BaseDatos.Eventos.Count;
                var repetida = GamificacionPartidaV2.Consultar(reabierto, partida.id);
                Assert.That(repetida.BuenasPracticas.Select(b => b.DecisionId), Is.EqualTo(presentacion.BuenasPracticas.Select(b => b.DecisionId)));
                Assert.That(GamificacionPartidaV2.Progreso(estadoReabierto, hito, false), Is.EqualTo(previo).Within(.00001f));
                Assert.That(reabierto.BaseDatos.Eventos.Count, Is.EqualTo(eventosAlReabrir));
                juego = reabierto; partida = juego.BaseDatos.Partidas.Single(p => p.id == partida.id);
                estado = juego.RecorridosV2.Consultar(partida.id);
                bool cambia = estado.DestinoVisualConfirmado != null;
                if (cambia) Assert.That(GamificacionPartidaV2.Progreso(estado, 0, true), Is.EqualTo(previo).Within(.00001f));
                estado = juego.RecorridosV2.ConfirmarPresentacion(partida.id, d.id);
                local = cambia ? 0 : hito;
                Assert.That(GamificacionPartidaV2.Progreso(estado, local, false), Is.EqualTo(previo).Within(.00001f));
            }
            var final = juego.RecorridosV2.Consultar(partida.id);
            Assert.That(final.CierrePresentado, Is.True); Assert.That(final.MapaActual, Is.EqualTo("DELIVERY"));
            Assert.That(GamificacionPartidaV2.Progreso(final, 0, false), Is.EqualTo(1));
            var gamificacion = GamificacionPartidaV2.Consultar(juego, partida.id);
            Assert.That(gamificacion.BuenasPracticas.Select(b => b.DecisionId).Distinct().Count(), Is.EqualTo(gamificacion.BuenasPracticas.Length));
        }

        [TestCase(1, 20, true)] [TestCase(2, 0, false)]
        public void ProyectaPuntosYBuenasPracticasSinEscribir(int alternativa, int puntos, bool buena)
        {
            var juego = Abrir(); var p = juego.RecorridosV2.IniciarPartida();
            juego.Partidas.Decidir(p.id, 1, alternativa);
            int eventos = juego.BaseDatos.Eventos.Count;
            for (int i = 0; i < 5; i++)
            {
                var g = GamificacionPartidaV2.Consultar(juego, p.id);
                Assert.That(g.Puntos, Is.EqualTo(puntos)); Assert.That(g.UltimaDecision.puntos_aplicados, Is.EqualTo(puntos));
                Assert.That(g.BuenasPracticas.Length, Is.EqualTo(buena ? 1 : 0));
            }
            Assert.That(juego.BaseDatos.Eventos.Count, Is.EqualTo(eventos));
            Assert.That(juego.BaseDatos.Decisiones.Count, Is.EqualTo(1));
        }

        [Test]
        public void PenalizacionRealSinReconocimientoYCompatibilidadV1()
        {
            var juego = Abrir(); var p = juego.RecorridosV2.IniciarPartida();
            var primera = juego.Partidas.Decidir(p.id, 1, 2).Decision;
            juego.RecorridosV2.ConfirmarPresentacion(p.id, primera.id);
            juego.Partidas.Decidir(p.id, 101, 102);
            var g = GamificacionPartidaV2.Consultar(juego, p.id);
            Assert.That(g.Puntos, Is.EqualTo(-20)); Assert.That(g.UltimaDecision.puntos_aplicados, Is.EqualTo(-20));
            Assert.That(g.BuenasPracticas, Is.Empty);
            var normal = new ServicioJuego(contenido, Path.Combine(carpeta, "V1"));
            normal.Usuarios.RegistrarJugador("V1 ficticio", "99003501", 1);
            var v1 = normal.Partidas.IniciarPartida(); Assert.That(v1.contenido_version, Is.EqualTo(1));
            Assert.Throws<ErrorJuego>(() => GamificacionPartidaV2.Consultar(normal, v1.id));
            Assert.That(normal.BaseDatos.Decisiones, Is.Empty);
        }
    }
}
