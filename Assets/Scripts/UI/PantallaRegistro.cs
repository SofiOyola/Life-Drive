// Prototipo 2 · HU-02: nombre, cédula y personaje. Al registrarse va directo a la partida.
using LifeDrive.Motor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.UI
{
    public class PantallaRegistro : MonoBehaviour
    {
        [SerializeField] TMP_InputField campoNombre;
        [SerializeField] TMP_InputField campoCedula;     // Content Type: Integer Number en el Inspector
        [SerializeField] Transform contenedorPersonajes;
        [SerializeField] Button prefabBoton;
        [SerializeField] Button botonIniciar;
        [SerializeField] Button botonVolver;
        [SerializeField] TMP_Text textoMensaje;

        int personajeId;

        void Start()
        {
            botonIniciar.onClick.AddListener(Registrar);
            botonVolver.onClick.AddListener(() => SceneManager.LoadScene(Escenas.Inicio));
            textoMensaje.text = "";

            BotonesLista.Limpiar(contenedorPersonajes);
            foreach (var p in Sesion.Juego.Contenido.personajes)
            {
                Button boton = null;
                boton = BotonesLista.Agregar(prefabBoton, contenedorPersonajes, p.nombre, () =>
                {
                    personajeId = p.id;
                    foreach (var b in contenedorPersonajes.GetComponentsInChildren<Button>()) b.interactable = b != boton;
                });
            }
        }

        void Registrar()
        {
            try
            {
                Sesion.Juego.Usuarios.RegistrarJugador(campoNombre.text, campoCedula.text, personajeId);
                SceneManager.LoadScene(Escenas.Partida);
            }
            catch (ErrorJuego e)
            {
                textoMensaje.text = e.Message;
            }
        }
    }
}
