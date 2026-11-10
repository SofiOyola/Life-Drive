// Validaciones del formulario de registro (HU-02). El servidor repite las mismas reglas;
// estas solo evitan una petición inútil y dan el mensaje al instante.
using System.Text.RegularExpressions;

namespace LifeDrive.Datos
{
    public static class Validacion
    {
        static readonly Regex Cedula = new Regex("^[0-9]{5,15}$");

        public static bool CedulaValida(string cedula) => cedula != null && Cedula.IsMatch(cedula);

        public const int NombreMaximo = 60; // el mismo tope del campo de Registro

        public static bool NombreValido(string nombre) => !string.IsNullOrWhiteSpace(nombre);

        // Devuelve el primer error del formulario o null si está completo.
        public static string ErrorRegistro(string nombre, string cedula, int personajeId)
        {
            if (!NombreValido(nombre)) return "Escribe tu nombre";
            if (nombre.Length > NombreMaximo) return $"El nombre no puede tener más de {NombreMaximo} caracteres";
            if (!CedulaValida(cedula)) return "La cédula debe tener entre 5 y 15 dígitos, sin puntos ni letras";
            if (personajeId <= 0) return "Elige un personaje";
            return null;
        }
    }
}
