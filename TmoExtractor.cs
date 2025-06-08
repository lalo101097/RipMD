using HtmlAgilityPack;
using RipMD;
using RipMD.Services;
using System;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RipMD.Extractors
{
    public class TmoExtractor
    {
        private readonly DownloaderService downloader;
        private readonly string baseDirectory;

        public TmoExtractor(DownloaderService downloader)
        {
            this.downloader = downloader;
            this.baseDirectory = downloader.GetOutputDirectory();
        }

        public async Task DescargarCapitulo(string url, Action<int> ActProgCap, Action<string> ActCapDes)
        {
            // Modificar URL para cargar modo cascada (todas las imágenes en orden)
            string html = await downloader.DescargarHtml(url.Replace("paginated", "cascade"));

            // Extraer título para carpeta
            var match = Regex.Match(html, @"<title>\s*(.*?)\s*</title>", RegexOptions.Singleline);
            if (!match.Success) throw new Exception("No se pudo encontrar el título.");

            string nombre = LimpiaNombre(match.Groups[1].Value);
            string path = Path.Combine(baseDirectory, nombre);
            Directory.CreateDirectory(path);

            ActCapDes?.Invoke(nombre);

            string[] imagenes;

            if (html.Contains("let dirPath")) // modo moderno detectado
            {
                var baseMatch = Regex.Match(html, @"(https?:\/\/[^""']+\/(?:uploads|data)\/)");
                if (!baseMatch.Success) throw new Exception("No se pudo encontrar la URL base de las imágenes.");

                string baseUrl = baseMatch.Groups[1].Value;

                var listaMatch = Regex.Match(html, @"let\s+images\s*=\s*(\[[^\]]+\])", RegexOptions.Singleline);
                if (!listaMatch.Success)
                    throw new Exception("No se pudo encontrar la lista de imágenes.");

                string rawArray = listaMatch.Groups[1].Value;

                imagenes = Regex.Matches(rawArray, "\"(.*?)\"")
                                .Cast<Match>()
                                .Select(m => m.Groups[1].Value)
                                .ToArray();

                for (int i = 0; i < imagenes.Length; i++)
                {
                    if (!imagenes[i].StartsWith("http"))
                        imagenes[i] = baseUrl + imagenes[i];
                }

                await DescargarImagenes(imagenes, "", path, url, ActProgCap);
            }
            else // modo legacy o lazy-load con data-src
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var nodes = doc.DocumentNode.SelectNodes("//img[@data-src]");
                if (nodes != null && nodes.Count > 0)
                {
                    imagenes = nodes.Select(n => n.GetAttributeValue("data-src", null))
                                    .Where(src => !string.IsNullOrWhiteSpace(src))
                                    .ToArray();
                }
                else
                {
                    var matches = Regex.Matches(html, @"https?:\/\/.*?\/uploads\/.*?\.(webp|jpg|png)", RegexOptions.IgnoreCase);
                    imagenes = matches.Cast<Match>().Select(m => m.Value).ToArray();
                }

                await DescargarImagenes(imagenes, "", path, url, ActProgCap);
            }
        }



        private async Task DescargarImagenes(string[] urls, string baseUrl, string path, string urlex, Action<int> ActProgCap)
        {

            for (int i = 0; i < urls.Length; i++)
            {
                ActProgCap?.Invoke((i + 1) * 100 / urls.Length);
                string archivo = (i + 1).ToString("D3");
                string url = string.IsNullOrEmpty(baseUrl) ? urls[i] : baseUrl + urls[i];

                string ext = Path.GetExtension(url).ToLower();
                if (ext == ".webp")
                    await downloader.DescargarYConvertirWebP(url, Path.Combine(path, archivo), urlex);
                else
                    await downloader.DescargarArchivo(url, Path.Combine(path, archivo + ext), urlex);
            }

        }

        private string LimpiaNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "Capítulo";

            nombre = nombre.Replace(" - ", " ");

            // Decodifica HTML (&amp;, &nbsp;, etc.)
            nombre = WebUtility.HtmlDecode(nombre);

            // Elimina caracteres inválidos en nombres de archivo/carpeta
            nombre = Regex.Replace(nombre, @"[\\/:*?""<>|]", "");

            // Limpia saltos de línea, tabs y espacios invisibles
            nombre = Regex.Replace(nombre, @"[\r\n\t\u00A0\u200B]+", " ");

            // Reemplaza caracteres Unicode problemáticos pero preserva acentos
            nombre = nombre.Replace('“', ' ')
                           .Replace('”', ' ')
                           .Replace('‘', '\'')
                           .Replace('’', '\'')
                           .Replace('–', '-')
                           .Replace('—', '-')
                           .Replace('¡', ' ')
                           .Replace('¿', ' ')
                           .Replace('?', ' ')
                           .Replace('!', ' ');

            // Reduce múltiples espacios a uno solo
            nombre = Regex.Replace(nombre, @"\s{2,}", " ").Trim();

            // Asegura que no termine en espacio o punto
            nombre = nombre.TrimEnd('.', ' ');

            // Busca la palabra "Capítulo" para dividir el nombre
            int idx = nombre.IndexOf("Capítulo", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                int corte = Math.Min(40, idx);
                string parte1 = nombre.Substring(0, corte).Trim();
                string parte2 = nombre.Substring(idx).Trim();
                nombre = $"{parte1} {parte2}";
            }

            // Finalmente, limitar longitud a 80 caracteres por seguridad de ruta
            return nombre.Length > 80 ? nombre.Substring(0, 80).TrimEnd('.', ' ') : nombre;
        }


    }
}
