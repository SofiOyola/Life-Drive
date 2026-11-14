using System;
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Mapas;
using LifeDrive.Mapas.Demo;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using UnityEngine;

namespace LifeDrive.IntegracionV2
{
    [DefaultExecutionOrder(100)]
    public sealed partial class PruebaIntegracionV2 : MonoBehaviour
    {
        public enum Fase { Preparada, EnMarcha, Situacion, Consecuencia, Transicion, Entregada, Error }

        [SerializeField] DemoRecorrido reproductor;
        [SerializeField] CatalogoMapas catalogo;
        ServicioJuego juego;
        int partidaId;
        EstadoRecorridoV2 estado;
        Situacion situacion;
        bool pausa;
        string error;
        Guid? sesionPruebas;
        Action<int> alResultado;
        Action<int> alAbrirPartida;

        // Explicit injection from the isolated player pilot, before Start. The standalone test stays unchanged.
        public void ConectarJugador(ServicioJuego servicio, Action<int> resultado, Action<int> abierta = null,
            Action inicioJugador = null, bool usarV1 = false, Func<AparienciaJugadorPilotoV1> apariencia = null)
        {
            if (juego != null) throw new InvalidOperationException("La sesión ya se abrió.");
            juego = servicio ?? throw new ArgumentNullException(nameof(servicio));
            alResultado = resultado ?? throw new ArgumentNullException(nameof(resultado));
            alAbrirPartida = abierta;
            alInicioJugador = inicioJugador;
            esV1 = usarV1;
            consultarApariencia = apariencia;
        }

        public void VerResultado()
        {
            if (salidaAbierta) return;
            if (Estado == Fase.Entregada && (esV1 || estado.CierrePresentado)) alResultado?.Invoke(partidaId);
        }

        public Fase Estado { get; private set; }
        public string MapaVisible => reproductor.MapaActual?.Id;
        public float ProgresoMapa => reproductor.Progreso;
        public bool Pausada => pausa;
        public int SituacionVisibleId => Estado == Fase.Situacion ? situacion.id : 0;
        public string CarpetaDatos => Path.Combine(Application.temporaryCachePath, "LifeDrive-PruebaIntegracionV2",
            sesionPruebas?.ToString("N") ?? "sesion-local");

        public void AislarSesion(Guid identificador)
        {
            if (juego != null) throw new InvalidOperationException("La sesión ya se abrió.");
            sesionPruebas = identificador;
        }

        void Awake() => reproductor.ControlExterno = true;

        void Start()
        {
            ConstruirVista();
            Ejecutar(() => reproductor.MostrarMapa("S0"));
            ActualizarVista();
            if (alResultado != null) Abrir(false);
        }

        void Update()
        {
            AjustarVista();
            AnimarVolante();
            if (configuracionAbierta) ActualizarEtiquetaMusica(); // la tecla M puede cambiarla con el panel abierto
            if (salidaAbierta || configuracionAbierta || reconocimientosAbiertos || pausa || (Estado != Fase.EnMarcha && Estado != Fase.Transicion)) return;
            Ejecutar(Avanzar);
        }

        void Abrir(bool nueva)
        {
            if (salidaAbierta) return;
            if (esV1) { Ejecutar(AbrirV1); return; }
            Ejecutar(() =>
            {
                if (juego == null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_WEBGL
                    var contenido = RutasContenido.Carpeta;
                    var abierto = new ServicioJuego(contenido, CarpetaDatos, catalogo);
                    abierto.Usuarios.RegistrarJugador("Conductor de prueba V2", "990002", 1);
                    juego = abierto;
#else
                    throw new ErrorJuego("Abre el recorrido desde Inicio.");
#endif
                }
                var propias = juego.BaseDatos.Partidas.Where(p => p.jugador_id == juego.Usuarios.Actual.JugadorId);
                var guardada = propias.FirstOrDefault(p => p.estado == EstadoPartida.EnCurso) ??
                    propias.LastOrDefault(p => p.contenido_version == 2);
                var partida = nueva || guardada == null ? juego.RecorridosV2.IniciarPartida() : guardada;
                partidaId = partida.id;
                Restaurar();
            });
        }

        void ReiniciarVista()
        {
            if (!salidaAbierta && partidaId != 0) Ejecutar(Restaurar);
        }

        void Restaurar()
        {
            if (esV1) { RestaurarV1(); return; }
            alAbrirPartida?.Invoke(partidaId); // Retry a failed appearance snapshot before allowing the match to continue.
            pausa = false;
            estado = juego.RecorridosV2.Consultar(partidaId);
            reproductor.MostrarMapa(estado.MapaActual);
            if (estado.PresentacionPendiente || estado.DecisionConfirmada != null &&
                estado.DestinoVisualConfirmado == null)
                reproductor.PosicionarProgreso(Hito(estado.DecisionConfirmada.situacion_id));
            PrepararSiguiente();
        }

        void PrepararSiguiente()
        {
            if (estado.PresentacionPendiente)
                Estado = Fase.Consecuencia;
            else if (estado.CierrePresentado)
                Estado = Fase.Entregada;
            else
            {
                situacion = juego.Partidas.SituacionActual(juego.BaseDatos.Partidas.Single(p => p.id == partidaId));
                if (situacion == null || situacion.id != estado.SituacionPendienteId)
                    throw new ErrorJuego("No se encontró la situación pendiente del recorrido.");
                reproductor.Reproducir();
                Estado = Fase.EnMarcha;
            }
            ActualizarVista();
        }

        // Hitos de esta escena en la trayectoria existente. s1 y s3 preceden a otra pregunta en el mismo mapa.
        static float Hito(int situacionId) => GamificacionPartidaV2.HitoVisual(situacionId);

        void Avanzar()
        {
            if (esV1) { AvanzarV1(); return; }
            if (Estado == Fase.Transicion)
            {
                reproductor.Avanzar(Time.deltaTime);
                if (reproductor.EnTransicion || MapaVisible != estado.DestinoVisualConfirmado) return;
                ConfirmarPresentacion();
                return;
            }
            float hito = Hito(situacion.id);
            float restante = Mathf.Max(0, reproductor.Longitud * hito - reproductor.Distancia);
            reproductor.Avanzar(Mathf.Min(Time.deltaTime, restante / reproductor.VelocidadEfectiva));
            if (reproductor.Progreso < hito - .00001f) return;
            reproductor.Pausar();
            Estado = Fase.Situacion;
            ActualizarVista();
        }

        void Responder(int indice)
        {
            if (salidaAbierta || configuracionAbierta || Estado != Fase.Situacion || indice >= situacion.alternativas.Length) return;
            Estado = Fase.Consecuencia;
            DeshabilitarRespuestas();
            Ejecutar(() =>
            {
                if (esV1)
                {
                    ultimaV1 = juego.Partidas.Decidir(partidaId, situacion.id, situacion.alternativas[indice].id).Decision;
                    ActualizarVista(); return;
                }
                juego.Partidas.Decidir(partidaId, situacion.id, situacion.alternativas[indice].id);
                estado = juego.RecorridosV2.Consultar(partidaId);
                PrepararSiguiente();
            });
        }

        void Continuar()
        {
            if (salidaAbierta || configuracionAbierta || Estado != Fase.Consecuencia) return;
            Ejecutar(() =>
            {
                if (esV1) { PrepararSiguienteV1(); return; }
                if (estado.DestinoVisualConfirmado == null) ConfirmarPresentacion();
                else
                {
                    reproductor.TransicionarA(estado.DestinoVisualConfirmado, !estado.DecisionConfirmada.es_acertada);
                    Estado = Fase.Transicion;
                    ActualizarVista();
                }
            });
        }

        void ConfirmarPresentacion()
        {
            estado = juego.RecorridosV2.ConfirmarPresentacion(partidaId, estado.DecisionConfirmada.id);
            PrepararSiguiente();
        }

        void AlternarPausa()
        {
            if (salidaAbierta || Estado != Fase.EnMarcha && Estado != Fase.Transicion) return;
            pausa = !pausa;
            if (pausa) reproductor.Pausar();
            else reproductor.Reproducir();
            ActualizarVista();
        }

        void Ejecutar(Action accion)
        {
            try { accion(); }
            catch (Exception e) when (e is ErrorJuego || e is IOException || e is ArgumentException ||
                e is UnauthorizedAccessException || e is InvalidDataException)
            {
                error = e.Message;
                reproductor.Pausar();
                Estado = Fase.Error;
                ActualizarVista();
            }
        }
    }
}
