using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LifeDrive.Mapas.InterfazVisual
{
    public sealed class FocoCampoUI : MonoBehaviour
    {
        public TMP_InputField campo;
        public Graphic borde;
        public Graphic caretVacio;
        public Color reposo=new Color32(82,99,60,255), enfocado=new Color32(223,193,105,255);
        void LateUpdate()
        {
            bool activo=campo.isFocused;
            borde.color=Color.Lerp(borde.color,activo?enfocado:reposo,1-Mathf.Exp(-16*Time.unscaledDeltaTime));
            // TMP's empty text geometry can yield a tiny caret. Use a fixed 32 UI-unit
            // cursor only while empty; TMP retains selection/IME and normal text caret.
            bool vacio=activo&&string.IsNullOrEmpty(campo.text);
            caretVacio.enabled=vacio&&(Time.unscaledTime%1.1f<.65f);
            campo.caretColor=vacio?Color.clear:enfocado;
        }
    }
}
