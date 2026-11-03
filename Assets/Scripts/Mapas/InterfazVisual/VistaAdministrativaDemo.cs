using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LifeDrive.Mapas.InterfazVisual
{
    [Serializable] public sealed class JugadorAdministrativoDemo
    {
        public string nombre;
        public int puntos, correctas, incorrectas, retraso;
    }
    public sealed class VistaAdministrativaDemo : MonoBehaviour
    {
        public bool login;
        public TMP_InputField usuario,contrasena,busqueda;
        public TMP_Text mensaje,detalle;
        public Button ingresar,volver,buscar,ver,pdf;
        public Button[] jugadores;
        public JugadorAdministrativoDemo[] ejemplos;
        public int Seleccionado {get;private set;}=-1;
        void Start()
        {
            volver.onClick.AddListener(()=>Navegar("Inicio"));
            if(login){ingresar.onClick.AddListener(()=>{usuario.text="";contrasena.text="";Navegar("Admin");});return;}
            for(int i=0;i<jugadores.Length;i++){int n=i;jugadores[i].onClick.AddListener(()=>Seleccionar(n));}
            buscar.onClick.AddListener(()=>{for(int i=0;i<jugadores.Length;i++)jugadores[i].gameObject.SetActive(ejemplos[i].nombre.IndexOf(busqueda.text,StringComparison.OrdinalIgnoreCase)>=0);mensaje.text="Filtro local DEMO. No se consultó la base de datos.";});
            ver.onClick.AddListener(MostrarDetalle);
            pdf.onClick.AddListener(()=>mensaje.text="PDF real pendiente de integración. No se generó ni guardó ningún reporte.");
            Seleccionar(0);
        }
        public void Seleccionar(int i){Seleccionado=i;detalle.text="Seleccionado: "+ejemplos[i].nombre+"\nPulsa Ver desempeño para consultar el ejemplo.";for(int n=0;n<jugadores.Length;n++)jugadores[n].GetComponent<RespuestaBotonUI>().marcado=n==i;}
        public void MostrarDetalle(){if(Seleccionado<0)return;var e=ejemplos[Seleccionado];detalle.text=e.nombre+" · DATOS DEMO\n\nPartidas finalizadas: 1\nPuntos de seguridad: "+e.puntos+"\nRespuestas correctas: "+e.correctas+"\nRespuestas incorrectas: "+e.incorrectas+"\nRetraso acumulado: "+e.retraso+" min\n\nInstantánea ficticia. Sin evaluación ni consulta real.";}
        void Navegar(string escena){if(PrevisualizacionPantallas.NavegarEnEditor!=null&&PrevisualizacionPantallas.NavegarEnEditor(escena))return;mensaje.text="Abrir esta previsualización en el Editor. Navegación pendiente en compilación.";}
    }
}
