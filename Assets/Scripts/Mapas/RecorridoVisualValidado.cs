using System;
using System.Collections.Generic;
using System.Linq;
using LifeDrive.Mapas.Demo;

namespace LifeDrive.Mapas
{
    // Visual graph cursor only. No evaluation, decision persistence or implicit ID mapping.
    public sealed class RecorridoVisualValidado
    {
        readonly Dictionary<string, MapaVisual> mapas;
        readonly List<string> historial = new List<string>();
        public string Actual => historial[historial.Count - 1];
        public string[] Historial => historial.ToArray();
        public bool Entregado => Actual == "DELIVERY";
        public string[] Permitidos => mapas[Actual].siguientes.Select(m => m.Id).ToArray();

        public RecorridoVisualValidado(CatalogoMapas catalogo)
        {
            var rutas = RecorridosVisualesDemo.Obtener(catalogo);
            mapas = catalogo.mapas.ToDictionary(m => m.Id, StringComparer.Ordinal);
            string[] hojas = {"R3A","R3B","R3C","R3D","R3E","R3F","R3G","R3H"};
            if (mapas.Count != 16 || !rutas.Select(r => r[3].Id).OrderBy(x => x).SequenceEqual(hojas))
                throw new ArgumentException("Se requiere el árbol visual completo de 16 mapas.");
            var esperado = new Dictionary<string,string[]> {
                {"S0",new[]{"R1A","R1B"}}, {"R1A",new[]{"R2A","R2B"}}, {"R1B",new[]{"R2C","R2D"}},
                {"R2A",new[]{"R3A","R3B"}}, {"R2B",new[]{"R3C","R3D"}},
                {"R2C",new[]{"R3E","R3F"}}, {"R2D",new[]{"R3G","R3H"}}, {"DELIVERY",new string[0]}
            };
            foreach (var hoja in hojas) esperado.Add(hoja,new[]{"DELIVERY"});
            foreach (var par in esperado)
                if (!mapas[par.Key].siguientes.Select(m => m.Id).OrderBy(x => x).SequenceEqual(par.Value.OrderBy(x => x)))
                    throw new ArgumentException("Conexiones inesperadas en " + par.Key);
            historial.Add("S0");
        }

        // The caller must supply an already confirmed visual destination through an
        // approved content adapter. This method never calls ServicioPartidas.Decidir.
        public void AvanzarConfirmado(string destino)
        {
            if (destino == null || !Permitidos.Contains(destino))
                throw new InvalidOperationException("Destino visual no permitido desde " + Actual);
            historial.Add(destino);
        }
    }
}
