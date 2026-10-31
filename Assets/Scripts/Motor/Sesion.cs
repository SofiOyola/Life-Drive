// Punto de entrada compartido por todas las escenas: crea el ServicioJuego una sola vez
// con las carpetas reales de Unity y recuerda la partida abierta entre escenas.
using System.IO;
using UnityEngine;

namespace LifeDrive.Motor
{
    public static class Sesion
    {
        static ServicioJuego juego;

        // Lanza ErrorJuego si ruta.json tiene errores o la base de datos no abre:
        // la pantalla de inicio lo muestra en vez de dejar jugar.
        public static ServicioJuego Juego => juego ??= new ServicioJuego(
            Path.Combine(Application.streamingAssetsPath, "Contenido"),
            Application.persistentDataPath);

        public static int PartidaId;   // partida en curso o la que se acaba de terminar

        public static void Cerrar()
        {
            juego?.Usuarios.CerrarSesion();
            PartidaId = 0;
        }

        // Carpeta donde se guardan los reportes PDF generados.
        public static string CarpetaReportes => Path.Combine(Application.persistentDataPath, "Reportes");
    }

    // Nombres de las escenas: deben coincidir con los archivos de Assets/Scenes y estar en Build Profiles.
    public static class Escenas
    {
        public const string Inicio = "Inicio";
        public const string Registro = "Registro";
        public const string Partida = "Partida";
        public const string Resultado = "Resultado";
        public const string AdminLogin = "AdminLogin";
        public const string Admin = "Admin";
    }
}
