using RipMD.Helpers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace RipMD.Services
{
    public class DownloaderService
    {
        private readonly string _userAgent;
        private readonly string _descargasDir;
        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;
        public HttpClient GetHttpClient() => _client;

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

        public async Task DescargarYConvertirWebP(string url, string destinoSinExtension, string referer)
        {
            string nombreBase = Path.GetFileName(destinoSinExtension);
            string tempWebP = RutaTemporalHelper.CrearRutaTemporal(nombreBase + ".webp");
            string rutaJPEG = destinoSinExtension + ".jpg";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Referrer = new Uri(referer);

                using (var response = await _client.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        string detalle = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException(
                            $"Error HTTP {response.StatusCode} ({(int)response.StatusCode}) al descargar '{url}'.\nContenido:\n{detalle.Substring(0, Math.Min(500, detalle.Length))}"
                        );
                    }

                    using (var fs = new FileStream(tempWebP, FileMode.Create, FileAccess.Write))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }

                WebPHelper.ConvertToJpeg(tempWebP, rutaJPEG);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Descarga fallida:\n{ex.Message}", "Error HTTP", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error general:\n{ex.Message}", "Error desconocido", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (File.Exists(tempWebP))
                    File.Delete(tempWebP);
            }
        }

        public async Task DescargarArchivo(string url, string rutaDestino, string referer)
        {
            string nombreArchivo = Path.GetFileName(rutaDestino);
            string tempPath = RutaTemporalHelper.CrearRutaTemporal(nombreArchivo);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Referrer = new Uri(referer);

                using (var response = await _client.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        string detalle = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException(
                            $"Error HTTP {response.StatusCode} ({(int)response.StatusCode}) al descargar '{url}'.\nContenido:\n{detalle.Substring(0, Math.Min(500, detalle.Length))}"
                        );
                    }

                    using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                    {
                        await response.Content.CopyToAsync(fs);
                    }

                    File.Move(tempPath, rutaDestino, overwrite: true);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Descarga fallida:\n{ex.Message}", "Error HTTP", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error general:\n{ex.Message}", "Error desconocido", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath); // Por si hubo error antes de mover
            }
        }

        public async Task<string> DescargarHtml(string url)
        {
            var response = await _client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new Exception($"Error {response.StatusCode} - Probablemente Cloudflare bloqueó el acceso. Requiere actualizar cf_clearance.");
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> ObtenerUrlFinalConReferer(string urlInicial, string referer)
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true, // sigue redirecciones automáticamente
                MaxAutomaticRedirections = 10
            };

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Referrer = new Uri(referer);

                // Solo hacemos una petición HEAD para evitar descargar el contenido completo (opcional)
                var request = new HttpRequestMessage(HttpMethod.Get, urlInicial);

                var response = await client.SendAsync(request);

                // La URL final será la del response.RequestMessage.RequestUri tras redirecciones
                return response.RequestMessage.RequestUri.ToString();
            }
        }

        public static class RutaTemporalHelper
        {
            private static readonly string CarpetaRaiz = Path.Combine(Path.GetTempPath(), "RipMD");

            static RutaTemporalHelper()
            {
                if (!Directory.Exists(CarpetaRaiz))
                    Directory.CreateDirectory(CarpetaRaiz);
            }

            public static string CrearRutaTemporal(string nombreArchivoConExtension)
            {
                return Path.Combine(CarpetaRaiz, nombreArchivoConExtension);
            }

            public static string CarpetaTemporal => CarpetaRaiz;
        }
    }
}
