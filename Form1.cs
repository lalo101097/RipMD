using Microsoft.Web.WebView2.WinForms;
using RipMD.Extractors;
using RipMD.Helpers;
using RipMD.Models;
using RipMD.Parsers;
using RipMD.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;

namespace RipMD
{
    public partial class Form1 : Form
    {
        private const string userAgent = "Mozilla/5.0 (Nintendo Switch; WifiWebAuthApplet) AppleWebKit/606.4 (KHTML, like Gecko) NF/6.0.1.16.11 NintendoBrowser/5.1.0.20935";
        private DownloaderService downloader;
        private string cfClearance = "";
        private WebView2 webView2Zonatmo;
        private CancellationTokenSource cts; // Para la cancelación

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await cfRun();
            // CORRECCIÓN: Se elimina la limpieza de la carpeta temporal al inicio.
            // Esta línea era la que causaba el error.
            // RutaTemporalHelper.LimpiarCarpetaTemporal(); 
        }

        private async Task<bool> cfRun()
        {
            try
            {
                SetUIState(true);
                BtnCookies.Enabled = false;

                webView2Zonatmo = new WebView2 { Dock = DockStyle.Fill };
                PnlWeb.Controls.Add(webView2Zonatmo);

                string url = "https://www.zonatmo.com/";
                cfClearance = await CfClearanceHelper.ObtenerCfClearanceAsync(url);
                downloader = new DownloaderService(userAgent, "Descargas", cfClearance);

                await webView2Zonatmo.EnsureCoreWebView2Async();
                var cookie = webView2Zonatmo.CoreWebView2.CookieManager.CreateCookie("cf_clearance", cfClearance, ".zonatmo.com", "/");
                cookie.Expires = DateTime.Now.AddHours(1);
                webView2Zonatmo.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
                webView2Zonatmo.CoreWebView2.Navigate(url);

                SetUIState(false);
                BtnCookies.Enabled = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener cf_clearance: " + ex.Message);
                SetUIState(false);
                BtnCookies.Enabled = true;
                return false;
            }
        }

        #region Actualización de UI
        private void ActProgCap(int valor)
        {
            if (InvokeRequired) Invoke(new Action<int>(ActProgCap), valor);
            else PbrCapitulo.Value = valor;
        }

        private void ActCapDes(string valor)
        {
            if (InvokeRequired) Invoke(new Action<string>(ActCapDes), valor);
            else LblCapDescar.Text = valor;
        }

        private void LogError(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(LogError), message);
            }
            else
            {
                TxtLogs.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            }
        }

        private void SetUIState(bool isRunning)
        {
            BtnDesCap.Enabled = !isRunning;
            BtnDesMan.Enabled = !isRunning;
            BtnDesCola.Enabled = !isRunning;
            BtnCookies.Enabled = !isRunning;
            BtnOrdenarCaps.Enabled = !isRunning;
            BtnAbrirDes.Enabled = !isRunning;
            BtnCancelar.Enabled = isRunning;
        }
        #endregion

        #region Lógica de Descarga Refactorizada
        private async Task DescargarCapituloUnico(string url)
        {
            var extractor = new TmoExtractor(downloader);
            await ReintentarSiFallaAsync(
                () => extractor.DescargarCapitulo(url, cts.Token, ActProgCap, ActCapDes)
            );
        }

        private async Task DescargarMangaAsync(string url, double? capInicio, double? capFinal)
        {
            var mangaService = new MangaService(downloader);
            var capitulos = await ReintentarSiFallaAsync(() => mangaService.ObtenerCapitulos(url, capInicio, capFinal, cts.Token));

            if (capitulos == null || capitulos.Count == 0)
            {
                LogError($"No se encontraron capítulos para la URL: {url}");
                return;
            }

            PbrManga.Minimum = 0;
            PbrManga.Maximum = capitulos.Count;
            PbrManga.Value = 0;

            var extractor = new TmoExtractor(downloader);
            foreach (var item in capitulos)
            {
                cts.Token.ThrowIfCancellationRequested();
                PbrManga.Value += 1;

                string urlCapitulo = await ReintentarSiFallaAsync(
                    () => downloader.ObtenerUrlFinalConReferer(item.UrlVer, item.UrlPagina, cts.Token)
                );

                if (urlCapitulo != null)
                {
                    await ReintentarSiFallaAsync(
                        () => extractor.DescargarCapitulo(urlCapitulo, cts.Token, ActProgCap, ActCapDes)
                    );
                }
            }
        }
        #endregion

        #region Event Handlers
        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
            LogError("Operación de descarga cancelada por el usuario.");
        }

        private async void BtnDesCap_Click(object sender, EventArgs e)
        {
            string url = TxtDesCap.Text.Trim();
            if (string.IsNullOrEmpty(url)) { MessageBox.Show("Ingrese URL del capítulo."); return; }

            cts = new CancellationTokenSource();
            SetUIState(true);

            try
            {
                await DescargarCapituloUnico(url);
                MessageBox.Show("Capítulo descargado con éxito.");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                LogError($"Error al descargar capítulo: {ex.Message}");
                MessageBox.Show($"Error al descargar capítulo: {ex.Message}", "Error");
            }
            finally
            {
                SetUIState(false);
                PbrCapitulo.Value = 0;
                LblCapDescar.Text = "";
            }
        }

        private async void BtnDesMan_Click(object sender, EventArgs e)
        {
            string url = TxtMangas.Text.Trim();
            if (string.IsNullOrEmpty(url)) { MessageBox.Show("Ingrese URL del manga."); return; }

            double? capInicio = null, capFinal = null;
            if (!string.IsNullOrWhiteSpace(TxtCapInicio.Text) && double.TryParse(TxtCapInicio.Text.Trim(), out double inicio)) capInicio = inicio;
            if (!string.IsNullOrWhiteSpace(TxtCapFinal.Text) && double.TryParse(TxtCapFinal.Text.Trim(), out double final)) capFinal = final;
            if (capInicio.HasValue && capFinal.HasValue && capInicio > capFinal) { MessageBox.Show("El capítulo de inicio no puede ser mayor que el final."); return; }

            cts = new CancellationTokenSource();
            SetUIState(true);

            try
            {
                await DescargarMangaAsync(url, capInicio, capFinal);
                MessageBox.Show("Descarga de manga completada.");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                LogError($"Error en la descarga de manga: {ex.Message}");
                MessageBox.Show($"Error en la descarga de manga: {ex.Message}", "Error");
            }
            finally
            {
                SetUIState(false);
                PbrManga.Value = 0;
                PbrCapitulo.Value = 0;
                LblCapDescar.Text = "";
            }
        }

        private async void BtnDesCola_Click(object sender, EventArgs e)
        {
            string[] lineas = TxtCola.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            PbrCola.Maximum = lineas.Length;
            PbrCola.Value = 0;
            TxtLogs.Clear();

            cts = new CancellationTokenSource();
            SetUIState(true);

            try
            {
                foreach (string linea in lineas)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    var parser = new Funciones_Extras();
                    var (url, capInicio, capFinal) = parser.ParsearEntrada(linea.Trim());

                    if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) { LogError($"URL inválida: {linea}"); continue; }

                    try
                    {
                        if (url.Contains("/viewer/")) await DescargarCapituloUnico(url);
                        else if (url.Contains("/library/")) await DescargarMangaAsync(url, capInicio, capFinal);
                        else if (url.Contains("/lists/"))
                        {
                            var mangaService = new MangaService(downloader);
                            var mangas = await ReintentarSiFallaAsync(() => mangaService.ObtenerMangas(url, cts.Token));
                            foreach (var manga in mangas)
                            {
                                cts.Token.ThrowIfCancellationRequested();
                                if (!manga.Url.Contains("/one_shot/"))
                                    await DescargarMangaAsync(manga.Url, null, null);
                            }
                        }
                        else LogError($"URL no reconocida: {url}");
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex) { LogError($"Error procesando '{linea}': {ex.Message}"); }

                    PbrCola.Value += 1;
                }
                MessageBox.Show("Descarga en cola completada.");
            }
            catch (OperationCanceledException) { }
            finally
            {
                SetUIState(false);
                PbrCola.Value = 0; PbrManga.Value = 0; PbrCapitulo.Value = 0; LblCapDescar.Text = "";
            }
        }

        private async void BtnCookies_Click(object sender, EventArgs e)
        {
            PnlWeb.Controls.Clear();
            if (await cfRun()) MessageBox.Show("Cookie cf_clearance obtenida correctamente.");
            else MessageBox.Show("No se pudo obtener la cookie cf_clearance.", "Error");
        }

        private void BtnAbrirDes_Click(object sender, EventArgs e)
        {
            string rutaDescargas = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Descargas");
            if (!Directory.Exists(rutaDescargas))
            {
                if (MessageBox.Show("La carpeta de descargas no existe. ¿Deseas crearla?", "Carpeta no encontrada", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Directory.CreateDirectory(rutaDescargas);
                }
                else return;
            }
            Process.Start("explorer.exe", rutaDescargas);
        }

        private void BtnOrdenarCaps_Click(object sender, EventArgs e)
        {
            var funcionesExtras = new Funciones_Extras();
            funcionesExtras.AgruparCarpetasPorNombreBase("Descargas");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            RutaTemporalHelper.LimpiarCarpetaTemporal();
        }
        #endregion

        #region Lógica de Reintentos
        private async Task ReintentarSiFallaAsync(Func<Task> accion)
        {
            while (true)
            {
                try
                {
                    await accion();
                    break;
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                {
                    cts.Token.ThrowIfCancellationRequested();
                    int delaySeconds = (int)NumReintentosDelay.Value;
                    LogError($"Error 429. Reintentando en {delaySeconds} segundos...");
                    await Task.Delay(delaySeconds * 1000, cts.Token);
                }
            }
        }

        private async Task<T> ReintentarSiFallaAsync<T>(Func<Task<T>> funcion)
        {
            while (true)
            {
                try
                {
                    return await funcion();
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                {
                    cts.Token.ThrowIfCancellationRequested();
                    int delaySeconds = (int)NumReintentosDelay.Value;
                    LogError($"Error 429. Reintentando en {delaySeconds} segundos...");
                    await Task.Delay(delaySeconds * 1000, cts.Token);
                }
            }
        }
        #endregion
    }
}