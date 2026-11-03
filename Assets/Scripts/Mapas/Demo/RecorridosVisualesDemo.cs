using System;
using System.Collections.Generic;
using System.Linq;

namespace LifeDrive.Mapas.Demo
{
    // Derives the demo choices from the existing visual graph, never from route.json.
    public static class RecorridosVisualesDemo
    {
        public static MapaVisual[][] Obtener(CatalogoMapas catalogo)
        {
            if (catalogo == null || catalogo.mapas == null)
                throw new ArgumentException("Catálogo no disponible.");
            var inicio = catalogo.mapas.Where(m => m != null && m.Id == "S0").ToArray();
            if (inicio.Length != 1) throw new ArgumentException("Se necesita un único S0.");
            var rutas = new List<MapaVisual[]>();
            Visitar(inicio[0], new List<MapaVisual>(), catalogo, rutas);
            if (rutas.Count != 8) throw new ArgumentException("Se requieren ocho recorridos completos válidos.");
            return rutas.OrderBy(r => r[3].Id, StringComparer.Ordinal).ToArray();
        }

        static void Visitar(MapaVisual mapa, List<MapaVisual> anteriores, CatalogoMapas catalogo, List<MapaVisual[]> rutas)
        {
            if (mapa == null || !catalogo.mapas.Contains(mapa) || anteriores.Contains(mapa) || anteriores.Count >= 5)
                throw new ArgumentException("Conexión visual inválida, externa al catálogo o cíclica.");
            var camino = new List<MapaVisual>(anteriores) { mapa };
            if (mapa.Id == "DELIVERY")
            {
                if (camino.Count != 5 || !camino[1].Id.StartsWith("R1") || !camino[2].Id.StartsWith("R2") || !camino[3].Id.StartsWith("R3"))
                    throw new ArgumentException("Cada recorrido debe contener S0, R1, R2, R3 y DELIVERY.");
                rutas.Add(camino.ToArray());
                return;
            }
            if (mapa.siguientes == null || mapa.siguientes.Length == 0 || mapa.siguientes.Distinct().Count() != mapa.siguientes.Length)
                throw new ArgumentException("Mapa sin continuación válida: " + mapa.Id);
            foreach (var siguiente in mapa.siguientes) Visitar(siguiente, camino, catalogo, rutas);
        }
    }
}
