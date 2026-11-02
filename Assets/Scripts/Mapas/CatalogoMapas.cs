using System;
using UnityEngine;

namespace LifeDrive.Mapas
{
    [Serializable]
    public class AsociacionVisual
    {
        public MapaVisual mapa;
        [Tooltip("ID existente de ruta.json. Cero = pendiente, no asignado.")]
        public int tramoId;
        [Tooltip("Opcional. Cero = sin filtro de situación.")]
        public int situacionId;
        [Tooltip("Opcional. Cero = sin filtro de alternativa.")]
        public int alternativaId;
    }

    [CreateAssetMenu(menuName = "Life Drive/Mapas/Catálogo")]
    public class CatalogoMapas : ScriptableObject
    {
        public MapaVisual[] mapas = new MapaVisual[0];
        public AsociacionVisual[] asociaciones = new AsociacionVisual[0];
    }
}
