using UnityEngine;

namespace LifeDrive.Mapas.Demo
{
    public sealed partial class DemoRecorrido
    {
        CaminoBifurcado bifurcacion;
        string destinoDesvio, destinoPrincipal;
        bool usandoDesvio;
        public bool MostrarBifurcaciones { get; set; }
        public bool BifurcacionVisible => bifurcacion != null;
        public bool UsandoDesvio => usandoDesvio;
        public Vector2 SalidaDesvio => bifurcacion?.Posicion(bifurcacion.Longitud) ?? Vector2.zero;

        public void ConfigurarBifurcacion(float hito, string principal, string desvio)
        {
            if (recorrido == null || principal == desvio) return;
            if (bifurcacion != null && destinoPrincipal == principal && destinoDesvio == desvio) return;
            bifurcacion = new CaminoBifurcado(recorrido, hito, MapaActual.trayectoriaDesvio);
            destinoPrincipal = principal; destinoDesvio = desvio; usandoDesvio = false;
        }
        public void LimpiarBifurcacion()
        {
            bifurcacion = null; usandoDesvio = false;
            destinoPrincipal = destinoDesvio = null;
        }
        Vector2 PosicionEnCamino(float d) => usandoDesvio && bifurcacion != null && d >= bifurcacion.Inicio
            ? bifurcacion.Posicion(d - bifurcacion.Inicio) : recorrido?.Posicion(d) ?? Vector2.zero;
        Vector2 TangenteEnCamino(float d) => usandoDesvio && bifurcacion != null && d >= bifurcacion.Inicio
            ? bifurcacion.Tangente(d - bifurcacion.Inicio) : recorrido?.Tangente(d) ?? Vector2.right;

    }
}
