using LifeDrive.IntegracionV2;
using LifeDrive.Mapas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LifeDrive.Editor.InterfazVisual
{
    [InitializeOnLoad]
    public static class FlujoJugadorPilotoV2Editor
    {
        const string Solicitud = "LifeDrive.AbrirFlujoJugadorV2";
        static FlujoJugadorPilotoV2Editor() => EditorApplication.playModeStateChanged += estado =>
        {
            if (estado != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Solicitud, false)) return;
            SessionState.SetBool(Solicitud, false);
            Activar();
        };

        [MenuItem("Life Drive/Piloto V2/Abrir flujo del jugador")]
        public static void Abrir()
        {
            if (EditorApplication.isPlaying) { Activar(); return; }
            // Never discard unsaved scenes while launching the pilot.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            SessionState.SetBool(Solicitud, true);
            EditorApplication.isPlaying = true;
        }

        static void Activar()
        {
            if (Object.FindAnyObjectByType<FlujoJugadorPilotoV2>() != null) return;
            FlujoJugadorPilotoV2.Activar(AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset"),
                "sesion-local", Cargar);
        }

        [MenuItem("Life Drive/Piloto V2/Configurar administradores de prueba")]
        public static void ConfigurarAdministradores() => LifeDrive.Editor.VentanaAdministradores.AbrirPiloto(
            FlujoJugadorPilotoV2.PrepararContenidoPiloto("sesion-local"));

        public static void Cargar(string nombre) => EditorSceneManager.LoadSceneInPlayMode(
            "Assets/Scenes/" + nombre + ".unity", new LoadSceneParameters(LoadSceneMode.Single));
    }
}
