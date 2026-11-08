using System;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Motor;
using UnityEngine;

namespace LifeDrive.Mapas
{
    public sealed class BuenaPracticaRegistradaV2
    {
        public int DecisionId { get; internal set; }
        public int EventoId { get; internal set; }
        public string Descripcion { get; internal set; }
    }

    // Read-only projection. No rewards, thresholds, point evaluation or writes of any kind.
    public sealed class GamificacionPartidaV2
    {
        public int Puntos { get; private set; }
        public int SituacionesResueltas { get; private set; }
        public Decision UltimaDecision { get; private set; }
        public BuenaPracticaRegistradaV2[] BuenasPracticas { get; private set; }

        public static GamificacionPartidaV2 Consultar(ServicioJuego juego, int partidaId)
        {
            var resumen = juego.Desempeno.Resumen(partidaId); // Existing ownership / administrator authorization.
            if (resumen.Partida.contenido_version != 2) throw new ErrorJuego("Esta presentación requiere una partida V2.");
            var decisiones = resumen.Historial.Select(h => h.Decision).OrderBy(d => d.id).ToArray();
            var eventos = juego.BaseDatos.Eventos.Where(e => e.partida_id == partidaId &&
                e.jugador_id == resumen.Jugador.id && e.tipo == TipoEvento.BuenaPractica).ToArray();
            return new GamificacionPartidaV2
            {
                Puntos = resumen.Partida.puntos,
                SituacionesResueltas = decisiones.Select(d => d.situacion_id).Distinct().Count(),
                UltimaDecision = decisiones.LastOrDefault(),
                BuenasPracticas = decisiones.Where(d => d.es_buena_practica && eventos.Any(e => e.situacion_id == d.situacion_id))
                    .GroupBy(d => d.id).Select(g => g.First()).Select(d => new BuenaPracticaRegistradaV2
                    {
                        DecisionId = d.id,
                        EventoId = eventos.First(e => e.situacion_id == d.situacion_id).id,
                        Descripcion = d.alternativa
                    }).ToArray()
            };
        }

        // Presentation milestones, shared with the existing scene. Not an educational or KPI rule.
        public static float HitoVisual(int situacionId) => situacionId switch { 1 or 3 => .24f, 7 => .82f, _ => .56f };

        // Five visual stages, each one fifth. Normalize to the scene's departure/arrival stop.
        // During a fade the source stage is complete; acknowledging its destination starts the next at zero.
        public static float Progreso(EstadoRecorridoV2 estado, float progresoMapa, bool enTransicion)
        {
            if (estado == null) return 0;
            if (estado.CierrePresentado) return 1;
            float limite = estado.MapaActual == "DELIVERY" ? .82f : .56f;
            float local = enTransicion ? 1 : Mathf.Clamp01((float.IsFinite(progresoMapa) ? progresoMapa : 0) / limite);
            return Mathf.Clamp01((estado.Historial.Length - 1 + local) / 5f);
        }
    }
}
