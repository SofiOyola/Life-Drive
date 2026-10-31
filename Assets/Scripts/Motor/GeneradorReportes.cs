// GeneradorReportes (BL-16, BL-18): PDF de una partida con datos, métricas, gráficos de barras y decisiones.
using System;
using System.Linq;

namespace LifeDrive.Motor
{
    public static class GeneradorReportes
    {
        const string Bueno = "#2E7D32", Malo = "#C62828", Neutro = "#1565C0", Texto = "#222222", Suave = "#666666";
        const float Margen = 50, AnchoUtil = DocumentoPdf.Ancho - 2 * Margen, Limite = DocumentoPdf.Alto - 60;

        public static byte[] GenerarPdf(Resumen r)
        {
            var pdf = new DocumentoPdf();
            var p = r.Partida;
            float y = 50;

            pdf.Texto(Margen, y, "Life Drive · Reporte de desempeño", 18, true); y += 30;
            pdf.Texto(Margen, y, $"Jugador: {r.Jugador.nombre}   ·   Cédula: {r.Jugador.cedula}", 10, false, Suave); y += 15;
            pdf.Texto(Margen, y, $"Partida #{p.id}   ·   Inicio: {p.fecha_inicio}   ·   Fin: {p.fecha_fin}", 10, false, Suave); y += 26;
            pdf.Texto(Margen, y, $"Puntos de seguridad: {p.puntos}      Retraso acumulado: {p.retraso_min} min", 13, true); y += 18;
            pdf.Texto(Margen, y, "Llegar más rápido no significa un mejor resultado: el desempeño se mide por las decisiones seguras.", 9, false, Suave); y += 26;

            y = Barras(pdf, y, "Preguntas de conocimiento",
                ("Correctas", r.PreguntasCorrectas, Bueno), ("Incorrectas", r.PreguntasIncorrectas, Malo));
            y = Barras(pdf, y, "Decisiones, riesgos y emergencias",
                ("Seguras", r.DecisionesSeguras, Bueno), ("Inseguras", r.DecisionesInseguras, Malo),
                ("Riesgos enfrentados", r.RiesgosEnfrentados, Neutro), ("Buenas prácticas", r.BuenasPracticas, Bueno),
                ("Cambios de ruta", r.CambiosDeRuta, Neutro));

            pdf.Texto(Margen, y, "Ruta recorrida", 11, true); y += 16;
            y = pdf.Parrafo(Margen, y, AnchoUtil, r.RutaRecorrida.Count > 0 ? string.Join("  >  ", r.RutaRecorrida) : "—", 9) + 12;

            pdf.Texto(Margen, y, "Decisiones en orden", 11, true); y += 18;
            var n = 0;
            foreach (var h in r.Historial)
            {
                if (y > Limite) { pdf.NuevaPagina(); y = 50; }
                var d = h.Decision;
                var titulo = $"{++n}. {d.tipo} · {h.TramoOrigen} · {(d.es_acertada ? "Segura/correcta" : "Insegura/incorrecta")}"
                             + (d.es_buena_practica ? " · Buena práctica" : "");
                y = pdf.Parrafo(Margen, y, AnchoUtil, titulo, 9, true, d.es_acertada ? Bueno : Malo);
                y = pdf.Parrafo(Margen, y, AnchoUtil, "Elección: " + d.alternativa, 9);
                var signo = d.puntos_aplicados >= 0 ? "+" : "";
                y = pdf.Parrafo(Margen, y, AnchoUtil, $"{d.consecuencia}  ({signo}{d.puntos_aplicados} pts, acumulado {h.PuntosAcumulados})", 9, false, Suave) + 6;
            }
            return pdf.Generar();
        }

        // Barras horizontales escaladas al mayor valor del grupo.
        static float Barras(DocumentoPdf pdf, float y, string titulo, params (string etiqueta, int valor, string color)[] filas)
        {
            pdf.Texto(Margen, y, titulo, 11, true); y += 18;
            var max = Math.Max(1, filas.Max(f => f.valor));
            foreach (var (etiqueta, valor, color) in filas)
            {
                var largo = Math.Max(1f, valor / (float)max * (AnchoUtil - 150));
                pdf.Texto(Margen, y + 1, etiqueta, 9);
                pdf.Rectangulo(Margen + 115, y, largo, 11, color);
                pdf.Texto(Margen + 120 + largo, y + 1, valor.ToString(), 9);
                y += 17;
            }
            return y + 10;
        }
    }
}
