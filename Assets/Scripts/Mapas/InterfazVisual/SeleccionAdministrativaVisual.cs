using TMPro;
using UnityEngine;
namespace LifeDrive.Mapas.InterfazVisual
{
    // Presentation only; keeps the existing selection and report actions intact.
    public sealed class SeleccionAdministrativaVisual : MonoBehaviour
    {
        public VistaAdministrativaDemo vista;
        public GameObject composicion;
        public TMP_Text encabezado;
        void LateUpdate()
        {
            bool seleccionado=vista.detalle.text.StartsWith("Seleccionado:");
            composicion.SetActive(seleccionado);vista.detalle.alpha=seleccionado?0:1;
            if(seleccionado&&vista.Seleccionado>=0)encabezado.text="Seleccionado: "+vista.ejemplos[vista.Seleccionado].nombre.Replace(" · DEMO","");
        }
    }
}
