// Escritor PDF mínimo (texto Helvetica y rectángulos) para no depender de librerías externas en Unity.
// Coordenadas en puntos desde la esquina superior izquierda de una hoja carta (612 × 792).
// ponytail: el ajuste de línea estima el ancho de cada letra; usar métricas reales si un texto se sale del margen
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LifeDrive.Motor
{
    public class DocumentoPdf
    {
        public const float Ancho = 612, Alto = 792;
        readonly List<MemoryStream> paginas = new List<MemoryStream>();
        MemoryStream actual;

        public DocumentoPdf() => NuevaPagina();

        public void NuevaPagina()
        {
            actual = new MemoryStream();
            paginas.Add(actual);
        }

        public void Texto(float x, float y, string texto, float tamano, bool negrita = false, string colorHex = "#222222")
        {
            Escribir($"BT /{(negrita ? "F2" : "F1")} {N(tamano)} Tf {Color(colorHex)} rg {N(x)} {N(Alto - y - tamano)} Td (");
            var bytes = WinAnsi(texto);
            actual.Write(bytes, 0, bytes.Length);
            Escribir(") Tj ET\n");
        }

        public void Rectangulo(float x, float y, float ancho, float alto, string colorHex) =>
            Escribir($"{Color(colorHex)} rg {N(x)} {N(Alto - y - alto)} {N(ancho)} {N(alto)} re f\n");

        // Texto con ajuste de línea; devuelve la coordenada y debajo del párrafo.
        public float Parrafo(float x, float y, float ancho, string texto, float tamano, bool negrita = false, string colorHex = "#222222")
        {
            var porLinea = (int)(ancho / (tamano * (negrita ? 0.56f : 0.5f)));
            var linea = new StringBuilder();
            foreach (var palabra in (texto ?? "").Split(' '))
            {
                if (linea.Length > 0 && linea.Length + 1 + palabra.Length > porLinea)
                {
                    Texto(x, y, linea.ToString(), tamano, negrita, colorHex);
                    y += tamano * 1.3f;
                    linea.Clear();
                }
                if (linea.Length > 0) linea.Append(' ');
                linea.Append(palabra);
            }
            Texto(x, y, linea.ToString(), tamano, negrita, colorHex);
            return y + tamano * 1.3f;
        }

        public byte[] Generar()
        {
            var salida = new MemoryStream();
            var posiciones = new List<long>();
            void Objeto(string cuerpo, byte[] flujo = null)
            {
                posiciones.Add(salida.Position);
                Ascii(salida, $"{posiciones.Count} 0 obj\n{cuerpo}\n");
                if (flujo != null) { Ascii(salida, "stream\n"); salida.Write(flujo, 0, flujo.Length); Ascii(salida, "\nendstream\n"); }
                Ascii(salida, "endobj\n");
            }

            Ascii(salida, "%PDF-1.4\n");
            var kids = new StringBuilder();
            for (var i = 0; i < paginas.Count; i++) kids.Append($"{5 + i * 2} 0 R ");
            Objeto("<< /Type /Catalog /Pages 2 0 R >>");
            Objeto($"<< /Type /Pages /Kids [{kids}] /Count {paginas.Count} >>");
            Objeto("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
            Objeto("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");
            for (var i = 0; i < paginas.Count; i++)
            {
                var contenido = paginas[i].ToArray();
                Objeto($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {N(Ancho)} {N(Alto)}] " +
                       $"/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {6 + i * 2} 0 R >>");
                Objeto($"<< /Length {contenido.Length} >>", contenido);
            }

            var inicioXref = salida.Position;
            Ascii(salida, $"xref\n0 {posiciones.Count + 1}\n0000000000 65535 f \n");
            foreach (var p in posiciones) Ascii(salida, $"{p:D10} 00000 n \n");
            Ascii(salida, $"trailer\n<< /Size {posiciones.Count + 1} /Root 1 0 R >>\nstartxref\n{inicioXref}\n%%EOF\n");
            return salida.ToArray();
        }

        void Escribir(string ascii) => Ascii(actual, ascii);
        static void Ascii(Stream s, string texto) { var b = Encoding.ASCII.GetBytes(texto); s.Write(b, 0, b.Length); }
        static string N(float v) => v.ToString("0.##", CultureInfo.InvariantCulture);

        static string Color(string hex)
        {
            int Canal(int i) => int.Parse(hex.Substring(i, 2), NumberStyles.HexNumber);
            return $"{N(Canal(1) / 255f)} {N(Canal(3) / 255f)} {N(Canal(5) / 255f)}";
        }

        // Texto a WinAnsiEncoding (tildes y ñ incluidas) con los caracteres especiales de PDF escapados.
        static byte[] WinAnsi(string texto)
        {
            var bytes = new List<byte>();
            foreach (var c in texto ?? "")
            {
                if (c == '(' || c == ')' || c == '\\') bytes.Add((byte)'\\');
                switch (c)
                {
                    case '—': bytes.Add(151); break;
                    case '–': bytes.Add(150); break;
                    case '•': bytes.Add(149); break;
                    case '“': bytes.Add(147); break;
                    case '”': bytes.Add(148); break;
                    case '‘': bytes.Add(145); break;
                    case '’': bytes.Add(146); break;
                    case '…': bytes.Add(133); break;
                    case '−': bytes.Add((byte)'-'); break;
                    case '→': bytes.Add((byte)'>'); break;
                    default: bytes.Add(c < 256 && (c >= 32 || c == '\t') ? (byte)c : (byte)'?'); break;
                }
            }
            return bytes.ToArray();
        }
    }
}
