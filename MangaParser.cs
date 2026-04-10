using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RipMD
{
    internal class MangaParser
    {
        // ELIMINADO: HttpClient estático

        public static List<MangaInfo> ObtenerMangasDesdeHtml(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var mangas = new List<MangaInfo>();
            var nodos = doc.DocumentNode.SelectNodes("//div[contains(@class, 'element') and @data-identifier]");

            if (nodos == null) return mangas;

            foreach (var nodo in nodos)
            {
                try
                {
                    var enlaceNodo = nodo.SelectSingleNode(".//a");
                    var url = enlaceNodo?.GetAttributeValue("href", "").Trim();
                    if (string.IsNullOrWhiteSpace(url)) continue;

                    var tituloNodo = nodo.SelectSingleNode(".//h4[@class='text-truncate']");
                    var titulo = tituloNodo?.GetAttributeValue("title", "").Trim();

                    var demografiaNodo = nodo.SelectSingleNode(".//span[contains(@class, 'demography')]");
                    var demografia = demografiaNodo?.InnerText?.Trim() ?? "";

                    var puntuacionNodo = nodo.SelectSingleNode(".//span[@class='score']//span");
                    var puntuacion = puntuacionNodo?.InnerText?.Trim() ?? "";

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
                catch { continue; }
            }
            return mangas;
        }
    }
}