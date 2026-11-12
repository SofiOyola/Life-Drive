using LifeDrive.Mapas;
using LifeDrive.Mapas.Demo;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LifeDrive.Tests
{
    public class CaminoBifurcadoTests
    {
        [Test]
        public void TerrenoReduceVelocidadPausaYRecuperaAntesDeRevelarElMapa()
        {
            var go = new GameObject("Ensayo de movimiento");
            try
            {
                var demo = go.AddComponent<DemoRecorrido>();
                var so = new SerializedObject(demo);
                so.FindProperty("catalogo").objectReferenceValue = AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset");
                so.ApplyModifiedPropertiesWithoutUndo();
                demo.ControlExterno = demo.MostrarBifurcaciones = demo.CaminoDeteriorado = true;
                demo.MostrarMapa("R1B"); demo.Velocidad = 24; demo.Reproducir();
                demo.Avanzar(1);
                Assert.That(demo.Distancia, Is.EqualTo(18).Within(.001f));
                demo.Pausar(); demo.Avanzar(10);
                Assert.That(demo.Distancia, Is.EqualTo(18).Within(.001f));
                Assert.That(demo.SuspensionVisual, Is.Zero);
                demo.TransicionarA("R2C", false); demo.Avanzar(100);
                Assert.That(demo.MapaActual.Id, Is.EqualTo("R2C"));
                Assert.That(demo.OpacidadFundido, Is.EqualTo(1));
                Assert.That(demo.CaminoDeteriorado, Is.False);
                Assert.That(demo.VelocidadEfectiva, Is.EqualTo(24));
                demo.Avanzar(1);
                Assert.That(demo.EnTransicion, Is.False);
                Assert.That(demo.Distancia, Is.Zero, "La entrada visual no debe saltarse preguntas.");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [TestCase("S0")][TestCase("R1A")][TestCase("R1B")]
        [TestCase("R2A")][TestCase("R2B")][TestCase("R2C")][TestCase("R2D")]
        public void ElDesvioConectaSinSaltosYConservaLaViaPrincipal(string id)
        {
            var mapa = AssetDatabase.LoadAssetAtPath<MapaVisual>("Assets/Mapas/" + id + ".asset");
            var antes = (Vector2[])mapa.trayectoria.Clone();
            var principal = new RecorridoDemo(mapa, 6);
            Assert.That(mapa.fondoBifurcacion, Is.Not.Null, "La bifurcación debe estar pintada en el mapa.");
            Assert.That(mapa.trayectoriaDesvio.Length, Is.GreaterThanOrEqualTo(2));
            var desvio = new CaminoBifurcado(principal, .56f, mapa.trayectoriaDesvio);
            Assert.That(Vector2.Distance(desvio.Posicion(0), principal.Posicion(desvio.Inicio)), Is.LessThan(.001f));
            Assert.That(Vector2.Dot(desvio.Tangente(0), principal.Tangente(desvio.Inicio)), Is.GreaterThan(.98f));
            Assert.That(desvio.Posicion(desvio.Longitud).x, Is.EqualTo(480).Within(.001f));
            Assert.That(Vector2.Distance(desvio.Posicion(desvio.Longitud), principal.Posicion(principal.Longitud)), Is.GreaterThan(60));
            for (float d = 0; d < desvio.Longitud; d += 1)
            {
                var p = desvio.Posicion(d);
                Assert.That(p.x, Is.InRange(0, 480)); Assert.That(p.y, Is.InRange(0, 270));
                Assert.That(Vector2.Distance(p, desvio.Posicion(d + 1)), Is.LessThanOrEqualTo(1.001f));
            }
            Assert.That(mapa.trayectoria, Is.EqualTo(antes), "La carretera original no se debe sustituir.");
        }
    }
}
