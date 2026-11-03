using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeDrive.Mapas.Demo
{
    // Geometry solely for the isolated demo. Never writes back to the map asset.
    public sealed class RecorridoDemo
    {
        public static readonly Vector2 Tamano = new Vector2(480, 270);
        readonly List<Vector2> puntos = new List<Vector2>();
        readonly List<float> distancias = new List<float>();
        public float Longitud => distancias.Count == 0 ? 0 : distancias[distancias.Count - 1];
        public bool AjusteLimites { get; private set; }

        public RecorridoDemo(MapaVisual mapa, float carrilDerecho)
        {
            if (mapa == null || mapa.trayectoria == null || mapa.trayectoria.Length < 2)
                throw new ArgumentException("Mapa sin trayectoria suficiente.");
            var original = new List<Vector2>();
            foreach (var p in mapa.trayectoria)
            {
                if (!float.IsFinite(p.x) || !float.IsFinite(p.y) || p.x < 0 || p.x > 1 || p.y < 0 || p.y > 1)
                    throw new ArgumentException("La trayectoria contiene puntos inválidos.");
                var pixel = Vector2.Scale(p, Tamano);
                if (original.Count == 0 || Vector2.Distance(original[original.Count - 1], pixel) > .001f) original.Add(pixel);
            }
            if (original.Count < 2) throw new ArgumentException("La trayectoria no tiene longitud.");
            var central = new List<Vector2> { original[0] };
            for (int i = 1; i < original.Count - 1; i++)
            {
                var a = original[i] - original[i - 1]; var b = original[i + 1] - original[i];
                float corte = Mathf.Min(4, Mathf.Min(a.magnitude, b.magnitude) * .22f);
                var inicio = original[i] - a.normalized * corte;
                var fin = original[i] + b.normalized * corte;
                central.Add(inicio);
                for (int k = 1; k <= 8; k++)
                {
                    float t = k / 8f;
                    central.Add((1 - t) * (1 - t) * inicio + 2 * (1 - t) * t * original[i] + t * t * fin);
                }
            }
            central.Add(original[original.Count - 1]);
            var centralDist = Distancias(central);
            float total = centralDist[centralDist.Count - 1];
            int pasos = Mathf.Max(2, Mathf.CeilToInt(total));
            for (int i = 0; i <= pasos; i++)
            {
                float d = total * i / pasos;
                var p = Evaluar(central, centralDist, d);
                var atras = Evaluar(central, centralDist, Mathf.Max(0, d - 3));
                var delante = Evaluar(central, centralDist, Mathf.Min(total, d + 3));
                var tangente = (delante - atras).normalized;
                var derecha = new Vector2(tangente.y, -tangente.x);
                var desplazado = p + derecha * carrilDerecho;
                var limitado = new Vector2(Mathf.Clamp(desplazado.x, 0, 480), Mathf.Clamp(desplazado.y, 0, 270));
                if (Vector2.Distance(limitado, desplazado) > .001f) AjusteLimites = true;
                if (puntos.Count == 0 || Vector2.Distance(puntos[puntos.Count - 1], limitado) > .0001f) puntos.Add(limitado);
            }
            distancias = Distancias(puntos);
        }

        static List<float> Distancias(List<Vector2> points)
        {
            var result = new List<float> { 0 };
            for (int i = 1; i < points.Count; i++) result.Add(result[i - 1] + Vector2.Distance(points[i - 1], points[i]));
            return result;
        }
        static Vector2 Evaluar(List<Vector2> points, List<float> lengths, float distance)
        {
            if (distance <= 0) return points[0];
            if (distance >= lengths[lengths.Count - 1]) return points[points.Count - 1];
            int lo = 0, hi = lengths.Count - 1;
            while (hi - lo > 1) { int mid = (lo + hi) / 2; if (lengths[mid] <= distance) lo = mid; else hi = mid; }
            return Vector2.Lerp(points[lo], points[hi], Mathf.InverseLerp(lengths[lo], lengths[hi], distance));
        }
        public Vector2 Posicion(float distance) => Evaluar(puntos, distancias, Mathf.Clamp(distance, 0, Longitud));
        public Vector2 Tangente(float distance)
        {
            var delta = Posicion(Mathf.Min(Longitud, distance + 2)) - Posicion(Mathf.Max(0, distance - 2));
            return delta.sqrMagnitude > .00001f ? delta.normalized : Vector2.right;
        }
    }
}
