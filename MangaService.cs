using RipMD.Models;
using RipMD.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RipMD.Services
{
    public class MangaService
    {
        private readonly DownloaderService _downloader;

        public MangaService(DownloaderService downloader)
        {
            _downloader = downloader;
        }

        public async Task<List<ChapterInfo>> ObtenerCapitulos(string url, double? desde, double? hasta, CancellationToken cancellationToken)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentException("La URL proporcionada no es válida.", nameof(url));

            var html = await _downloader.DescargarHtml(url, cancellationToken);
            var lista = ChapterParser.ObtenerCapitulosDesdeHtml(html, url);

            if (lista.Count == 0)
                throw new Exception("No se encontraron capítulos. La página podría no haber cargado correctamente.");

            lista = lista
                .GroupBy(c => c.Numero)
                .Select(g => g.First())
                .ToList();

            return FiltrarPorRango(lista, desde, hasta);
        }

        public async Task<List<MangaInfo>> ObtenerMangas(string url, CancellationToken cancellationToken)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentException("La URL proporcionada no es válida.", nameof(url));

            var html = await _downloader.DescargarHtml(url, cancellationToken);
            var lista = MangaParser.ObtenerMangasDesdeHtml(html);

            if (lista.Count == 0)
                throw new Exception("No se encontraron mangas. La página podría no haber cargado correctamente.");

            lista = lista
                .GroupBy(c => c.Titulo?.Trim())
                .Select(g => g.First())
                .ToList();

            return lista;
        }

        private List<ChapterInfo> FiltrarPorRango(List<ChapterInfo> capitulos, double? desde, double? hasta)
        {
            if (desde == null && hasta == null) return capitulos;
            return capitulos.FindAll(c => (!desde.HasValue || c.Numero >= desde) && (!hasta.HasValue || c.Numero <= hasta));
        }
    }
}