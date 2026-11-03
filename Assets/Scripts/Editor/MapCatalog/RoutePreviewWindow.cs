using System.Collections.Generic;
using System.IO;
using System.Linq;
using LifeDrive.Mapas;
using UnityEditor;
using UnityEngine;

namespace LifeDrive.Editor
{
    public class RoutePreviewWindow : EditorWindow
    {
        [SerializeField] CatalogoMapas catalogo;
        [SerializeField] MapaVisual mapa;
        [SerializeField] float progreso, largo = 18, ancho = 7, lateral, zoom = 1;
        [SerializeField] bool verRuta = true, verPuntos = true, verCamion = true;
        [SerializeField] Sprite camionSprite;
        [SerializeField] float escalaSprite = 1, orientacionBase;
        [SerializeField] bool verHuella = true;
        Rect camionUV = new Rect(0, 0, 1, 1);
        Vector2 scroll, imagenScroll;
        Texture2D lectura;
        string diagnostico = "Pulsa Analizar para obtener advertencias con las dimensiones actuales.";
        readonly List<string> conexiones = new List<string>();

        [MenuItem("Tools/Life Drive/Route Preview")]
        public static void Abrir() => GetWindow<RoutePreviewWindow>("Route Preview");
        void OnEnable()
        {
            minSize = new Vector2(780, 650);
            catalogo = AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset");
            if (camionSprite == null) camionSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Mapas/Vehiculos/CamionCarga.png");
            ActualizarSpriteUV();
            if (mapa == null && catalogo != null) Seleccionar(catalogo.mapas.FirstOrDefault());
            else if (mapa != null) Seleccionar(mapa);
        }
        void OnDisable() { if (lectura != null) DestroyImmediate(lectura); }
        public void Seleccionar(MapaVisual m)
        {
            if (lectura != null) DestroyImmediate(lectura);
            mapa = m; progreso = 0;
            lectura = m == null ? null : RoutePreviewAnalysis.Read(m);
            diagnostico = "Análisis pendiente para este mapa y estas dimensiones.";
            Repaint();
        }
        public void EstablecerProgreso(float value) { progreso = Mathf.Clamp01(value); Repaint(); }
        public void MostrarCapas(bool ruta, bool puntos) { verRuta = ruta; verPuntos = puntos; Repaint(); }
        public void ConfigurarCamion(Sprite sprite, float escala, float grados)
        {
            camionSprite = sprite; escalaSprite = Mathf.Clamp(escala, .1f, 4); orientacionBase = grados;
            ActualizarSpriteUV(); Repaint();
        }
        public Vector2 DimensionesCamion => new Vector2(largo, ancho) * escalaSprite;
        public void AjustarProporcionSprite()
        {
            if (camionSprite == null) return;
            float ratio = camionUV.width * camionSprite.texture.width / (camionUV.height * camionSprite.texture.height);
            ancho = Mathf.Clamp(largo / Mathf.Max(.1f, ratio), 1, 50);
            Repaint();
        }
        void ActualizarSpriteUV()
        {
            camionUV = new Rect(0, 0, 1, 1);
            if (camionSprite == null) return;
            // Trim only for display: PNG, pivot and Sprite resource remain unchanged.
            var path = AssetDatabase.GetAssetPath(camionSprite);
            if (!File.Exists(path)) return;
            var t = new Texture2D(2, 2);
            try
            {
                ImageConversion.LoadImage(t, File.ReadAllBytes(path));
                int x0 = t.width, y0 = t.height, x1 = -1, y1 = -1;
                var pixels = t.GetPixels32();
                var source = camionSprite.rect;
                for (int y = (int)source.yMin; y < source.yMax; y++)
                    for (int x = (int)source.xMin; x < source.xMax; x++)
                        if (pixels[y * t.width + x].a > 8) { x0 = Mathf.Min(x0, x); y0 = Mathf.Min(y0, y); x1 = Mathf.Max(x1, x); y1 = Mathf.Max(y1, y); }
                if (x1 >= x0) camionUV = new Rect(x0 / (float)t.width, y0 / (float)t.height, (x1 - x0 + 1f) / t.width, (y1 - y0 + 1f) / t.height);
            }
            finally { DestroyImmediate(t); }
        }
        public Vector2 PosicionActual
        {
            get { Vector2 t; var p = RoutePreviewAnalysis.Evaluate(mapa, progreso, out t); return p + new Vector2(-t.y, t.x) * lateral; }
        }

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (catalogo == null) { EditorGUILayout.HelpBox("No se encontró el catálogo.", MessageType.Warning); EditorGUILayout.EndScrollView(); return; }
            var mapas = catalogo.mapas.Where(m => m != null).ToArray();
            int index = System.Array.IndexOf(mapas, mapa);
            int next = EditorGUILayout.Popup("Mapa", Mathf.Max(0, index), mapas.Select(m => m.Id + " · " + m.nombre).ToArray());
            if (index != next && next >= 0 && next < mapas.Length) Seleccionar(mapas[next]);
            if (mapa == null) { EditorGUILayout.EndScrollView(); return; }
            EditorGUILayout.HelpBox("Revisión de Editor, sin gameplay. El camión es una huella provisional. El color del PNG solo estima superficies: NO garantiza cabida física ni evita obstáculos automáticamente.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            verRuta = GUILayout.Toggle(verRuta, "Trayectoria"); verPuntos = GUILayout.Toggle(verPuntos, "Puntos"); verCamion = GUILayout.Toggle(verCamion, "Camión/huella");
            if (GUILayout.Button("Recurso en Project")) { Selection.activeObject = mapa; EditorGUIUtility.PingObject(mapa); }
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            var nuevoSprite = (Sprite)EditorGUILayout.ObjectField("Sprite del camión", camionSprite, typeof(Sprite), false, GUILayout.Height(20));
            if (nuevoSprite != camionSprite) { camionSprite = nuevoSprite; ActualizarSpriteUV(); }
            escalaSprite = EditorGUILayout.Slider("Escala del sprite y huella", escalaSprite, .1f, 4);
            orientacionBase = EditorGUILayout.FloatField("Corrección de orientación · grados", orientacionBase);
            verHuella = EditorGUILayout.Toggle("Superponer huella diagnóstica", verHuella);
            largo = Mathf.Clamp(EditorGUILayout.FloatField("Largo · píxeles lógicos 480×270", largo), 1, 100);
            ancho = Mathf.Clamp(EditorGUILayout.FloatField("Ancho · píxeles lógicos", ancho), 1, 50);
            if (camionSprite != null && GUILayout.Button("Ajustar ancho para conservar proporción del sprite")) AjustarProporcionSprite();
            lateral = EditorGUILayout.Slider("Desplazamiento lateral · px (+izquierda)", lateral, -20, 20);
            if (EditorGUI.EndChangeCheck()) diagnostico = "Dimensiones o desplazamiento cambiados: vuelve a analizar.";
            EditorGUILayout.LabelField($"Dimensión visible aprox.: {largo * escalaSprite:F1} × {ancho * escalaSprite:F1} px lógicos · vía nominal {mapa.anchoViaPixeles * .75f:F1}px / carril {mapa.anchoViaPixeles * .375f:F1}px.", EditorStyles.wordWrappedMiniLabel);
            progreso = EditorGUILayout.Slider("Progreso · %", progreso * 100, 0, 100) / 100;
            zoom = EditorGUILayout.Slider("Zoom", zoom, .5f, 3);
            EditorGUILayout.LabelField("Entrada verde · intermedios cian · final naranja · zona DELIVERY magenta. Tangente por segmento, sin interpolación de gameplay.", EditorStyles.wordWrappedMiniLabel);
            imagenScroll = EditorGUILayout.BeginScrollView(imagenScroll, GUILayout.Height(400));
            var area = GUILayoutUtility.GetRect(960 * zoom, 540 * zoom, GUILayout.ExpandWidth(false));
            if (mapa.fondo != null) GUI.DrawTexture(area, mapa.fondo, ScaleMode.StretchToFill);
            if (Event.current.type == EventType.Repaint) Dibujar(area);
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Analizar mapa con estas dimensiones")) diagnostico = RoutePreviewAnalysis.Analyze(mapa, largo * escalaSprite, ancho * escalaSprite, lateral, orientacionBase).Summary;
            EditorGUILayout.HelpBox(diagnostico, MessageType.Info);
            if (GUILayout.Button("Revisar los 16 y registrar informe")) RevisarTodos();
            foreach (var c in conexiones) EditorGUILayout.LabelField(c, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Conexiones del mapa · bordes reales del PNG", EditorStyles.boldLabel);
            foreach (var parent in mapas.Where(m => m.siguientes != null && m.siguientes.Contains(mapa))) Comparar(parent, mapa);
            foreach (var child in mapa.siguientes ?? new MapaVisual[0]) if (child != null) Comparar(mapa, child);
            EditorGUILayout.EndScrollView();
        }

        static Vector3 Pantalla(Rect rect, Vector2 p) => new Vector3(rect.x + p.x / 480 * rect.width, rect.yMax - p.y / 270 * rect.height, 0);
        void Dibujar(Rect rect)
        {
            Handles.BeginGUI();
            var points = (mapa.trayectoria ?? new Vector2[0]).Select(RoutePreviewAnalysis.Pixel).ToArray();
            if (mapa.categoria == CategoriaMapa.Entrega)
            {
                var z = mapa.zonaEntrega;
                var a = Pantalla(rect, RoutePreviewAnalysis.Pixel(z.min)); var b = Pantalla(rect, RoutePreviewAnalysis.Pixel(z.max));
                Handles.DrawSolidRectangleWithOutline(Rect.MinMaxRect(a.x, b.y, b.x, a.y), new Color(1, 0, 1, .10f), Color.magenta);
            }
            if (verRuta && points.Length > 1) { Handles.color = Color.cyan; Handles.DrawAAPolyLine(3, points.Select(p => Pantalla(rect, p)).ToArray()); }
            if (verPuntos)
                for (int i = 0; i < points.Length; i++)
                {
                    var p = Pantalla(rect, points[i]);
                    Handles.color = i == 0 ? Color.green : i == points.Length - 1 ? new Color(1, .5f, 0) : Color.cyan;
                    Handles.DrawSolidDisc(p, Vector3.forward, i == 0 || i == points.Length - 1 ? 6 : 3);
                    if (zoom > 1.4f || i == 0 || i == points.Length - 1) GUI.Label(new Rect(p.x + 5, p.y - 18, 80, 20), i.ToString());
                }
            if (verCamion && points.Length > 1)
            {
                Vector2 t; var p = RoutePreviewAnalysis.Evaluate(mapa, progreso, out t); p += new Vector2(-t.y, t.x) * lateral;
                float angle = Mathf.Atan2(t.y, t.x) * Mathf.Rad2Deg;
                var axis = Quaternion.Euler(0, 0, orientacionBase) * (Vector3)t;
                var hull = RoutePreviewAnalysis.Hull(p, axis, largo * escalaSprite, ancho * escalaSprite);
                bool suspect = hull.Append(p).Any(v => !RoutePreviewAnalysis.Plausible(lectura, v, mapa.categoria == CategoriaMapa.Entrega));
                if (camionSprite != null)
                {
                    var center = Pantalla(rect, p);
                    var saved = GUI.matrix;
                    GUIUtility.RotateAroundPivot(-(angle + orientacionBase), center);
                    float width = largo * escalaSprite * rect.width / 480;
                    float height = ancho * escalaSprite * rect.height / 270;
                    GUI.DrawTextureWithTexCoords(new Rect(center.x - width / 2, center.y - height / 2, width, height), camionSprite.texture, camionUV, true);
                    GUI.matrix = saved;
                }
                if (verHuella || camionSprite == null)
                {
                Handles.color = suspect ? new Color(1, .2f, .1f, .7f) : new Color(1, .85f, .1f, .7f);
                if (camionSprite != null) Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, .15f);
                Handles.DrawAAConvexPolygon(hull.Select(v => Pantalla(rect, v)).ToArray());
                Handles.color = Color.white;
                Handles.DrawAAPolyLine(2, hull.Concat(new[] { hull[0] }).Select(v => Pantalla(rect, v)).ToArray());
                if (camionSprite == null)
                {
                    var cab = RoutePreviewAnalysis.Hull(p + t * largo * escalaSprite * .32f, t, largo * escalaSprite * .25f, ancho * escalaSprite * .9f);
                    Handles.color = new Color(.15f, .5f, 1, 1); Handles.DrawAAConvexPolygon(cab.Select(v => Pantalla(rect, v)).ToArray());
                }
                Handles.color = Color.white; Handles.DrawAAPolyLine(2, Pantalla(rect, p), Pantalla(rect, p + t * largo));
                }
                GUI.Label(new Rect(rect.x + 8, rect.y + 8, 650, 35), suspect ? "HUELLA ROJA: color o límite sospechoso; inspeccionar visualmente." : "Huella amarilla: sin alerta de color local; NO certifica cabida.");
            }
            Handles.EndGUI();
        }

        void Comparar(MapaVisual from, MapaVisual to)
        {
            EditorGUILayout.LabelField(RoutePreviewAnalysis.Connection(from, to), EditorStyles.wordWrappedLabel);
            EditorGUILayout.BeginHorizontal();
            var a = GUILayoutUtility.GetRect(300, 95); var b = GUILayoutUtility.GetRect(300, 95);
            if (from.fondo != null) GUI.DrawTextureWithTexCoords(a, from.fondo, new Rect(.88f, 0, .12f, 1));
            if (to.fondo != null) GUI.DrawTextureWithTexCoords(b, to.fondo, new Rect(0, 0, .12f, 1));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Izquierda: extremo final del origen · derecha: entrada del destino (recortes ampliados, misma escala vertical).", EditorStyles.miniLabel);
        }

        public string RevisarTodos()
        {
            conexiones.Clear();
            var lines = new List<string> { "# Life Drive · revisión Route Preview", "", $"Huella {largo * escalaSprite}×{ancho * escalaSprite} px lógicos; desplazamiento lateral {lateral} px. Progreso por longitud de polilínea.", "", "## Geometría y estimación visual por mapa", "" };
            foreach (var m in catalogo.mapas.Where(m => m != null)) lines.Add("- " + RoutePreviewAnalysis.Analyze(m, largo * escalaSprite, ancho * escalaSprite, lateral, orientacionBase).Summary);
            lines.Add("\n## Conexiones · estimación raster\n");
            foreach (var m in catalogo.mapas.Where(m => m != null))
                foreach (var d in m.siguientes ?? new MapaVisual[0]) if (d != null) { var c = RoutePreviewAnalysis.Connection(m, d); conexiones.Add(c); lines.Add("- " + c); }
            lines.Add("\n## Límites de la revisión\n\nLos segmentos largos indican escasez de puntos, no necesariamente teletransporte. El radio es una aproximación de polilínea: vértices y ruido pueden producir falsos positivos. La clasificación por color no distingue asfalto de tejados o rocas grises, ni reconoce guardarraíles o árboles semánticamente. Sombras y marcas pueden producir falsos positivos. Las huellas que salen en 0/100% pueden estar parcialmente fuera de pantalla por la entrada/salida del sprite. El eje almacenado suele ser la línea central de dos carriles; seleccionar desplazamiento lateral antes de certificar un carril. No existen máscaras de superficie, sprite final, colliders ni validación física. DELIVERY usa transición de escena y el patio tiene una clasificación más permisiva. Ningún PNG o .asset se corrige automáticamente. Revisar todas las alertas visualmente; no aprobar la cabida solo con límites o colores.");
            const string path = "Assets/Mapas/RoutePreviewReview.md";
            File.WriteAllText(path, string.Join("\n", lines)); AssetDatabase.ImportAsset(path);
            diagnostico = "Informe actualizado: " + path;
            return path;
        }
    }
}
