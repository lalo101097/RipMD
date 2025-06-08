using HtmlAgilityPack;
using RipMD.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RipMD.Parsers
{
    public static class ChapterParser
    {
        private static readonly HttpClientHandler _handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10
        };

        private static readonly HttpClient _httpClient = new HttpClient(_handler);

        public static async Task<List<ChapterInfo>> ObtenerCapitulosDesdeHtml(string html, string paginaManga)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var capitulos = new List<ChapterInfo>();

            var nodos = doc.DocumentNode.SelectNodes("//li[@class='list-group-item']");

            if (nodos == null)
                return capitulos;

            foreach (var nodo in nodos)
            {
                var nombreNodo = nodo.SelectSingleNode(".//ancestor::li[@data-index]//h4//a[contains(@class, 'btn-collapse')]");
                var nombreTexto = nombreNodo?.InnerText?.Trim() ?? "";

                if (!nombreTexto.Contains("Capítulo"))
                    continue;

                var numero = ParsearNumero(nombreTexto);
                var linkVerNodo = nodo.SelectSingleNode(".//a[contains(@href, '/view_uploads/')]");
                if (linkVerNodo == null) continue;

                string href = linkVerNodo.GetAttributeValue("href", "").Trim();
                if (string.IsNullOrWhiteSpace(href))
                    continue;

                var cap = new ChapterInfo
                {
                    Nombre = nombreTexto,
                    Numero = numero,
                    UrlVer = href,
                    UrlPagina = paginaManga
                };

                capitulos.Add(cap);
            }

            return capitulos.OrderBy(c => c.Numero).ToList();
        }

        private static double ParsearNumero(string texto)
        {
            try
            {
                var partes = texto.Split(' ');
                foreach (var parte in partes)
                {
                    if (parte.ToLower().Contains("capítulo") && partes.Length > 1)
                        continue;

                    if (double.TryParse(parte, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
                        return num;
                }
            }
            catch { }
            return -1;
        }

        public static async Task<string> ObtenerUrlFinalConReferer(string urlInicial, string referer)
        {
            _httpClient.DefaultRequestHeaders.Referrer = new Uri(referer);

            var request = new HttpRequestMessage(HttpMethod.Get, urlInicial);

            var response = await _httpClient.SendAsync(request);

            return response.RequestMessage.RequestUri.ToString();
        }
    }
}
