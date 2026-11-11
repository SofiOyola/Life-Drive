using UnityEngine;

namespace LifeDrive.Mapas.Demo
{
    public sealed partial class DemoRecorrido
    {
        CaminoBifurcado bifurcacion;
        string destinoDesvio, destinoPrincipal, rotuloPrincipal, rotuloDesvio;
        bool usandoDesvio;
        GUIStyle estiloSenal;
        public bool BifurcacionVisible => bifurcacion != null;
        public bool UsandoDesvio => usandoDesvio;
        public Vector2 SalidaDesvio => bifurcacion?.Posicion(bifurcacion.Longitud) ?? Vector2.zero;

        public void ConfigurarBifurcacion(float hito, string principal, string desvio, int numeroPrincipal, int numeroDesvio)
        {
            if (recorrido == null || principal == desvio) return;
            if (bifurcacion != null && destinoPrincipal == principal && destinoDesvio == desvio) return;
            bifurcacion = new CaminoBifurcado(recorrido, hito);
            destinoPrincipal = principal; destinoDesvio = desvio; usandoDesvio = false;
            rotuloPrincipal = numeroPrincipal + " · VÍA PRINCIPAL";
            rotuloDesvio = numeroDesvio + " · DESVÍO";
        }
        public void LimpiarBifurcacion()
        {
            bifurcacion = null; usandoDesvio = false;
            destinoPrincipal = destinoDesvio = null;
        }
        Vector2 PosicionEnCamino(float d) => usandoDesvio && bifurcacion != null && d >= bifurcacion.Inicio
            ? bifurcacion.Posicion(d - bifurcacion.Inicio) : recorrido?.Posicion(d) ?? Vector2.zero;
        Vector2 TangenteEnCamino(float d) => usandoDesvio && bifurcacion != null && d >= bifurcacion.Inicio
            ? bifurcacion.Tangente(d - bifurcacion.Inicio) : recorrido?.Tangente(d) ?? Vector2.right;

        void DibujarBifurcacion(Rect area)
        {
            if (bifurcacion == null || Event.current.type != EventType.Repaint) return;
            float escala = area.width / 480f;
            var matriz = GUI.matrix; var color = GUI.color;
            GUI.BeginGroup(area);
            // Earth shoulder, pale edge, then asphalt: the old road is kept intact.
            DibujarRamal(escala, 29, new Color32(143, 120, 77, 255));
            DibujarRamal(escala, 24, new Color32(219, 211, 181, 255));
            DibujarRamal(escala, 21, new Color32(66, 65, 55, 255));
            for (float d = 20; d < bifurcacion.Longitud; d += 16)
                Segmento(Pantalla(bifurcacion.Posicion(d), escala), Pantalla(bifurcacion.Posicion(Mathf.Min(d + 7, bifurcacion.Longitud)), escala), 1.5f * escala, new Color32(230, 190, 58, 255));
            // Road signs sit beside each physical exit, numbered like the answer buttons.
            Senal(Pantalla(recorrido.Posicion(recorrido.Longitud - 70), escala), rotuloPrincipal, escala, false);
            Senal(Pantalla(bifurcacion.Posicion(bifurcacion.Longitud - 70), escala), rotuloDesvio, escala, true);
            GUI.EndGroup(); GUI.matrix = matriz; GUI.color = color;
        }
        void DibujarRamal(float escala, float ancho, Color color)
        {
            for (float d = 0; d < bifurcacion.Longitud; d += 4)
                Segmento(Pantalla(bifurcacion.Posicion(d), escala), Pantalla(bifurcacion.Posicion(Mathf.Min(d + 4.5f, bifurcacion.Longitud)), escala), ancho * escala, color);
        }
        static Vector2 Pantalla(Vector2 p, float escala) => new Vector2(p.x, 270 - p.y) * escala;
        static void Segmento(Vector2 a, Vector2 b, float ancho, Color color)
        {
            var anterior = GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg, a);
            GUI.color = color;
            GUI.DrawTexture(new Rect(a.x, a.y - ancho / 2, Vector2.Distance(a, b) + 1, ancho), Texture2D.whiteTexture);
            GUI.matrix = anterior;
        }
        void Senal(Vector2 punto, string texto, float escala, bool desvio)
        {
            if (estiloSenal == null) estiloSenal = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            estiloSenal.fontSize = Mathf.Max(11, Mathf.RoundToInt(6.5f * escala));
            estiloSenal.normal.textColor = new Color32(255, 245, 210, 255);
            float w = 112 * escala, h = 15 * escala;
            var rect = new Rect(Mathf.Clamp(punto.x - w/2, 0, 480*escala-w), Mathf.Clamp(punto.y - 33*escala, 2, 270*escala-h), w, h);
            GUI.color = desvio ? new Color32(109, 70, 31, 255) : new Color32(35, 72, 48, 255);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white; GUI.Label(rect, texto, estiloSenal);
        }
    }
}
