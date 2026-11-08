#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LifeDrive.Tests
{
    public partial class AdministracionPilotoV2Tests
    {
        [UnityTest]
        public IEnumerator AuditoriaPdfConNombreMaximoDesdePanelReal()
        {
            Resolucion(1280); Activar();
            var nombre = new string('W', 60);
            var jugador = piloto.Juego.Usuarios.RegistrarJugador(nombre, "99003799", 1);
            var partida = Completar(true);
            piloto.Juego.Usuarios.CerrarSesion();
            yield return Login();
            Vista().busqueda.text = jugador.cedula; Click(Vista().buscar);
            Click(Vista().jugadores[0]); Click(Boton("Partida piloto 0")); Click(Vista().ver);
            Assert.That(Panel().ResumenVisible.Jugador.nombre, Is.EqualTo(nombre));
            Assert.That(Panel().ResumenVisible.Partida.id, Is.EqualTo(partida.id));
            Click(Vista().pdf);
            Assert.That(File.Exists(Panel().UltimoPdf), Is.True);
            var contenido = Encoding.GetEncoding(1252).GetString(File.ReadAllBytes(Panel().UltimoPdf));
            Assert.That(contenido, Does.StartWith("%PDF-1.4"));
            Assert.That(contenido, Does.Contain(nombre));
            Assert.That(contenido, Does.Contain(jugador.cedula));
            var destino = Path.GetFullPath(Path.Combine(Application.dataPath, "../TestEvidence/AuditoriaV2"));
            Directory.CreateDirectory(destino);
            File.Copy(Panel().UltimoPdf, Path.Combine(destino, "Pdf-nombre-largo.pdf"), true);
            yield return Captura("Nombre-largo");
            // Structural generation is verified here. Page layout is audited separately from the rendered PDF.
        }
    }
}
#endif
