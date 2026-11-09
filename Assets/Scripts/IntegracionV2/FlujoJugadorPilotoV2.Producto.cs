using System;
using System.Collections.Generic;
using System.Linq;
using LifeDrive.Datos;
using LifeDrive.Mapas.InterfazVisual;
using LifeDrive.Motor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LifeDrive.IntegracionV2
{
    public sealed partial class FlujoJugadorPilotoV2
    {
        public bool EsPilotoV2 { get; private set; } = true;
        Scene escenaEnlazada;
        readonly Dictionary<int,AparienciaJugadorPilotoV1> aparienciaSesion = new Dictionary<int,AparienciaJugadorPilotoV1>();
        public static FlujoJugadorPilotoV2 ActivarVigente(ServicioJuego juego, string carpeta, Action<string> navegar)
        {
            var f = new GameObject("Flujo Life Drive V1").AddComponent<FlujoJugadorPilotoV2>();
            DontDestroyOnLoad(f.gameObject);
            f.Juego = juego; f.CarpetaDatos = carpeta;
            f.cargar = navegar; f.EsPilotoV2 = false;
            SceneManager.sceneLoaded += f.AlCargar; f.gameObject.AddComponent<TecladoPilotoV2>();
            return f;
        }
        public void ConectarEntradaActual() => AlCargar(SceneManager.GetActiveScene(),LoadSceneMode.Single);
        bool CierreDisponible() => EsPilotoV2 ? Juego.RecorridosV2.Consultar(PartidaId).CierrePresentado :
            Juego.Desempeno.Resumen(PartidaId).Partida.estado == EstadoPartida.Finalizada;

        Action<Jugador> PrepararAparienciaSesion(PersonalizacionRegistroUI v)
        {
            bool editada=false;
            foreach(var b in v.estilos.Concat(v.piel).Concat(v.ojos).Concat(v.cabello).Concat(v.traje)) b.onClick.AddListener(()=>editada=true);
            return j =>
            {
                if (!aparienciaSesion.TryGetValue(j.id,out var a) || editada) a=VariantesAparienciaPilotoV1.Crear(j.personaje_id,v.Estilo,v.Selecciones);
                aparienciaSesion[j.id]=a; AparienciaPartidaActual=a;
                VariantesAparienciaPilotoV1.Aplicar(v,a);
            };
        }
    }
}
