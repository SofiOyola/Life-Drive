using UnityEngine;

namespace LifeDrive.Mapas
{
    public enum CategoriaMapa { Salida, Ronda1, Ronda2, Ronda3, Entrega }

    [CreateAssetMenu(menuName = "Life Drive/Mapas/Definición")]
    public class MapaVisual : ScriptableObject
    {
        [SerializeField] string id;
        public string Id => id;
        public string nombre;
        [TextArea] public string descripcion;
        public CategoriaMapa categoria;
        public Texture2D fondo;
        public Texture2D miniatura;
        public MapaVisual origen;
        public MapaVisual[] siguientes = new MapaVisual[0];
    }
}
