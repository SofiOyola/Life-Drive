using System;
using UnityEngine;

namespace LifeDrive.Mapas.Demo
{
    // A real second road, sampled by arc length so the truck keeps its speed.
    // Coordinates match the existing maps: 480 x 270, Y upwards.
    public sealed class CaminoBifurcado
    {
        readonly Vector2[] puntos = new Vector2[97];
        readonly float[] distancias = new float[97];
        public float Inicio { get; }
        public float Longitud => distancias[96];
        public CaminoBifurcado(RecorridoDemo principal, float hito)
        {
            if (principal == null || hito < .1f || hito > .85f)
                throw new ArgumentException("La bifurcación necesita un punto interior de la carretera.");
            Inicio = principal.Longitud * hito;
            var a = principal.Posicion(Inicio);
            var tangente = principal.Tangente(Inicio);
            // Pick the side with most free terrain, away from the main road's exit.
            float y = a.y >= 135 ? 32 : 238;
            var b = a + tangente * 38;
            var c = new Vector2(405, y);
            var d = new Vector2(480, y);
            for (int i = 0; i < puntos.Length; i++)
            {
                float t = i / 96f, u = 1 - t;
                puntos[i] = u*u*u*a + 3*u*u*t*b + 3*u*t*t*c + t*t*t*d;
                if (i > 0) distancias[i] = distancias[i-1] + Vector2.Distance(puntos[i-1], puntos[i]);
            }
        }
        public Vector2 Posicion(float distancia)
        {
            if (distancia <= 0) return puntos[0];
            if (distancia >= Longitud) return puntos[96];
            int lo = 0, hi = 96;
            while (hi - lo > 1) { int m = (lo + hi) / 2; if (distancias[m] <= distancia) lo = m; else hi = m; }
            return Vector2.Lerp(puntos[lo], puntos[hi], Mathf.InverseLerp(distancias[lo], distancias[hi], distancia));
        }
        public Vector2 Tangente(float distancia) =>
            (Posicion(Mathf.Min(Longitud, distancia + 1)) - Posicion(Mathf.Max(0, distancia - 1))).normalized;
    }
}
