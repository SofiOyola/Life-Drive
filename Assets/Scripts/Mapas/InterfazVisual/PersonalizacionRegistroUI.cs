using LifeDrive.Datos;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.Mapas.InterfazVisual
{
    public sealed class PersonalizacionRegistroUI : MonoBehaviour
    {
        public bool registro;
        public Button jugar,administrador,volver,comprobar;
        public TMP_InputField nombre,documento;
        public TMP_Text mensaje,estiloTexto;
        public AvatarPersonalizableUI avatar;
        public Button[] estilos;
        public Button[] piel,ojos,cabello,traje;
        public Color[] tonosPiel,tonosOjos,tonosCabello,tonosTraje;
        public TMP_Text[] valores;
        public int Estilo {get;private set;}
        public int[] Selecciones {get;private set;}=new int[4];
        readonly string[][] nombres={new[]{"Marfil","Arena","Canela","Cacao","Ébano"},new[]{"Café","Verde","Azul","Miel","Gris"},new[]{"Negro","Castaño","Cobre","Rubio","Plata"},new[]{"Musgo","Azul","Terracota","Crema","Ciruela"}};
        void Start()
        {
            if(!registro){jugar.onClick.AddListener(()=>Navegar("Registro"));administrador.onClick.AddListener(()=>mensaje.text="Acceso administrativo pendiente de integración.");return;}
            volver.onClick.AddListener(()=>Navegar("Inicio"));comprobar.onClick.AddListener(Comprobar);
            for(int i=0;i<estilos.Length;i++){int j=i;estilos[i].onClick.AddListener(()=>ElegirEstilo(j));}
            var grupos=new[]{piel,ojos,cabello,traje};
            for(int g=0;g<4;g++)for(int i=0;i<grupos[g].Length;i++){int a=g,b=i;grupos[g][i].onClick.AddListener(()=>ElegirColor(a,b));}
            ElegirEstilo(0);for(int g=0;g<4;g++)ElegirColor(g,0);
        }
        public void ElegirEstilo(int indice)
        {
            if(indice<0||indice>1)return;Estilo=indice;avatar.mujer=indice==1;avatar.Actualizar();
            estiloTexto.text=indice==0?"HOMBRE · VISTA PREVIA":"MUJER · VISTA PREVIA";
            for(int i=0;i<estilos.Length;i++){estilos[i].GetComponent<RespuestaBotonUI>().marcado=i==indice;estilos[i].GetComponentInChildren<TMP_Text>().text=(i==indice?"• ":"")+(i==0?"Hombre":"Mujer");}
        }
        public void ElegirColor(int grupo,int indice)
        {
            if(grupo<0||grupo>3||indice<0||indice>4)return;
            var colores=new[]{tonosPiel,tonosOjos,tonosCabello,tonosTraje};var botones=new[]{piel,ojos,cabello,traje};
            Selecciones[grupo]=indice;Color c=colores[grupo][indice];
            if(grupo==0)avatar.piel=c;else if(grupo==1)avatar.ojos=c;else if(grupo==2)avatar.cabello=c;else avatar.traje=c;
            valores[grupo].text=nombres[grupo][indice];
            for(int i=0;i<5;i++){botones[grupo][i].GetComponent<RespuestaBotonUI>().marcado=i==indice;botones[grupo][i].GetComponentInChildren<TMP_Text>().text=i==indice?"•":"";}
            avatar.Actualizar();
        }
        public void Comprobar()
        {
            // Pure existing format checks only. Appearance is deliberately NOT mapped
            // to any personaje_id; submitting this preview to the service is forbidden.
            if(!Validacion.NombreValido(nombre.text)){mensaje.text="Escribe tu nombre para completar la vista previa.";return;}
            if(!Validacion.CedulaValida(documento.text)){mensaje.text="Documento: utiliza de 5 a 15 dígitos, sin puntos ni letras.";return;}
            mensaje.text="¡Tu apariencia está lista! Previsualización solamente: no se guardaron datos ni se creó un usuario.";
        }
        public void Navegar(string destino)
        {
            if(PrevisualizacionPantallas.NavegarEnEditor!=null&&PrevisualizacionPantallas.NavegarEnEditor(destino))return;
            if(SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/"+destino+".unity")>=0)SceneManager.LoadScene(destino);
            else mensaje.text="Abre esta previsualización desde Unity. Navegación de compilación pendiente.";
        }
    }
}
