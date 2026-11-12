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
        public Texture2D fondoBifurcacion;
        public Texture2D fondoDeteriorado;
        [Tooltip("Recorrido alternativo dibujado en el fondo, coordenadas normalizadas Y hacia arriba.")]
        public Vector2[] trayectoriaDesvio = new Vector2[0];
        public Texture2D miniatura;
        public MapaVisual origen;
        public MapaVisual[] siguientes = new MapaVisual[0];
        [Header("Continuidad visual · coordenadas normalizadas, Y hacia arriba")]
        public Vector2 entrada = new Vector2(0, .5f);
        public Vector2 salida = new Vector2(1, .5f);
        public float anchoViaPixeles = 32;
        public Vector2[] trayectoria = new Vector2[0];
        [TextArea] public string paisaje;
        [TextArea] public string vegetacionRelieve;
        [TextArea] public string caracteristicasVia;
        [TextArea] public string continuidad;
        [Tooltip("DELIVERY admite varios predecesores; origen solo representa el padre único de las ramas.")]
        public MapaVisual[] origenes = new MapaVisual[0];
        [Header("Solo mapas de entrega · área normalizada, Y hacia arriba")]
        [Tooltip("Zona libre para detener el sprite. En Entrega, salida indica el punto final interior, no una conexión posterior.")]
        public Rect zonaEntrega;
    }
}
