// GeneradorReportes (BL-16, BL-18): PDF de una partida con datos, métricas, gráficos de barras y decisiones.
using System;
using System.Linq;

namespace LifeDrive.Motor
{
    public static class GeneradorReportes
    {
        const string Bueno = "#2E7D32", Malo = "#C62828", Neutro = "#1565C0", Texto = "#222222", Suave = "#666666";
        const float Margen = 50, AnchoUtil = DocumentoPdf.Ancho - 2 * Margen, Limite = DocumentoPdf.Alto - 60;

        // acumulado: indicadores del jugador sobre todas sus partidas finalizadas (opcional).
        public static byte[] GenerarPdf(Resumen r, Indicadores acumulado = null)
        {
            var pdf = new DocumentoPdf();
            var p = r.Partida;
            float y = 50;

            pdf.Texto(Margen, y, "Life Drive · Reporte de desempeño", 18, true); y += 30;
            // Cédula primero y nombre con ajuste de línea: un nombre largo no debe empujar la cédula fuera de la hoja.
            pdf.Texto(Margen, y, $"Cédula: {r.Jugador.cedula}", 10, false, Suave); y += 15;
            y = pdf.Parrafo(Margen, y, AnchoUtil, $"Jugador: {r.Jugador.nombre}", 10, false, Suave) + 2;
            pdf.Texto(Margen, y, $"Partida #{p.id}   ·   Inicio: {p.fecha_inicio}   ·   Fin: {p.fecha_fin}", 10, false, Suave); y += 26;
            pdf.Texto(Margen, y, $"Puntos de seguridad: {p.puntos}      Retraso acumulado: {p.retraso_min} min", 13, true); y += 18;
            pdf.Texto(Margen, y, "Llegar más rápido no significa un mejor resultado: el desempeño se mide por las decisiones seguras.", 9, false, Suave); y += 26;
            if (r.Indicadores != null) y = Indicadores(pdf, y, r.Indicadores, acumulado);

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

        // S3-09: tabla con los cuatro KPI de esta partida y, si se entrega, el acumulado del jugador.
        static float Indicadores(DocumentoPdf pdf, float y, Indicadores partida, Indicadores acumulado)
        {
            const float ColPartida = Margen + 240, ColAcumulado = Margen + 372;
            pdf.Texto(Margen, y, "Indicadores de desempeño (S3-09)", 11, true); y += 18;
            pdf.Texto(ColPartida, y, "Esta partida", 9, true, Suave);
            if (acumulado != null)
                pdf.Texto(ColAcumulado, y, $"Acumulado ({acumulado.PartidasConsideradas} finalizadas)", 9, true, Suave);
            y += 15;
            var filasAcumulado = acumulado?.Todos;
            for (var i = 0; i < partida.Todos.Count; i++)
            {
                var k = partida.Todos[i];
                pdf.Texto(Margen, y, $"{k.Sigla} · {k.Nombre}", 9);
                pdf.Texto(ColPartida, y, k.Texto, 9);
                if (filasAcumulado != null) pdf.Texto(ColAcumulado, y, filasAcumulado[i].Texto, 9);
                y += 15;
            }
            if (partida.ContenidoSinValidar || (acumulado?.ContenidoSinValidar ?? false))
                y = pdf.Parrafo(Margen, y + 2, AnchoUtil,
                    "Valores provisionales: se calcularon con situaciones que aún no están validadas con la organización.", 8, false, Malo);
            return y + 14;
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
