// Patrón Strategy (S2-04, diagrama de clases): una estrategia por tipo de situación.
// Los puntos y retrasos vienen de cada alternativa en ruta.json (reglas 1.3.3).
using System.Collections.Generic;
using LifeDrive.Datos;

namespace LifeDrive.Motor
{
    public class ResultadoEvaluacion
    {
        public int Puntos;
        public int RetrasoMin;
        public string Explicacion;
        public bool EsAcertada;
        public bool EsBuenaPractica;
        public int SiguienteTramoId;   // 0 = sigue en el mismo camino
    }

    public interface IEstrategiaEvaluacion
    {
        ResultadoEvaluacion Evaluar(Situacion situacion, Alternativa alternativa);
    }

    static class Base
    {
        public static ResultadoEvaluacion De(Alternativa a) => new ResultadoEvaluacion
        {
            Puntos = a.puntos, Explicacion = a.explicacion, EsAcertada = a.es_acertada, EsBuenaPractica = a.es_buena_practica,
        };
    }

    // Pregunta de conocimiento: puntos y explicación; nunca cambia la ruta ni suma retraso.
    public class EvaluacionPregunta : IEstrategiaEvaluacion
    {
        public ResultadoEvaluacion Evaluar(Situacion s, Alternativa a) => Base.De(a);
    }

    // Decisión de ruta: aplica el desvío y el tiempo que cuesta la alternativa.
    public class EvaluacionRuta : IEstrategiaEvaluacion
    {
        public ResultadoEvaluacion Evaluar(Situacion s, Alternativa a)
        {
            var r = Base.De(a);
            r.RetrasoMin = a.retraso_min;
            r.SiguienteTramoId = a.tramo_destino_id;
            return r;
        }
    }

    // Riesgo o emergencia: puntos o penalización, retraso y, si la acción insegura lo define, una ruta más difícil.
    public class EvaluacionRiesgoEmergencia : IEstrategiaEvaluacion
    {
        public ResultadoEvaluacion Evaluar(Situacion s, Alternativa a)
        {
            var r = Base.De(a);
            r.RetrasoMin = a.retraso_min;
            r.SiguienteTramoId = a.tramo_destino_id;
            return r;
        }
    }

    // Selecciona la estrategia según el tipo de la situación actual.
    public class EvaluadorSituaciones
    {
        readonly Dictionary<string, IEstrategiaEvaluacion> estrategias = new Dictionary<string, IEstrategiaEvaluacion>
        {
            [TipoSituacion.Pregunta] = new EvaluacionPregunta(),
            [TipoSituacion.DecisionRuta] = new EvaluacionRuta(),
            [TipoSituacion.Riesgo] = new EvaluacionRiesgoEmergencia(),
            [TipoSituacion.Emergencia] = new EvaluacionRiesgoEmergencia(),
        };

        public IEstrategiaEvaluacion SeleccionarEstrategia(string tipo) =>
            estrategias.TryGetValue(tipo ?? "", out var e) ? e : throw new KeyNotFoundException($"Sin estrategia para el tipo {tipo}");

        public ResultadoEvaluacion Evaluar(Situacion s, Alternativa a) => SeleccionarEstrategia(s.tipo).Evaluar(s, a);
    }
}
