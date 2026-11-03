using System;
using System.IO;
using LifeDrive.Mapas.InterfazVisual;
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
    public static class RedisenoBaseUI
    {
        const string Root="Assets/Mapas/InterfazVisual/BaseNaturaleza/";
        static TMP_FontAsset pixel,lectura;
        static readonly Color Bosque=Hex("18291C"),Panel=Hex("2B402C"),Musgo=Hex("52653B"),Dorado=Hex("DFC169"),Crema=Hex("F5EBCF"),Suave=Hex("C2C9A8");
        static Color Hex(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c;}
        [MenuItem("Tools/Life Drive/Interfaz/Aplicar base naturaleza a Inicio y Registro")]
        public static void Aplicar()
        {
            if(Application.isPlaying||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Detener Play y conservar los cambios de escena antes de aplicar el rediseño.");
            Fuentes();CrearInicio();CrearRegistro();EditorSceneManager.OpenScene("Assets/Scenes/Inicio.unity");
        }
        static void Fuentes()
        {
            Directory.CreateDirectory(Root+"Prefabs");AssetDatabase.Refresh();
            string path=Root+"FuentePixel.asset";pixel=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if(pixel==null)
            {
                pixel=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(Root+"Fuentes/PixelifySans.ttf"),64,8,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
                pixel.name="Pixelify Sans · Life Drive";AssetDatabase.CreateAsset(pixel,path);pixel.material.hideFlags=HideFlags.None;AssetDatabase.AddObjectToAsset(pixel.material,pixel);
                foreach(var tex in pixel.atlasTextures){tex.hideFlags=HideFlags.None;AssetDatabase.AddObjectToAsset(tex,pixel);}
            }
            string secondary=Root+"FuenteLectura.asset";
            if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(secondary)==null)AssetDatabase.CopyAsset("Assets/Mapas/InterfazVisual/FuenteInterfaz.asset",secondary);
            lectura=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(secondary);
            const string caracteres="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ÁÉÍÓÚáéíóúÑñüÜ¿?¡!.,:;-/() +•·%<>";
            pixel.TryAddCharacters(caracteres,out string missing);lectura.TryAddCharacters(caracteres,out string missing2);
            if(!string.IsNullOrEmpty(missing))Debug.LogWarning("Fuente pixel: faltan "+missing);
            EditorUtility.SetDirty(pixel);EditorUtility.SetDirty(lectura);AssetDatabase.SaveAssetIfDirty(pixel);AssetDatabase.SaveAssetIfDirty(lectura);
        }
        static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x+w/2,-y-h/2);r.sizeDelta=new Vector2(w,h);return r;
        }
        static FormaRedondeadaUI PanelUI(string name,Transform p,float x,float y,float w,float h,Color color,float radius=24,bool circle=false)
        {
            var r=Rect(name,p,x,y,w,h);var f=r.gameObject.AddComponent<FormaRedondeadaUI>();f.color=color;f.radio=radius;f.circular=circle;f.raycastTarget=false;return f;
        }
        static TMP_Text Texto(string name,Transform p,float x,float y,float w,float h,string text,float size,bool principal=false,Color? color=null)
        {
            var r=Rect(name,p,x,y,w,h);r.gameObject.SetActive(false);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=principal?pixel:lectura;t.text=text;t.fontSize=size;t.color=color??Crema;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Overflow;t.alignment=TextAlignmentOptions.MidlineLeft;r.gameObject.SetActive(true);return t;
        }
        static Button Boton(Transform p,string name,string text,float x,float y,float w,float h,bool primary=false)
        {
            var f=PanelUI(name,p,x,y,w,h,primary?Dorado:Musgo,18);f.raycastTarget=true;var b=f.gameObject.AddComponent<Button>();b.targetGraphic=f;b.transition=Selectable.Transition.None;
            var shade=f.gameObject.AddComponent<Shadow>();shade.effectColor=new Color(0,0,0,.28f);shade.effectDistance=new Vector2(0,-3);
            var feedback=f.gameObject.AddComponent<RespuestaBotonUI>();feedback.normal=f.color;feedback.hover=primary?Hex("F2D98B"):Hex("77864C");feedback.sombra=shade;
            var t=Texto("Etiqueta",f.transform,12,4,w-24,h-8,text,primary?38:30,true,primary?Bosque:Crema);t.alignment=TextAlignmentOptions.Center;
            return b;
        }
        static TMP_InputField Campo(Transform p,string name,string hint,float x,float y)
        {
            var border=PanelUI(name,p,x,y,474,82,Musgo,19);border.raycastTarget=true;
            var bg=PanelUI("Interior",border.transform,3,3,468,76,Bosque,16);bg.raycastTarget=true;
            var input=border.gameObject.AddComponent<TMP_InputField>();input.targetGraphic=bg;input.transition=Selectable.Transition.None;input.lineType=TMP_InputField.LineType.SingleLine;
            var viewport=Rect("Viewport",border.transform,22,15,430,52);viewport.gameObject.AddComponent<RectMask2D>();
            var text=Texto("Texto",viewport,0,0,430,52,"",32);text.enableAutoSizing=false;
            var placeholder=Texto("Placeholder",viewport,0,0,430,52,hint,28,false,Suave);
            input.textViewport=viewport;input.textComponent=(TextMeshProUGUI)text;input.placeholder=placeholder;input.customCaretColor=true;input.caretColor=Dorado;input.caretWidth=3;input.selectionColor=new Color(.7f,.65f,.3f,.4f);
            var caret=PanelUI("Caret vacío",viewport,1,10,3,32,Dorado,0);caret.enabled=false;
            var focus=border.gameObject.AddComponent<FocoCampoUI>();focus.campo=input;focus.borde=border;focus.caretVacio=caret;
            return input;
        }
        static Transform Base(string name)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            // Reload after NewScene, which can unload unused assets referenced by static fields.
            pixel=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root+"FuentePixel.asset");lectura=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root+"FuenteLectura.asset");
            var camera=new GameObject("Main Camera",typeof(Camera));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;camera.GetComponent<Camera>().backgroundColor=Bosque;
            new GameObject("Directional Light",typeof(Light)).GetComponent<Light>().type=LightType.Directional;
            var canvas=new GameObject("Interfaz "+name,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scale=canvas.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1920,1080);scale.matchWidthOrHeight=.5f;
            new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule)).GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var raw=Rect("Paisaje S0",canvas.transform,0,0,1920,1080).gameObject.AddComponent<RawImage>();raw.texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Mapas/Backgrounds/S0.png");raw.raycastTarget=false;
            var ratio=raw.gameObject.AddComponent<AspectRatioFitter>();ratio.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;ratio.aspectRatio=16f/9;
            PanelUI("Velo bosque",canvas.transform,0,0,1920,1080,new Color(Bosque.r,Bosque.g,Bosque.b,name=="Inicio"?.28f:.92f),0);
            return canvas.transform;
        }
        static void Prefab(GameObject go,string name){PrefabUtility.SaveAsPrefabAsset(go,Root+"Prefabs/"+name+".prefab");}
        static void CrearInicio()
        {
            var c=Base("Inicio");
            var rim=PanelUI("Volante · aro exterior",c,980,78,894,894,Hex("111C15"),0,true);
            PanelUI("Volante · borde oliva",rim.transform,16,16,862,862,Musgo,0,true);
            PanelUI("Volante · cara",rim.transform,34,34,826,826,Bosque,0,true);
            // Three spoke accents frame the text rather than crossing through it.
            var spoke=PanelUI("Radio izquierdo",rim.transform,36,430,90,30,Musgo,12);spoke.rectTransform.localEulerAngles=new Vector3(0,0,-18);
            spoke=PanelUI("Radio derecho",rim.transform,768,430,90,30,Musgo,12);spoke.rectTransform.localEulerAngles=new Vector3(0,0,18);
            PanelUI("Radio inferior",rim.transform,430,787,34,74,Musgo,12);
            var t=Texto("Ruta",rim.transform,172,105,550,50,"COLOMBIA · EN RUTA",30,true,Dorado);t.alignment=TextAlignmentOptions.Center;
            t=Texto("Marca",rim.transform,146,152,602,260,"LIFE\nDRIVE",106,true);t.alignment=TextAlignmentOptions.Center;t.lineSpacing=-14;
            t=Texto("Lema",rim.transform,175,418,544,80,"Tu camino. Tus decisiones.\nConduce con propósito.",28);t.alignment=TextAlignmentOptions.Center;
            var play=Boton(rim.transform,"Jugar","JUGAR",200,541,494,90,true);
            var admin=Boton(rim.transform,"Administradores","Acceso para administradores",200,658,494,68);
            var msg=Texto("Mensaje",rim.transform,216,743,462,70,"",21,false,Suave);msg.alignment=TextAlignmentOptions.Center;
            Texto("Pie",c,70,966,740,50,"NATURALEZA · CARRETERA · DECISIÓN",28,true);
            Texto("Previsualizacion",c,70,1018,700,30,"PREVISUALIZACIÓN VISUAL · SIN CONEXIÓN AL MOTOR",18,false,Crema);
            var host=new GameObject("Presentación naturaleza").AddComponent<PersonalizacionRegistroUI>();host.jugar=play;host.administrador=admin;host.mensaje=msg;
            Prefab(play.gameObject,"BotonPrimario");Prefab(admin.gameObject,"BotonSecundario");
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/Inicio.unity");
        }
        static void CrearRegistro()
        {
            var c=Base("Registro");
            var titulo=Texto("Titulo",c,68,42,1500,82,"PREPARA TU RECORRIDO",64,true);Prefab(titulo.gameObject,"TituloPixel");
            Texto("Subtitulo",c,72,130,1460,44,"Crea tu apariencia y dale identidad a tu viaje.",27,false,Suave);
            var back=Boton(c,"Volver","Volver",1610,66,238,68);
            var datos=PanelUI("Panel datos",c,64,220,538,674,Panel);
            Texto("Paso A",datos.transform,32,27,474,60,"01  TUS DATOS",34,true,Dorado);
            Texto("Nombre etiqueta",datos.transform,32,116,474,40,"Nombre del jugador",25);
            var nombre=Campo(datos.transform,"Nombre","Escribe tu nombre",32,166);nombre.characterLimit=60;
            Texto("Documento etiqueta",datos.transform,32,280,474,40,"Número de documento",25);
            var doc=Campo(datos.transform,"Documento","De 5 a 15 dígitos",32,330);doc.contentType=TMP_InputField.ContentType.IntegerNumber;doc.characterLimit=15;
            Texto("Privacidad",datos.transform,32,456,474,135,"Esta es una vista de prueba.\nTu nombre, documento y apariencia no se guardan.",24,false,Suave);
            var avatarPanel=PanelUI("Tarjeta apariencia",c,626,220,500,674,Panel);
            Texto("Paso B",avatarPanel.transform,28,27,444,60,"02  TU PERSONAJE",34,true,Dorado);
            var hombre=Boton(avatarPanel.transform,"Hombre","Hombre",28,112,212,64);
            var mujer=Boton(avatarPanel.transform,"Mujer","Mujer",260,112,212,64);
            PanelUI("Marco retrato",avatarPanel.transform,74,202,352,380,Bosque,32);
            var av=Rect("Avatar pixel por capas",avatarPanel.transform,106,202,288,384).gameObject.AddComponent<AvatarPersonalizableUI>();av.raycastTarget=false;
            var estilo=Texto("Estilo",avatarPanel.transform,28,610,444,36,"HOMBRE · VISTA PREVIA",22,true,Suave);estilo.alignment=TextAlignmentOptions.Center;
            var ajustes=PanelUI("Panel personalizacion",c,1150,220,706,674,Panel);
            Texto("Paso C",ajustes.transform,32,27,642,60,"03  A TU ESTILO",34,true,Dorado);
            var host=new GameObject("Presentación naturaleza").AddComponent<PersonalizacionRegistroUI>();host.registro=true;host.nombre=nombre;host.documento=doc;host.volver=back;host.avatar=av;host.estilos=new[]{hombre,mujer};host.estiloTexto=estilo;
            host.tonosPiel=new[]{Hex("EAC6A3"),Hex("D6A37A"),Hex("B77D53"),Hex("855438"),Hex("54392C")};
            host.tonosOjos=new[]{Hex("694527"),Hex("65834D"),Hex("5185A5"),Hex("BB9247"),Hex("87938A")};
            host.tonosCabello=new[]{Hex("292723"),Hex("61422F"),Hex("9E5030"),Hex("D0AC67"),Hex("AAAFA5")};
            host.tonosTraje=new[]{Hex("78834C"),Hex("54748F"),Hex("AE6348"),Hex("D1C3A0"),Hex("82647C")};
            var colors=new[]{host.tonosPiel,host.tonosOjos,host.tonosCabello,host.tonosTraje};var groups=new Button[4][];host.valores=new TMP_Text[4];string[] labels={"Piel","Ojos","Cabello","Traje"};
            for(int g=0;g<4;g++)
            {
                float y=113+g*132;Texto("Etiqueta "+labels[g],ajustes.transform,32,y,280,32,labels[g],25);
                host.valores[g]=Texto("Valor "+labels[g],ajustes.transform,330,y,340,32,"",22,false,Suave);host.valores[g].alignment=TextAlignmentOptions.MidlineRight;
                groups[g]=new Button[5];
                for(int i=0;i<5;i++)
                {
                    var b=Boton(ajustes.transform,labels[g]+" opción "+(i+1),"",32+i*126,y+44,112,62);
                    var feedback=b.GetComponent<RespuestaBotonUI>();feedback.normal=Bosque;feedback.hover=Dorado;b.targetGraphic.color=Bosque;
                    var fill=PanelUI("Muestra",b.transform,5,5,102,52,colors[g][i],14);fill.transform.SetAsFirstSibling();
                    var label=b.GetComponentInChildren<TMP_Text>();label.color=(colors[g][i].grayscale>.5f)?Bosque:Crema;label.fontSize=36;
                    groups[g][i]=b;
                }
            }
            host.piel=groups[0];host.ojos=groups[1];host.cabello=groups[2];host.traje=groups[3];
            var footer=PanelUI("Estado",c,64,920,1266,112,Bosque,20);
            host.mensaje=Texto("Mensaje",footer.transform,24,12,1218,88,"Personaliza tu personaje y comprueba la vista previa. Sin registro real.",24,false,Suave);
            host.comprobar=Boton(c,"Comprobar vista","VER MI PERSONAJE",1360,930,496,92,true);
            Prefab(datos.gameObject,"PanelFormulario");Prefab(nombre.gameObject,"CampoEntrada");Prefab(avatarPanel.gameObject,"TarjetaPersonalizacion");Prefab(ajustes.gameObject,"ContenedorSeleccion");Prefab(av.gameObject,"AvatarPersonalizable");
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/Registro.unity");
        }
    }
}
