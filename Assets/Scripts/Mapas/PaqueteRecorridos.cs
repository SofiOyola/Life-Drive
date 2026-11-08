using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LifeDrive.Datos;
using LifeDrive.Motor;
using UnityEngine;

namespace LifeDrive.Mapas
{
    // Explicit loader for verification of the candidate. The game still uses Almacen directly.
    [Serializable]
    public sealed class PaqueteRecorridos
    {
        public int contenido_version;
        public string estado;
        public Contenido contenido;
        public NodoRecorrido[] nodos;
        public AsociacionRecorrido[] asociaciones;
        internal string Huella { get; private set; }

        public static PaqueteRecorridos Cargar(string carpeta, int versionGuardada)
        {
            switch (versionGuardada)
            {
                case 0:
                case 1:
                    return new PaqueteRecorridos
                    {
                        contenido_version = 1,
                        contenido = new Almacen(carpeta).CargarContenido()
                    };
                case 2:
                    var json = File.ReadAllText(Path.Combine(carpeta, "ruta-v2-candidata.json"));
                    var paquete = JsonUtility.FromJson<PaqueteRecorridos>(json);
                    if (paquete == null || paquete.contenido_version != 2 || paquete.estado != "CANDIDATO")
                        throw new InvalidDataException("El archivo no identifica el paquete candidato V2.");
                    using (var sha = SHA256.Create())
                        paquete.Huella = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(json)));
                    return paquete;
                default:
                    throw new ArgumentOutOfRangeException(nameof(versionGuardada), versionGuardada,
                        "Versión de contenido desconocida.");
            }
        }

        public void Validar(CatalogoMapas catalogo)
        {
            _ = new RecorridoVisualValidado(catalogo);
            Exigir(contenido_version == 2 && estado == "CANDIDATO", "Se esperaba el paquete candidato V2.");
            Exigir(contenido?.tramos != null && contenido.situaciones != null &&
                contenido.tramos.All(t => t != null) && contenido.situaciones.All(s =>
                    s?.alternativas != null && s.alternativas.All(a => a != null)), "Contenido incompleto.");
            var problemas = ValidadorRuta.Revisar(contenido);
            Exigir(problemas.Count == 0, string.Join("\n", problemas));
            Exigir(contenido.situaciones.All(s => !s.validada && string.IsNullOrEmpty(s.fecha_validacion)),
                "Las situaciones del paquete candidato requieren revisión de la organización.");

            var mapas = catalogo.mapas.ToDictionary(m => m.Id, StringComparer.Ordinal);
            var tramos = contenido.tramos.ToDictionary(t => t.id);
            Exigir(nodos != null && nodos.Length == mapas.Count && nodos.Length == tramos.Count &&
                nodos.All(n => n != null && !string.IsNullOrEmpty(n.mapa_id) &&
                    mapas.ContainsKey(n.mapa_id) && tramos.ContainsKey(n.tramo_id)) &&
                nodos.Select(n => n.mapa_id).Distinct().Count() == nodos.Length &&
                nodos.Select(n => n.tramo_id).Distinct().Count() == nodos.Length,
                "Cada mapa requiere un tramo existente y exclusivo.");
            var porMapa = nodos.ToDictionary(n => n.mapa_id, StringComparer.Ordinal);

            Exigir(asociaciones != null && asociaciones.All(a => a != null && a.contenido_version == contenido_version &&
                !string.IsNullOrEmpty(a.mapa_origen) && porMapa.ContainsKey(a.mapa_origen) &&
                !string.IsNullOrEmpty(a.mapa_destino) && porMapa.ContainsKey(a.mapa_destino)),
                "Asociación con mapa o versión inexistente.");
            var porOrigen = asociaciones.ToLookup(a => a.mapa_origen, StringComparer.Ordinal);
            var porTramo = contenido.situaciones.ToLookup(s => s.tramo_id);
            foreach (var nodo in nodos)
            {
                var tramo = tramos[nodo.tramo_id];
                var hijos = mapas[nodo.mapa_id].siguientes.Select(m => m.Id).ToArray();
                var filas = porOrigen[nodo.mapa_id].ToArray();
                var situaciones = porTramo[tramo.id].OrderBy(s => s.orden).ToArray();
                Exigir(tramo.es_salida == (nodo.mapa_id == "S0") &&
                    tramo.es_destino == (nodo.mapa_id == "DELIVERY"), $"Extremo lógico inválido en {nodo.mapa_id}.");
                Exigir(hijos.Length == 0 ? tramo.siguiente_tramo_id == 0 :
                    hijos.Any(id => porMapa[id].tramo_id == tramo.siguiente_tramo_id),
                    $"El avance por defecto sale del árbol en {nodo.mapa_id}.");

                var navegables = situaciones.Where(s => s.alternativas.Any(a => a.tramo_destino_id != 0)).ToArray();
                if (hijos.Length != 2)
                {
                    Exigir(filas.Length == 0 && navegables.Length == 0,
                        $"No se permiten desvíos desde {nodo.mapa_id}.");
                    continue;
                }

                Exigir(filas.Length == 2 && hijos.All(h => filas.Count(f => f.mapa_destino == h) == 1),
                    $"Se requiere una asociación para cada hijo de {nodo.mapa_id}.");
                Exigir(navegables.Length == 1 && navegables[0] == situaciones.Last(),
                    $"La bifurcación debe ser la última situación de {nodo.mapa_id}.");
                var bifurcacion = navegables[0];
                Exigir(bifurcacion.tipo != TipoSituacion.Pregunta && bifurcacion.alternativas.Length == 2 &&
                    filas.Select(f => f.alternativa_id).Distinct().Count() == 2,
                    $"Se requieren dos alternativas de navegación en {nodo.mapa_id}.");
                foreach (var fila in filas)
                {
                    var alternativa = bifurcacion.alternativas.SingleOrDefault(a => a.id == fila.alternativa_id);
                    Exigir(fila.situacion_id == bifurcacion.id && alternativa != null &&
                        alternativa.tramo_destino_id == porMapa[fila.mapa_destino].tramo_id,
                        $"La referencia {fila.situacion_id}/{fila.alternativa_id} no coincide con el destino de {nodo.mapa_id}.");
                }
            }
        }

        static void Exigir(bool condicion, string mensaje)
        {
            if (!condicion) throw new InvalidDataException(mensaje);
        }
    }

    [Serializable]
    public sealed class NodoRecorrido
    {
        public string mapa_id;
        public int tramo_id;
    }

    [Serializable]
    public sealed class AsociacionRecorrido
    {
        public int contenido_version;
        public int situacion_id;
        public int alternativa_id;
        public string mapa_origen;
        public string mapa_destino;
    }
}
