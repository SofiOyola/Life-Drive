using System.Collections.Generic;
using UnityEngine;

namespace LifeDrive.Mapas.Demo
{
    // Tiny native Unity textures, generated once in memory. No PNGs or dependencies.
    public sealed class IconosReconocimientoDemo
    {
        readonly Dictionary<IconoReconocimientoDemo, Texture2D> cache = new Dictionary<IconoReconocimientoDemo, Texture2D>();
        public Texture2D Obtener(IconoReconocimientoDemo icono)
        {
            if (cache.TryGetValue(icono, out var existente)) return existente;
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[256];
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
            {
                bool marca = icono == IconoReconocimientoDemo.Escudo
                    ? y >= 2 && y <= 13 && Mathf.Abs(x - 7.5f) <= Mathf.Min(5, y * .65f)
                    : icono == IconoReconocimientoDemo.Camino
                    ? x >= 3 && x <= 12 && y >= 2 && y <= 13 && (x == 3 || x == 12 || ((x == 7 || x == 8) && y % 5 < 3))
                    : ((x >= 6 && x <= 9 && y >= 2 && y <= 13) || (y >= 6 && y <= 9 && x >= 2 && x <= 13));
                pixels[y * 16 + x] = marca ? Color.white : Color.clear;
            }
            tex.SetPixels(pixels); tex.Apply(false, true); cache.Add(icono, tex); return tex;
        }
        public void Liberar() { foreach (var tex in cache.Values) Object.Destroy(tex); cache.Clear(); }
    }
}
