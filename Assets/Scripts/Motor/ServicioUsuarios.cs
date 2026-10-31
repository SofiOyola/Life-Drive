// Módulo "Usuarios y acceso" (S2-03): registro del jugador, sesión del administrador y permisos.
using System;
using System.Linq;
using LifeDrive.Datos;

namespace LifeDrive.Motor
{
    public class ServicioUsuarios
    {
        readonly BaseDatos db;
        readonly Contenido contenido;
        readonly ListaAdministradores administradores;
        readonly RegistroEventos eventos;

        public Usuario Actual { get; private set; } = Usuario.Anonimo;

        public ServicioUsuarios(BaseDatos db, Contenido contenido, ListaAdministradores administradores, RegistroEventos eventos)
        {
            this.db = db;
            this.contenido = contenido;
            this.administradores = administradores;
            this.eventos = eventos;
        }

        static string Normalizar(string s) => string.Join(" ", (s ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries));

        // HU-02. El jugador no tiene contraseña: la cédula lo identifica.
        // Si la cédula ya existe con el mismo nombre, vuelve a entrar (puede tener varias partidas).
        public Jugador RegistrarJugador(string nombre, string cedula, int personajeId)
        {
            nombre = Normalizar(nombre);
            cedula = (cedula ?? "").Trim();
            var error = Validacion.ErrorRegistro(nombre, cedula, personajeId);
            if (error != null) throw new ErrorJuego(error);
            if (!contenido.personajes.Any(p => p.id == personajeId)) throw new ErrorJuego("Elige un personaje válido");

            var existente = db.Jugadores.FirstOrDefault(j => j.cedula == cedula);
            if (existente != null && !string.Equals(Normalizar(existente.nombre), nombre, StringComparison.CurrentCultureIgnoreCase))
                throw new ErrorJuego("Esta cédula ya está registrada con otro nombre");

            var jugador = db.Ejecutar(() =>
            {
                var j = existente ?? db.Insertar(new Jugador
                {
                    nombre = nombre, cedula = cedula, personaje_id = personajeId, fecha_registro = RegistroEventos.Ahora(),
                });
                var quien = ComoJugador(j);
                eventos.Registrar(existente == null ? TipoEvento.JugadorRegistrado : TipoEvento.IngresoJugador, quien,
                    detalle: existente == null ? $"Personaje {personajeId}" : null);
                return j;
            });
            Actual = ComoJugador(jugador);
            return jugador;
        }

        static Usuario ComoJugador(Jugador j) =>
            new Usuario { Rol = Rol.Jugador, JugadorId = j.id, Nombre = j.nombre, Identificador = j.cedula };

        // HU-03. Las cuentas se crean desde el Editor (Life Drive > Administradores), nunca desde el juego.
        public void IniciarSesionAdministrador(string usuario, string contrasena)
        {
            usuario = (usuario ?? "").Trim();
            var cuenta = administradores.administradores.FirstOrDefault(a => a.usuario == usuario);
            var valida = cuenta != null && Contrasenas.Verificar(cuenta, contrasena);
            var quien = new Usuario { Rol = valida ? Rol.Administrador : Rol.Ninguno, Nombre = usuario, Identificador = usuario };
            db.Ejecutar(() => eventos.Registrar(valida ? TipoEvento.SesionIniciada : TipoEvento.SesionFallida, quien));
            if (!valida) throw new ErrorJuego("Usuario o contraseña incorrectos");
            Actual = quien;
        }

        public void CerrarSesion()
        {
            if (Actual.Rol != Rol.Ninguno)
            {
                try { db.Ejecutar(() => eventos.Registrar(TipoEvento.SesionCerrada, Actual)); }
                catch (ErrorJuego) { /* cerrar sesión no depende de poder registrarlo */ }
            }
            Actual = Usuario.Anonimo;
        }

        // HU-04. Rechaza la acción si el rol actual no la permite; el intento queda en el registro de eventos.
        public void Exigir(Accion accion)
        {
            if (!Permisos.Permite(Actual.Rol, accion)) Denegar(accion, 0);
        }

        // Para acciones sobre una partida: además del rol, la partida debe ser del jugador actual.
        public void ExigirPartidaPropia(Partida p, Accion accion)
        {
            Exigir(accion);
            if (p == null || p.jugador_id != Actual.JugadorId) Denegar(accion, p?.id ?? 0);
        }

        void Denegar(Accion accion, int partidaId)
        {
            try { db.Ejecutar(() => eventos.Registrar(TipoEvento.AccesoDenegado, Actual, partidaId, detalle: accion.ToString())); }
            catch (ErrorJuego) { /* el rechazo no depende de poder registrarlo */ }
            throw new ErrorJuego("No tienes permiso para esta acción");
        }
    }
}
