using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LifeDrive.Datos
{
    // Lo que cambia al jugar en el navegador (WebGL). En el resto de plataformas estos métodos hacen lo de siempre o nada.
    public static class RutasContenido
    {
        public static readonly string[] Archivos = { "ruta.json", "ruta-v2-candidata.json", "administradores.json" };

        // En la web StreamingAssets no es una carpeta del disco: CargadorWeb copia aquí los JSON antes de que empiece el juego.
        public static string Carpeta =>
#if UNITY_WEBGL && !UNITY_EDITOR
            Path.Combine(Application.persistentDataPath, "ContenidoWeb");
#else
            Path.Combine(Application.streamingAssetsPath, "Contenido");
#endif
    }

    public static class ArchivosWeb
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void LD_DescargarArchivo(string nombre, byte[] datos, int longitud, string tipo);
#endif

        // Muestra un archivo guardado: en la web lo descarga con el navegador; en el escritorio lo abre con el visor del sistema.
        public static void Abrir(string ruta)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var datos = File.ReadAllBytes(ruta);
            LD_DescargarArchivo(Path.GetFileName(ruta), datos, datos.Length, "application/pdf");
#else
            Application.OpenURL(new Uri(ruta).AbsoluteUri);
#endif
        }
    }
}
