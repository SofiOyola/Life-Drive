using System.Collections.Generic;
using System.Linq;
using LifeDrive.Mapas.InterfazVisual;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.IntegracionV2
{
    // Presentation only, created by the explicit pilot. Uses the existing EventSystem and submit handlers.
    [DefaultExecutionOrder(-200)]
    public sealed class TecladoPilotoV2 : MonoBehaviour
    {
        static readonly Color Bosque = new Color32(24, 41, 28, 255), Crema = new Color32(245, 235, 207, 255);
        readonly List<Selectable> orden = new List<Selectable>();
        EventSystem eventos;
        bool navegacionAnterior;
        Scene escenaAnterior;
        string faseAnterior;
        GameObject ambitoAnterior;
        TMP_Text guia;
        ScrollRect lectura;
        bool bloquearHastaSoltar;

        void Update()
        {
            if (eventos == EventSystem.current) return;
            RestaurarEventos(); eventos = EventSystem.current;
            if (eventos != null)
            {
                navegacionAnterior = eventos.sendNavigationEvents;
                // InputSystemUIInputModule still sends updateSelected to TMP before this early return.
                // Pointer and text editing remain native. Prevent its submit/move from duplicating ours.
                eventos.sendNavigationEvents = false;
            }
        }

        void LateUpdate()
        {
            if (eventos == null) return;
            ReconstruirOrden();
            if (orden.Count == 0) return;
            var teclado = Keyboard.current;
            if (teclado != null)
            {
                if (!teclado.enterKey.isPressed && !teclado.numpadEnterKey.isPressed && !teclado.spaceKey.isPressed) bloquearHastaSoltar = false;
                if (teclado.tabKey.wasPressedThisFrame) Mover(teclado.shiftKey.isPressed ? -1 : 1);
                else if (teclado.leftArrowKey.wasPressedThisFrame || teclado.rightArrowKey.wasPressedThisFrame)
                {
                    var resultado = FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
                    if (resultado != null && resultado.HistorialAbierto)
                        Enviar(resultado.selectores[teclado.leftArrowKey.wasPressedThisFrame ? 0 : 1]);
                }
                else if (teclado.escapeKey.wasPressedThisFrame && ambitoAnterior != null) Enviar(orden[0]);
                else if (!bloquearHastaSoltar && (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame))
                {
                    var actual = eventos.currentSelectedGameObject;
                    if (actual != null && actual.TryGetComponent<TMP_InputField>(out _))
                    {
                        if (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame) Mover(1);
                        // Space in a field remains text, never a submit.
                    }
                    else if (actual != null && actual.TryGetComponent<Button>(out var boton) && orden.Contains(boton))
                    { bloquearHastaSoltar = true; Enviar(boton); }
                }
                if (lectura != null && eventos.currentSelectedGameObject == lectura.gameObject)
                {
                    float paso = lectura.content.rect.height <= lectura.viewport.rect.height ? 1 :
                        .85f * lectura.viewport.rect.height / (lectura.content.rect.height - lectura.viewport.rect.height);
                    if (teclado.pageDownKey.wasPressedThisFrame) lectura.verticalNormalizedPosition = Mathf.Clamp01(lectura.verticalNormalizedPosition - paso);
                    if (teclado.pageUpKey.wasPressedThisFrame) lectura.verticalNormalizedPosition = Mathf.Clamp01(lectura.verticalNormalizedPosition + paso);
                }
            }
            foreach (var boton in FindObjectsByType<Button>())
            {
                if (boton.targetGraphic == null) continue;
                var color = Contraste(Crema, boton.targetGraphic.color) >= Contraste(Bosque, boton.targetGraphic.color) ? Crema : Bosque;
                foreach (var texto in boton.GetComponentsInChildren<TMP_Text>()) texto.color = color;
            }
            if (guia != null)
            {
                var actual = eventos.currentSelectedGameObject;
                guia.text = actual != null && lectura != null && actual == lectura.gameObject
                    ? "Lectura · RePág / AvPág o rueda para desplazar · Tab para seguir"
                    : "Tab / Mayús+Tab: foco · Enter: avanzar o activar · Espacio: activar botones";
                var registro = FindAnyObjectByType<PersonalizacionRegistroUI>();
                if (registro != null && registro.registro && actual != null)
                {
                    var grupos = new[] { registro.piel, registro.ojos, registro.cabello, registro.traje };
                    for (int g = 0; g < grupos.Length; g++)
                        for (int i = 0; i < grupos[g].Length; i++)
                            if (grupos[g][i].gameObject == actual) guia.text = new[] { "Piel", "Ojos", "Cabello", "Traje" }[g] + ": opción " + (i + 1) + " de 5 · Enter o Espacio para elegir · Tab para seguir";
                }
            }
        }

        void ReconstruirOrden()
        {
            var escena = SceneManager.GetActiveScene();
            bool nuevaEscena = escena != escenaAnterior;
            if (nuevaEscena) { guia = null; escenaAnterior = escena; faseAnterior = null; ambitoAnterior = null; }
            orden.Clear(); lectura = null;
            var botones = FindObjectsByType<Button>();
            var salir = botones.FirstOrDefault(b => b.name == "Seguir en la operación" && b.gameObject.activeInHierarchy);
            var cerrar = botones.FirstOrDefault(b => b.name == "Cerrar reconocimientos" && b.gameObject.activeInHierarchy);
            var ajustes = botones.FirstOrDefault(b => b.name == "Cerrar configuración" && b.gameObject.activeInHierarchy);
            var configuracion = botones.FirstOrDefault(b => b.name == "Configuración" && b.gameObject.activeInHierarchy); // Inicio, Registro y Resultado
            GameObject ambito = salir != null ? salir.transform.parent.gameObject : cerrar != null ? cerrar.transform.parent.gameObject :
                ajustes != null ? ajustes.transform.parent.gameObject : null;
            if (ambito != null)
            {
                Agregar(salir != null ? salir : cerrar != null ? cerrar : ajustes);
                if (salir != null) Agregar(botones.FirstOrDefault(b => b.name == "Confirmar Inicio"));
                if (ajustes != null)
                    foreach (var nombre in new[] { "Pausa", "Reiniciar vista", "Reinicio visual", "Música del juego" })
                        Agregar(botones.FirstOrDefault(b => b.name == nombre && b.gameObject.activeInHierarchy));
            }
            else
            {
                var registro = FindAnyObjectByType<PersonalizacionRegistroUI>();
                var admin = FindAnyObjectByType<VistaAdministrativaDemo>(FindObjectsInactive.Include);
                if (registro != null)
                {
                    if (!registro.registro) { Agregar(registro.jugar); Agregar(registro.administrador); Agregar(configuracion); }
                    else
                    {
                        Agregar(registro.nombre); Agregar(registro.documento);
                        foreach (var b in botones.Where(b => b.name.StartsWith("Ficha piloto ")).OrderBy(b => b.name)) Agregar(b);
                        foreach (var b in registro.estilos.Concat(registro.piel).Concat(registro.ojos).Concat(registro.cabello).Concat(registro.traje)) Agregar(b);
                        Agregar(botones.FirstOrDefault(b => b.name == "Recuperar apariencia")); Agregar(registro.comprobar); Agregar(registro.volver); Agregar(configuracion);
                    }
                }
                else if (admin != null)
                {
                    if (admin.login) { Agregar(admin.usuario); Agregar(admin.contrasena); Agregar(admin.ingresar); Agregar(admin.volver); }
                    else
                    {
                        Agregar(admin.busqueda); Agregar(admin.buscar);
                        var detalle = FindObjectsByType<ScrollRect>().FirstOrDefault(s => s.name == "Detalle desplazable");
                        if (detalle != null && detalle.TryGetComponent<Image>(out var fondo))
                            fondo.color = string.IsNullOrWhiteSpace(admin.detalle.text) ? Color.clear : new Color32(43,64,44,255);
                        foreach (var b in admin.jugadores) Agregar(b);
                        foreach (var nombre in new[] { "Jugadores anteriores", "Jugadores siguientes", "Partida piloto 0", "Partida piloto 1", "Partidas anteriores", "Partidas siguientes" }) Agregar(botones.FirstOrDefault(b => b.name == nombre));
                        Agregar(admin.ver); AgregarLectura(); Agregar(admin.pdf); Agregar(botones.FirstOrDefault(b => b.name == "Abrir PDF piloto")); Agregar(admin.volver);
                    }
                }
                else
                {
                    var resultado = FindAnyObjectByType<PrevisualizacionResultado>(FindObjectsInactive.Include);
                    if (resultado != null)
                    {
                        Agregar(resultado.revisar);
                        foreach (var b in resultado.selectores) Agregar(b);
                        Agregar(resultado.volver);
                        Agregar(configuracion);
                    }
                    else
                    {
                        // Reading order within rows, then next row. New situations focus their first answer below.
                        foreach (var s in FindObjectsByType<Selectable>().OrderByDescending(s => Mathf.Round(s.transform.position.y / 24)).ThenBy(s => s.transform.position.x)) Agregar(s);
                        AgregarLectura();
                    }
                }
            }
            var partida = FindAnyObjectByType<PruebaIntegracionV2>();
            string fase = partida == null ? escena.name : partida.Estado.ToString();
            bool cambioAmbito = ambito != ambitoAnterior;
            var elegido = eventos.currentSelectedGameObject;
            if (nuevaEscena || cambioAmbito && ambito != null || fase != faseAnterior || !orden.Any(s => s.gameObject == elegido))
            {
                string principal = partida == null ? null : partida.Estado == PruebaIntegracionV2.Fase.Situacion ? "Respuesta 1" :
                    partida.Estado == PruebaIntegracionV2.Fase.Consecuencia ? "Continuar" : partida.Estado == PruebaIntegracionV2.Fase.Entregada ? "Nueva prueba" : "Menú del jugador";
                var inicial = ambito != null ? orden.FirstOrDefault() : orden.FirstOrDefault(s => s.name == principal) ?? orden.FirstOrDefault();
                Seleccionar(inicial);
                var teclado = Keyboard.current;
                if (teclado != null && (teclado.enterKey.isPressed || teclado.numpadEnterKey.isPressed || teclado.spaceKey.isPressed)) bloquearHastaSoltar = true;
            }
            faseAnterior = fase; ambitoAnterior = ambito;
            if (guia == null && partida == null) CrearGuia();
        }

        void Agregar(Selectable s)
        {
            if (s == null || !s.isActiveAndEnabled || !s.IsInteractable() || orden.Contains(s)) return;
            orden.Add(s);
            // Sin recuadro de foco (decisión de Javier, 2026-11-14): el botón con foco solo se aclara y crece un poco (RespuestaBotonUI).
            var feedback = s.GetComponent<RespuestaBotonUI>();
            if (feedback != null && Mathf.Max(Contraste(Crema, feedback.hover), Contraste(Bosque, feedback.hover)) < 4.5f)
                feedback.hover = Color.Lerp(feedback.hover, Bosque, .25f);
        }

        void AgregarLectura()
        {
            lectura = FindObjectsByType<ScrollRect>().FirstOrDefault(s => s.content != null && s.content.GetComponent<TMP_Text>() != null && !string.IsNullOrWhiteSpace(s.content.GetComponent<TMP_Text>().text));
            if (lectura == null) return;
            var control = lectura.GetComponent<Selectable>();
            if (control == null) { control = lectura.gameObject.AddComponent<Selectable>(); control.transition = Selectable.Transition.None; }
            Agregar(control);
        }

        void CrearGuia()
        {
            var escena = SceneManager.GetActiveScene();
            var lienzo = FindObjectsByType<Canvas>().FirstOrDefault(c => c.isRootCanvas && c.gameObject.scene == escena);
            var fuente = FindObjectsByType<TMP_Text>().FirstOrDefault(t => t.gameObject.scene == escena && t.font != null && t.name != "Instrucciones de teclado");
            if (lienzo == null || fuente == null) return;
            var fondo = new GameObject("Ayuda de teclado", typeof(RectTransform), typeof(Image));
            fondo.transform.SetParent(lienzo.transform, false); fondo.GetComponent<Image>().color = Bosque; fondo.GetComponent<Image>().raycastTarget = false;
            var r = (RectTransform)fondo.transform; r.anchorMin = new Vector2(0,1); r.anchorMax = Vector2.one; r.pivot = new Vector2(.5f,1);
            r.offsetMin = new Vector2(20,-30); r.offsetMax = new Vector2(-20,-2);
            var textoGuia = new GameObject("Instrucciones de teclado", typeof(RectTransform));
            textoGuia.SetActive(false);
            guia = textoGuia.AddComponent<TextMeshProUGUI>();
            guia.transform.SetParent(fondo.transform,false); guia.rectTransform.anchorMin = Vector2.zero; guia.rectTransform.anchorMax = Vector2.one;
            guia.rectTransform.offsetMin = new Vector2(8,0); guia.rectTransform.offsetMax = new Vector2(-8,0);
            guia.font = fuente.font; guia.fontSize = 20; guia.color = Crema; guia.alignment = TextAlignmentOptions.Center; guia.raycastTarget = false; guia.richText = false;
            guia.enableAutoSizing = true; guia.fontSizeMin = 16; guia.fontSizeMax = 20;
            textoGuia.SetActive(true);
        }

        void Mover(int paso)
        {
            int actual = orden.FindIndex(s => s.gameObject == eventos.currentSelectedGameObject);
            Seleccionar(orden[(actual + paso + orden.Count) % orden.Count]);
        }
        void Seleccionar(Selectable s)
        {
            if (s == null) return;
            eventos.SetSelectedGameObject(s.gameObject);
            if (s is TMP_InputField campo) campo.ActivateInputField();
        }
        void Enviar(Selectable s) { if (s != null && s.IsInteractable()) ExecuteEvents.Execute(s.gameObject, new BaseEventData(eventos), ExecuteEvents.submitHandler); }
        void RestaurarEventos() { if (eventos != null) eventos.sendNavigationEvents = navegacionAnterior; }
        void OnDestroy()
        {
            // The pilot releases its temporary fonts on teardown. Remove our canvas text before its next rebuild.
            if (guia != null) { guia.transform.parent.gameObject.SetActive(false); Destroy(guia.transform.parent.gameObject); }
            RestaurarEventos();
        }

        public static float Contraste(Color a, Color b)
        {
            float Luz(Color c) => .2126f * Mathf.GammaToLinearSpace(c.r) + .7152f * Mathf.GammaToLinearSpace(c.g) + .0722f * Mathf.GammaToLinearSpace(c.b);
            float x = Luz(a), y = Luz(b); return (Mathf.Max(x,y)+.05f)/(Mathf.Min(x,y)+.05f);
        }
    }
}
