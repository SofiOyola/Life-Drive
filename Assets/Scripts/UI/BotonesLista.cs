// Listas de botones generadas en tiempo de ejecución (alternativas, personajes, jugadores, partidas).
// El prefab es un Button con un TMP_Text como hijo; se reutiliza en todas las pantallas.
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace LifeDrive.UI
{
    public static class BotonesLista
    {
        public static void Limpiar(Transform contenedor)
        {
            for (int i = contenedor.childCount - 1; i >= 0; i--) Object.Destroy(contenedor.GetChild(i).gameObject);
        }

        public static Button Agregar(Button prefab, Transform contenedor, string texto, UnityAction alPulsar)
        {
            var boton = Object.Instantiate(prefab, contenedor);
            boton.GetComponentInChildren<TMP_Text>().text = texto;
            boton.onClick.AddListener(alPulsar);
            return boton;
        }
    }
}
