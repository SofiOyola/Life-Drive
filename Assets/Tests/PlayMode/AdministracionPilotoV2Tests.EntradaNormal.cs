#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.IntegracionV2;
using LifeDrive.Motor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace LifeDrive.Tests
{
    public partial class AdministracionPilotoV2Tests
    {
        [UnityTearDown] public IEnumerator LimpiarEntradaNormalAdmin()
        {
            EntradaJuego.CarpetaPruebas = null;
            EntradaJuego.ClasicoEnPruebas = false;
            yield return null;
        }
        [UnityTest] public IEnumerator AdminDesdeInicioNormalV1([ValueSource(nameof(Resoluciones))] int resolucion)
            => ConsultarDesdeInicioNormal(resolucion, clasico: true);

        [UnityTest] public IEnumerator AdminDesdeInicioNormalV2([ValueSource(nameof(Resoluciones))] int resolucion)
            => ConsultarDesdeInicioNormal(resolucion, clasico: false);

        IEnumerator ConsultarDesdeInicioNormal(int resolucion, bool clasico)
        {
            if (clasico) EntradaJuego.ClasicoEnPruebas = true;
            Resolucion(resolucion);EntradaJuego.CarpetaPruebas=carpeta;
            Cargar("Inicio");yield return Pantalla("Inicio");
            piloto = Object.FindAnyObjectByType<FlujoJugadorPilotoV2>();
            Assert.That(piloto, Is.Not.Null);
            Assert.That(piloto.EsPilotoV2, Is.EqualTo(!clasico));
            var jugador = piloto.Juego.Usuarios.RegistrarJugador("Ensayo admin", "99004003", 1);
            var partida = clasico ? piloto.Juego.Partidas.IniciarPartida() : Completar(false);
            if (clasico)
            {
                for (int paso = 0; paso < 32 && partida.estado != EstadoPartida.Finalizada; paso++)
                {
                    var s = piloto.Juego.Partidas.SituacionActual(partida);
                    piloto.Juego.Partidas.Decidir(partida.id, s.id, s.alternativas[0].id);
                }
            }
            piloto.Juego.Usuarios.CerrarSesion();yield return Login();
            Click(Vista().jugadores[0]);Click(Boton("Partida piloto 0"));Click(Vista().ver);
            Assert.That(Panel().ResumenVisible.Jugador.id,Is.EqualTo(jugador.id));
            Assert.That(Panel().ResumenVisible.Partida.contenido_version, Is.EqualTo(clasico ? 1 : 2));
            Assert.That(Panel().ResumenVisible.Partida.puntos,Is.EqualTo(partida.puntos));
            Assert.That(Panel().ResumenVisible.Historial.Count,Is.EqualTo(piloto.Juego.BaseDatos.Decisiones.Count));
            yield return Captura(clasico ? "Entrada-normal-V1" : "Entrada-normal-V2");
            Click(Vista().volver);yield return Pantalla("Inicio");
            Assert.That(piloto.Juego.Usuarios.Actual.Rol,Is.EqualTo(Rol.Ninguno));
        }
    }
}
#endif
