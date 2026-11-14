using System;
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.Mapas.InterfazVisual
{
    // Presentation-only host. Does not initialize Sesion or submit a registration.
    public sealed class PrevisualizacionPantallas : MonoBehaviour
    {
        public bool registro;
        public Button jugar, administrador, volver, comprobar;
        public TMP_InputField nombre, documento;
        public TMP_Text mensaje;
        public TarjetaPersonajeVisual[] tarjetas;
        public Texture2D atlas;
        public int PersonajeSeleccionado { get; private set; }
        public int[] IdsDisponibles => tarjetas == null ? new int[0] : tarjetas.Select(t => t.PersonajeId).ToArray();
        public static Func<string, bool> NavegarEnEditor;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RestablecerPuente() => NavegarEnEditor = null;

        void Start()
        {
            if (!registro)
            {
                jugar.onClick.AddListener(() => Navegar("Registro"));
                administrador.onClick.AddListener(() => mensaje.text = "Acceso administrativo pendiente: AdminLogin aún no está montada. Esta vista no inicia sesión.");
                return;
            }
            volver.onClick.AddListener(() => Navegar("Inicio"));
            comprobar.onClick.AddListener(ComprobarDatos);
            try
            {
                var contenido = new Almacen(RutasContenido.Carpeta).CargarContenido();
                var personajes = contenido.personajes.OrderBy(p => p.id).ToArray();
                if (personajes.Length != 4 || !personajes.Select(p => p.id).SequenceEqual(new[] { 1, 2, 3, 4 }) || tarjetas.Length != 4)
                    throw new InvalidOperationException("El catálogo de retratos necesita revisar sus cuatro IDs de contenido.");
                for (int i = 0; i < 4; i++)
                {
                    int indice = i;
                    var p = personajes[i];
                    tarjetas[i].Configurar(p.id, p.nombre, p.descripcion, atlas, new Rect(i % 2 * .5f, i < 2 ? .5f : 0, .5f, .5f));
                    tarjetas[i].boton.onClick.AddListener(() => SeleccionarPersonaje(personajes[indice].id));
                }
            }
            catch (Exception e) when (e is IOException || e is InvalidOperationException || e is ArgumentException)
            {
                mensaje.text = "No se pudo preparar la vista: " + e.Message;
                comprobar.interactable = false;
            }
        }
        public void SeleccionarPersonaje(int id)
        {
            if (!tarjetas.Any(t => t.PersonajeId == id)) return;
            PersonajeSeleccionado = id;
            foreach (var t in tarjetas) t.Seleccionar(t.PersonajeId == id);
            mensaje.text = "Personaje seleccionado. Puedes comprobar el formulario sin crear un usuario.";
        }
        public void ComprobarDatos()
        {
            var error = Validacion.ErrorRegistro(nombre.text, documento.text, PersonajeSeleccionado);
            mensaje.text = error ?? "Datos válidos para esta previsualización. No se creó un usuario. La conexión con Partida queda pendiente.";
        }
        public void Navegar(string escena)
        {
            if (NavegarEnEditor != null && NavegarEnEditor(escena)) return;
            string ruta = "Assets/Scenes/" + escena + ".unity";
            if (SceneUtility.GetBuildIndexByScenePath(ruta) >= 0) SceneManager.LoadScene(escena);
            else mensaje.text = "La escena " + escena + " no está incluida en la configuración de compilación. Abrir en el Editor para previsualizar.";
        }
    }
}
