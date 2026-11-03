using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LifeDrive.Mapas.InterfazVisual
{
    public sealed class TarjetaPersonajeVisual : MonoBehaviour
    {
        public Button boton;
        public TMP_Text nombre, descripcion, estado;
        public RawImage retrato;
        public Image marco;
        public int PersonajeId { get; private set; }
        public void Configurar(int id, string titulo, string detalle, Texture atlas, Rect uv)
        {
            PersonajeId = id; nombre.text = titulo; descripcion.text = detalle;
            retrato.texture = atlas; retrato.uvRect = uv;
            Seleccionar(false);
        }
        public void Seleccionar(bool elegido)
        {
            marco.color = elegido ? new Color32(100, 218, 190, 255) : new Color32(65, 88, 94, 255);
            estado.text = elegido ? "SELECCIONADO" : "Seleccionar";
            estado.color = elegido ? new Color32(100, 218, 190, 255) : new Color32(244, 240, 217, 255);
        }
    }
}
