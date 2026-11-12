using System;
using UnityEngine;

namespace LifeDrive.Mapas.Demo
{
    // A real second road, sampled by arc length so the truck keeps its speed.
    // Coordinates match the existing maps: 480 x 270, Y upwards.
    public sealed class CaminoBifurcado
    {
        readonly Vector2[] puntos = new Vector2[385];
        readonly float[] distancias = new float[385];
        readonly Vector2 tangenteInicial;
        public float Inicio { get; }
        public float Longitud => distancias[384];
        public CaminoBifurcado(RecorridoDemo principal, float hito, Vector2[] trazado = null)
        {
            if (principal == null || hito < .1f || hito > .85f)
                throw new ArgumentException("La bifurcación necesita un punto interior de la carretera.");
            Inicio = principal.Longitud * hito;
            var a = principal.Posicion(Inicio);
            var tangente = principal.Tangente(Inicio);
            tangenteInicial = tangente;
            if (trazado != null && trazado.Length >= 2)
            {
                // Authored center of the driving lane, following the painted background.
                var nodos = new Vector2[trazado.Length + 1];
                nodos[0] = a;
                for (int n = 0; n < trazado.Length; n++)
                    nodos[n + 1] = Vector2.Scale(trazado[n], RecorridoDemo.Tamano);
                for (int i = 0; i < puntos.Length; i++)
                {
                    float paso = i / 384f * (nodos.Length - 1);
                    int n = Mathf.Min(Mathf.FloorToInt(paso), nodos.Length - 2);
                    float t = paso - n, t2 = t * t, t3 = t2 * t;
                    var p = nodos[n]; var q = nodos[n + 1];
                    var m0 = n == 0 ? tangente * Mathf.Min(8, Vector2.Distance(p, q)) : (q - nodos[n - 1]) * .5f;
                    var m1 = n + 2 < nodos.Length ? (nodos[n + 2] - p) * .5f : Vector2.right * Vector2.Distance(p, q);
                    puntos[i] = (2*t3 - 3*t2 + 1)*p + (t3 - 2*t2 + t)*m0 + (-2*t3 + 3*t2)*q + (t3 - t2)*m1;
                    if (i > 0) distancias[i] = distancias[i-1] + Vector2.Distance(puntos[i-1], puntos[i]);
                }
                return;
            }
            // Pick the side with most free terrain, away from the main road's exit.
            float y = a.y >= 135 ? 32 : 238;
            var b = a + tangente * 38;
            var c = new Vector2(405, y);
            var d = new Vector2(480, y);
            for (int i = 0; i < puntos.Length; i++)
            {
                float t = i / 384f, u = 1 - t;
                puntos[i] = u*u*u*a + 3*u*u*t*b + 3*u*t*t*c + t*t*t*d;
                if (i > 0) distancias[i] = distancias[i-1] + Vector2.Distance(puntos[i-1], puntos[i]);
            }
        }
        public Vector2 Posicion(float distancia)
        {
            if (distancia <= 0) return puntos[0];
            if (distancia >= Longitud) return puntos[384];
            int lo = 0, hi = 384;
            while (hi - lo > 1) { int m = (lo + hi) / 2; if (distancias[m] <= distancia) lo = m; else hi = m; }
            return Vector2.Lerp(puntos[lo], puntos[hi], Mathf.InverseLerp(distancias[lo], distancias[hi], distancia));
        }
        public Vector2 Tangente(float distancia) => distancia <= 0 ? tangenteInicial :
            (Posicion(Mathf.Min(Longitud, distancia + .05f)) - Posicion(Mathf.Max(0, distancia - .05f))).normalized;
    }
}
