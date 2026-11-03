using System;
using System.Linq;
using UnityEngine;

namespace LifeDrive.Mapas.Demo
{
    [DefaultExecutionOrder(100)]
    public sealed class DemoInterfazPartida : MonoBehaviour
    {
        public enum Fase { EnMarcha, Situacion, Retroalimentacion, Finalizado, Error }
        [SerializeField] DemoRecorrido reproductor;
        [SerializeField] CatalogoMapas catalogo;
        [SerializeField] SituacionInterfazDemo datos;
        [SerializeField] SituacionInterfazDemo[] situacionesExtra = new SituacionInterfazDemo[0];
        [SerializeField] GamificacionDemo gamificacion;
        SituacionInterfazDemo[] situaciones;
        int indiceSituacion;
        EstadoGamificacionDemo estadoGamificacion;
        readonly IconosReconocimientoDemo iconos = new IconosReconocimientoDemo();
        readonly System.Collections.Generic.Queue<string> avisos = new System.Collections.Generic.Queue<string>();
        string aviso;
        float tiempoAviso;
        bool mostrarInsignias;
        SituacionInterfazDemo DatosActuales => situaciones[Mathf.Min(indiceSituacion, situaciones.Length - 1)];
        MapaVisual[][] rutas;
        int[] rutasRespuestas;
        int respuesta = -1;
        bool pausa;
        string error;
        GUIStyle titulo, cuerpo, pequeno, numero, boton, etiqueta;
        readonly Color tinta = new Color32(13, 28, 36, 255);
        readonly Color panel = new Color32(23, 44, 52, 255);
        readonly Color claro = new Color32(244, 240, 217, 255);
        readonly Color aqua = new Color32(100, 218, 190, 255);
        readonly Color oro = new Color32(255, 207, 106, 255);
        public Fase Estado { get; private set; }
        public bool Pausada => pausa;
        public int PuntosSimulados => estadoGamificacion?.Puntos ?? 0;
        public EstadoGamificacionDemo Gamificacion => estadoGamificacion;
        public bool PanelInsigniasAbierto => mostrarInsignias;
        public DemoRecorrido Reproductor => reproductor;

        void Awake() { if (reproductor != null) reproductor.ControlExterno = true; }
        void Start()
        {
            try
            {
                if (reproductor == null || datos == null || datos.respuestas == null || datos.respuestas.Length != 2)
                    throw new ArgumentException("Faltan referencias o las dos respuestas ficticias.");
                rutas = RecorridosVisualesDemo.Obtener(catalogo);
                situaciones = new[] { datos }.Concat(situacionesExtra).ToArray();
                if (situaciones.Any(s => s == null || string.IsNullOrWhiteSpace(s.idDemo) || string.IsNullOrWhiteSpace(s.prefijoMapa) || s.respuestas == null || s.respuestas.Length != 2) || situaciones.Select(s => s.idDemo).Distinct().Count() != situaciones.Length)
                    throw new ArgumentException("Situaciones ficticias incompletas o IDs duplicados.");
                rutasRespuestas = datos.respuestas.Select(r => Array.FindIndex(rutas, camino => camino[3] == r.hojaVisual)).ToArray();
                if (rutasRespuestas.Any(r => r < 0)) throw new ArgumentException("Una respuesta no tiene continuación visual válida.");
                Reiniciar();
            }
            catch (ArgumentException e) { error = e.Message; Estado = Fase.Error; reproductor?.Pausar(); }
        }
        void Update()
        {
            AjustarArea(); Avanzar(Time.deltaTime);
            tiempoAviso -= Time.unscaledDeltaTime;
            if (tiempoAviso <= 0) { aviso = avisos.Count > 0 ? avisos.Dequeue() : null; tiempoAviso = aviso == null ? 0 : 4; }
        }
        void OnDestroy() => iconos.Liberar();
        void AjustarArea()
        {
            float s = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
            var origen = new Vector2((Screen.width - 1600 * s) / 2, (Screen.height - 900 * s) / 2);
            if (reproductor != null) reproductor.AreaVisualExterna = new Rect(origen.x + 32 * s, origen.y + 150 * s, 1040 * s, 585 * s);
        }
        public void Reiniciar()
        {
            if (rutas == null || rutas.Length == 0) return;
            estadoGamificacion = new EstadoGamificacionDemo(gamificacion, situaciones.Length);
            respuesta = -1; indiceSituacion = 0; pausa = false; mostrarInsignias = false;
            avisos.Clear(); aviso = null; tiempoAviso = 0;
            reproductor.SeleccionarRecorrido(0); reproductor.Reproducir();
            Estado = Fase.EnMarcha;
        }
        public void AlternarPausa()
        {
            if (Estado != Fase.EnMarcha) return;
            pausa = !pausa;
            if (pausa) reproductor.Pausar(); else reproductor.Reproducir();
        }
        public void AlternarInsignias()
        {
            if (estadoGamificacion == null) return;
            mostrarInsignias = !mostrarInsignias;
            if (mostrarInsignias) reproductor.Pausar();
            else if (Estado == Fase.EnMarcha && !pausa) reproductor.Reproducir();
        }
        public void Avanzar(float segundos)
        {
            if (estadoGamificacion == null || Estado != Fase.EnMarcha || pausa || mostrarInsignias || segundos <= 0 || !float.IsFinite(segundos)) return;
            // Small presentation steps avoid skipping a question while a fade ends.
            while (segundos > 0 && Estado == Fase.EnMarcha)
            {
                float paso = Mathf.Min(.02f, segundos); segundos -= paso;
                bool hayHito = indiceSituacion < situaciones.Length && !reproductor.EnTransicion && reproductor.MapaActual.Id.StartsWith(DatosActuales.prefijoMapa, StringComparison.Ordinal);
                if (hayHito)
                {
                    float punto = Mathf.Clamp(DatosActuales.puntoDemostracion, .05f, .9f);
                    float restante = Mathf.Max(0, reproductor.Longitud * punto - reproductor.Distancia);
                    reproductor.Avanzar(Mathf.Min(paso, restante / reproductor.Velocidad));
                    if (reproductor.Progreso >= punto - .00001f) { reproductor.Pausar(); Estado = Fase.Situacion; }
                }
                else reproductor.Avanzar(paso);
                estadoGamificacion.ActualizarTrayecto(reproductor.ProgresoTotal);
                if (reproductor.EntregaCompletada) Estado = Fase.Finalizado;
            }
        }
        public void Responder(int indice)
        {
            if (Estado != Fase.Situacion || mostrarInsignias || indice < 0 || indice >= DatosActuales.respuestas.Length) return;
            var r = DatosActuales.respuestas[indice];
            if (!estadoGamificacion.Aplicar(new ResultadoPresentacionDemo { IdSituacion = DatosActuales.idDemo, CambioPuntos = r.puntosSimulados, Explicacion = r.explicacionPuntos, Senales = r.senalesSimuladas }, out var nuevas)) return;
            foreach (var nombre in nuevas)
            {
                if (aviso == null) { aviso = nombre; tiempoAviso = 4; }
                else avisos.Enqueue(nombre);
            }
            respuesta = indice;
            Estado = Fase.Retroalimentacion;
        }
        public void Continuar()
        {
            if (Estado != Fase.Retroalimentacion || mostrarInsignias) return;
            // Fictional branch only. Retain the current S0 position, never submit a motor decision.
            float progreso = reproductor.Progreso;
            if (indiceSituacion == 0)
            {
                reproductor.SeleccionarRecorrido(rutasRespuestas[respuesta]);
                reproductor.PosicionarProgreso(progreso);
            }
            indiceSituacion++; respuesta = -1; pausa = false; Estado = Fase.EnMarcha;
            estadoGamificacion.ActualizarTrayecto(reproductor.ProgresoTotal);
            reproductor.Reproducir();
        }
        void Estilos()
        {
            if (titulo != null) return;
            titulo = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, wordWrap = true, normal = { textColor = claro } };
            cuerpo = new GUIStyle(titulo) { fontSize = 23, fontStyle = FontStyle.Normal };
            pequeno = new GUIStyle(cuerpo) { fontSize = 18 };
            numero = new GUIStyle(titulo) { fontSize = 34 };
            etiqueta = new GUIStyle(pequeno) { fontStyle = FontStyle.Bold, normal = { textColor = aqua } };
            boton = new GUIStyle(cuerpo) { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(20, 20, 8, 8) };
        }
        void Bloque(Rect r, Color c) { var previo = GUI.color; GUI.color = c; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = previo; }
        void Texto(float x, float y, float w, float h, string t, GUIStyle estilo) => GUI.Label(new Rect(x, y, w, h), t, estilo);
        bool Boton(Rect r, string texto, bool principal = false)
        {
            bool hover = r.Contains(Event.current.mousePosition);
            Bloque(new Rect(r.x, r.y + 5, r.width, r.height), Color.black);
            Bloque(r, principal ? aqua : hover ? new Color32(63, 87, 93, 255) : new Color32(38, 65, 74, 255));
            Bloque(new Rect(r.x, r.y, 5, r.height), principal ? claro : aqua);
            var anterior = boton.normal.textColor; boton.normal.textColor = principal ? tinta : claro;
            bool click = GUI.Button(r, texto, boton); boton.normal.textColor = anterior;
            return click;
        }
        void Barra(Rect r, float valor, Color color)
        {
            Bloque(r, tinta);
            float ancho = (r.width - 38) / 20;
            for (int i = 0; i < 20; i++) Bloque(new Rect(r.x + i * (ancho + 2), r.y, ancho, r.height), i / 20f < valor ? color : new Color32(45, 67, 72, 255));
        }
        void OnGUI()
        {
            Estilos(); AjustarArea();
            var matriz = GUI.matrix; int depth = GUI.depth;
            GUI.depth = -10;
            float s = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1600 * s) / 2, (Screen.height - 900 * s) / 2), Quaternion.identity, Vector3.one * s);
            // No full-screen fill: the existing renderer owns the map layer beneath these panels.
            Bloque(new Rect(24, 20, 1552, 104), tinta);
            Texto(48, 32, 500, 38, "LIFE DRIVE  /  EN RUTA", titulo);
            Texto(48, 78, 570, 28, "PROTOTIPO · DATOS FICTICIOS · SIN GUARDADO", etiqueta);
            Texto(660, 34, 300, 30, "TRAYECTO DE LA MISIÓN", pequeno);
            Barra(new Rect(660, 79, 260, 12), estadoGamificacion?.ProgresoTrayecto ?? 0, aqua);
            Texto(938, 63, 115, 40, (estadoGamificacion?.ProgresoTrayecto ?? 0).ToString("P0"), numero);
            Texto(1090, 32, 260, 26, "SEGURIDAD · SIMULADA", pequeno);
            Texto(1090, 61, 260, 42, (PuntosSimulados >= 0 ? "+" : "") + PuntosSimulados + " pts", numero);
            if ((estadoGamificacion?.SituacionesResueltas ?? 0) > 0)
                Texto(1090, 100, 280, 24, "Último cambio: " + Cambio(estadoGamificacion.UltimoCambio) + " · simulado", pequeno);
            if (Estado == Fase.EnMarcha && !mostrarInsignias && Boton(new Rect(1380, 41, 168, 56), pausa ? "Reanudar" : "Pausar")) AlternarPausa();
            Bloque(new Rect(24, 742, 1056, 45), tinta);
            Texto(44, 750, 690, 30, "MAPA " + (reproductor?.MapaActual == null ? "—" : reproductor.MapaActual.Id) + "  /  " + EstadoTexto(), pequeno);
            Texto(750, 750, 320, 30, "SITUACIONES " + (estadoGamificacion?.SituacionesResueltas ?? 0) + "/" + (situaciones?.Length ?? 0), etiqueta);
            Bloque(new Rect(1100, 144, 476, 643), panel);
            Bloque(new Rect(1100, 144, 476, 6), aqua);
            Texto(1124, 172, 426, 30, mostrarInsignias ? "INSIGNIAS · DEMO" : "BITÁCORA DE PRUEBA", etiqueta);
            if (mostrarInsignias) DibujarInsignias();
            else if (Estado == Fase.Situacion)
            {
                Texto(1124, 216, 420, 80, DatosActuales.titulo, titulo);
                Texto(1124, 310, 420, 168, DatosActuales.enunciado, cuerpo);
                Texto(1124, 487, 420, 36, "Elige una respuesta para esta maqueta.", pequeno);
                for (int i = 0; i < DatosActuales.respuestas.Length; i++)
                    if (Boton(new Rect(1124, 536 + i * 110, 428, 96), DatosActuales.respuestas[i].texto)) Responder(i);
            }
            else if (Estado == Fase.Retroalimentacion)
            {
                var r = DatosActuales.respuestas[respuesta];
                Texto(1124, 220, 420, 80, "Respuesta registrada\nen esta demo", titulo);
                Texto(1124, 322, 420, 140, r.retroalimentacion, cuerpo);
                Bloque(new Rect(1124, 480, 428, 122), tinta);
                Texto(1144, 495, 390, 30, "CAMBIO SIMULADO: " + Cambio(estadoGamificacion.UltimoCambio), etiqueta);
                Texto(1144, 535, 390, 60, estadoGamificacion.UltimaExplicacion, pequeno);
                Texto(1124, 620, 420, 50, "Puntos ilustrativos. No afectan una partida.", pequeno);
                if (Boton(new Rect(1124, 694, 428, 64), "Continuar recorrido  →", true)) Continuar();
            }
            else if (Estado == Fase.Finalizado)
            {
                Texto(1124, 225, 420, 95, "Entrega completada", titulo);
                Texto(1124, 350, 420, 170, "El camión llegó al patio.\n\nHas terminado la demostración de interfaz.", cuerpo);
                Texto(1124, 584, 420, 110, "Sin clasificación de final, puntuación real ni decisiones guardadas.", pequeno);
            }
            else if (Estado == Fase.Error)
                Texto(1124, 225, 420, 350, "No se puede iniciar la demo.\n\n" + error, cuerpo);
            else
            {
                Texto(1124, 225, 420, 90, pausa ? "Recorrido en pausa" : "El camino continúa", titulo);
                Texto(1124, 344, 420, 170, pausa ? "Reanuda cuando quieras. El camión espera en su posición." : indiceSituacion > 0 ? "Continuación visual ficticia. El camión se detendrá en los siguientes puntos de prueba antes de DELIVERY." : "El camión avanza automáticamente. Se detendrá para mostrar una situación de ejemplo.", cuerpo);
                Texto(1124, 605, 420, 100, "Respuestas mediante clic.\nSin conducción manual.", pequeno);
            }
            Bloque(new Rect(24, 808, 1552, 70), tinta);
            Texto(48, 822, 1030, 45, "DEMOSTRACIÓN VISUAL  ·  Puntos, situaciones e insignias de prueba", pequeno);
            if (Boton(new Rect(1100, 815, 250, 52), "Insignias " + (estadoGamificacion?.InsigniasDesbloqueadas ?? 0) + "/" + (estadoGamificacion?.TotalInsignias ?? 0))) AlternarInsignias();
            if (Boton(new Rect(1380, 815, 168, 52), "Reiniciar")) Reiniciar();
            if (aviso != null)
            {
                Bloque(new Rect(40, 814, 1040, 56), tinta);
                Texto(60, 822, 1020, 45, "INSIGNIA DE PRUEBA DESBLOQUEADA · " + aviso, etiqueta);
            }
            GUI.matrix = matriz; GUI.depth = depth;
        }
        static string Cambio(int valor) => (valor >= 0 ? "+" : "") + valor + " pts";
        void DibujarInsignias()
        {
            if (Boton(new Rect(1420, 166, 132, 44), "Cerrar")) AlternarInsignias();
            int i = 0;
            foreach (var d in gamificacion.reconocimientos)
            {
                float y = 236 + i++ * 174;
                bool abierta = estadoGamificacion.Desbloqueada(d.id);
                var previo = GUI.color; GUI.color = abierta ? oro : new Color(.45f, .55f, .57f);
                GUI.DrawTexture(new Rect(1124, y, 48, 48), iconos.Obtener(d.icono)); GUI.color = previo;
                Texto(1185, y, 365, 54, d.nombre, cuerpo);
                Texto(1124, y + 55, 428, 28, abierta ? "DESBLOQUEADA · SIMULADA" : "BLOQUEADA", etiqueta);
                Texto(1124, y + 86, 428, 40, d.descripcion, pequeno);
                Texto(1124, y + 128, 428, 44, "Prueba: " + d.condicionSimulada, pequeno);
            }
        }
        string EstadoTexto() => Estado == Fase.Situacion ? "CAMIÓN DETENIDO · SITUACIÓN DE EJEMPLO" : Estado == Fase.Retroalimentacion ? "CAMIÓN DETENIDO · RETROALIMENTACIÓN" : Estado == Fase.Finalizado ? "EN ZONA DE ENTREGA" : pausa ? "PAUSA" : "MOVIMIENTO AUTOMÁTICO";
    }
}
