using RipMD.Helpers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace RipMD.Services
{
    public class DownloaderService
    {
        private readonly string _userAgent;
        private readonly string _descargasDir;
        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;

        public DownloaderService(string userAgent, string descargasDir, string cfClearance)
        {
            _userAgent = userAgent;
            _descargasDir = descargasDir;

            _cookieContainer = new CookieContainer();
            AddCookie("cf_clearance", cfClearance, ".zonatmo.com");
            AddCookie("cf_clearance", cfClearance, ".imgtmo.com");

            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = _cookieContainer,
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
            _client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            _client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
            _client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("es-ES,es;q=0.9");
            _client.DefaultRequestHeaders.Connection.Add("keep-alive");
            _client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            _client.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");
            _client.DefaultRequestHeaders.Referrer = new Uri("https://www.zonatmo.com/");
        }

        public void AddCookie(string name, string value, string domain)
        {
            _cookieContainer.Add(new Cookie(name, value, "/", domain));
        }

        public string GetOutputDirectory() => _descargasDir;

        public async Task DescargarYConvertirWebP(string url, string destinoSinExtension, string referer, CancellationToken cancellationToken)
        {
            string nombreBase = Path.GetFileName(destinoSinExtension);
            string tempWebP = Path.Combine(RutaTemporalHelper.CarpetaTemporal, nombreBase + ".webp");
            string rutaJPEG = destinoSinExtension + ".jpg";

            try
            {
                await DescargarConReintento(url, referer, tempWebP, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested(); // Comprobar cancelación después de la descarga
                WebPHelper.ConvertToJpeg(tempWebP, rutaJPEG);
            }
            finally
            {
                if (File.Exists(tempWebP))
                    File.Delete(tempWebP);
            }
        }

        public async Task DescargarArchivo(string url, string rutaDestino, string referer, CancellationToken cancellationToken)
        {
            string nombreArchivo = Path.GetFileName(rutaDestino);
            string tempPath = Path.Combine(RutaTemporalHelper.CarpetaTemporal, nombreArchivo);

            try
            {
                await DescargarConReintento(url, referer, tempPath, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested(); // Comprobar cancelación
                File.Move(tempPath, rutaDestino, true);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        private async Task DescargarConReintento(string url, string referer, string rutaDestino, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Referrer = new Uri(referer);

            using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                using (var fs = new FileStream(rutaDestino, FileMode.Create, FileAccess.Write))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }
        }

        public async Task<string> DescargarHtml(string url, CancellationToken cancellationToken)
        {
            var response = await _client.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new Exception($"Error {(int)response.StatusCode} - Probablemente Cloudflare bloqueó el acceso. Requiere actualizar cf_clearance.");
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // NUEVO: Este método ahora usa el HttpClient centralizado
        public async Task<string> ObtenerUrlFinalConReferer(string urlInicial, string referer, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, urlInicial);
            request.Headers.Referrer = new Uri(referer);

            var response = await _client.SendAsync(request, cancellationToken);
            return response.RequestMessage.RequestUri.ToString();
        }
    }
}