using System;
using System.IO;
using System.Linq;
using LifeDrive.Motor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LifeDrive.Editor
{
    // Compilación del juego para el navegador (WebGL).
    //   unity build <proyecto> --target WebGL --execute-method LifeDrive.Editor.ConstructorWeb.Construir --output-path <carpeta>
    // El resultado es una carpeta con index.html que se publica tal cual en cualquier servidor de archivos estáticos.
    public static class ConstructorWeb
    {
        const string EscenaCargando = "Assets/Scenes/Cargando.unity";

        // Crea (si no existe) la escena inicial que descarga el contenido en el navegador y la deja primera en la lista de compilación.
        public static void PrepararEscenaCargando()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(EscenaCargando) == null)
            {
                var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var camara = new GameObject("Main Camera").AddComponent<Camera>();
                camara.clearFlags = CameraClearFlags.SolidColor;
                camara.backgroundColor = new Color32(24, 41, 28, 255);
                camara.tag = "MainCamera";
                new GameObject("Cargador web").AddComponent<CargadorWeb>();
                EditorSceneManager.SaveScene(escena, EscenaCargando);
            }
            var lista = EditorBuildSettings.scenes.Where(s => s.path != EscenaCargando).ToList();
            lista.Insert(0, new EditorBuildSettingsScene(EscenaCargando, true));
            EditorBuildSettings.scenes = lista.ToArray();
        }

        public static void Construir()
        {
            string salida = Argumento("-buildOutput") ?? "Builds/Web";
            PrepararEscenaCargando();
            AjustarAudioWeb();
            AjustarFondosWeb();

            PlayerSettings.productName = "Life Drive"; // título de la página
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true; // funciona aunque el servidor no envíe Content-Encoding
            PlayerSettings.WebGL.initialMemorySize = 256;      // el audio descomprimido y las texturas necesitan margen
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.template = "PROJECT:LifeDrive"; // Assets/WebGLTemplates/LifeDrive: ocupa la ventana en 16:9
            PlayerSettings.WebGL.showDiagnostics = false;

            var opciones = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = salida,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None
            };
            var informe = BuildPipeline.BuildPlayer(opciones);
            Debug.Log("Build web: " + informe.summary.result + " · " + informe.summary.totalSize / (1024 * 1024) + " MB · " + informe.summary.totalTime + " · " + salida);
            if (informe.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }

        // Los 30 fondos de mapa son ~73 MB de los ~89 MB de la descarga. Son ilustraciones detalladas: sin comprimir el gzip apenas
        // los reduce, y DXT1 sin más tampoco (se comprime mal). Crunch (DXT1 + codificación entrópica) sí. Exige lados múltiplos de 4:
        // los de 1920x1080 se quedan a su tamaño; los de 1672x941 (bifurcaciones y deteriorados) se reducen a 1024x576.
        // Solo en la web: el proyecto y el build de Windows no cambian.
        static void AjustarFondosWeb()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Mapas/Backgrounds" }))
            {
                var ruta = AssetDatabase.GUIDToAssetPath(guid);
                if (ruta.Contains("_thumbnail")) continue; // miniaturas de 480x270: pequeñas y no aptas para Crunch
                var importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
                importador.GetSourceTextureWidthAndHeight(out int ancho, out int alto);
                var ajustes = importador.GetPlatformTextureSettings("WebGL");
                ajustes.overridden = true;
                ajustes.maxTextureSize = ancho % 4 == 0 && alto % 4 == 0 ? 2048 : 1024;
                ajustes.format = TextureImporterFormat.DXT1Crunched;
                ajustes.crunchedCompression = true;
                ajustes.compressionQuality = 75;
                importador.SetPlatformTextureSettings(ajustes);
                importador.SaveAndReimport();
            }
            foreach (var ruta in new[] { "Assets/Mapas/Backgrounds/R1A.png", "Assets/Mapas/Backgrounds/Bifurcaciones/R1A.png" })
            {
                var t = AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
                Debug.Log("Fondos web: " + ruta + " -> " + (t != null ? t.width + "x" + t.height + " " + t.format : "no encontrado"));
            }
        }

        // WebGL no admite pistas en Streaming y descomprime el audio en memoria: se cargan comprimidas y a 32 kHz (un tercio menos).
        static void AjustarAudioWeb()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio/Resources/Musica" }))
            {
                var importador = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                var ajustes = new AudioImporterSampleSettings
                {
                    loadType = AudioClipLoadType.CompressedInMemory,
                    compressionFormat = AudioCompressionFormat.Vorbis,
                    quality = .5f,
                    sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate,
                    sampleRateOverride = 32000
                };
                importador.SetOverrideSampleSettings("WebGL", ajustes);
                importador.SaveAndReimport();
            }
        }

        static string Argumento(string nombre)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, nombre);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }
    }
}
