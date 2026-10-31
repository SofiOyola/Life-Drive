// Prototipo 3 · HU-03: solo usuario y contraseña; no hay autorregistro.
using LifeDrive.Motor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.UI
{
    public class PantallaAdminLogin : MonoBehaviour
    {
        [SerializeField] TMP_InputField campoUsuario;
        [SerializeField] TMP_InputField campoContrasena;   // Content Type: Password en el Inspector
        [SerializeField] Button botonIngresar;
        [SerializeField] Button botonVolver;
        [SerializeField] TMP_Text textoMensaje;

        void Start()
        {
            textoMensaje.text = "";
            botonIngresar.onClick.AddListener(Ingresar);
            botonVolver.onClick.AddListener(() => SceneManager.LoadScene(Escenas.Inicio));
        }

        void Ingresar()
        {
            try
            {
                Sesion.Juego.Usuarios.IniciarSesionAdministrador(campoUsuario.text, campoContrasena.text);
                SceneManager.LoadScene(Escenas.Admin);
            }
            catch (ErrorJuego e)
            {
                campoContrasena.text = "";
                textoMensaje.text = e.Message;
            }
        }
    }
}
