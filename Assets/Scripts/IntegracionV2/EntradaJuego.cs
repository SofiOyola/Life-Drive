using System;
using System.IO;
using LifeDrive.Mapas;
using LifeDrive.Motor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LifeDrive.IntegracionV2
{
    [DefaultExecutionOrder(-300)]
    public sealed class EntradaJuego : MonoBehaviour
    {
        [SerializeField] CatalogoMapas catalogoV2;
        FlujoJugadorPilotoV2 flujo;
#if UNITY_EDITOR
        public static string CarpetaPruebas { get; set; }
        public static bool ClasicoEnPruebas { get; set; }
#endif
        public static string CarpetaEnsayo()
        {
#if UNITY_EDITOR
            return CarpetaPruebas;
#elif DEVELOPMENT_BUILD
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "--lifedrive-test-dir");
            return i >= 0 && i+1 < args.Length ? args[i+1] : null;
#else
            return null;
#endif
        }
        void Awake()
        {
            // The Editor Game view can otherwise render this lightweight game without
            // a frame limit. Cap the loop before constructing any player screens.
            Application.targetFrameRate = 60;
            if (FindAnyObjectByType<FlujoJugadorPilotoV2>() != null) return;
#if UNITY_EDITOR
            if (ClasicoEnPruebas && !string.IsNullOrEmpty(CarpetaPruebas))
            {
                var carpeta = Path.GetFullPath(CarpetaPruebas);
                var contenido = Path.Combine(carpeta, "Contenido");
                Directory.CreateDirectory(contenido);
                var ruta = Path.Combine(contenido, "ruta.json");
                if (!File.Exists(ruta))
                    File.Copy(Path.Combine(Application.streamingAssetsPath, "Contenido", "ruta.json"), ruta);
                flujo = FlujoJugadorPilotoV2.ActivarVigente(new ServicioJuego(contenido, carpeta), carpeta, Cargar);
                return;
            }
#endif
#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_WEBGL
            flujo = FlujoJugadorPilotoV2.Activar(catalogoV2, "sesion-local", Cargar,
                abrirInicio: false, carpetaEnsayo: CarpetaEnsayo());
#else
            flujo = FlujoJugadorPilotoV2.ActivarVigente(Sesion.Juego, Application.persistentDataPath, Cargar);
#endif
        }
        static void Cargar(string escena) => SceneManager.LoadScene(escena);
        void Start() { if (flujo != null) flujo.ConectarEntradaActual(); }
    }
}
