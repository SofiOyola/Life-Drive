// Lee el contenido que viaja dentro del juego (StreamingAssets/Contenido):
// ruta.json (personajes, tramos, situaciones) y administradores.json (cuentas).
// Los datos que genera el juego van en BaseDatos.
using System.IO;
using UnityEngine;

namespace LifeDrive.Datos
{
    public class Almacen
    {
        readonly string carpeta;

        public Almacen(string carpetaContenido) => carpeta = carpetaContenido;

        // ponytail: File.ReadAllText sirve en Windows/Mac/Linux; Android y WebGL necesitan UnityWebRequest para StreamingAssets
        public Contenido CargarContenido() => JsonUtility.FromJson<Contenido>(File.ReadAllText(Path.Combine(carpeta, "ruta.json")));

        public ListaAdministradores CargarAdministradores()
        {
            var ruta = Path.Combine(carpeta, "administradores.json");
            return File.Exists(ruta) ? JsonUtility.FromJson<ListaAdministradores>(File.ReadAllText(ruta)) : new ListaAdministradores();
        }
    }
}
