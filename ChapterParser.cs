using HtmlAgilityPack;
using RipMD.Models;
using RipMD.Services; // Añadido
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;      // Añadido
using System.Threading.Tasks;

namespace RipMD.Parsers
{
    public static class ChapterParser
    {
        // ELIMINADO: HttpClient estático, ahora usaremos el DownloaderService

        public static List<ChapterInfo> ObtenerCapitulosDesdeHtml(string html, string paginaManga)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var capitulos = new List<ChapterInfo>();
            var nodos = doc.DocumentNode.SelectNodes("//li[@class='list-group-item']");

            if (nodos == null) return capitulos;

            foreach (var nodo in nodos)
            {
                var nombreNodo = nodo.SelectSingleNode(".//ancestor::li[@data-index]//h4//a[contains(@class, 'btn-collapse')]");
                var nombreTexto = nombreNodo?.InnerText?.Trim() ?? "";

                if (!nombreTexto.Contains("Capítulo")) continue;

                var numero = ParsearNumero(nombreTexto);
                var linkVerNodo = nodo.SelectSingleNode(".//a[contains(@href, '/view_uploads/')]");
                if (linkVerNodo == null) continue;

                string href = linkVerNodo.GetAttributeValue("href", "").Trim();
                if (string.IsNullOrWhiteSpace(href)) continue;

                capitulos.Add(new ChapterInfo
                {
                    Nombre = nombreTexto,
                    Numero = numero,
                    UrlVer = href,
                    UrlPagina = paginaManga
                });
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
                    if (parte.ToLower().Contains("capítulo") && partes.Length > 1) continue;
                    if (double.TryParse(parte, NumberStyles.Float, CultureInfo.InvariantCulture, out double num)) return num;
                }
            }
            catch { }
            return -1;
        }

        // MODIFICADO: Ahora es un método de extensión para DownloaderService
        public static async Task<string> ObtenerUrlFinalConReferer(this DownloaderService downloader, string urlInicial, string referer, CancellationToken cancellationToken)
        {
            return await downloader.ObtenerUrlFinalConReferer(urlInicial, referer, cancellationToken);
        }
    }
}