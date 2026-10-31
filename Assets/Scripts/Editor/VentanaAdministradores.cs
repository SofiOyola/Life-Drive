// HU-03: las cuentas de administrador se crean aquí (menú Life Drive > Administradores), nunca desde el juego.
// Se guardan en StreamingAssets/Contenido/administradores.json con la contraseña cifrada (PBKDF2).
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Motor;
using UnityEditor;
using UnityEngine;

namespace LifeDrive.Editor
{
    public class VentanaAdministradores : EditorWindow
    {
        static string Archivo => Path.Combine(Application.streamingAssetsPath, "Contenido", "administradores.json");

        string usuario = "", contrasena = "", mensaje = "";

        [MenuItem("Life Drive/Administradores")]
        static void Abrir() => GetWindow<VentanaAdministradores>("Administradores");

        void OnGUI()
        {
            var lista = Cargar();
            EditorGUILayout.LabelField("Cuentas actuales", EditorStyles.boldLabel);
            foreach (var a in lista.administradores)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(a.usuario);
                if (GUILayout.Button("Eliminar", GUILayout.Width(80)))
                {
                    lista.administradores = lista.administradores.Where(x => x.usuario != a.usuario).ToArray();
                    Guardar(lista, $"Se eliminó «{a.usuario}»");
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Nueva cuenta", EditorStyles.boldLabel);
            usuario = EditorGUILayout.TextField("Usuario", usuario);
            contrasena = EditorGUILayout.PasswordField("Contraseña", contrasena);
            if (GUILayout.Button("Crear administrador")) Crear(lista);
            if (mensaje != "") EditorGUILayout.HelpBox(mensaje, MessageType.Info);
        }

        void Crear(ListaAdministradores lista)
        {
            usuario = usuario.Trim();
            if (usuario == "") { mensaje = "Escribe un usuario"; return; }
            if (contrasena.Length < 10) { mensaje = "La contraseña debe tener al menos 10 caracteres"; return; }
            if (lista.administradores.Any(a => a.usuario == usuario)) { mensaje = $"El usuario «{usuario}» ya existe"; return; }
            lista.administradores = lista.administradores.Append(Contrasenas.Crear(usuario, contrasena)).ToArray();
            Guardar(lista, $"Administrador «{usuario}» creado");
            usuario = contrasena = "";
        }

        static ListaAdministradores Cargar() =>
            File.Exists(Archivo) ? JsonUtility.FromJson<ListaAdministradores>(File.ReadAllText(Archivo)) : new ListaAdministradores();

        void Guardar(ListaAdministradores lista, string aviso)
        {
            File.WriteAllText(Archivo, JsonUtility.ToJson(lista, true));
            AssetDatabase.Refresh();
            mensaje = aviso;
        }
    }
}
