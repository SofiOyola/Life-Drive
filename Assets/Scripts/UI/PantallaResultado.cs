// Prototipo 7 · HU-09: resumen consolidado, no solo el puntaje.
using LifeDrive.Motor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.UI
{
    public class PantallaResultado : MonoBehaviour
    {
        [SerializeField] TMP_Text textoPuntos;
        [SerializeField] TMP_Text textoDetalle;
        [SerializeField] TMP_Text textoRuta;
        [SerializeField] Button botonSalir;

        void Start()
        {
            botonSalir.onClick.AddListener(() => { Sesion.Cerrar(); SceneManager.LoadScene(Escenas.Inicio); });
            if (Sesion.PartidaId == 0) { SceneManager.LoadScene(Escenas.Inicio); return; }

            var r = Sesion.Juego.Desempeno.Resumen(Sesion.PartidaId);
            textoPuntos.text = $"Puntos de seguridad: {r.Partida.puntos}";
            textoDetalle.text =
                $"Respuestas correctas: {r.PreguntasCorrectas}\n" +
                $"Respuestas incorrectas: {r.PreguntasIncorrectas}\n" +
                $"Decisiones seguras: {r.DecisionesSeguras}\n" +
                $"Decisiones inseguras: {r.DecisionesInseguras}\n" +
                $"Riesgos enfrentados: {r.RiesgosEnfrentados}\n" +
                $"Buenas prácticas: {r.BuenasPracticas}\n" +
                $"Retraso acumulado: {r.Partida.retraso_min} min\n\n" +
                "Llegar más rápido no significa un mejor resultado: cuenta lo seguro que fue tu recorrido.";
            textoRuta.text = string.Join("  >  ", r.RutaRecorrida);
        }
    }
}
