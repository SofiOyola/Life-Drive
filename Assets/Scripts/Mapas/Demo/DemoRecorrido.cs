using System;
using System.Linq;
using UnityEngine;

namespace LifeDrive.Mapas.Demo
{
    public sealed class DemoRecorrido : MonoBehaviour
    {
        [SerializeField] CatalogoMapas catalogo;
        [SerializeField] Sprite camion;
        [SerializeField] Rect camionUV = new Rect(0, 0, 1, 1);
        [SerializeField] float largoCamion = 18;
        [SerializeField] float velocidad = 24;
        [SerializeField] float carrilDerecho = 6;
        [SerializeField] float tiempoGiro = .12f;
        [SerializeField] bool iniciarAutomaticamente = true;
        [SerializeField, Min(.1f)] float duracionFundido = .35f;
        enum FaseFundido { Ninguna, Saliendo, Entrando }
        FaseFundido fase;
        float relojFundido;
        bool recorridoCompleto;
        int? siguienteExterno;
        int rutaSeleccionada, etapa;
        MapaVisual[][] rutas = new MapaVisual[0][];
        string[] nombresRutas = new string[0];
        float[] longitudesRuta = new float[0];
        string errorRutas;
        int indice;
        RecorridoDemo recorrido;
        float distancia, angulo, giroVelocidad;
        string error;
        string[] nombres;
        // Optional host presentation; defaults retain the original standalone demo.
        public bool ControlExterno { get; set; }
        public Rect AreaVisualExterna { get; set; }
        public bool Reproduciendo { get; private set; }
        bool FinMapa => recorrido != null && distancia >= recorrido.Longitud;
        public bool Terminado => FinMapa && !siguienteExterno.HasValue &&
            (!recorridoCompleto || etapa == RutaActual.Length - 1) && fase == FaseFundido.Ninguna;
        MapaVisual[] RutaActual => rutas.Length == 0 ? new MapaVisual[0] : rutas[rutaSeleccionada];
        public bool ModoCompleto => recorridoCompleto;
        public bool EntregaCompletada => recorridoCompleto && Terminado && MapaActual != null && MapaActual.Id == "DELIVERY";
        public bool EnTransicion => fase != FaseFundido.Ninguna;
        public int EtapaActual => etapa;
        public string[] RecorridosDisponibles => (string[])nombresRutas.Clone();
        public float OpacidadFundido => fase == FaseFundido.Ninguna ? 0 : Mathf.SmoothStep(0, 1, fase == FaseFundido.Saliendo ? relojFundido / DuracionFundido : 1 - relojFundido / DuracionFundido);
        float DuracionFundido => Mathf.Max(.1f, duracionFundido);
        public float ProgresoTotal => !recorridoCompleto ? Progreso : Terminado ? 1 : longitudesRuta.Length == 0 ? 0 : Mathf.Clamp01((longitudesRuta.Take(etapa).Sum() + distancia) / Mathf.Max(.001f, longitudesRuta.Sum()));
        public float Progreso => recorrido == null || recorrido.Longitud == 0 ? 0 : distancia / recorrido.Longitud;
        public Vector2 Posicion => recorrido == null ? Vector2.zero : recorrido.Posicion(distancia);
        public float Angulo => angulo;
        public float Distancia => distancia;
        public float Longitud => recorrido?.Longitud ?? 0;
        public bool AjusteLimites => recorrido != null && recorrido.AjusteLimites;
        public MapaVisual MapaActual => catalogo != null && catalogo.mapas != null && indice >= 0 && indice < catalogo.mapas.Length ? catalogo.mapas[indice] : null;
        public float Velocidad { get => velocidad; set => velocidad = Mathf.Clamp(value, 1, 160); }
        public float CarrilDerecho
        {
            get => carrilDerecho;
            set
            {
                float nuevo = Mathf.Clamp(value, 0, 12);
                if (Mathf.Approximately(nuevo, carrilDerecho)) return;
                float progress = Progreso; carrilDerecho = nuevo;
                Construir(); distancia = progress * Longitud; ActualizarLongitudes(); Alinear();
            }
        }
        void Start()
        {
            nombres = catalogo == null || catalogo.mapas == null ? new string[0] : catalogo.mapas.Select(m => m == null ? "Sin mapa" : m.Id).ToArray();
            try
            {
                rutas = RecorridosVisualesDemo.Obtener(catalogo);
                nombresRutas = rutas.Select(r => string.Join(" → ", r.Select(m => m.Id))).ToArray();
            }
            catch (ArgumentException e) { errorRutas = e.Message; }
            SeleccionarMapa(0);
            if (iniciarAutomaticamente) Reproducir();
        }
        void Update() { if (!ControlExterno) Avanzar(Time.deltaTime); }
        public void PosicionarProgreso(float progreso)
        {
            if (EnTransicion || recorrido == null) return;
            Pausar(); distancia = Mathf.Clamp01(progreso) * Longitud; Alinear();
        }
        void Construir()
        {
            error = null;
            try { recorrido = new RecorridoDemo(MapaActual, carrilDerecho); }
            catch (ArgumentException e) { recorrido = null; Reproduciendo = false; error = e.Message; }
        }
        public void SeleccionarMapa(int value)
        {
            recorridoCompleto = false;
            indice = catalogo == null || catalogo.mapas == null ? 0 : Mathf.Clamp(value, 0, catalogo.mapas.Length - 1);
            Construir(); Reiniciar();
        }
        public void MostrarMapa(string mapaId)
        {
            var nuevo = Array.FindIndex(catalogo.mapas, m => m.Id == mapaId);
            if (nuevo < 0) throw new ArgumentException("Mapa desconocido: " + mapaId);
            SeleccionarMapa(nuevo);
            if (recorrido == null) throw new ArgumentException(error);
        }

        // The host supplies one confirmed edge. Standalone demo routes keep their original autoplay.
        public void TransicionarA(string mapaId)
        {
            if (!ControlExterno || recorridoCompleto || EnTransicion || siguienteExterno.HasValue ||
                MapaActual == null || !MapaActual.siguientes.Any(m => m.Id == mapaId))
                throw new ArgumentException("Transición visual no permitida: " + mapaId);
            siguienteExterno = Array.FindIndex(catalogo.mapas, m => m.Id == mapaId);
            Reproducir();
        }
        public void SeleccionarRecorrido(int value)
        {
            if (rutas.Length == 0) { error = errorRutas ?? "No hay recorridos válidos."; return; }
            rutaSeleccionada = Mathf.Clamp(value, 0, rutas.Length - 1);
            recorridoCompleto = true;
            ActualizarLongitudes();
            Reiniciar();
        }
        void ActualizarLongitudes()
        {
            if (!recorridoCompleto) return;
            try { longitudesRuta = RutaActual.Select(m => new RecorridoDemo(m, carrilDerecho).Longitud).ToArray(); }
            catch (ArgumentException e) { longitudesRuta = new float[0]; error = e.Message; Reproduciendo = false; }
        }
        void AbrirEtapa(int value)
        {
            etapa = value;
            indice = Array.IndexOf(catalogo.mapas, RutaActual[etapa]);
            Construir(); distancia = 0; Alinear();
        }
        void Alinear()
        {
            var t = recorrido?.Tangente(distancia) ?? Vector2.right;
            angulo = Mathf.Atan2(t.y, t.x) * Mathf.Rad2Deg; giroVelocidad = 0;
        }
        public void Reiniciar()
        {
            siguienteExterno = null;
            fase = FaseFundido.Ninguna; relojFundido = 0;
            if (recorridoCompleto) AbrirEtapa(0);
            distancia = 0; Reproduciendo = false; Alinear();
        }
        public void Pausar() => Reproduciendo = false;
        public void Reproducir() { if (recorrido != null && !Terminado) Reproduciendo = true; }
        public void Avanzar(float segundos)
        {
            if (!Reproduciendo || recorrido == null || segundos <= 0 || !float.IsFinite(segundos)) return;
            // Consume the remainder across phases; no skipped road or fade on a long frame.
            for (int seguridad = 0; segundos > 0 && Reproduciendo && seguridad < 32; seguridad++)
            {
                if (EnTransicion)
                {
                    float paso = Mathf.Min(segundos, DuracionFundido - relojFundido);
                    relojFundido += paso; segundos -= paso;
                    if (relojFundido >= DuracionFundido)
                    {
                        relojFundido = 0;
                        if (fase == FaseFundido.Saliendo)
                        {
                            if (siguienteExterno.HasValue)
                            {
                                indice = siguienteExterno.Value;
                                siguienteExterno = null;
                                Construir(); distancia = 0; Alinear();
                            }
                            else AbrirEtapa(etapa + 1);
                            fase = FaseFundido.Entrando;
                            // Present at least one fully black frame before revealing the new art.
                            return;
                        }
                        else
                        {
                            fase = FaseFundido.Ninguna;
                            if (!recorridoCompleto) Pausar();
                        }
                    }
                    continue;
                }
                float delta = Mathf.Min(segundos, Mathf.Max(0, Longitud - distancia) / Velocidad);
                Mover(delta); segundos -= delta;
                if (FinMapa)
                {
                    if (siguienteExterno.HasValue || recorridoCompleto && etapa < RutaActual.Length - 1)
                    { fase = FaseFundido.Saliendo; relojFundido = 0; }
                    else Reproduciendo = false;
                }
            }
        }
        void Mover(float segundos)
        {
            distancia = Mathf.Min(Longitud, distancia + Velocidad * segundos);
            if (Longitud - distancia < .0001f) distancia = Longitud;
            var tangent = recorrido.Tangente(distancia);
            float objetivo = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            angulo = Mathf.SmoothDampAngle(angulo, objetivo, ref giroVelocidad, Mathf.Max(.02f, tiempoGiro), Mathf.Infinity, segundos);
            if (FinMapa) { angulo = objetivo; giroVelocidad = 0; }
        }
        void OnGUI()
        {
            float controles = recorridoCompleto && Screen.width < 900 ? 330 : 230;
            float disponible = Mathf.Max(1, Screen.height - controles);
            float ancho = Mathf.Min(Screen.width, disponible * 16f / 9);
            float alto = ancho * 9f / 16;
            var image = new Rect((Screen.width - ancho) / 2, (disponible - alto) / 2, ancho, alto);
            if (ControlExterno)
            {
                var area = AreaVisualExterna;
                ancho = Mathf.Min(area.width, area.height * 16f / 9);
                alto = ancho * 9f / 16;
                image = new Rect(area.center.x - ancho / 2, area.center.y - alto / 2, ancho, alto);
            }
            if (MapaActual != null && MapaActual.fondo != null)
            {
                GUI.DrawTexture(image, MapaActual.fondo, ScaleMode.ScaleToFit);
                if (camion != null && recorrido != null)
                {
                    // Clip at map edges, never clamp/warp the body to hide a road mismatch.
                    GUI.BeginGroup(image);
                    var p = Posicion;
                    var centro = new Vector2(p.x / 480 * ancho, (1 - p.y / 270) * alto);
                    float ratio = camionUV.width * camion.texture.width / Mathf.Max(1, camionUV.height * camion.texture.height);
                    float width = largoCamion / 480 * ancho, height = width / Mathf.Max(.1f, ratio);
                    var saved = GUI.matrix;
                    GUIUtility.RotateAroundPivot(-angulo, centro);
                    GUI.DrawTextureWithTexCoords(new Rect(centro.x - width / 2, centro.y - height / 2, width, height), camion.texture, camionUV, true);
                    GUI.matrix = saved;
                    GUI.EndGroup();
                }
            }
            if (OpacidadFundido > 0)
            {
                var color = GUI.color;
                GUI.color = new Color(0, 0, 0, OpacidadFundido);
                GUI.DrawTexture(image, Texture2D.whiteTexture);
                GUI.color = color;
            }
            if (ControlExterno) return;
            GUILayout.BeginArea(new Rect(8, Screen.height - controles + 4, Screen.width - 16, controles - 8), GUI.skin.box);
            GUILayout.Label("DEMO INDEPENDIENTE · Recorrido automático · " + (MapaActual == null ? "sin mapa" : MapaActual.Id));
            int modo = GUILayout.Toolbar(recorridoCompleto ? 1 : 0, new[] { "MAPA ÚNICO", "RECORRIDO COMPLETO" });
            if (modo != (recorridoCompleto ? 1 : 0)) { if (modo == 1) SeleccionarRecorrido(rutaSeleccionada); else SeleccionarMapa(indice); }
            if (recorridoCompleto)
            {
                int nuevaRuta = GUILayout.SelectionGrid(rutaSeleccionada, nombresRutas, Screen.width >= 900 ? 2 : 1);
                if (nuevaRuta != rutaSeleccionada) SeleccionarRecorrido(nuevaRuta);
            }
            else if (nombres != null && nombres.Length > 0)
            {
                int nuevo = GUILayout.SelectionGrid(indice, nombres, Mathf.Clamp(Screen.width / 100, 2, 8));
                if (nuevo != indice) SeleccionarMapa(nuevo);
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Reproduciendo ? "Pausar" : Terminado ? "Completado" : "Reproducir / Reanudar", GUILayout.Width(180))) { if (Reproduciendo) Pausar(); else Reproducir(); }
            if (GUILayout.Button("Reiniciar", GUILayout.Width(100))) Reiniciar();
            GUILayout.Label($"Mapa {Progreso:P1}" + (recorridoCompleto ? $" · Total {ProgresoTotal:P1} · {etapa + 1}/5" : ""));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Velocidad {Velocidad:F0} px/s", GUILayout.Width(150)); Velocidad = GUILayout.HorizontalSlider(Velocidad, 1, 160);
            GUILayout.Label($"Carril derecho {CarrilDerecho:F1} px", GUILayout.Width(170)); CarrilDerecho = GUILayout.HorizontalSlider(CarrilDerecho, 0, 12);
            GUILayout.EndHorizontal();
            GUILayout.Label(EntregaCompletada ? "Entrega completada" : error ?? errorRutas ?? (AjusteLimites ? "AVISO: centro limitado al lienzo. Revisar desplazamiento." : "Escala y carril provisionales; no certifican cabida física."));
            GUILayout.EndArea();
        }
    }
}
