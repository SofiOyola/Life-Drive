using System;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Motor;
using UnityEngine;

namespace LifeDrive.Mapas.InterfazVisual
{
    // Guarda la apariencia en el modelo: Jugador.apariencia (perfil vigente) y Partida.apariencia (copia al iniciar).
    // Sirve igual para V1 y V2. Un registro sin apariencia (jugador o partida antiguos) devuelve null y no se escribe nada.
    // El nombre conserva "PilotoV2" por compatibilidad con el resto del flujo.
    public sealed class PersistenciaAparienciaPilotoV2
    {
        readonly ServicioJuego juego;
        public PersistenciaAparienciaPilotoV2(ServicioJuego juego) => this.juego = juego ?? throw new ArgumentNullException(nameof(juego));

        Jugador Actual()
        {
            juego.Usuarios.Exigir(Accion.Jugar);
            return juego.BaseDatos.Jugadores.Single(j => j.id == juego.Usuarios.Actual.JugadorId);
        }
        static AparienciaJugadorPilotoV1 Leer(string json, Jugador jugador)
        {
            if (string.IsNullOrEmpty(json)) return null;
            AparienciaJugadorPilotoV1 a;
            try { a = JsonUtility.FromJson<AparienciaJugadorPilotoV1>(json); }
            catch (ArgumentException) { throw new ErrorJuego("La apariencia guardada está dañada. No se sobrescribió."); }
            VariantesAparienciaPilotoV1.Validar(a, jugador.personaje_id);
            return a;
        }
        public AparienciaJugadorPilotoV1 LeerPerfilActual() { var j = Actual(); return Leer(j.apariencia, j); }

        public void GuardarPerfilActual(AparienciaJugadorPilotoV1 apariencia)
        {
            var jugador = Actual();
            VariantesAparienciaPilotoV1.Validar(apariencia, jugador.personaje_id);
            if (!juego.Contenido.personajes.Any(p => p.id == apariencia.personaje_id)) throw new ErrorJuego("Ficha original inexistente.");
            Leer(jugador.apariencia, jugador); // Nunca reemplaza un formato desconocido o dañado por valores por defecto.
            var json = JsonUtility.ToJson(apariencia);
            if (jugador.apariencia == json) return;
            juego.BaseDatos.Ejecutar(() => jugador.apariencia = json);
        }

        public AparienciaJugadorPilotoV1 FijarPartida(int partidaId)
        {
            var partida = juego.BaseDatos.Partidas.SingleOrDefault(p => p.id == partidaId);
            juego.Usuarios.ExigirPartidaPropia(partida, Accion.Jugar);
            var jugador = Actual();
            var fijada = Leer(partida.apariencia, jugador);
            if (fijada != null) return fijada;
            var perfil = LeerPerfilActual();
            if (perfil == null) return null; // Jugador antiguo: la ausencia es válida, no se guarda un valor por defecto.
            var json = JsonUtility.ToJson(perfil);
            juego.BaseDatos.Ejecutar(() => partida.apariencia = json);
            return perfil;
        }

        public AparienciaJugadorPilotoV1 LeerPartida(int partidaId)
        {
            var resumen = juego.Desempeno.Resumen(partidaId); // Mismos permisos de dueño/administrador que el resto.
            return Leer(resumen.Partida.apariencia, resumen.Jugador);
        }
    }
}
