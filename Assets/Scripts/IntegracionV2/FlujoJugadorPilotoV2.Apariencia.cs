using System;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LifeDrive.IntegracionV2
{
    public sealed partial class FlujoJugadorPilotoV2
    {
        Action<Jugador> PrepararPersistenciaRegistro(PersonalizacionRegistroUI vista)
        {
            bool editada = false;
            int estiloInicial = vista.Estilo;
            int[] indicesIniciales = (int[])vista.Selecciones.Clone();
            foreach (var b in vista.estilos.Concat(vista.piel).Concat(vista.ojos).Concat(vista.cabello).Concat(vista.traje))
                b.onClick.AddListener(() => editada = true);

            // Reuse the existing button style without rebuilding the approved registration scene.
            var recuperar = Instantiate(vista.volver, vista.volver.transform.parent);
            recuperar.name = "Recuperar apariencia";
            var rect = (RectTransform)recuperar.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(1168, -978); rect.sizeDelta = new Vector2(288, 88);
            var etiqueta = recuperar.GetComponentInChildren<TMP_Text>();
            etiqueta.text = "Recuperar apariencia"; etiqueta.fontSize = 25;
            etiqueta.rectTransform.anchorMin = Vector2.zero; etiqueta.rectTransform.anchorMax = Vector2.one;
            etiqueta.rectTransform.offsetMin = new Vector2(10, 4); etiqueta.rectTransform.offsetMax = new Vector2(-10, -4);
            vista.mensaje.rectTransform.sizeDelta = new Vector2(900, 88);
            vista.mensaje.rectTransform.anchoredPosition = new Vector2(474, -56);
            recuperar.onClick.RemoveAllListeners();
            recuperar.onClick.AddListener(() =>
            {
                try
                {
                    var previo = Juego.BaseDatos.Jugadores.FirstOrDefault(j => j.cedula == vista.documento.text.Trim());
                    if (previo == null) throw new ErrorJuego("No hay un jugador guardado con esa cédula. Elige una ficha para iniciar.");
                    var jugador = Juego.Usuarios.RegistrarJugador(vista.nombre.text, vista.documento.text, previo.personaje_id);
                    var guardada = Apariencias.LeerPerfilActual();
                    var apariencia = guardada ?? VariantesAparienciaPilotoV1.Crear(jugador.personaje_id, 0, 0, 0, 0, 0);
                    VariantesAparienciaPilotoV1.Aplicar(vista, apariencia);
                    PersonajeId = jugador.personaje_id;
                    foreach (var ficha in vista.nombre.transform.parent.GetComponentsInChildren<Button>())
                        if (ficha.name.StartsWith("Ficha piloto ")) ficha.GetComponent<RespuestaBotonUI>().marcado = ficha.name == "Ficha piloto " + PersonajeId;
                    estiloInicial = vista.Estilo; indicesIniciales = (int[])vista.Selecciones.Clone(); editada = false;
                    vista.mensaje.text = guardada == null ? "Jugador antiguo sin apariencia guardada. Puedes personalizar antes de iniciar." :
                        "Apariencia recuperada. La partida en curso conserva su configuración inicial.";
                }
                catch (ErrorJuego e) { vista.mensaje.text = e.Message; }
            });

            return jugador =>
            {
                var guardada = Apariencias.LeerPerfilActual();
                bool cambio = editada || vista.Estilo != estiloInicial || !vista.Selecciones.SequenceEqual(indicesIniciales);
                if (guardada != null && !cambio)
                {
                    VariantesAparienciaPilotoV1.Aplicar(vista, guardada);
                    return; // Fresh form defaults must never overwrite a saved profile.
                }
                var elegida = VariantesAparienciaPilotoV1.Crear(jugador.personaje_id, vista.Estilo, vista.Selecciones);
                VariantesAparienciaPilotoV1.Aplicar(vista, elegida); // Verify it still matches the approved scene palette.
                Apariencias.GuardarPerfilActual(elegida);
            };
        }
    }
}
