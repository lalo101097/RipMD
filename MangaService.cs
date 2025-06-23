using RipMD.Models;
using RipMD.Parsers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RipMD.Services
{
    public class MangaService
    {
        private readonly DownloaderService _downloader;
        private readonly int _reintentosMax = 3;

        public MangaService(DownloaderService downloader)
        {
            _downloader = downloader;
        }

        public async Task<List<ChapterInfo>> ObtenerCapitulos(string url, double? desde, double? hasta)
        {
            int intento = 0;
            string html = null;

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentException("La URL proporcionada no es válida o no es absoluta.", nameof(url));

            while (intento < _reintentosMax)
            {
                try
                {
                    html = await _downloader.DescargarHtml(url);
                    var lista = await ChapterParser.ObtenerCapitulosDesdeHtml(html, url);

                    if (lista.Count == 0)
                        throw new Exception("No se encontraron capítulos. Probablemente la página no cargó correctamente.");

                    // Filtrar duplicados por número de capítulo
                    // Quedamos con el primero que aparece para cada número
                    lista = lista
                        .GroupBy(c => c.Numero)       // Agrupa por número (ajusta 'Numero' a tu propiedad real)
                        .Select(g => g.First())       // Toma el primer capítulo de cada grupo
                        .ToList();

                    return FiltrarPorRango(lista, desde, hasta);
                }
                catch (Exception ex)
                {
                    intento++;
                    if (intento >= _reintentosMax)
                        throw new Exception($"Error tras {intento} intentos:\n{ex.Message}");

                    await Task.Delay(20000); // Espera 20s antes de reintentar
                }
            }

            return new List<ChapterInfo>();
        }

        public async Task<List<MangaInfo>> ObtenerMangas(string url)
        {
            int intento = 0;
            string html = null;

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentException("La URL proporcionada no es válida o no es absoluta.", nameof(url));

            while (intento < _reintentosMax)
            {
                try
                {
                    html = await _downloader.DescargarHtml(url);
                    var lista = await MangaParser.ObtenerMangasDesdeHtml(html);

                    if (lista.Count == 0)
                        throw new Exception("No se encontraron mangas. Probablemente la página no cargó correctamente."); // ← ajusté texto

                    // ⚠️ Esta parte puede ser confusa:
                    // Estás agrupando por Título, pero el comentario dice "número de capítulo".
                    // Asegúrate que realmente quieres evitar títulos duplicados.
                    lista = lista
                        .GroupBy(c => c.Titulo?.Trim()) // mejor con Trim para evitar duplicados fantasmas
                        .Select(g => g.First())
                        .ToList();

                    return lista;
                }
                catch (Exception ex)
                {
                    intento++;
                    if (intento >= _reintentosMax)
                        throw new Exception($"Error tras {intento} intentos:\n{ex.Message}", ex); // ← conviene encadenar la excepción original

                    await Task.Delay(20000); // Espera 20 segundos antes de reintentar
                }
            }

            return new List<MangaInfo>(); // ← no deberías llegar aquí, pero está bien por seguridad
        }



        private List<ChapterInfo> FiltrarPorRango(List<ChapterInfo> capitulos, double? desde, double? hasta)
        {
            if (desde == null && hasta == null)
                return capitulos;

            return capitulos.FindAll(c =>
                (!desde.HasValue || c.Numero >= desde) &&
                (!hasta.HasValue || c.Numero <= hasta));
        }
    }
}
