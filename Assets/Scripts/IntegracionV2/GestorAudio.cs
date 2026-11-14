using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LifeDrive.IntegracionV2
{
    // Música de fondo: una pista por pantalla y cambio suave entre pistas. Se crea sola al arrancar,
    // así que no hay que tocar ninguna escena. Las pistas están en Assets/Audio/Resources/Musica (Ogg, en bucle continuo).
    //   Inicio, Registro, Resultado -> InicioRuta | recorrido -> RutaNormal | situación abierta -> TomaDecision
    // Accesibilidad: la música se pausa con el juego; la tecla M, o el botón «Configuración» de cada pantalla, la silencian
    // (se recuerda entre partidas). El botón de Inicio, Registro y Resultado lo añade ConfiguracionPantalla; el del recorrido
    // es parte de su propio panel.
    public sealed class GestorAudio : MonoBehaviour
    {
        public enum Pista { Ninguna, Inicio, Ruta, Decision }

        public const string ClaveSilencio = "lifedrive.audio.silencio";
        const float Volumen = .6f, SegundosCruce = 1.2f;
        static readonly string[] Archivos = { null, "InicioRuta", "RutaNormal", "TomaDecision" };
        static GestorAudio instancia;

        readonly AudioSource[] voces = new AudioSource[2];
        readonly float[] posicion = new float[4]; // para retomar «Ruta normal» donde se quedó tras una decisión
        int activa;
        Pista escena, actual;
        bool decidiendo, pausado, vocesPausadas, silenciado, configuracionLista;

        public static GestorAudio Instancia => instancia;
        public Pista PistaActual => actual;
        public bool Pausado => pausado;
        public bool Silenciado
        {
            get => silenciado;
            set { silenciado = value; PlayerPrefs.SetInt(ClaveSilencio, value ? 1 : 0); }
        }

        public static Pista DeEscena(string nombre) => nombre switch
        {
            "Inicio" or "Registro" or "Resultado" => Pista.Inicio,
            "PruebaIntegracionV2" => Pista.Ruta,
            _ => Pista.Ninguna // Admin y demos: en silencio
        };

        // Lo llama la pantalla del recorrido cada vez que cambia su estado.
        public static void Sincronizar(bool decidiendo, bool pausado)
        {
            if (instancia == null) return;
            instancia.decidiendo = decidiendo;
            instancia.pausado = pausado;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Crear()
        {
            if (instancia != null) return;
            var objeto = new GameObject("GestorAudio");
            DontDestroyOnLoad(objeto);
            instancia = objeto.AddComponent<GestorAudio>();
            instancia.escena = DeEscena(SceneManager.GetActiveScene().name);
        }

        void Awake()
        {
            silenciado = PlayerPrefs.GetInt(ClaveSilencio) == 1;
            // Ninguna escena del proyecto trae AudioListener: sin uno no se oiría nada.
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            for (int i = 0; i < voces.Length; i++)
            {
                voces[i] = gameObject.AddComponent<AudioSource>();
                voces[i].loop = true; voces[i].playOnAwake = false; voces[i].spatialBlend = 0; voces[i].volume = 0;
            }
        }

        void OnEnable() => SceneManager.sceneLoaded += AlCargarEscena;
        void OnDisable() => SceneManager.sceneLoaded -= AlCargarEscena;

        void AlCargarEscena(Scene cargada, LoadSceneMode modo)
        {
            if (modo != LoadSceneMode.Single) return;
            escena = DeEscena(cargada.name);
            decidiendo = pausado = configuracionLista = false;
        }

        void Update()
        {
            var teclado = Keyboard.current;
            if (teclado != null && teclado.mKey.wasPressedThisFrame && !EscribiendoTexto()) Silenciado = !Silenciado;

            Poner(escena == Pista.Ruta && decidiendo ? Pista.Decision : escena);
            // La UI de cada pantalla se construye en su Start (el resultado, al llegar): se reintenta hasta que existe.
            if (escena == Pista.Inicio && !configuracionLista) configuracionLista = ConfiguracionPantalla.Inyectar();

            if (pausado != vocesPausadas)
            {
                foreach (var voz in voces) { if (pausado) voz.Pause(); else voz.UnPause(); }
                vocesPausadas = pausado;
            }
            if (pausado) return;

            float paso = Volumen * Time.unscaledDeltaTime / SegundosCruce;
            for (int i = 0; i < voces.Length; i++)
            {
                float meta = i == activa && !silenciado ? Volumen : 0;
                voces[i].volume = Mathf.MoveTowards(voces[i].volume, meta, paso);
                if (i != activa && voces[i].volume == 0 && voces[i].isPlaying) voces[i].Stop();
            }
        }

        void Poner(Pista nueva)
        {
            if (nueva == actual) return;
            if (voces[activa].clip != null) posicion[(int)actual] = voces[activa].time;
            actual = nueva;
            activa = 1 - activa; // la voz anterior se apaga sola con el cruce
            var voz = voces[activa];
            voz.Stop();
            voz.clip = Archivos[(int)nueva] == null ? null : Resources.Load<AudioClip>("Musica/" + Archivos[(int)nueva]);
            if (voz.clip == null) return;
            voz.volume = 0;
            voz.Play();
            // Solo se retoma si ya hay un punto guardado y el clip ya conoce su duración (en la web carga de forma asíncrona).
            if (posicion[(int)nueva] > 0 && voz.clip.length > 1) voz.time = Mathf.Min(posicion[(int)nueva], voz.clip.length - .1f);
            if (pausado) voz.Pause();
        }

        static bool EscribiendoTexto()
        {
            var seleccionado = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            return seleccionado != null && seleccionado.TryGetComponent<TMP_InputField>(out var campo) && campo.isFocused;
        }
    }
}
