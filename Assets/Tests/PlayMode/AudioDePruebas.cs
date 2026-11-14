#if UNITY_EDITOR
using LifeDrive.IntegracionV2;
using NUnit.Framework;
using UnityEngine;

namespace LifeDrive.Tests
{
    // Las pruebas de audio y de interfaz cambian la preferencia de silencio. Al empezar la corrida se fija en «música activa»
    // (resultados repetibles) y al terminar se devuelve la que tuviera el usuario en su Editor.
    [SetUpFixture]
    public class AudioDePruebas
    {
        int previa;

        [OneTimeSetUp]
        public void Preparar()
        {
            previa = PlayerPrefs.GetInt(GestorAudio.ClaveSilencio);
            PlayerPrefs.SetInt(GestorAudio.ClaveSilencio, 0);
            if (GestorAudio.Instancia != null) GestorAudio.Instancia.Silenciado = false;
        }

        [OneTimeTearDown]
        public void Restaurar()
        {
            PlayerPrefs.SetInt(GestorAudio.ClaveSilencio, previa);
            if (GestorAudio.Instancia != null) GestorAudio.Instancia.Silenciado = previa == 1;
        }
    }
}
#endif
