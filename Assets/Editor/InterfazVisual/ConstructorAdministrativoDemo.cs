using System;
using System.Reflection;
using LifeDrive.Mapas.InterfazVisual;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LifeDrive.Editor.InterfazVisual
{
    public static class ConstructorAdministrativoDemo
    {
        // Reuse the approved builder primitives, fonts and interaction components.
        static object UI(string method,params object[] args)=>typeof(RedisenoBaseUI).GetMethod(method,BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,args);
        static TMP_Text Text(Transform p,string name,string value,float x,float y,float w,float h,float size=28)=> (TMP_Text)UI("Texto",name,p,x,y,w,h,value,size,false,(Color?)null);
        static Button Button(Transform p,string name,string value,float x,float y,float w,float h)=>(Button)UI("Boton",p,name,value,x,y,w,h,false);
        public static void Construir()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Detener Play y conservar los cambios pendientes.");
            foreach(string name in new[]{"AdminLogin","Admin"})
            {
                string path="Assets/Scenes/"+name+".unity";if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)!=null)continue;
                var c=(Transform)UI("Base",name);var host=new GameObject("Administración DEMO").AddComponent<VistaAdministrativaDemo>();host.login=name=="AdminLogin";
                Text(c,"Titulo",host.login?"ACCESO ADMINISTRATIVO":"PANEL ADMINISTRATIVO",80,55,1600,80,52).font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Mapas/InterfazVisual/BaseNaturaleza/FuentePixel.asset");
                Text(c,"Aviso demo","DEMO · Sin autenticación, consultas ni datos guardados",80,150,1700,55);
                host.volver=Button(c,"Volver","Volver a Inicio",1390,925,450,80);
                host.mensaje=Text(c,"Mensaje","",80,925,1240,100,24);
                if(host.login)
                {
                    Text(c,"Usuario etiqueta","Usuario · campo de prueba",420,290,1000,50);
                    host.usuario=(TMP_InputField)UI("Campo",c,"Usuario","Texto de prueba opcional",420f,350f);
                    Text(c,"Contraseña etiqueta","Contraseña · no utilizar credenciales reales",420,470,1100,50);
                    host.contrasena=(TMP_InputField)UI("Campo",c,"Contraseña","Campo oculto de prueba",420f,530f);host.contrasena.contentType=TMP_InputField.ContentType.Password;
                    host.ingresar=Button(c,"Ingresar","Ingresar · DEMO",420,680,650,90);
                    Text(c,"Nota acceso","Este botón abre la maqueta sin comprobar credenciales.\nLos campos se limpian al continuar.",420,790,1100,90,26);
                }
                else
                {
                    host.busqueda=(TMP_InputField)UI("Campo",c,"Busqueda","Buscar jugador DEMO",80f,270f);host.buscar=Button(c,"Buscar","Buscar",580,270,250,82);
                    host.ejemplos=new[]{new JugadorAdministrativoDemo{nombre="Jugador A · DEMO",puntos=25,correctas=3},new JugadorAdministrativoDemo{nombre="Jugador B · DEMO",puntos=-5,correctas=1,incorrectas=2,retraso=10},new JugadorAdministrativoDemo{nombre="Jugador C · DEMO",puntos=-20,incorrectas=3,retraso=25}};
                    host.jugadores=new Button[3];for(int i=0;i<3;i++)host.jugadores[i]=Button(c,"Jugador "+i,host.ejemplos[i].nombre,80,410+i*115,750,85);
                    host.detalle=Text(c,"Detalle","",920,270,900,470,30);
                    host.ver=Button(c,"Ver","Ver desempeño · DEMO",920,760,540,80);host.pdf=Button(c,"PDF","PDF",1490,760,330,80);
                }
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),path);
            }
        }
    }
}
