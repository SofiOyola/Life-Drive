#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using LifeDrive.IntegracionV2;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Pista = LifeDrive.IntegracionV2.GestorAudio.Pista;

namespace LifeDrive.Tests
{
    public class GestorAudioTests
    {
        [TestCase("Inicio", Pista.Inicio)]
        [TestCase("Registro", Pista.Inicio)]
        [TestCase("Resultado", Pista.Inicio)]
        [TestCase("PruebaIntegracionV2", Pista.Ruta)]
        [TestCase("Admin", Pista.Ninguna)]
        [TestCase("AdminLogin", Pista.Ninguna)]
        public void CadaPantallaTieneSuPista(string escena, Pista esperada) =>
            Assert.That(GestorAudio.DeEscena(escena), Is.EqualTo(esperada));

        [UnityTest]
        public IEnumerator RutaDecisionPausaYSilencio()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/PruebaIntegracionV2.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return new WaitForSecondsRealtime(.3f);
            var gestor = GestorAudio.Instancia;
            Assert.That(gestor, Is.Not.Null, "El gestor se crea solo al arrancar.");
            Assert.That(Object.FindObjectsByType<AudioListener>().Length, Is.EqualTo(1), "Sin AudioListener no se oye nada, y con dos Unity avisa.");
            var voces = gestor.GetComponents<AudioSource>();
            bool silenciadoAntes = gestor.Silenciado;
            try
            {
                gestor.Silenciado = false;
                Assert.That(gestor.PistaActual, Is.EqualTo(Pista.Ruta));

                GestorAudio.Sincronizar(true, false);
                yield return new WaitForSecondsRealtime(1.5f);
                Assert.That(gestor.PistaActual, Is.EqualTo(Pista.Decision), "Con una situación abierta suena la pista de decisión.");
                Assert.That(voces.Count(v => v.isPlaying), Is.EqualTo(1), "El cruce termina y queda una sola pista sonando.");
                Assert.That(voces.Max(v => v.volume), Is.GreaterThan(.5f));

                GestorAudio.Sincronizar(false, false);
                yield return new WaitForSecondsRealtime(1.5f);
                Assert.That(gestor.PistaActual, Is.EqualTo(Pista.Ruta), "Al decidir vuelve la pista del recorrido.");

                GestorAudio.Sincronizar(false, true);
                yield return new WaitForSecondsRealtime(.3f);
                Assert.That(voces.Any(v => v.isPlaying), Is.False, "Con el juego en pausa la música se detiene.");
                GestorAudio.Sincronizar(false, false);
                yield return new WaitForSecondsRealtime(.3f);
                Assert.That(voces.Any(v => v.isPlaying), Is.True, "Al reanudar la música sigue.");

                gestor.Silenciado = true;
                yield return new WaitForSecondsRealtime(1.5f);
                Assert.That(voces.Max(v => v.volume), Is.Zero, "Silenciada no se oye nada.");
            }
            finally { gestor.Silenciado = silenciadoAntes; GestorAudio.Sincronizar(false, false); }
        }
    }
}
#endif
