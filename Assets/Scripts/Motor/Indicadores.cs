// Indicadores de desempeño (S3-09): TDS, IEE, PPS y PCRS.
// Fórmulas del documento de S3-09 (recogidas en el informe integral, sección 17.3):
//   TDS  = decisiones seguras / decisiones en RIESGO, EMERGENCIA y DECISION_RUTA × 100
//   IEE  = preguntas acertadas / preguntas respondidas × 100
//   PPS  = suma de puntos de las partidas FINALIZADAS / número de partidas FINALIZADAS
//   PCRS = elecciones de ruta segura / decisiones de ruta × 100
// Sin casos el valor es 0, como pide el documento; Denominador permite distinguir un 0 real de "sin casos".
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LifeDrive.Datos;

namespace LifeDrive.Motor
{
    public class Indicador
    {
        public string Sigla;
        public string Nombre;
        public bool EsPorcentaje;
        public int Numerador;     // aciertos, o suma de puntos en PPS
        public int Denominador;   // casos, o partidas finalizadas en PPS

        public bool SinCasos => Denominador == 0;
        public double Valor => SinCasos ? 0 : EsPorcentaje ? 100.0 * Numerador / Denominador : (double)Numerador / Denominador;

        // "75,0 % (3 de 4)" · "42,5 (85 pts en 2 partidas)" · "0 % (sin casos)"
        public string Texto
        {
            get
            {
                if (EsPorcentaje)
                    return SinCasos ? "0 % (sin casos)" : $"{Decimal(Valor)} % ({Numerador} de {Denominador})";
                return SinCasos ? "0 (sin partidas finalizadas)"
                    : $"{Decimal(Valor)} ({Numerador} pts en {Denominador} {(Denominador == 1 ? "partida" : "partidas")})";
            }
        }

        // Coma decimal fija: no depende de la cultura del equipo donde corre el juego.
        static string Decimal(double v) => v.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', ',');
    }

    public class Indicadores
    {
        public Indicador TDS, IEE, PPS, PCRS;
        public int PartidasConsideradas;
        // Alguna decisión salió de una situación no validada con la organización: los valores son provisionales.
        public bool ContenidoSinValidar;

        public IReadOnlyList<Indicador> Todos => new[] { TDS, IEE, PPS, PCRS };
    }

    public static class CalculadoraIndicadores
    {
        static readonly string[] TiposDecision = { TipoSituacion.Riesgo, TipoSituacion.Emergencia, TipoSituacion.DecisionRuta };

        // Calcula sobre las partidas dadas. Para una partida, pasar solo esa; para el acumulado, las finalizadas.
        // Las decisiones de otras partidas se ignoran, así que se puede pasar la tabla completa.
        public static Indicadores Calcular(IEnumerable<Partida> partidas, IEnumerable<Decision> decisiones)
        {
            var lista = partidas.ToList();
            var ids = new HashSet<int>(lista.Select(p => p.id));
            var propias = decisiones.Where(d => ids.Contains(d.partida_id)).ToList();
            var finalizadas = lista.Where(p => p.estado == EstadoPartida.Finalizada).ToList();

            return new Indicadores
            {
                TDS = Porcentaje("TDS", "Tasa de Decisiones Seguras", propias.Where(d => TiposDecision.Contains(d.tipo))),
                IEE = Porcentaje("IEE", "Índice de Efectividad Educativa", propias.Where(d => d.tipo == TipoSituacion.Pregunta)),
                PPS = new Indicador
                {
                    Sigla = "PPS", Nombre = "Promedio de Puntos de Seguridad",
                    Numerador = finalizadas.Sum(p => p.puntos), Denominador = finalizadas.Count,
                },
                PCRS = Porcentaje("PCRS", "Porcentaje de Cumplimiento de Rutas Seguras", propias.Where(d => d.tipo == TipoSituacion.DecisionRuta)),
                PartidasConsideradas = lista.Count,
            };
        }

        static Indicador Porcentaje(string sigla, string nombre, IEnumerable<Decision> casos)
        {
            var lista = casos.ToList();
            return new Indicador
            {
                Sigla = sigla, Nombre = nombre, EsPorcentaje = true,
                Numerador = lista.Count(d => d.es_acertada), Denominador = lista.Count,
            };
        }
    }
}
