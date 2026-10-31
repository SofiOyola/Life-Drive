// Pruebas de lógica sin ejecutar el juego: Window > General > Test Runner > EditMode > Run All.
using LifeDrive.Datos;
using NUnit.Framework;

namespace LifeDrive.Tests
{
    public class DatosTests
    {
        [TestCase("1098765432", true)]
        [TestCase("12345", true)]
        [TestCase("1234", false)]
        [TestCase("1234567890123456", false)]
        [TestCase("10.987.654", false)]
        [TestCase("12a45", false)]
        [TestCase("", false)]
        public void CedulaSoloAceptaEntre5y15Digitos(string cedula, bool esperado) =>
            Assert.AreEqual(esperado, Validacion.CedulaValida(cedula));

        [Test]
        public void RegistroExigeNombreCedulaYPersonaje()
        {
            Assert.IsNotNull(Validacion.ErrorRegistro("   ", "123456", 1));
            Assert.IsNotNull(Validacion.ErrorRegistro("Ana", "123456", 0));
            Assert.IsNull(Validacion.ErrorRegistro("Ana", "123456", 1));
        }
    }
}
