using System;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Motor;
using UnityEngine;

namespace LifeDrive.Mapas.InterfazVisual
{
    // Isolated extension over the existing generic event/detail API, instantiated only by the pilot.
    // No new database, schema fields, central event constants or changes to Javier's services.
    public sealed class PersistenciaAparienciaPilotoV2
    {
        public const string EventoPerfil = "PILOTO_APARIENCIA_PERFIL_V1";
        public const string EventoPartida = "PILOTO_APARIENCIA_PARTIDA_V1";
        readonly ServicioJuego juego;
        public PersistenciaAparienciaPilotoV2(ServicioJuego juego) => this.juego = juego ?? throw new ArgumentNullException(nameof(juego));

        Jugador Actual()
        {
            juego.Usuarios.Exigir(Accion.Jugar);
            return juego.BaseDatos.Jugadores.Single(j => j.id == juego.Usuarios.Actual.JugadorId);
        }
        AparienciaJugadorPilotoV1 Leer(Jugador jugador, string tipo, int partidaId)
        {
            var evento = juego.BaseDatos.Eventos.Where(e => e.tipo == tipo && e.jugador_id == jugador.id && e.partida_id == partidaId)
                .OrderByDescending(e => e.id).FirstOrDefault();
            if (evento == null) return null;
            AparienciaJugadorPilotoV1 a;
            try { a = JsonUtility.FromJson<AparienciaJugadorPilotoV1>(evento.detalle); }
            catch (ArgumentException) { throw new ErrorJuego("La apariencia guardada está dañada. No se sobrescribió."); }
            VariantesAparienciaPilotoV1.Validar(a, jugador.personaje_id);
            return a;
        }
        public AparienciaJugadorPilotoV1 LeerPerfilActual() => Leer(Actual(), EventoPerfil, 0);

        public void GuardarPerfilActual(AparienciaJugadorPilotoV1 apariencia)
        {
            var jugador = Actual();
            VariantesAparienciaPilotoV1.Validar(apariencia, jugador.personaje_id);
            if (!juego.Contenido.personajes.Any(p => p.id == apariencia.personaje_id)) throw new ErrorJuego("Ficha original inexistente.");
            var previa = Leer(jugador, EventoPerfil, 0); // Never replace an unknown format with defaults.
            var json = JsonUtility.ToJson(apariencia);
            if (previa != null && JsonUtility.ToJson(previa) == json) return;
            juego.BaseDatos.Ejecutar(() => juego.Eventos.Registrar(EventoPerfil, juego.Usuarios.Actual, detalle: json));
        }

        public AparienciaJugadorPilotoV1 FijarPartida(int partidaId)
        {
            var partida = juego.BaseDatos.Partidas.SingleOrDefault(p => p.id == partidaId);
            juego.Usuarios.ExigirPartidaPropia(partida, Accion.Jugar);
            if (partida.contenido_version != 2) return null;
            var jugador = Actual();
            var fijada = Leer(jugador, EventoPartida, partidaId);
            if (fijada != null) return fijada;
            var perfil = LeerPerfilActual();
            if (perfil == null) return null; // Legacy player or match: absence is valid, not a new saved default.
            juego.BaseDatos.Ejecutar(() => juego.Eventos.Registrar(EventoPartida, juego.Usuarios.Actual,
                partidaId: partidaId, detalle: JsonUtility.ToJson(perfil)));
            return perfil;
        }

        public AparienciaJugadorPilotoV1 LeerPartida(int partidaId)
        {
            var resumen = juego.Desempeno.Resumen(partidaId); // Existing ownership/administrator permissions.
            return resumen.Partida.contenido_version == 2 ? Leer(resumen.Jugador, EventoPartida, partidaId) : null;
        }
    }
}
