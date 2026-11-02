using System.Linq;
using LifeDrive.Mapas;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace LifeDrive.Editor
{
    public class MapCatalogWindow : EditorWindow
    {
        [SerializeField] CatalogoMapas catalogo;
        [SerializeField] MapaVisual seleccionado;
        Vector2 scroll, previewScroll, datosScroll;
        float zoom = 1;
        UnityEditor.Editor inspector;

        [MenuItem("Tools/Life Drive/Map Catalog")]
        public static void Abrir() => GetWindow<MapCatalogWindow>("Map Catalog");

        [OnOpenAsset]
        static bool AbrirRecurso(int id, int line)
        {
            var mapa = Selection.activeObject as MapaVisual;
            if (mapa == null) return false;
            GetWindow<MapCatalogWindow>("Map Catalog").Seleccionar(mapa);
            return true;
        }

        public void Seleccionar(MapaVisual mapa)
        {
            seleccionado = mapa;
            zoom = 1;
            previewScroll = Vector2.zero;
            if (inspector != null) DestroyImmediate(inspector);
            Repaint();
        }

        void OnEnable()
        {
            minSize = new Vector2(760, 600);
            if (catalogo == null)
                catalogo = AssetDatabase.LoadAssetAtPath<CatalogoMapas>("Assets/Mapas/CatalogoMapas.asset");
        }

        void OnDisable() { if (inspector != null) DestroyImmediate(inspector); }

        void OnGUI()
        {
            catalogo = (CatalogoMapas)EditorGUILayout.ObjectField("Catálogo", catalogo, typeof(CatalogoMapas), false);
            if (catalogo == null) { EditorGUILayout.HelpBox("Selecciona un catálogo.", MessageType.Info); return; }
            var problemas = ValidadorContinuidad.Revisar(catalogo);
            EditorGUILayout.HelpBox(problemas.Count == 0 ? "Continuidad: 16 fondos y árbol completo válidos." : string.Join("\n", problemas), problemas.Count == 0 ? MessageType.Info : MessageType.Warning);
            EditorGUILayout.LabelField($"{catalogo.mapas.Length} fondos · clic para ampliar", EditorStyles.boldLabel);
            if (GUILayout.Button("Seleccionar catálogo y editar asociaciones en Inspector")) Selection.activeObject = catalogo;
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(235));
            int columnas = Mathf.Max(1, (int)(position.width / 145));
            for (int i = 0; i < catalogo.mapas.Length; i += columnas)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = i; j < Mathf.Min(i + columnas, catalogo.mapas.Length); j++)
                {
                    var m = catalogo.mapas[j];
                    if (m == null) { GUILayout.Label("Referencia ausente"); continue; }
                    EditorGUILayout.BeginVertical(GUILayout.Width(135));
                    var rect = GUILayoutUtility.GetRect(130, 74);
                    Dibujar(rect, m.miniatura != null ? m.miniatura : m.fondo, m.Id);
                    if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) Seleccionar(m);
                    GUILayout.Label(m.Id + (m.fondo == null ? " · PLACEHOLDER" : ""));
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            if (seleccionado == null) return;
            EditorGUILayout.LabelField(seleccionado.Id + " · " + seleccionado.nombre, EditorStyles.boldLabel);
            if (seleccionado.fondo == null) EditorGUILayout.HelpBox("PLACEHOLDER: falta imagen de fondo definitiva.", MessageType.Warning);
            if (seleccionado.miniatura == null) EditorGUILayout.HelpBox("Falta miniatura; se usa el fondo si existe.", MessageType.Info);
            if (!catalogo.asociaciones.Any(a => a != null && a.mapa == seleccionado && a.tramoId > 0))
                EditorGUILayout.HelpBox("Asociación visual pendiente. Editar en el catálogo; no se modifica ruta.json.", MessageType.Warning);
            if (GUILayout.Button("Seleccionar fondo en Project")) { Selection.activeObject = seleccionado; EditorGUIUtility.PingObject(seleccionado); }
            zoom = EditorGUILayout.Slider("Zoom", zoom, .25f, 4f);
            previewScroll = EditorGUILayout.BeginScrollView(previewScroll, GUILayout.Height(200));
            var tex = seleccionado.fondo;
            var area = GUILayoutUtility.GetRect((tex != null ? tex.width : 640) * zoom, (tex != null ? tex.height : 360) * zoom, GUILayout.ExpandWidth(false));
            Dibujar(area, tex, seleccionado.Id);
            if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
            { previewScroll -= Event.current.delta; Event.current.Use(); Repaint(); }
            EditorGUILayout.EndScrollView();
            datosScroll = EditorGUILayout.BeginScrollView(datosScroll);
            if (inspector == null || inspector.target != seleccionado) UnityEditor.Editor.CreateCachedEditor(seleccionado, null, ref inspector);
            inspector.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
        }

        static void Dibujar(Rect rect, Texture2D textura, string id)
        {
            EditorGUI.DrawRect(rect, new Color(.12f, .16f, .20f));
            if (textura != null) GUI.DrawTexture(rect, textura, ScaleMode.ScaleToFit);
            else GUI.Label(rect, id + "\nPLACEHOLDER", new GUIStyle(EditorStyles.centeredGreyMiniLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 13 });
        }
    }
}

