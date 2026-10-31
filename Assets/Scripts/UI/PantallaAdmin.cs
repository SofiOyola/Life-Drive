// Prototipos 8 y 9 · HU-10: buscar jugador → elegir partida → ver desempeño (PDF).
// El PDF se guarda en la carpeta de reportes y se abre en el visor del sistema,
// que ya ofrece vista previa, guardar e imprimir.
using System.IO;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Motor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeDrive.UI
{
    public class PantallaAdmin : MonoBehaviour
    {
        [SerializeField] TMP_InputField campoBusqueda;
        [SerializeField] Button botonBuscar;
        [SerializeField] Transform contenedorJugadores;
        [SerializeField] Transform contenedorPartidas;
        [SerializeField] Button prefabBoton;
        [SerializeField] Button botonSalir;
        [SerializeField] TMP_Text textoMensaje;
        [SerializeField] TMP_Text textoEventos;   // últimos eventos del jugador seleccionado

        ServicioJuego juego;

        void Start()
        {
            juego = Sesion.Juego;
            if (juego.Usuarios.Actual.Rol != Rol.Administrador) { SceneManager.LoadScene(Escenas.AdminLogin); return; }
            botonBuscar.onClick.AddListener(Buscar);
            botonSalir.onClick.AddListener(() => { Sesion.Cerrar(); SceneManager.LoadScene(Escenas.Inicio); });
            Buscar();
        }

        void Buscar()
        {
            BotonesLista.Limpiar(contenedorJugadores);
            BotonesLista.Limpiar(contenedorPartidas);
            textoEventos.text = "";
            var filas = juego.Desempeno.ListarJugadores(campoBusqueda.text);
            Mostrar(filas.Count == 0 ? "No hay jugadores con ese nombre o cédula" : "");
            foreach (var f in filas)
                BotonesLista.Agregar(prefabBoton, contenedorJugadores,
                    $"{f.Jugador.nombre} · {f.Jugador.cedula} · {f.PartidasFinalizadas} partida(s)",
                    () => MostrarPartidas(f.Jugador.id));
        }

        void MostrarPartidas(int jugadorId)
        {
            BotonesLista.Limpiar(contenedorPartidas);
            var finalizadas = 0;
            foreach (var p in juego.Desempeno.PartidasDe(jugadorId))
            {
                if (p.estado != EstadoPartida.Finalizada) continue;   // solo las finalizadas tienen reporte
                finalizadas++;
                BotonesLista.Agregar(prefabBoton, contenedorPartidas,
                    $"Partida #{p.id} · {p.fecha_fin} · {p.puntos} pts · Ver desempeño",
                    () => VerDesempeno(p.id));
            }
            Mostrar(finalizadas == 0 ? "Este jugador no tiene partidas finalizadas" : "");

            textoEventos.text = "";
            foreach (var e in juego.Desempeno.Eventos(jugadorId: jugadorId).Take(15))
                textoEventos.text += $"{e.fecha_hora}  {e.tipo}{(e.partida_id != 0 ? $"  #{e.partida_id}" : "")}  {e.detalle}\n";
        }

        void VerDesempeno(int partidaId)
        {
            try
            {
                Directory.CreateDirectory(Sesion.CarpetaReportes);
                var ruta = Path.Combine(Sesion.CarpetaReportes, $"lifedrive-partida-{partidaId}.pdf");
                File.WriteAllBytes(ruta, juego.Desempeno.ReportePdf(partidaId));
                Application.OpenURL("file:///" + ruta.Replace('\\', '/'));
                Mostrar($"Reporte guardado en {ruta}");
            }
            catch (IOException e) { Mostrar("No se pudo guardar el reporte: " + e.Message + ". Intenta de nuevo."); }
            catch (ErrorJuego e) { Mostrar(e.Message); }
        }

        void Mostrar(string mensaje) => textoMensaje.text = mensaje;
    }
}
