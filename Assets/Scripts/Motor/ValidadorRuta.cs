// Revisa ruta.json antes de jugar (antes vistas v_tramos_sin_salida y v_situaciones_incompletas).
// Riesgo R-03 / prueba 1.7.2: todos los caminos deben llegar al destino.
using System.Collections.Generic;
using System.Linq;
using LifeDrive.Datos;

namespace LifeDrive.Motor
{
    public static class ValidadorRuta
    {
        static readonly HashSet<string> Tipos = new HashSet<string>
            { TipoSituacion.Pregunta, TipoSituacion.DecisionRuta, TipoSituacion.Riesgo, TipoSituacion.Emergencia };

        // Lista de problemas en palabras; vacía = el contenido se puede jugar.
        public static List<string> Revisar(Contenido c)
        {
            var problemas = new List<string>();
            var tramos = c?.tramos ?? new Tramo[0];
            var situaciones = c?.situaciones ?? new Situacion[0];
            var ids = new HashSet<int>(tramos.Select(t => t.id));

            if ((c?.personajes?.Length ?? 0) == 0) problemas.Add("No hay personajes");
            if (tramos.Count(t => t.es_salida) != 1) problemas.Add("Debe haber exactamente un tramo de salida");
            if (!tramos.Any(t => t.es_destino)) problemas.Add("No hay tramo de destino");
            Duplicados(tramos.Select(t => t.id), "tramo", problemas);
            Duplicados(situaciones.Select(s => s.id), "situación", problemas);
            Duplicados(situaciones.SelectMany(s => s.alternativas ?? new Alternativa[0]).Select(a => a.id), "alternativa", problemas);

            foreach (var t in tramos)
            {
                if (t.es_destino && t.siguiente_tramo_id != 0) problemas.Add($"El destino «{t.nombre}» no debe tener siguiente tramo");
                if (!t.es_destino && !ids.Contains(t.siguiente_tramo_id)) problemas.Add($"El tramo «{t.nombre}» no tiene un siguiente tramo válido");
            }

            foreach (var s in situaciones)
            {
                var alts = s.alternativas ?? new Alternativa[0];
                var nombre = $"Situación {s.id}";
                if (!ids.Contains(s.tramo_id)) problemas.Add($"{nombre}: el tramo {s.tramo_id} no existe");
                if (!Tipos.Contains(s.tipo)) problemas.Add($"{nombre}: tipo «{s.tipo}» desconocido");
                if (string.IsNullOrWhiteSpace(s.enunciado)) problemas.Add($"{nombre}: sin enunciado");
                if (alts.Length < 2) problemas.Add($"{nombre}: necesita al menos dos alternativas");
                if (s.tipo == TipoSituacion.Pregunta && alts.Count(a => a.es_acertada) != 1) problemas.Add($"{nombre}: una pregunta necesita exactamente una respuesta correcta");
                if (s.tipo == TipoSituacion.DecisionRuta && alts.All(a => a.tramo_destino_id == 0)) problemas.Add($"{nombre}: una decisión de ruta necesita al menos un desvío");
                if (situaciones.Count(o => o.tramo_id == s.tramo_id && o.orden == s.orden) > 1) problemas.Add($"{nombre}: orden repetido en su tramo");
                foreach (var a in alts)
                {
                    if (a.tramo_destino_id != 0 && !ids.Contains(a.tramo_destino_id)) problemas.Add($"{nombre}, alternativa {a.id}: el tramo {a.tramo_destino_id} no existe");
                    if (a.es_buena_practica && !a.es_acertada) problemas.Add($"{nombre}, alternativa {a.id}: una buena práctica debe ser acertada");
                    if (a.retraso_min < 0) problemas.Add($"{nombre}, alternativa {a.id}: retraso negativo");
                    if (string.IsNullOrWhiteSpace(a.explicacion)) problemas.Add($"{nombre}, alternativa {a.id}: sin explicación");
                }
            }

            if (ids.Count != tramos.Length) return problemas;   // con ids repetidos no se puede recorrer el grafo

            // Tramos desde los que no se llega al destino ni por el camino por defecto ni por un desvío.
            var salidas = tramos.ToDictionary(t => t.id, t => new HashSet<int>());
            foreach (var t in tramos) if (ids.Contains(t.siguiente_tramo_id)) salidas[t.id].Add(t.siguiente_tramo_id);
            foreach (var s in situaciones.Where(s => ids.Contains(s.tramo_id)))
                foreach (var a in s.alternativas ?? new Alternativa[0])
                    if (ids.Contains(a.tramo_destino_id)) salidas[s.tramo_id].Add(a.tramo_destino_id);
            var llegan = new HashSet<int>(tramos.Where(t => t.es_destino).Select(t => t.id));
            for (var cambio = true; cambio;)
            {
                cambio = false;
                foreach (var t in tramos)
                    if (!llegan.Contains(t.id) && salidas[t.id].Any(llegan.Contains)) { llegan.Add(t.id); cambio = true; }
            }
            foreach (var t in tramos.Where(t => !llegan.Contains(t.id))) problemas.Add($"Desde el tramo «{t.nombre}» no se llega al destino");

            return problemas;
        }

        static void Duplicados(IEnumerable<int> ids, string que, List<string> problemas)
        {
            foreach (var g in ids.GroupBy(i => i).Where(g => g.Count() > 1)) problemas.Add($"Id de {que} repetido: {g.Key}");
        }
    }
}
