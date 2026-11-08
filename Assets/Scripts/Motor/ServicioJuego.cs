// Raíz de la lógica de negocio: carga el contenido, abre la base de datos y conecta los servicios.
// Las pantallas usan Sesion.Juego.Usuarios, .Partidas y .Desempeno; nunca tocan archivos.
using System;
using System.IO;
using LifeDrive.Datos;
using LifeDrive.Mapas;

namespace LifeDrive.Motor
{
    // Error que debe ver el usuario (mensaje en español, listo para mostrar en pantalla).
    public class ErrorJuego : Exception
    {
        public ErrorJuego(string mensaje, Exception causa = null) : base(mensaje, causa) { }
    }

    public static class Transacciones
    {
        // Ejecuta una transacción y traduce sus fallos a mensajes para el usuario.
        // Si algo falla no queda nada guardado a medias, así que el usuario puede reintentar.
        public static T Ejecutar<T>(this BaseDatos db, Func<T> cambios)
        {
            try { return db.Transaccion(cambios); }
            catch (ErrorIntegridad e) { throw new ErrorJuego(e.Message, e); }
            catch (IOException e) { throw new ErrorJuego("No se pudo guardar el progreso. Intenta de nuevo.", e); }
            catch (UnauthorizedAccessException e) { throw new ErrorJuego("No se pudo guardar el progreso. Intenta de nuevo.", e); }
        }

        public static void Ejecutar(this BaseDatos db, Action cambios) => db.Ejecutar(() => { cambios(); return 0; });
    }

    public class ServicioJuego
    {
        public Contenido Contenido { get; }
        public BaseDatos BaseDatos { get; }
        public RegistroEventos Eventos { get; }
        public ServicioUsuarios Usuarios { get; }
        public ServicioPartidas Partidas { get; }
        public ServicioDesempeno Desempeno { get; }
        public ReanudacionRecorridosV2 RecorridosV2 { get; }

        public ServicioJuego(string carpetaContenido, string carpetaDatos)
            : this(carpetaContenido, carpetaDatos, null) { }

        public ServicioJuego(string carpetaContenido, string carpetaDatos, CatalogoMapas catalogoV2)
        {
            var almacen = new Almacen(carpetaContenido);
            Contenido = almacen.CargarContenido();
            var problemas = ValidadorRuta.Revisar(Contenido);
            if (problemas.Count > 0) throw new ErrorJuego("El contenido de la ruta tiene errores:\n- " + string.Join("\n- ", problemas));

            PaqueteRecorridos paquete = null;
            if (catalogoV2 != null)
            {
                try
                {
                    paquete = PaqueteRecorridos.Cargar(carpetaContenido, 2);
                    paquete.Validar(catalogoV2);
                }
                catch (Exception e) when (e is IOException || e is ArgumentException || e is InvalidDataException)
                {
                    throw new ErrorJuego("El paquete de recorridos V2 no está disponible o es inválido.", e);
                }
            }

            try { BaseDatos = new BaseDatos(carpetaDatos); }
            catch (IOException e) { throw new ErrorJuego("No se pudo abrir la base de datos: " + e.Message, e); }

            Eventos = new RegistroEventos(BaseDatos);
            Usuarios = new ServicioUsuarios(BaseDatos, Contenido, almacen.CargarAdministradores(), Eventos);
            Partidas = new ServicioPartidas(BaseDatos, Contenido, Usuarios, Eventos, paquete?.contenido, paquete?.Huella);
            Desempeno = new ServicioDesempeno(BaseDatos, Contenido, Usuarios, Eventos, Partidas.Tramo);
            if (paquete != null)
                RecorridosV2 = new ReanudacionRecorridosV2(BaseDatos, Usuarios, Partidas, paquete, catalogoV2);
        }
    }
}
