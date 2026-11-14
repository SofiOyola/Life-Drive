using System.Collections;
using System.IO;
using LifeDrive.Datos;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace LifeDrive.Motor
{
    // Primera escena del juego. En el navegador, StreamingAssets no se puede leer con File.*: se descargan los JSON del contenido
    // y se guardan en la carpeta que usa el resto del juego (RutasContenido.Carpeta). En las demás plataformas pasa a Inicio al instante.
    public sealed class CargadorWeb : MonoBehaviour
    {
        string mensaje = "Cargando…";

        IEnumerator Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Directory.CreateDirectory(RutasContenido.Carpeta);
            foreach (var nombre in RutasContenido.Archivos)
            {
                using (var peticion = UnityWebRequest.Get(Application.streamingAssetsPath + "/Contenido/" + nombre))
                {
                    yield return peticion.SendWebRequest();
                    if (peticion.result != UnityWebRequest.Result.Success)
                    {
                        mensaje = "No se pudo cargar el contenido del juego (" + nombre + "). Recarga la página.";
                        yield break;
                    }
                    File.WriteAllBytes(Path.Combine(RutasContenido.Carpeta, nombre), peticion.downloadHandler.data);
                }
            }
#endif
            SceneManager.LoadScene("Inicio");
            yield break;
        }

        void OnGUI()
        {
            var estilo = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.Max(18, Screen.height / 30) };
            estilo.normal.textColor = new Color32(245, 235, 207, 255);
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), mensaje, estilo);
        }
    }
}
