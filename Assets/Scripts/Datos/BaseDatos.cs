// Base de datos local de Life Drive: las tablas del modelo S2-06 guardadas en
// Application.persistentDataPath/lifedrive-db.json, con las mismas reglas de integridad.
//
// Todo cambio pasa por Transaccion(): si una regla falla o el archivo no se puede escribir,
// los cambios en memoria se revierten y el archivo queda como estaba (todo o nada).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LifeDrive.Datos
{
    // Un dato viola una regla de integridad (equivale a un CHECK, UNIQUE o FOREIGN KEY rechazado).
    public class ErrorIntegridad : Exception { public ErrorIntegridad(string mensaje) : base(mensaje) { } }

    public class BaseDatos
    {
        public const int VersionEsquema = 1;
        static readonly Regex Cedula = new Regex("^[0-9]{5,15}$");

        readonly string archivo;
        TablasLocales t;
        bool enTransaccion;

        public BaseDatos(string carpeta)
        {
            Directory.CreateDirectory(carpeta);
            archivo = Path.Combine(carpeta, "lifedrive-db.json");
            t = Abrir();
        }

        public IReadOnlyList<Jugador> Jugadores => t.jugador;
        public IReadOnlyList<Partida> Partidas => t.partida;
        public IReadOnlyList<Decision> Decisiones => t.decision;
        public IReadOnlyList<Evento> Eventos => t.evento;

        // ---------- Inserciones (solo dentro de una transacción) ----------

        public Jugador Insertar(Jugador j) { Exigir(); j.id = ++t.secuencias.jugador; t.jugador.Add(j); return j; }
        public Partida Insertar(Partida p) { Exigir(); p.id = ++t.secuencias.partida; t.partida.Add(p); return p; }
        public Decision Insertar(Decision d) { Exigir(); d.id = ++t.secuencias.decision; t.decision.Add(d); return d; }
        public Evento Insertar(Evento e) { Exigir(); e.id = ++t.secuencias.evento; t.evento.Add(e); return e; }

        void Exigir()
        {
            if (!enTransaccion) throw new InvalidOperationException("Los cambios a la base de datos deben hacerse dentro de Transaccion()");
        }

        // ---------- Transacciones ----------

        public void Transaccion(Action cambios) => Transaccion(() => { cambios(); return 0; });

        // Las filas leídas también se pueden modificar dentro de la transacción (UPDATE).
        // Al terminar se revisan todas las reglas y se guarda; ante cualquier error se revierte.
        public T Transaccion<T>(Func<T> cambios)
        {
            if (enTransaccion) return cambios();   // transacción anidada: la confirma la exterior
            var copia = JsonUtility.ToJson(t);
            enTransaccion = true;
            try
            {
                var resultado = cambios();
                Verificar();
                Guardar();
                return resultado;
            }
            catch
            {
                t = JsonUtility.FromJson<TablasLocales>(copia);
                throw;
            }
            finally
            {
                enTransaccion = false;
            }
        }

        // ---------- Reglas de integridad (las mismas del esquema PostgreSQL del S2-06) ----------

        // ponytail: revisa todas las filas en cada confirmación; con miles de partidas conviene revisar solo las tocadas
        void Verificar()
        {
            var jugadores = new HashSet<int>();
            var cedulas = new HashSet<string>();
            foreach (var j in t.jugador)
            {
                if (string.IsNullOrWhiteSpace(j.nombre)) Falla($"Jugador {j.id}: el nombre no puede estar vacío");
                if (j.cedula == null || !Cedula.IsMatch(j.cedula)) Falla($"Jugador {j.id}: la cédula debe tener entre 5 y 15 dígitos");
                if (!cedulas.Add(j.cedula)) Falla($"La cédula {j.cedula} ya está registrada");
                if (j.personaje_id <= 0) Falla($"Jugador {j.id}: falta el personaje");
                jugadores.Add(j.id);
            }

            var partidas = new HashSet<int>();
            var enCurso = new HashSet<int>();
            foreach (var p in t.partida)
            {
                if (!jugadores.Contains(p.jugador_id)) Falla($"Partida {p.id}: el jugador {p.jugador_id} no existe");
                if (p.estado != EstadoPartida.EnCurso && p.estado != EstadoPartida.Finalizada) Falla($"Partida {p.id}: estado «{p.estado}» no válido");
                if ((p.estado == EstadoPartida.EnCurso) != string.IsNullOrEmpty(p.fecha_fin)) Falla($"Partida {p.id}: solo una partida terminada tiene fecha de fin");
                if (p.estado == EstadoPartida.EnCurso && !enCurso.Add(p.jugador_id)) Falla($"El jugador {p.jugador_id} ya tiene una partida en curso");
                if (p.tramo_actual_id <= 0 || p.retraso_min < 0) Falla($"Partida {p.id}: tramo o retraso no válido");
                partidas.Add(p.id);
            }

            var resueltas = new HashSet<(int, int)>();
            foreach (var d in t.decision)
            {
                if (!partidas.Contains(d.partida_id)) Falla($"Decisión {d.id}: la partida {d.partida_id} no existe");
                if (!resueltas.Add((d.partida_id, d.situacion_id))) Falla($"La situación {d.situacion_id} ya tiene una decisión en la partida {d.partida_id}");
                if (d.tramo_origen_id <= 0 || d.tramo_destino_id <= 0 || d.retraso_aplicado < 0) Falla($"Decisión {d.id}: tramos o retraso no válidos");
            }

            foreach (var e in t.evento)
            {
                if (string.IsNullOrEmpty(e.tipo)) Falla($"Evento {e.id}: falta el tipo");
                if (e.jugador_id != 0 && !jugadores.Contains(e.jugador_id)) Falla($"Evento {e.id}: el jugador {e.jugador_id} no existe");
                if (e.partida_id != 0 && !partidas.Contains(e.partida_id)) Falla($"Evento {e.id}: la partida {e.partida_id} no existe");
            }
        }

        static void Falla(string mensaje) => throw new ErrorIntegridad(mensaje);

        // ---------- Archivo ----------

        // Escribe en un temporal y lo reemplaza de una vez: un corte de luz no deja el archivo a medias.
        // La versión anterior queda en lifedrive-db.json.bak.
        void Guardar()
        {
            var temporal = archivo + ".tmp";
            File.WriteAllText(temporal, JsonUtility.ToJson(t, true));
            if (File.Exists(archivo)) File.Replace(temporal, archivo, archivo + ".bak");
            else File.Move(temporal, archivo);
        }

        // Si el archivo está dañado se aparta (nunca se sobrescribe) y se usa la copia .bak.
        TablasLocales Abrir()
        {
            if (!File.Exists(archivo)) return new TablasLocales();
            var tablas = Leer(archivo);
            if (tablas == null)
            {
                var danado = $"{archivo}.danado-{DateTime.Now:yyyyMMdd-HHmmss}";
                File.Move(archivo, danado);
                tablas = Leer(archivo + ".bak")
                    ?? throw new IOException($"La base de datos está dañada y no hay copia válida. Se apartó en {danado}");
            }
            if (tablas.version > VersionEsquema)
                throw new IOException($"La base de datos es de una versión más nueva del juego ({tablas.version})");
            return tablas;
        }

        static TablasLocales Leer(string ruta)
        {
            if (!File.Exists(ruta)) return null;
            try
            {
                var tablas = JsonUtility.FromJson<TablasLocales>(File.ReadAllText(ruta));
                return tablas?.jugador == null ? null : tablas;
            }
            catch (Exception) { return null; }
        }
    }
}
