// Prototipo 1 · HU-01: separa desde el inicio el flujo del jugador y el del administrador.
// También carga el contenido: si ruta.json tiene errores, los muestra y no deja jugar.
using LifeDrive.Motor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.UI
{
    public class PantallaInicio : MonoBehaviour
    {
        [SerializeField] Button botonJugar;
        [SerializeField] Button botonAdministrador;
        [SerializeField] TMP_Text textoMensaje;

        void Start()
        {
            botonJugar.onClick.AddListener(() => SceneManager.LoadScene(Escenas.Registro));
            botonAdministrador.onClick.AddListener(() => SceneManager.LoadScene(Escenas.AdminLogin));
            try
            {
                Sesion.Cerrar();
                _ = Sesion.Juego;   // carga y valida el contenido
                textoMensaje.text = "";
            }
            catch (ErrorJuego e)
            {
                botonJugar.interactable = false;
                botonAdministrador.interactable = false;
                textoMensaje.text = e.Message;
                Debug.LogError(e.Message);
            }
        }
    }
}
