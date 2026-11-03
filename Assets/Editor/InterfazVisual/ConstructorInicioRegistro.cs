using System;
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace LifeDrive.Editor.InterfazVisual
{
    public static class ConstructorInicioRegistro
    {
        const string Carpeta = "Assets/Mapas/InterfazVisual/";
        static TMP_FontAsset fuente;
        static GameObject botonPrefab, campoPrefab, tarjetaPrefab;
        static readonly Color Tinta = new Color32(13,28,36,255), Panel = new Color32(23,44,52,255), Claro = new Color32(244,240,217,255), Aqua = new Color32(100,218,190,255);

        public static string Construir()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().isDirty) return "Detener Play Mode y conservar primero cambios pendientes.";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Inicio.unity") != null || AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Registro.unity") != null)
                return "Las escenas ya existen; no se sobrescriben.";
            var importer = (TextureImporter)AssetImporter.GetAtPath(Carpeta + "Personajes/PersonajesAtlas.png");
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048; importer.SaveAndReimport();
            CrearFuente(); CrearPrefabs(); CrearInicio(); CrearRegistro();
            EditorSceneManager.OpenScene("Assets/Scenes/Inicio.unity");
            return "Inicio y Registro creadas. Controladores originales configurados y desactivados; previsualización sin base de datos. Build Settings intactos.";
        }
        static void CrearFuente()
        {
            if (!AssetDatabase.IsValidFolder(Carpeta + "Resources")) AssetDatabase.CreateFolder(Carpeta.TrimEnd('/'), "Resources");
            var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(Carpeta + "Resources/TMP Settings.asset");
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<TMP_Settings>();
                var serialized = new SerializedObject(settings); serialized.FindProperty("assetVersion").stringValue = "2"; serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(settings, Carpeta + "Resources/TMP Settings.asset");
            }
            string path = Carpeta + "FuenteInterfaz.asset";
            fuente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fuente != null) return;
            fuente = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(Carpeta + "Fuentes/Inter-Regular.ttf"), 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            fuente.name = "Fuente Interfaz Life Drive";
            AssetDatabase.CreateAsset(fuente, path);
            fuente.material.hideFlags = HideFlags.None; AssetDatabase.AddObjectToAsset(fuente.material, fuente);
            foreach (var atlas in fuente.atlasTextures) { atlas.hideFlags = HideFlags.None; AssetDatabase.AddObjectToAsset(atlas, fuente); }
            fuente.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ÁÉÍÓÚáéíóúÑñüÜ¿?¡!.,:;-/() +•", out string faltantes);
            EditorUtility.SetDirty(fuente); AssetDatabase.SaveAssetIfDirty(fuente);
            var ss = new SerializedObject(settings); ss.FindProperty("m_defaultFontAsset").objectReferenceValue = fuente; ss.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(settings);
        }
        static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = new Vector2(0,1); r.pivot = new Vector2(0,1); r.anchoredPosition = new Vector2(x,-y); r.sizeDelta = new Vector2(w,h); return r;
        }
        static Image Caja(string name, Transform parent, float x, float y, float w, float h, Color color)
        {
            var r = Rect(name,parent,x,y,w,h); var i = r.gameObject.AddComponent<Image>(); i.color = color; i.raycastTarget = false; return i;
        }
        static TMP_Text Texto(string name, Transform parent, float x, float y, float w, float h, string text, float size, Color color, bool bold = false)
        {
            var r = Rect(name,parent,x,y,w,h); r.gameObject.SetActive(false);
            var t = r.gameObject.AddComponent<TextMeshProUGUI>(); t.font = fuente; t.text = text; t.fontSize = size; t.color = color; t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            t.textWrappingMode = TextWrappingModes.Normal; t.overflowMode = TextOverflowModes.Truncate; t.raycastTarget = false;
            r.gameObject.SetActive(true); return t;
        }
        static void Colores(Button b)
        {
            var c = b.colors; c.normalColor = Color.white; c.highlightedColor = new Color(.8f,1,.9f); c.pressedColor = new Color(.6f,.85f,.7f); c.selectedColor = new Color(.8f,1,.9f); c.disabledColor = new Color(.55f,.6f,.6f); c.fadeDuration = .08f; b.colors = c;
        }
        static void CrearPrefabs()
        {
            var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Recursos temporales",typeof(RectTransform));
            var bg = Caja("BotonPrincipal",root.transform,0,0,600,84,Aqua); bg.raycastTarget = true;
            var b = bg.gameObject.AddComponent<Button>(); b.targetGraphic = bg; Colores(b);
            var label = Texto("Etiqueta",bg.transform,24,10,552,64,"Jugar",32,Tinta,true); label.alignment = TextAlignmentOptions.Midline;
            botonPrefab = PrefabUtility.SaveAsPrefabAsset(bg.gameObject, Carpeta + "Prefabs/BotonPrincipal.prefab");

            var fieldBg = Caja("CampoTexto",root.transform,0,120,420,74,Panel); fieldBg.raycastTarget = true;
            var field = fieldBg.gameObject.AddComponent<TMP_InputField>(); field.targetGraphic = fieldBg;
            var view = Rect("Viewport",fieldBg.transform,18,8,384,58); view.gameObject.AddComponent<RectMask2D>();
            var inputText = Texto("Texto",view,0,0,384,58,"",28,Claro); inputText.alignment = TextAlignmentOptions.MidlineLeft;
            var placeholder = Texto("Placeholder",view,0,0,384,58,"Escribe aquí",26,new Color(.65f,.72f,.72f)); placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            field.textViewport = view; field.textComponent = (TextMeshProUGUI)inputText; field.placeholder = placeholder; field.lineType = TMP_InputField.LineType.SingleLine; field.customCaretColor = true; field.caretColor = Aqua; field.selectionColor = new Color(.2f,.7f,.6f,.4f);
            campoPrefab = PrefabUtility.SaveAsPrefabAsset(fieldBg.gameObject, Carpeta + "Prefabs/CampoTexto.prefab");

            var marco = Caja("TarjetaPersonaje",root.transform,500,0,290,550,new Color32(65,88,94,255)); marco.raycastTarget = true;
            var cuerpo = Caja("Cuerpo",marco.transform,4,4,282,542,Panel); cuerpo.raycastTarget = true;
            var button = marco.gameObject.AddComponent<Button>(); button.targetGraphic = cuerpo; Colores(button);
            var nombre = Texto("Nombre",marco.transform,20,322,250,52,"Personaje",34,Claro,true);
            var rawRect = Rect("Retrato",marco.transform,16,30,258,258); var raw = rawRect.gameObject.AddComponent<RawImage>(); raw.raycastTarget = false;
            var descripcion = Texto("Descripcion",marco.transform,20,390,250,85,"Descripción del contenido",23,Claro);
            var estado = Texto("Seleccion",marco.transform,20,493,250,38,"Seleccionar",24,Aqua,true);
            var tarjeta = marco.gameObject.AddComponent<TarjetaPersonajeVisual>(); tarjeta.boton = button; tarjeta.nombre = nombre; tarjeta.descripcion = descripcion; tarjeta.estado = estado; tarjeta.retrato = raw; tarjeta.marco = marco;
            tarjetaPrefab = PrefabUtility.SaveAsPrefabAsset(marco.gameObject, Carpeta + "Prefabs/TarjetaPersonaje.prefab");
            UnityEngine.Object.DestroyImmediate(root);
        }
        static Transform Base(string nombre)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera = new GameObject("Main Camera"); camera.tag = "MainCamera"; var cam = camera.AddComponent<Camera>(); cam.backgroundColor = Tinta; cam.clearFlags = CameraClearFlags.SolidColor; camera.transform.position = new Vector3(0,0,-10);
            new GameObject("Directional Light").AddComponent<Light>().type = LightType.Directional;
            var canvasGo = new GameObject("Interfaz " + nombre,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = .5f;
            var es = new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule)); es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var bgRect = Rect("Fondo S0 existente",canvasGo.transform,0,0,1920,1080);
            var raw = bgRect.gameObject.AddComponent<RawImage>(); raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Mapas/Backgrounds/S0.png"); raw.raycastTarget = false;
            var ratio = bgRect.gameObject.AddComponent<AspectRatioFitter>(); ratio.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; ratio.aspectRatio = 16f/9;
            return canvasGo.transform;
        }
        static Button Boton(Transform parent, string name, string label, float x,float y,float w,float h)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(botonPrefab,parent); go.name = name;
            var r = go.GetComponent<RectTransform>(); r.anchoredPosition = new Vector2(x,-y); r.sizeDelta = new Vector2(w,h);
            var t = go.GetComponentInChildren<TMP_Text>(); t.text = label; var tr = t.rectTransform; tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.pivot = new Vector2(.5f,.5f); tr.offsetMin = new Vector2(20,5); tr.offsetMax = new Vector2(-20,-5);
            return go.GetComponent<Button>();
        }
        static TMP_InputField Campo(Transform parent,string name,string hint,float x,float y)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(campoPrefab,parent); go.name = name; go.GetComponent<RectTransform>().anchoredPosition = new Vector2(x,-y);
            var f = go.GetComponent<TMP_InputField>(); ((TMP_Text)f.placeholder).text = hint; return f;
        }
        static void Asignar(UnityEngine.Object objeto,string nombre,UnityEngine.Object valor)
        {
            var s = new SerializedObject(objeto); s.FindProperty(nombre).objectReferenceValue = valor; s.ApplyModifiedPropertiesWithoutUndo();
        }
        static void CrearInicio()
        {
            var c = Base("Inicio"); Caja("Sombra de fondo",c,0,0,1920,1080,new Color(0,0,0,.22f));
            Caja("Panel principal",c,80,115,810,850,new Color(.05f,.11f,.14f,.96f)); Caja("Acento",c,80,115,810,8,Aqua);
            Texto("Marca",c,132,174,700,44,"COLOMBIA  /  EN RUTA",27,Aqua,true);
            Texto("Life Drive",c,124,258,720,280,"LIFE\nDRIVE",112,Claro,true);
            Texto("Presentacion",c,132,548,670,84,"Carreteras colombianas.\nDecisiones conscientes.",29,Claro);
            var jugar = Boton(c,"Jugar","Jugar",132,650,700,88);
            var admin = Boton(c,"Administradores","Acceso para administradores",132,762,700,66); admin.GetComponent<Image>().color = new Color32(163,194,187,255);
            var mensaje = Texto("Mensajes",c,132,840,700,100,"Vista de presentación. No crea usuarios.",22,Claro);
            Texto("Nota de vista",c,1120,916,700,70,"LIFE DRIVE\nPREVISUALIZACIÓN DE INTERFAZ",25,Claro,true);
            var host = new GameObject("Presentacion Inicio"); var preview = host.AddComponent<PrevisualizacionPantallas>(); preview.jugar=jugar;preview.administrador=admin;preview.mensaje=mensaje;
            var original=host.AddComponent<PantallaInicio>(); original.enabled=false;
            Asignar(original,"botonJugar",jugar);Asignar(original,"botonAdministrador",admin);Asignar(original,"textoMensaje",mensaje);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/Inicio.unity");
        }
        static void CrearRegistro()
        {
            var c=Base("Registro");Caja("Fondo oscuro",c,0,0,1920,1080,new Color(.025f,.06f,.08f,.94f));
            Texto("Titulo",c,96,62,1250,76,"PREPARA TU RECORRIDO",54,Claro,true);
            Texto("Modo",c,96,144,1500,38,"VISTA DE PRESENTACIÓN  ·  NO REGISTRA USUARIOS",24,Aqua,true);
            var volver=Boton(c,"Volver","Volver",1640,76,184,58);
            Texto("Tus datos",c,96,230,420,60,"Tus datos",34,Claro,true);
            Texto("Etiqueta nombre",c,96,320,420,40,"Nombre del jugador",24,Claro);
            var nombre=Campo(c,"Nombre","Tu nombre",96,370);
            Texto("Etiqueta documento",c,96,478,420,40,"Número de documento",24,Claro);
            var documento=Campo(c,"Documento","De 5 a 15 dígitos",96,528);documento.contentType=TMP_InputField.ContentType.IntegerNumber;documento.characterLimit=15;
            var mensaje=Texto("Validacion",c,96,646,420,150,"Elige un personaje y comprueba los datos. No se guardará información.",24,Claro);
            var comprobar=Boton(c,"Comprobar datos","Comprobar datos",96,822,420,76);
            Texto("Elige personaje",c,580,230,1240,60,"¿Quién te acompaña al volante?",34,Claro,true);
            var personajes=new Almacen(Path.Combine(Application.streamingAssetsPath,"Contenido")).CargarContenido().personajes.OrderBy(p=>p.id).ToArray();
            var tarjetas=new TarjetaPersonajeVisual[4];var contenedor=Rect("Personajes",c,580,320,1244,550);
            var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(Carpeta+"Personajes/PersonajesAtlas.png");
            for(int i=0;i<4;i++)
            {
                var go=(GameObject)PrefabUtility.InstantiatePrefab(tarjetaPrefab,contenedor);go.name="Personaje "+personajes[i].id+" - "+personajes[i].nombre;go.GetComponent<RectTransform>().anchoredPosition=new Vector2(i*310,0);
                tarjetas[i]=go.GetComponent<TarjetaPersonajeVisual>();tarjetas[i].Configurar(personajes[i].id,personajes[i].nombre,personajes[i].descripcion,atlas,new Rect(i%2*.5f,i<2?.5f:0,.5f,.5f));
            }
            Texto("Nota retratos",c,580,912,1240,80,"Representaciones visuales de concepto. Sin habilidades ni estadísticas adicionales.",23,Claro);
            var host=new GameObject("Presentacion Registro");var preview=host.AddComponent<PrevisualizacionPantallas>();preview.registro=true;preview.nombre=nombre;preview.documento=documento;preview.mensaje=mensaje;preview.volver=volver;preview.comprobar=comprobar;preview.tarjetas=tarjetas;preview.atlas=atlas;
            var original=host.AddComponent<PantallaRegistro>();original.enabled=false;
            Asignar(original,"campoNombre",nombre);Asignar(original,"campoCedula",documento);Asignar(original,"contenedorPersonajes",contenedor);Asignar(original,"prefabBoton",tarjetaPrefab.GetComponent<Button>());Asignar(original,"botonIniciar",comprobar);Asignar(original,"botonVolver",volver);Asignar(original,"textoMensaje",mensaje);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/Registro.unity");
        }
    }
}
