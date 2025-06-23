using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RipMD
{
    internal class MangaParser
    {
        private static readonly HttpClientHandler _handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10
        };

        private static readonly HttpClient _httpClient = new HttpClient(_handler);
        public static async Task<List<MangaInfo>> ObtenerMangasDesdeHtml(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var mangas = new List<MangaInfo>();

            var nodos = doc.DocumentNode.SelectNodes("//div[contains(@class, 'element') and @data-identifier]");

            if (nodos == null)
                return mangas;

            foreach (var nodo in nodos)
            {
                try
                {
                    var enlaceNodo = nodo.SelectSingleNode(".//a");
                    var url = enlaceNodo?.GetAttributeValue("href", "").Trim();
                    if (string.IsNullOrWhiteSpace(url))
                        continue;

                    var tituloNodo = nodo.SelectSingleNode(".//h4[@class='text-truncate']");
                    var titulo = tituloNodo?.GetAttributeValue("title", "").Trim();

                    var demografiaNodo = nodo.SelectSingleNode(".//span[contains(@class, 'demography')]");
                    var demografia = demografiaNodo?.InnerText?.Trim() ?? "";

                    var puntuacionNodo = nodo.SelectSingleNode(".//span[@class='score']//span");
                    var puntuacion = puntuacionNodo?.InnerText?.Trim() ?? "";

                    // Buscar el background-image del estilo embebido
                    var estiloNodo = nodo.SelectSingleNode(".//style[contains(text(), 'background-image')]");
                    string imagenPortada = "";

                    if (estiloNodo != null)
                    {
                        var estilo = estiloNodo.InnerText;
                        var regex = new Regex(@"background-image:\s*url\('(?<url>[^']+)'\)");
                        var match = regex.Match(estilo);
                        if (match.Success)
                        {
                            imagenPortada = match.Groups["url"].Value;
                        }
                    }

                    mangas.Add(new MangaInfo
                    {
                        Titulo = titulo,
                        Url = url,
                        ImagenPortada = imagenPortada,
                        Demografia = demografia,
                        Puntuacion = puntuacion
                    });
                }
                catch
                {
                    // Saltar errores individuales por nodo roto
                    continue;
                }
            }

            return mangas;
        }
    }
}
