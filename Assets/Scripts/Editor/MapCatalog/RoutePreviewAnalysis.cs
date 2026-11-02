using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LifeDrive.Mapas;
using UnityEditor;
using UnityEngine;

namespace LifeDrive.Editor
{
    // Editor-only diagnostics. Color classification is deliberately NOT a collision mask.
    public static class RoutePreviewAnalysis
    {
        public static readonly Vector2 LogicalSize = new Vector2(480, 270);
        public static Vector2 Pixel(Vector2 p) => Vector2.Scale(p, LogicalSize);
        public static Texture2D Read(MapaVisual m)
        {
            var source = m.miniatura != null ? m.miniatura : m.fondo;
            if (source == null) return null;
            var t = new Texture2D(2, 2);
            try { ImageConversion.LoadImage(t, File.ReadAllBytes(AssetDatabase.GetAssetPath(source))); return t; }
            catch { UnityEngine.Object.DestroyImmediate(t); return null; }
        }

        public static Vector2 Evaluate(MapaVisual m, float progress, out Vector2 tangent)
        {
            tangent = Vector2.right;
            if (m == null || m.trayectoria == null || m.trayectoria.Length == 0) return Vector2.zero;
            var points = m.trayectoria.Select(Pixel).ToArray();
            float total = 0;
            for (int i = 1; i < points.Length; i++) total += Vector2.Distance(points[i - 1], points[i]);
            float remaining = Mathf.Clamp01(progress) * total;
            for (int i = 1; i < points.Length; i++)
            {
                var delta = points[i] - points[i - 1];
                float length = delta.magnitude;
                if (length < .0001f) continue;
                tangent = delta / length;
                if (remaining <= length) return points[i - 1] + tangent * remaining;
                remaining -= length;
            }
            return points[points.Length - 1];
        }

        public static Vector2[] Hull(Vector2 center, Vector2 tangent, float length, float width)
        {
            var side = new Vector2(-tangent.y, tangent.x);
            return new[] { center + tangent * length / 2 + side * width / 2, center + tangent * length / 2 - side * width / 2,
                center - tangent * length / 2 - side * width / 2, center - tangent * length / 2 + side * width / 2 };
        }

        public static bool Plausible(Texture2D t, Vector2 p, bool delivery)
        {
            if (t == null || p.x < 0 || p.x >= 480 || p.y < 0 || p.y >= 270) return false;
            var c = t.GetPixel(Mathf.Clamp(Mathf.RoundToInt(p.x / 480 * (t.width - 1)), 0, t.width - 1), Mathf.Clamp(Mathf.RoundToInt(p.y / 270 * (t.height - 1)), 0, t.height - 1));
            float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b)), min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            bool neutral = max - min < (delivery ? .20f : .10f);
            bool asphalt = neutral && max < .53f;
            bool marking = (neutral && min > .60f) || (c.r > .65f && c.g > .45f && c.b < .28f);
            return asphalt || marking || (delivery && neutral && min > .40f);
        }

        public sealed class Result
        {
            public string id;
            public int centerSuspect, hullSuspect, outside;
            public List<string> geometry = new List<string>();
            public string Summary => id + " · Geometría: " + (geometry.Count == 0 ? "sin alertas" : string.Join("; ", geometry))
                + $" · Color PNG (estimación): centro {centerSuspect}/201, huella {hullSuspect}/201 sospechosos; {outside} huellas fuera del lienzo.";
        }

        public static Result Analyze(MapaVisual m, float length, float width, float lateral)
        {
            var r = new Result { id = m.Id };
            var points = m.trayectoria ?? new Vector2[0];
            if (points.Length < 2) { r.geometry.Add("trayectoria insuficiente"); return r; }
            if (points.Any(p => !float.IsFinite(p.x) || !float.IsFinite(p.y))) { r.geometry.Add("puntos no finitos"); return r; }
            if (points.Any(p => p.x < 0 || p.x > 1 || p.y < 0 || p.y > 1)) r.geometry.Add("puntos fuera del lienzo");
            for (int i = 1; i < points.Length; i++)
            {
                float d = Vector2.Distance(Pixel(points[i]), Pixel(points[i - 1]));
                if (d > 80) r.geometry.Add($"segmento {i - 1}–{i} largo ({d:F0}px): revisar salto/escasez de puntos");
                if (d < .01f) r.geometry.Add($"punto {i} repetido");
                if (i >= points.Length - 1) continue;
                var a = Pixel(points[i] - points[i - 1]); var b = Pixel(points[i + 1] - points[i]);
                float angle = Vector2.Angle(a, b);
                float radius = Mathf.Min(a.magnitude, b.magnitude) / Mathf.Max(.001f, 2 * Mathf.Sin(angle * Mathf.Deg2Rad / 2));
                if (angle > 45 || (angle > 12 && radius < length * 1.5f)) r.geometry.Add($"curva {i}: giro {angle:F0}°, radio estimado {radius:F0}px");
            }
            if (width > m.anchoViaPixeles * .75f / 2) r.geometry.Add("ancho del camión mayor que medio ancho nominal de vía");
            var t = Read(m);
            if (t == null) { r.geometry.Add("PNG ausente/no legible"); return r; }
            try
            {
                for (int i = 0; i <= 200; i++)
                {
                    Vector2 tangent; var p = Evaluate(m, i / 200f, out tangent);
                    var side = new Vector2(-tangent.y, tangent.x); p += side * lateral;
                    bool terminal = m.categoria == CategoriaMapa.Entrega;
                    if (!Plausible(t, p, terminal)) r.centerSuspect++;
                    var hull = Hull(p, tangent, length, width);
                    var samples = hull.Concat(Enumerable.Range(0, 4).Select(k => (hull[k] + hull[(k + 1) % 4]) / 2)).Append(p);
                    if (samples.Any(v => !Plausible(t, v, terminal))) r.hullSuspect++;
                    if (hull.Any(v => v.x < 0 || v.y < 0 || v.x >= 480 || v.y >= 270)) r.outside++;
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(t); }
            return r;
        }

        static Vector2 Edge(MapaVisual m, bool end)
        {
            var t = Read(m); if (t == null) return new Vector2(-1, -1);
            try
            {
                int x = end ? t.width - 1 : 0;
                float expected = (end ? m.salida.y : m.entrada.y) * t.height;
                int start = -1, last = -1;
                float best = float.MaxValue;
                Vector2 match = new Vector2(-1, -1);
                for (int y = Mathf.Max(0, (int)expected - t.height / 8); y < Mathf.Min(t.height, expected + t.height / 8); y++)
                {
                    var c = t.GetPixel(x, y);
                    bool road = (Mathf.Max(c.r, Mathf.Max(c.g, c.b)) < .50f && Mathf.Abs(c.r - c.g) < .08f && Mathf.Abs(c.g - c.b) < .08f)
                        || (c.r > .65f && c.g > .45f && c.b < .28f);
                    if (road) { if (start < 0) start = y; last = y; }
                    if (start >= 0 && ((!road && y - last > 2) || y >= Mathf.Min(t.height, expected + t.height / 8) - 1))
                    {
                        float center = (start + last) * .5f, width = last - start + 1;
                        float score = Mathf.Abs(center - expected) + Mathf.Abs(width - t.height * m.anchoViaPixeles / 360) * .1f;
                        if (width >= t.height * .025f && score < best) { best = score; match = new Vector2(center / t.height * 270, width / t.height * 270); }
                        start = last = -1;
                    }
                }
                return match;
            }
            finally { UnityEngine.Object.DestroyImmediate(t); }
        }

        public static string Connection(MapaVisual from, MapaVisual to)
        {
            string label = from.Id + " → " + to.Id;
            if (to.Id == "DELIVERY") return label + ": transición de escena; no requiere encaje de bordes.";
            var a = Edge(from, true); var b = Edge(to, false);
            if (a.x < 0 || b.x < 0) return label + ": ADVERTENCIA: borde no clasificable por color.";
            float height = Mathf.Abs(a.x - b.x), width = Mathf.Abs(a.y - b.y);
            return label + $": {(height > 3 || width > 3 ? "ADVERTENCIA" : "dentro de tolerancia estimada")} · Δcentro {height:F1}px, Δancho {width:F1}px (umbral 3px).";
        }
    }
}
