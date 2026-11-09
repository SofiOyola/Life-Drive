#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace LifeDrive.Tests
{
    // Virtual device events use the production keyboard path. Text editing uses TMP's native public event entry.
    internal sealed class TeclasPilotoPruebas : IDisposable
    {
        readonly Keyboard anterior = Keyboard.current;
        readonly Keyboard teclado;
        readonly InputSettings ajustes = InputSystem.settings;
        readonly InputSettings.BackgroundBehavior segundoPlano;
        readonly InputSettings.EditorInputBehaviorInPlayMode enrutamiento;
        readonly bool ejecutabaEnSegundoPlano = Application.runInBackground;

        public TeclasPilotoPruebas()
        {
            segundoPlano = ajustes.backgroundBehavior;
            enrutamiento = ajustes.editorInputBehaviorInPlayMode;
            try
            {
                // MCP can run the Editor without foreground focus. Keep synthetic
                // events in the player loop, where the real keyboard handler reads them.
                Application.runInBackground = true;
                ajustes.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                ajustes.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                teclado = InputSystem.AddDevice<Keyboard>();
            }
            catch
            {
                RestaurarConfiguracion();
                throw;
            }
        }

        public IEnumerator Pulsar(Key tecla, bool mayus = false)
        {
            InputSystem.QueueStateEvent(teclado, mayus ? new KeyboardState(Key.LeftShift, tecla) : new KeyboardState(tecla));
            yield return null;
            InputSystem.QueueStateEvent(teclado, new KeyboardState());
            yield return null;
        }
        public IEnumerator MantenerEnter()
        {
            InputSystem.QueueStateEvent(teclado, new KeyboardState(Key.Enter));
            yield return null; yield return null; yield return null;
        }
        public IEnumerator Soltar()
        { InputSystem.QueueStateEvent(teclado, new KeyboardState()); yield return null; }
        public IEnumerator IrA(Selectable destino)
        {
            Assert.That(destino != null && destino.gameObject.activeInHierarchy && destino.IsInteractable(), Is.True, "Destino accesible: " + (destino == null ? "nulo" : destino.name));
            for (int i = 0; i < 90 && EventSystem.current.currentSelectedGameObject != destino.gameObject; i++) yield return Pulsar(Key.Tab);
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(destino.gameObject), destino.name);
        }
        public static void Escribir(string texto)
        {
            var campo = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
            Assert.That(campo, Is.Not.Null, "Foco en campo de entrada");
            Assert.That(campo.isFocused, Is.True);
            foreach (char c in texto) campo.ProcessEvent(new Event { type = EventType.KeyDown, character = c });
            campo.ForceLabelUpdate(); // The native OnUpdateSelected event loop performs this after processing its text events.
            Assert.That(campo.text, Does.EndWith(texto));
        }
        public void Dispose()
        {
            try
            {
                if (teclado.added) InputSystem.RemoveDevice(teclado);
                if (anterior != null && anterior.added) anterior.MakeCurrent();
            }
            finally
            {
                RestaurarConfiguracion();
            }
        }

        void RestaurarConfiguracion()
        {
            ajustes.editorInputBehaviorInPlayMode = enrutamiento;
            ajustes.backgroundBehavior = segundoPlano;
            Application.runInBackground = ejecutabaEnSegundoPlano;
        }
    }
}
#endif
