using LifeDrive.Mapas.InterfazVisual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LifeDrive.Editor.InterfazVisual
{
    [InitializeOnLoad]
    public static class NavegacionPreviewEditor
    {
        static NavegacionPreviewEditor() => EditorApplication.update += Preparar;
        static void Preparar()
        {
            if (!EditorApplication.isPlaying || PrevisualizacionPantallas.NavegarEnEditor != null) return;
            PrevisualizacionPantallas.NavegarEnEditor = nombre =>
            {
                if (nombre != "Inicio" && nombre != "Registro" && nombre != "AdminLogin" && nombre != "Admin") return false;
                string path = "Assets/Scenes/" + nombre + ".unity";
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return false;
                EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
                return true;
            };
        }
    }
}
