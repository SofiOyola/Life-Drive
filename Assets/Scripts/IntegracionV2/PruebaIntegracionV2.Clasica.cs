using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Motor;
using UnityEngine;

namespace LifeDrive.IntegracionV2
{
    public sealed partial class PruebaIntegracionV2
    {
        bool esV1, entregaV1;
        Decision ultimaV1;
        float hitoV1;
        void AbrirV1()
        {
            var propias = juego.BaseDatos.Partidas.Where(p=>p.jugador_id==juego.Usuarios.Actual.JugadorId);
            var activa = propias.FirstOrDefault(p=>p.estado==EstadoPartida.EnCurso);
            var p = activa != null ? juego.Partidas.IniciarPartida() : propias.LastOrDefault(p=>p.contenido_version<=1) ?? juego.Partidas.IniciarPartida();
            if (p.contenido_version>1) throw new ErrorJuego("Esta partida requiere su entorno V2 original.");
            partidaId=p.id; RestaurarV1();
        }
        void RestaurarV1()
        {
            alAbrirPartida?.Invoke(partidaId); pausa=false; entregaV1=false;
            ultimaV1=juego.BaseDatos.Decisiones.Where(d=>d.partida_id==partidaId).OrderBy(d=>d.id).LastOrDefault();
            // V1 has no approved tramo->map/edge contract. Do not invent one or force the V2 tree.
            reproductor.MostrarMapa("S0");
            reproductor.PosicionarProgreso(Mathf.Min(.9f,.12f*juego.BaseDatos.Decisiones.Count(d=>d.partida_id==partidaId)));
            if (ultimaV1!=null) { Estado=Fase.Consecuencia; ActualizarVista(); }
            else PrepararSiguienteV1();
        }
        void PrepararSiguienteV1()
        {
            var p=juego.BaseDatos.Partidas.Single(p=>p.id==partidaId);
            if(p.estado==EstadoPartida.Finalizada)
            {
                entregaV1=true; reproductor.MostrarMapa("DELIVERY"); hitoV1=.82f;
            }
            else
            {
                situacion=juego.Partidas.SituacionActual(p);
                if(situacion==null) throw new ErrorJuego("La operación V1 sigue en curso pero no ofrece una situación pendiente.");
                hitoV1=Mathf.Min(.94f,.12f*(juego.BaseDatos.Decisiones.Count(d=>d.partida_id==partidaId)+1));
            }
            Estado=Fase.EnMarcha; reproductor.Reproducir(); ActualizarVista();
        }
        void AvanzarV1()
        {
            float restante=Mathf.Max(0,reproductor.Longitud*hitoV1-reproductor.Distancia);
            reproductor.Avanzar(Mathf.Min(Time.deltaTime,restante/reproductor.Velocidad));
            if(reproductor.Progreso<hitoV1-.00001f)return;
            reproductor.Pausar(); Estado=entregaV1?Fase.Entregada:Fase.Situacion; ActualizarVista();
        }
        void ActualizarGamificacionV1()
        {
            Gamificacion=null;
            if(partidaId==0)return;
            var r=juego.Desempeno.Resumen(partidaId);
            variacionTexto.text=ultimaV1==null?"Sin respuestas guardadas":ultimaV1.puntos_aplicados.ToString("+0;-0;0")+" pts guardados";
            variacionTexto.color=Crema;
            situacionesTexto.text="Situaciones resueltas: "+r.Historial.Count+(Estado==Fase.Entregada?" · Entrega presentada":" · V1");
            reconocimientosTexto.text="Buenas prácticas: "+r.BuenasPracticas+" / Ver reconocimientos";
            practicasTexto.text="Buenas prácticas registradas: "+r.BuenasPracticas+"\nInsignias otorgadas: ninguna.";
        }
    }
}
