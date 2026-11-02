using System.Collections.Generic;
using System.Linq;
using LifeDrive.Mapas;
using UnityEngine;

namespace LifeDrive.Editor
{
    public static class ValidadorContinuidad
    {
        public static readonly string[] Ids = { "S0", "R1A", "R1B", "R2A", "R2B", "R2C", "R2D", "R3A", "R3B", "R3C", "R3D", "R3E", "R3F", "R3G", "R3H", "DELIVERY" };
        public static string[] Destinos(string id)
        {
            switch (id)
            {
                case "S0": return new[] { "R1A", "R1B" };
                case "R1A": return new[] { "R2A", "R2B" };
                case "R1B": return new[] { "R2C", "R2D" };
                case "R2A": return new[] { "R3A", "R3B" };
                case "R2B": return new[] { "R3C", "R3D" };
                case "R2C": return new[] { "R3E", "R3F" };
                case "R2D": return new[] { "R3G", "R3H" };
                case "DELIVERY": return new string[0];
                default: return id.StartsWith("R3") ? new[] { "DELIVERY" } : new string[0];
            }
        }

        public static List<string> Revisar(CatalogoMapas catalogo)
        {
            var errores = new List<string>();
            var mapas = catalogo.mapas.Where(m => m != null).ToArray();
            foreach (var id in Ids)
                if (mapas.Count(m => m.Id == id) != 1) errores.Add(id + ": falta definición o ID duplicado.");
            if (catalogo.mapas.Length != 16 || mapas.Length != 16) errores.Add("Se requieren exactamente 16 referencias válidas.");
            foreach (var m in mapas)
            {
                if (!Ids.Contains(m.Id)) errores.Add(m.Id + ": ID no previsto.");
                var destinos = m.siguientes ?? new MapaVisual[0];
                if (!destinos.Select(d => d == null ? "" : d.Id).OrderBy(x => x).SequenceEqual(Destinos(m.Id).OrderBy(x => x))) errores.Add(m.Id + ": conexiones faltantes o no permitidas.");
                var padres = mapas.Where(p => p.siguientes != null && p.siguientes.Contains(m)).ToArray();
                if (!(m.origenes ?? new MapaVisual[0]).OrderBy(p => p == null ? "" : p.Id).SequenceEqual(padres.OrderBy(p => p.Id))) errores.Add(m.Id + ": predecesores incoherentes.");
                if (m.origen != (padres.Length == 1 ? padres[0] : null)) errores.Add(m.Id + ": origen único incoherente.");
                if (m.trayectoria == null || m.trayectoria.Length < 2) { errores.Add(m.Id + ": trayectoria ausente."); continue; }
                if (Vector2.Distance(m.trayectoria[0], m.entrada) > .001f || Vector2.Distance(m.trayectoria.Last(), m.salida) > .001f) errores.Add(m.Id + ": extremos de trayectoria incorrectos.");
                if (m.trayectoria.Any(p => p.x < 0 || p.x > 1 || p.y < 0 || p.y > 1)) errores.Add(m.Id + ": trayectoria fuera del fondo.");
                if (Vector2.Distance((m.trayectoria[1] - m.entrada).normalized, Vector2.right) > .001f || Vector2.Distance((m.salida - m.trayectoria[m.trayectoria.Length - 2]).normalized, Vector2.right) > .001f) errores.Add(m.Id + ": tangente de enlace no orientada al este.");
                foreach (var d in destinos.Where(d => d != null))
                    if (!mapas.Contains(d) || Mathf.Abs(m.salida.y - d.entrada.y) > .001f || Mathf.Abs(m.anchoViaPixeles - d.anchoViaPixeles) > .001f) errores.Add(m.Id + " → " + d.Id + ": enlace incompatible.");
            }
            return errores;
        }
    }
}
