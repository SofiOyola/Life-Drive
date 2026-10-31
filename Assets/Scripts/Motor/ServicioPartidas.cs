// Módulos "Partidas", "Rutas y decisiones", "Preguntas y eventos" y "Puntuación" (S2-03).
// Ciclo (HU-05): iniciar → situación → decisión → consecuencia → siguiente camino → puntos → continuar.
using System.Linq;
using LifeDrive.Datos;

namespace LifeDrive.Motor
{
    public class ResultadoDecision
    {
        public Decision Decision;
        public Partida Partida;
        public bool CambioRuta;
        public string TramoDestino;
        public bool Repetida;          // la situación ya estaba resuelta (doble clic o reintento)
    }

    public class ServicioPartidas
    {
        readonly BaseDatos db;
        readonly Contenido contenido;
        readonly ServicioUsuarios usuarios;
        readonly RegistroEventos eventos;
        readonly EvaluadorSituaciones evaluador = new EvaluadorSituaciones();

        public ServicioPartidas(BaseDatos db, Contenido contenido, ServicioUsuarios usuarios, RegistroEventos eventos)
        {
            this.db = db;
            this.contenido = contenido;
            this.usuarios = usuarios;
            this.eventos = eventos;
        }

        public Tramo Tramo(int id) => contenido.tramos.First(t => t.id == id);

        // Inicia una operación desde la salida o retoma la que el jugador dejó en curso.
        public Partida IniciarPartida()
        {
            usuarios.Exigir(Accion.Jugar);
            var yo = usuarios.Actual;
            var id = db.Ejecutar(() =>
            {
                var enCurso = db.Partidas.FirstOrDefault(p => p.jugador_id == yo.JugadorId && p.estado == EstadoPartida.EnCurso);
                if (enCurso != null)
                {
                    eventos.Registrar(TipoEvento.PartidaRetomada, yo, enCurso.id);
                    return enCurso.id;
                }
                var nueva = db.Insertar(new Partida
                {
                    jugador_id = yo.JugadorId,
                    tramo_actual_id = contenido.tramos.Single(t => t.es_salida).id,
                    fecha_inicio = RegistroEventos.Ahora(),
                });
                eventos.Registrar(TipoEvento.PartidaIniciada, yo, nueva.id, detalle: Tramo(nueva.tramo_actual_id).nombre);
                Avanzar(nueva);
                return nueva.id;
            });
            return db.Partidas.First(p => p.id == id);
        }

        // Primera situación del tramo actual que la partida aún no ha resuelto; null si terminó.
        public Situacion SituacionActual(Partida p)
        {
            if (p.estado != EstadoPartida.EnCurso) return null;
            return contenido.situaciones
                .Where(s => s.tramo_id == p.tramo_actual_id && !db.Decisiones.Any(d => d.partida_id == p.id && d.situacion_id == s.id))
                .OrderBy(s => s.orden)
                .FirstOrDefault();
        }

        void CambiarTramo(Partida p, int tramoId)
        {
            if (p.tramo_actual_id == tramoId) return;
            p.tramo_actual_id = tramoId;
            eventos.Registrar(TipoEvento.TramoAlcanzado, usuarios.Actual, p.id, detalle: Tramo(tramoId).nombre);
        }

        // Sigue el camino por defecto hasta una situación pendiente o hasta terminar en el destino.
        void Avanzar(Partida p)
        {
            for (var saltos = 0; saltos <= contenido.tramos.Length * 2; saltos++)
            {
                if (SituacionActual(p) != null) return;
                var tramo = Tramo(p.tramo_actual_id);
                if (tramo.es_destino)
                {
                    p.estado = EstadoPartida.Finalizada;
                    p.fecha_fin = RegistroEventos.Ahora();
                    eventos.Registrar(TipoEvento.PartidaFinalizada, usuarios.Actual, p.id,
                        detalle: $"{p.puntos} puntos, {p.retraso_min} min de retraso");
                    return;
                }
                CambiarTramo(p, tramo.siguiente_tramo_id);
            }
            throw new ErrorJuego("La ruta no tiene salida desde este punto; revisa ruta.json");
        }

        // CU-03: evalúa la alternativa, aplica la consecuencia y guarda decisión, partida y eventos juntos.
        public ResultadoDecision Decidir(int partidaId, int situacionId, int alternativaId)
        {
            var p = db.Partidas.FirstOrDefault(x => x.id == partidaId);
            usuarios.ExigirPartidaPropia(p, Accion.Jugar);

            var previa = db.Decisiones.FirstOrDefault(d => d.partida_id == p.id && d.situacion_id == situacionId);
            if (previa != null) return Resultado(p, previa, true);

            if (p.estado != EstadoPartida.EnCurso) throw new ErrorJuego("La partida ya terminó");
            var s = SituacionActual(p);
            if (s == null || s.id != situacionId) throw new ErrorJuego("Esa situación no es la actual");
            var a = s.alternativas.FirstOrDefault(x => x.id == alternativaId)
                    ?? throw new ErrorJuego("La alternativa no pertenece a la situación");
            var r = evaluador.Evaluar(s, a);
            var yo = usuarios.Actual;

            var decisionId = db.Ejecutar(() =>
            {
                var partida = db.Partidas.First(x => x.id == partidaId);
                var d = db.Insertar(new Decision
                {
                    partida_id = partida.id, situacion_id = s.id, alternativa_id = a.id, fecha_hora = RegistroEventos.Ahora(),
                    tramo_origen_id = partida.tramo_actual_id,
                    tramo_destino_id = r.SiguienteTramoId != 0 ? r.SiguienteTramoId : partida.tramo_actual_id,
                    tipo = s.tipo, alternativa = a.descripcion,
                    es_acertada = r.EsAcertada, es_buena_practica = r.EsBuenaPractica,
                    puntos_aplicados = r.Puntos, retraso_aplicado = r.RetrasoMin, consecuencia = r.Explicacion,
                });
                eventos.Registrar(TipoEvento.DeSituacion(s.tipo), yo, partida.id, s.id,
                    $"{(r.EsAcertada ? "Segura/correcta" : "Insegura/incorrecta")}: {a.descripcion}");
                if (r.EsBuenaPractica) eventos.Registrar(TipoEvento.BuenaPractica, yo, partida.id, s.id, a.descripcion);

                partida.puntos += r.Puntos;
                partida.retraso_min += r.RetrasoMin;
                eventos.Registrar(TipoEvento.PuntosActualizados, yo, partida.id, s.id,
                    $"{(r.Puntos >= 0 ? "+" : "")}{r.Puntos} → {partida.puntos}");
                CambiarTramo(partida, d.tramo_destino_id);
                Avanzar(partida);
                return d.id;
            });

            var guardada = db.Partidas.First(x => x.id == partidaId);
            return Resultado(guardada, db.Decisiones.First(d => d.id == decisionId), false);
        }

        ResultadoDecision Resultado(Partida p, Decision d, bool repetida) => new ResultadoDecision
        {
            Partida = p, Decision = d, Repetida = repetida,
            CambioRuta = d.tramo_destino_id != d.tramo_origen_id,
            TramoDestino = Tramo(d.tramo_destino_id).nombre,
        };
    }
}
