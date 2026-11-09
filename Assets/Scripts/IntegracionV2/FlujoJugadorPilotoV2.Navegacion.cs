using System;
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using TMPro;

namespace LifeDrive.IntegracionV2
{
    public sealed partial class FlujoJugadorPilotoV2
    {
        bool regresoDePartida;

        void VolverAlInicioDesdePartida()
        {
            if (navegando) return;
            Juego.Usuarios.CerrarSesion();
            regresoDePartida = true;
            Navegar("Inicio");
        }

        void ActualizarMensajeRegistro(PersonalizacionRegistroUI vista, string mensajeSinPartida)
        {
            var jugador = Juego.BaseDatos.Jugadores.FirstOrDefault(j => j.cedula == vista.documento.text.Trim());
            var propias = Juego.BaseDatos.Partidas.Where(p => p.jugador_id == jugador?.id);
            var activa = propias.FirstOrDefault(p => p.estado == EstadoPartida.EnCurso);
            var conflicto = MensajeVersionIncompatible(activa);
            if (conflicto != null)
            {
                vista.mensaje.text = conflicto;
                return;
            }
            var ultima = activa ?? propias.LastOrDefault(p => EsPilotoV2 ? p.contenido_version == 2 : p.contenido_version <= 1);
            if (ultima == null)
            {
                vista.mensaje.text = mensajeSinPartida;
                return;
            }
            bool pendiente = activa != null || EsPilotoV2 &&
                Juego.BaseDatos.Decisiones.Any(d => d.partida_id == ultima.id && d.id > ultima.ultima_decision_presentada_id);
            vista.mensaje.text = pendiente ? "Al confirmar tus datos retomarás tu operación guardada." :
                "Al confirmar tus datos verás tu última entrega.";
        }

        void ComprobarVersionActiva(int jugadorId)
        {
            var activa = Juego.BaseDatos.Partidas.FirstOrDefault(p => p.jugador_id == jugadorId && p.estado == EstadoPartida.EnCurso);
            var conflicto = MensajeVersionIncompatible(activa);
            if (conflicto != null) throw new ErrorJuego(conflicto);
        }

        string MensajeVersionIncompatible(Partida activa)
        {
            if (activa == null) return null;
            if (!EsPilotoV2 && activa.contenido_version == 2)
                return "Retoma tu operación en el piloto de carreteras.";
            if (EsPilotoV2 && activa.contenido_version != 2)
                return "Retoma tu operación en el flujo original.";
            return null;
        }

        void PrepararNuevaOperacion(PrevisualizacionResultado vista)
        {
            var boton = vista.selectores[2]; // Reuse the third approved visual control, with no final-category rule.
            boton.gameObject.SetActive(true);
            boton.GetComponentInChildren<TMP_Text>().text = "Nueva operación";
            bool solicitada = false;
            boton.onClick.AddListener(() =>
            {
                if (navegando || solicitada) return;
                try
                {
                    if (!CierreDisponible())
                        throw new ErrorJuego("Completa la presentación de la entrega antes de iniciar otra operación.");
                    if (Juego.BaseDatos.Partidas.Any(p => p.jugador_id == Juego.Usuarios.Actual.JugadorId && p.estado == EstadoPartida.EnCurso))
                        throw new ErrorJuego("Ya tienes una operación en curso. Vuelve a Inicio para retomarla.");
                    // Existing engine command creates only after closure, otherwise resumes. No forced reset or deletion.
                    solicitada = true;
                    PartidaId = (EsPilotoV2 ? Juego.RecorridosV2.IniciarPartida() : Juego.Partidas.IniciarPartida()).id;
                    regresoDePartida = false;
                    Navegar("PruebaIntegracionV2");
                }
                catch (Exception e) when (e is ErrorJuego || e is IOException || e is UnauthorizedAccessException)
                { solicitada = false; vista.aviso.text = e.Message; }
            });
        }
    }
}
