// Usuarios y roles (S1-03, HU-01 a HU-04). Cada acción protegida exige un rol;
// la tabla de permisos es el único lugar donde se decide quién puede hacer qué.
using System.Collections.Generic;

namespace LifeDrive.Motor
{
    public enum Rol { Ninguno, Jugador, Administrador }

    public enum Accion
    {
        Jugar,                    // iniciar, retomar y decidir en una partida propia
        VerResumenPropio,         // resumen final de una partida propia
        ListarJugadores,
        VerPartidasDeJugador,
        VerResumenDeCualquiera,
        GenerarReporte,
        VerEventos,
    }

    public static class Permisos
    {
        static readonly Dictionary<Accion, Rol> RolRequerido = new Dictionary<Accion, Rol>
        {
            [Accion.Jugar] = Rol.Jugador,
            [Accion.VerResumenPropio] = Rol.Jugador,
            [Accion.ListarJugadores] = Rol.Administrador,
            [Accion.VerPartidasDeJugador] = Rol.Administrador,
            [Accion.VerResumenDeCualquiera] = Rol.Administrador,
            [Accion.GenerarReporte] = Rol.Administrador,
            [Accion.VerEventos] = Rol.Administrador,
        };

        public static bool Permite(Rol rol, Accion accion) => RolRequerido[accion] == rol;
    }

    // Quién está usando el juego en este momento.
    public class Usuario
    {
        public static readonly Usuario Anonimo = new Usuario { Rol = Rol.Ninguno };

        public Rol Rol;
        public int JugadorId;          // solo para Rol.Jugador
        public string Nombre;
        public string Identificador;   // cédula del jugador o usuario del administrador

        public string RolTexto => Rol == Rol.Jugador ? "JUGADOR" : Rol == Rol.Administrador ? "ADMINISTRADOR" : "NINGUNO";
    }
}
