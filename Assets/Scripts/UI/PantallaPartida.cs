// Prototipos 4, 5 y 6 · HU-05 a HU-08: barra con puntos siempre visible, ventana de situación y
// ventana de consecuencia antes de continuar. El mapa y la animación del vehículo se conectan en
// MostrarSituacion y MostrarConsecuencia (partida.tramo_actual_id indica dónde está el jugador).
using LifeDrive.Datos;
using LifeDrive.Motor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.UI
{
    public class PantallaPartida : MonoBehaviour
    {
        [Header("Barra superior")]
        [SerializeField] TMP_Text textoJugador;
        [SerializeField] TMP_Text textoPuntos;
        [SerializeField] TMP_Text textoTramo;

        [Header("Ventana de situación")]
        [SerializeField] GameObject panelSituacion;
        [SerializeField] TMP_Text textoTipo;
        [SerializeField] TMP_Text textoEnunciado;
        [SerializeField] Transform contenedorAlternativas;
        [SerializeField] Button prefabBoton;

        [Header("Ventana de consecuencia")]
        [SerializeField] GameObject panelConsecuencia;
        [SerializeField] TMP_Text textoVeredicto;
        [SerializeField] TMP_Text textoExplicacion;
        [SerializeField] GameObject distintivoBuenaPractica;
        [SerializeField] Button botonContinuar;

        [Header("Errores")]
        [SerializeField] TMP_Text textoError;

        ServicioJuego juego;
        Partida partida;

        void Start()
        {
            juego = Sesion.Juego;
            if (juego.Usuarios.Actual.Rol != Rol.Jugador) { SceneManager.LoadScene(Escenas.Inicio); return; }

            textoJugador.text = juego.Usuarios.Actual.Nombre;
            textoError.text = "";
            panelConsecuencia.SetActive(false);
            botonContinuar.onClick.AddListener(Continuar);

            try
            {
                partida = juego.Partidas.IniciarPartida();   // nueva o la que quedó en curso
            }
            catch (ErrorJuego e)
            {
                textoError.text = e.Message;
                return;
            }
            Sesion.PartidaId = partida.id;
            Continuar();
        }

        void ActualizarBarra()
        {
            textoPuntos.text = $"Puntos de seguridad: {partida.puntos}";
            textoTramo.text = juego.Partidas.Tramo(partida.tramo_actual_id).nombre;
        }

        void Continuar()
        {
            panelConsecuencia.SetActive(false);
            var s = juego.Partidas.SituacionActual(partida);
            if (s == null) { SceneManager.LoadScene(Escenas.Resultado); return; }
            MostrarSituacion(s);
        }

        void MostrarSituacion(Situacion s)
        {
            ActualizarBarra();
            textoTipo.text = Titulo(s.tipo);
            textoEnunciado.text = s.enunciado;
            BotonesLista.Limpiar(contenedorAlternativas);
            foreach (var a in s.alternativas)
                BotonesLista.Agregar(prefabBoton, contenedorAlternativas, a.descripcion, () => Decidir(s.id, a.id));
            panelSituacion.SetActive(true);
        }

        void Decidir(int situacionId, int alternativaId)
        {
            try
            {
                textoError.text = "";
                var r = juego.Partidas.Decidir(partida.id, situacionId, alternativaId);
                partida = r.Partida;
                MostrarConsecuencia(r);
            }
            catch (ErrorJuego e)
            {
                textoError.text = e.Message;   // p. ej. no se pudo guardar: el jugador vuelve a elegir sin perder nada
            }
        }

        void MostrarConsecuencia(ResultadoDecision r)
        {
            ActualizarBarra();
            panelSituacion.SetActive(false);
            var d = r.Decision;
            var signo = d.puntos_aplicados >= 0 ? "+" : "";
            textoVeredicto.text = (d.es_acertada ? "Decisión segura" : "Decisión insegura") + $"  {signo}{d.puntos_aplicados} puntos";
            textoExplicacion.text = d.consecuencia
                + (r.CambioRuta ? $"\nTu camino continúa por: {r.TramoDestino}" : "")
                + (d.retraso_aplicado > 0 ? $"\nRetraso: {d.retraso_aplicado} min" : "");
            distintivoBuenaPractica.SetActive(d.es_buena_practica);
            panelConsecuencia.SetActive(true);
        }

        static string Titulo(string tipo) => tipo switch
        {
            TipoSituacion.Pregunta => "Pregunta de conocimiento",
            TipoSituacion.DecisionRuta => "Decisión de ruta",
            TipoSituacion.Riesgo => "Riesgo en la vía",
            TipoSituacion.Emergencia => "Emergencia",
            _ => tipo,
        };
    }
}
