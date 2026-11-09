using LifeDrive.IntegracionV2;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace LifeDrive.Editor.InterfazVisual
{
    public static class FlujoJugadorPilotoV2Editor
    {
        [MenuItem("Life Drive/Piloto V2/Abrir flujo del jugador")]
        public static void Abrir()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/Scenes/Inicio.unity");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Life Drive/Piloto V2/Abrir flujo del jugador", true)]
        static bool ValidarAbrir() => !EditorApplication.isPlayingOrWillChangePlaymode;

        [MenuItem("Life Drive/Piloto V2/Configurar administradores de prueba")]
        public static void ConfigurarAdministradores() => LifeDrive.Editor.VentanaAdministradores.AbrirPiloto(
            FlujoJugadorPilotoV2.PrepararContenidoPiloto("sesion-local"));
    }
}
