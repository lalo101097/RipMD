using HtmlAgilityPack;
using RipMD.Helpers;
using RipMD.Services;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
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

        public async Task DescargarCapitulo(string url, CancellationToken cancellationToken, Action<int> actProgCap, Action<string> actCapDes)
        {
            // Modificar URL para cargar modo cascada
            string html = await downloader.DescargarHtml(url.Replace("paginated", "cascade"), cancellationToken);

            // Extraer título para carpeta
            var match = Regex.Match(html, @"<title>\s*(.*?)\s*</title>", RegexOptions.Singleline);
            if (!match.Success) throw new Exception("No se pudo encontrar el título.");

            string nombre = LimpiaNombre(match.Groups[1].Value);
            string path = Path.Combine(baseDirectory, nombre);
            Directory.CreateDirectory(path);

            actCapDes?.Invoke(nombre);

            string[] imagenes;

            if (html.Contains("let dirPath")) // modo moderno
            {
                var baseMatch = Regex.Match(html, @"(https?:\/\/[^""']+\/(?:uploads|data)\/)");
                if (!baseMatch.Success) throw new Exception("No se pudo encontrar la URL base de las imágenes.");
                string baseUrl = baseMatch.Groups[1].Value;

                var listaMatch = Regex.Match(html, @"let\s+images\s*=\s*(\[[^\]]+\])", RegexOptions.Singleline);
                if (!listaMatch.Success) throw new Exception("No se pudo encontrar la lista de imágenes.");
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
            }
            else // modo legacy
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
            }

            await DescargarImagenes(imagenes, "", path, url, cancellationToken, actProgCap);
        }

        private async Task DescargarImagenes(string[] urls, string baseUrl, string path, string urlex, CancellationToken cancellationToken, Action<int> actProgCap)
        {
            for (int i = 0; i < urls.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested(); // Punto de control para la cancelación

                actProgCap?.Invoke((i + 1) * 100 / urls.Length);
                string archivo = (i + 1).ToString("D3");
                string url = string.IsNullOrEmpty(baseUrl) ? urls[i] : baseUrl + urls[i];
                string ext = Path.GetExtension(url).ToLower();

                if (ext == ".webp")
                    await downloader.DescargarYConvertirWebP(url, Path.Combine(path, archivo), urlex, cancellationToken);
                else
                    await downloader.DescargarArchivo(url, Path.Combine(path, archivo + ext), urlex, cancellationToken);
            }
        }

        private string LimpiaNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "Capítulo";

            nombre = nombre.Replace(" - ", " ");
            nombre = WebUtility.HtmlDecode(nombre);
            nombre = Regex.Replace(nombre, @"[\\/:*?""<>|]", "");
            nombre = Regex.Replace(nombre, @"[\r\n\t\u00A0\u200B]+", " ");
            nombre = nombre.Replace('“', ' ').Replace('”', ' ').Replace('‘', '\'').Replace('’', '\'').Replace('–', '-').Replace('—', '-').Replace('¡', ' ').Replace('¿', ' ').Replace('?', ' ').Replace('!', ' ');
            nombre = Regex.Replace(nombre, @"\s{2,}", " ").Trim();
            nombre = nombre.TrimEnd('.', ' ');

            int idx = nombre.IndexOf("Capítulo", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                int corte = Math.Min(40, idx);
                string parte1 = nombre.Substring(0, corte).Trim();
                string parte2 = nombre.Substring(idx).Trim();
                nombre = $"{parte1} {parte2}";
            }

            return nombre.Length > 80 ? nombre.Substring(0, 80).TrimEnd('.', ' ') : nombre;
        }
    }
}