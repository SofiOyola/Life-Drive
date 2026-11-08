using System;
using System.Linq;
using LifeDrive.Motor;
using UnityEngine;

namespace LifeDrive.Mapas.InterfazVisual
{
    [Serializable]
    public sealed class AparienciaJugadorPilotoV1
    {
        public int version_formato;
        public int personaje_id;
        public string presentacion, piel, ojos, cabello, traje;
    }

    // Versioned names for the existing twenty swatches. No new art or educational character IDs.
    public static class VariantesAparienciaPilotoV1
    {
        static readonly string[][] ids =
        {
            new[] { "marfil", "arena", "canela", "cacao", "ebano" },
            new[] { "cafe", "verde", "azul", "miel", "gris" },
            new[] { "negro", "castano", "cobre", "rubio", "plata" },
            new[] { "musgo", "azul", "terracota", "crema", "ciruela" }
        };
        static readonly string[][] tonos =
        {
            new[] { "EAC6A3", "D6A37A", "B77D53", "855438", "54392C" },
            new[] { "694527", "65834D", "5185A5", "BB9247", "87938A" },
            new[] { "292723", "61422F", "9E5030", "D0AC67", "AAAFA5" },
            new[] { "78834C", "54748F", "AE6348", "D1C3A0", "82647C" }
        };
        public static AparienciaJugadorPilotoV1 Crear(int personajeId, int estilo, params int[] seleccion)
        {
            if ((estilo != 0 && estilo != 1) || seleccion == null || seleccion.Length != 4 || seleccion.Any(i => i < 0 || i >= 5))
                throw new ErrorJuego("La selección de apariencia no es válida.");
            return new AparienciaJugadorPilotoV1 { version_formato = 1, personaje_id = personajeId,
                presentacion = estilo == 1 ? "mujer" : "hombre", piel = ids[0][seleccion[0]], ojos = ids[1][seleccion[1]],
                cabello = ids[2][seleccion[2]], traje = ids[3][seleccion[3]] };
        }
        public static void Validar(AparienciaJugadorPilotoV1 a, int personajeId)
        {
            if (a == null || a.version_formato != 1 || a.personaje_id != personajeId || personajeId <= 0 ||
                (a.presentacion != "hombre" && a.presentacion != "mujer"))
                throw new ErrorJuego("La apariencia guardada no es compatible. No se sobrescribió.");
            _ = Indices(a);
        }
        public static int[] Indices(AparienciaJugadorPilotoV1 a)
        {
            var variantes = new[] { a.piel, a.ojos, a.cabello, a.traje };
            var resultado = variantes.Select((valor, grupo) => Array.IndexOf(ids[grupo], valor)).ToArray();
            if (resultado.Any(i => i < 0)) throw new ErrorJuego("Hay una variante visual desconocida. No se sobrescribió.");
            return resultado;
        }
        public static Color ColorDe(int grupo, int indice)
        {
            ColorUtility.TryParseHtmlString("#" + tonos[grupo][indice], out var color); return color;
        }
        public static void Aplicar(AvatarPersonalizableUI avatar, AparienciaJugadorPilotoV1 a)
        {
            Validar(a, a.personaje_id); var indices = Indices(a);
            avatar.mujer = a.presentacion == "mujer";
            avatar.piel = ColorDe(0, indices[0]); avatar.ojos = ColorDe(1, indices[1]);
            avatar.cabello = ColorDe(2, indices[2]); avatar.traje = ColorDe(3, indices[3]); avatar.Actualizar();
        }
        public static void Aplicar(PersonalizacionRegistroUI vista, AparienciaJugadorPilotoV1 a)
        {
            Validar(a, a.personaje_id);
            var paletas = new[] { vista.tonosPiel, vista.tonosOjos, vista.tonosCabello, vista.tonosTraje };
            for (int g = 0; g < 4; g++)
                if (paletas[g].Length != 5 || paletas[g].Where((c, i) => Vector4.Distance(c, ColorDe(g, i)) > .00001f).Any())
                    throw new ErrorJuego("La paleta de Registro cambió. Revisa el contrato visual V1 antes de guardar.");
            vista.ElegirEstilo(a.presentacion == "mujer" ? 1 : 0);
            var indices = Indices(a); for (int g = 0; g < 4; g++) vista.ElegirColor(g, indices[g]);
        }
    }
}
