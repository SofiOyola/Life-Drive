using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LifeDrive.Mapas.InterfazVisual
{
    [RequireComponent(typeof(Button))]
    public sealed class RespuestaBotonUI : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,ISelectHandler,IDeselectHandler
    {
        public Color normal=new Color32(77,97,49,255), hover=new Color32(115,132,62,255);
        public bool marcado;
        public Shadow sombra;
        bool encima,presionado,foco;
        Button boton;
        void Awake(){boton=GetComponent<Button>();}
        void Update()
        {
            bool activo=boton.interactable&&(encima||foco||marcado);
            if(!boton.interactable)presionado=false;
            float k=1-Mathf.Exp(-18*Time.unscaledDeltaTime);
            transform.localScale=Vector3.Lerp(transform.localScale,Vector3.one*(presionado?.98f:activo?1.025f:1),k);
            Color destino=!boton.interactable?Color.Lerp(normal,new Color32(24,41,28,255),.45f):activo?hover:normal;
            boton.targetGraphic.color=Color.Lerp(boton.targetGraphic.color,destino,k);
            if(sombra!=null)sombra.effectDistance=Vector2.Lerp(sombra.effectDistance,new Vector2(0,activo?-7:-3),k);
        }
        void OnDisable(){encima=presionado=foco=false;transform.localScale=Vector3.one;}
        public void OnPointerEnter(PointerEventData e){encima=true;}
        public void OnPointerExit(PointerEventData e){encima=false;presionado=false;}
        public void OnPointerDown(PointerEventData e){if(boton.interactable)presionado=true;}
        public void OnPointerUp(PointerEventData e){presionado=false;}
        public void OnSelect(BaseEventData e){foco=true;}
        public void OnDeselect(BaseEventData e){foco=false;presionado=false;}
    }
}
