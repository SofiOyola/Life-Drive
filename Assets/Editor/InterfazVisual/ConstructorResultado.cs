using System.Linq;
using LifeDrive.Mapas.Demo;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.Editor.InterfazVisual
{
    public static class ConstructorResultado
    {
        const string Raiz = "Assets/Mapas/InterfazVisual/";
        const string Datos = Raiz + "Resultados/";
        static TMP_FontAsset fuente;
        static readonly Color Fondo = new Color32(13,28,36,255), Panel = new Color32(23,44,52,255), Claro = new Color32(244,240,217,255), Aqua = new Color32(100,218,190,255);
        public static string Construir()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().isDirty) return "Conservar cambios pendientes y detener Play antes de construir.";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Resultado.unity") != null) return "Resultado ya existe: no se sobrescribe.";
            string fuentePath = Datos + "FuenteResultado.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fuentePath) == null) AssetDatabase.CopyAsset(Raiz + "FuenteInterfaz.asset", fuentePath);
            fuente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fuentePath);
            fuente.TryAddCharacters("%·", out string faltantes); EditorUtility.SetDirty(fuente); AssetDatabase.SaveAssetIfDirty(fuente);
            var ejemplos = CrearDatos();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            fuente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fuentePath);
            ejemplos = new[]{"OperacionSeguraDemo","EntregaComplicacionesDemo","OperacionCriticaDemo"}.Select(n=>AssetDatabase.LoadAssetAtPath<ResultadoEjemploDemo>(Datos+n+".asset")).ToArray();
            var camera = new GameObject("Main Camera");camera.tag="MainCamera";camera.AddComponent<Camera>().backgroundColor=Fondo;camera.transform.position=new Vector3(0,0,-10);
            new GameObject("Directional Light").AddComponent<Light>().type=LightType.Directional;
            var canvas=new GameObject("Resultado visual",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var es=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var c=canvas.transform;Caja(c,"Fondo",0,0,1920,1080,Fondo);
            Texto(c,"Modo",80,36,1760,35,"LIFE DRIVE  /  RESULTADOS DEMO",26,Aqua,true);
            var titulo=Texto(c,"Titulo",80,90,1760,72,ejemplos[0].titulo,50,Aqua,true);
            var aviso=Texto(c,"Aviso",80,165,1760,34,"DEMO · Tres ejemplos independientes. Sin clasificación automática ni datos guardados.",24,Claro);
            var opciones=new[]{"1 · Segura (DEMO)","2 · Complicaciones (DEMO)","3 · Crítica (DEMO)"};var selectores=new Button[3];
            for(int i=0;i<3;i++)selectores[i]=Boton(c,"Selector "+i,opciones[i],80+i*590,222,570,58);
            var acento=Caja(c,"Acento",80,298,1760,6,Aqua);
            Caja(c,"Panel entrega",80,314,670,602,Panel);Caja(c,"Panel balance",780,314,540,602,Panel);Caja(c,"Panel insignias",1350,314,490,602,Panel);
            var mapaR=Rect(c,"DELIVERY existente",100,335,630,354.375f);var mapa=mapaR.gameObject.AddComponent<RawImage>();mapa.texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Mapas/Backgrounds/DELIVERY.png");mapa.raycastTarget=false;
            var camionR=Rect(mapaR,"Camión independiente",630*.8f-11.8125f,354.375f*.48f-3.51f,23.625f,7.02f);var camion=camionR.gameObject.AddComponent<RawImage>();camion.texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Mapas/Vehiculos/CamionCarga.png");camion.uvRect=new Rect(.19386746f,.24678664f,.6177052f,.47686374f);camion.raycastTarget=false;
            var entrega=Texto(c,"Estado entrega",110,720,610,50,"ENTREGA COMPLETADA · DELIVERY",28,Claro,true);
            var progreso=Texto(c,"Progreso",110,792,610,40,"TRAYECTO COMPLETADO · 100%",26,Aqua,true);
            for(int i=0;i<20;i++)Caja(c,"Segmento "+i,110+i*30,854,27,16,Aqua);
            var retratoR=Rect(c,"Personaje",810,345,125,125);var retrato=retratoR.gameObject.AddComponent<RawImage>();retrato.raycastTarget=false;
            var jugador=Texto(c,"Jugador",954,355,335,44,"Conductor DEMO",29,Claro,true);
            var personaje=Texto(c,"Nombre personaje",954,410,335,60,"Personaje: Camila",25,Claro);
            Texto(c,"Etiqueta seguridad",810,502,480,35,"SEGURIDAD · PUNTOS SIMULADOS",24,Aqua,true);
            var puntos=Texto(c,"Puntos",810,548,480,76,"+25 pts DEMO",50,Claro,true);
            var resumen=Texto(c,"Resumen",810,650,480,108,ejemplos[0].resumen,25,Claro);
            var educativo=Texto(c,"Mensaje educativo",810,788,480,100,ejemplos[0].mensajeEducativo,23,Claro);
            var conteo=Texto(c,"Conteo insignias",1374,340,442,42,"INSIGNIAS DEMO · 3/3",28,Aqua,true);
            var nombres=new TMP_Text[3];var estados=new TMP_Text[3];var iconos=new RawImage[3];
            for(int i=0;i<3;i++)
            {
                float y=414+i*157;var r=Rect(c,"Icono insignia "+i,1378,y,54,54);iconos[i]=r.gameObject.AddComponent<RawImage>();iconos[i].color=Color.clear;iconos[i].raycastTarget=false;
                nombres[i]=Texto(c,"Nombre insignia "+i,1447,y,365,75,"Reconocimiento",26,Claro,true);
                estados[i]=Texto(c,"Estado insignia "+i,1378,y+88,435,47,"OBTENIDA EN ESTE EJEMPLO",20,Aqua);
            }
            var historial=Caja(c,"Decisiones del ejemplo",80,314,1760,602,new Color32(23,44,52,255)).gameObject;
            Texto(historial.transform,"Titulo decisiones",40,25,1680,50,"DECISIONES DEMO · NO ES UN REGISTRO REAL",32,Aqua,true);
            var filas=new TMP_Text[3];for(int i=0;i<3;i++)filas[i]=Texto(historial.transform,"Decision "+i,40,112+i*142,1680,118,"Decisión y consecuencia de ejemplo",27,Claro);
            Texto(historial.transform,"Nota decisiones",40,548,1680,38,"Datos escritos para esta maqueta. No se consultan decisiones ni eventos de una partida.",23,Claro);historial.SetActive(false);
            var revisar=Boton(c,"Revisar","Revisar decisiones DEMO",80,955,620,72);var volver=Boton(c,"Inicio","Volver a Inicio",1330,955,510,72);
            var host=new GameObject("Presentacion Resultado DEMO");var p=host.AddComponent<PrevisualizacionResultado>();
            p.ejemplos=ejemplos;p.definiciones=AssetDatabase.LoadAssetAtPath<GamificacionDemo>("Assets/Mapas/DemoInterfaz/GamificacionS304Demo.asset");p.titulo=titulo;p.jugador=jugador;p.personaje=personaje;p.puntos=puntos;p.resumen=resumen;p.educativo=educativo;p.entrega=entrega;p.progreso=progreso;p.conteoInsignias=conteo;p.aviso=aviso;p.nombresInsignias=nombres;p.estadosInsignias=estados;p.iconosInsignias=iconos;p.retrato=retrato;p.atlasPersonajes=AssetDatabase.LoadAssetAtPath<Texture2D>(Raiz+"Personajes/PersonajesAtlas.png");p.acento=acento;p.selectores=selectores;p.volver=volver;p.revisar=revisar;p.panelDecisiones=historial;p.filasDecisiones=filas;
            var original=host.AddComponent<PantallaResultado>();original.enabled=false;var s=new SerializedObject(original);s.FindProperty("textoPuntos").objectReferenceValue=puntos;s.FindProperty("textoDetalle").objectReferenceValue=resumen;s.FindProperty("textoRuta").objectReferenceValue=entrega;s.FindProperty("botonSalir").objectReferenceValue=volver;s.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/Resultado.unity");return "Resultado DEMO creado, sin cambiar escenas previas ni Build Settings.";
        }
        static ResultadoEjemploDemo[] CrearDatos()
        {
            var nombres=new[]{"OperacionSeguraDemo","EntregaComplicacionesDemo","OperacionCriticaDemo"};var titulos=new[]{"OPERACIÓN SEGURA","ENTREGA CON COMPLICACIONES","OPERACIÓN CRÍTICA"};var puntos=new[]{25,-5,-20};
            var insignias=new[]{new[]{"demo.preventivo","demo.responsable","demo.emergencia"},new[]{"demo.preventivo"},new string[0]};
            var resumenes=new[]{"La entrega de este ejemplo se completa sin complicaciones importantes.","La entrega se completa con coordinación pendiente y retrasos moderados en este ejemplo.","La entrega se completa, pero este ejemplo reporta consecuencias graves para la operación."};
            var mensajes=new[]{"La prevención y la comunicación acompañan una entrega segura.","Revisar lo ocurrido ayuda a mejorar la siguiente operación.","Llegar al destino no elimina las consecuencias de las decisiones."};
            var output=new ResultadoEjemploDemo[3];
            for(int i=0;i<3;i++)
            {
                string path=Datos+nombres[i]+".asset";output[i]=AssetDatabase.LoadAssetAtPath<ResultadoEjemploDemo>(path);if(output[i]!=null)continue;
                var e=ScriptableObject.CreateInstance<ResultadoEjemploDemo>();e.titulo=titulos[i];e.tono=(TonoResultadoDemo)i;e.puntosSimulados=puntos[i];e.insignias=insignias[i];e.resumen=resumenes[i];e.mensajeEducativo=mensajes[i];
                e.decisiones=new[]{new DecisionResumenDemo{decision=i<2?"Confirmar el aviso de acceso":"Omitir la confirmación del aviso",consecuencia=i<2?"En el ejemplo se dispone de información para preparar la salida.":"El ejemplo presenta una salida con información incompleta.",cambioIlustrativo=i<2?10:-5},new DecisionResumenDemo{decision=i==0?"Coordinar el corredor":"Continuar sin confirmar el enlace",consecuencia=i==0?"El equipo mantiene una coordinación clara en el ejercicio.":"El ejemplo incluye una reprogramación y dificultades de coordinación.",cambioIlustrativo=i==0?5:-5},new DecisionResumenDemo{decision=i==0?"Confirmar recepción del aviso de apoyo":"Omitir el aviso del ejercicio",consecuencia=i==0?"El aviso simulado se atiende antes de llegar al patio.":i==1?"La entrega se completa con complicaciones moderadas en esta instantánea.":"La entrega llega a DELIVERY con una afectación grave reportada en esta instantánea ficticia.",cambioIlustrativo=i==0?10:-10}};
                AssetDatabase.CreateAsset(e,path);output[i]=e;
            }
            return output;
        }
        static RectTransform Rect(Transform p,string n,float x,float y,float w,float h)
        {
            var g=new GameObject(n,typeof(RectTransform));g.transform.SetParent(p,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        static Image Caja(Transform p,string n,float x,float y,float w,float h,Color color){var r=Rect(p,n,x,y,w,h);var i=r.gameObject.AddComponent<Image>();i.color=color;i.raycastTarget=false;return i;}
        static TMP_Text Texto(Transform p,string n,float x,float y,float w,float h,string texto,float tam,Color color,bool bold=false)
        {
            var r=Rect(p,n,x,y,w,h);r.gameObject.SetActive(false);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=fuente;t.text=texto;t.fontSize=tam;t.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;t.color=color;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Truncate;t.raycastTarget=false;r.gameObject.SetActive(true);return t;
        }
        static Button Boton(Transform p,string n,string texto,float x,float y,float w,float h)
        {
            var g=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Raiz+"Prefabs/BotonPrincipal.prefab"),p);g.name=n;var r=g.GetComponent<RectTransform>();r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);var t=g.GetComponentInChildren<TMP_Text>();t.font=fuente;t.text=texto;t.fontSize=28;t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.pivot=new Vector2(.5f,.5f);t.rectTransform.offsetMin=new Vector2(14,5);t.rectTransform.offsetMax=new Vector2(-14,-5);return g.GetComponent<Button>();
        }
    }
}
