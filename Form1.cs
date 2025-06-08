using Microsoft.Web.WebView2.WinForms; 
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RipMD.Extractors;
using RipMD.Helpers;
using RipMD.Parsers;
using RipMD.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace RipMD
{
    public partial class Form1 : Form
    {
        private const string userAgent = "Mozilla/5.0 (Nintendo Switch; WifiWebAuthApplet) AppleWebKit/606.4 (KHTML, like Gecko) NF/6.0.1.16.11 NintendoBrowser/5.1.0.20935";
        private DownloaderService downloader;
        private string cfClearance = "";
        private WebView2 webView2Zonatmo;

        public Form1()
        {
            InitializeComponent();

        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            await cfRun();
            RutaTemporalHelper.LimpiarCarpetaTemporal();


        }

        private async Task<bool> cfRun()
        {

            try
            {
                BtnDesCap.Enabled = false;
                BtnDesMan.Enabled = false;  
                BtnDesCola.Enabled = false;

                webView2Zonatmo = new WebView2
                {
                    Dock = DockStyle.Fill
                };
                PnlWeb.Controls.Add(webView2Zonatmo); // o agrégalo a un panel si prefieres

                string url = "https://www.zonatmo.com/";
                cfClearance = await CfClearanceHelper.ObtenerCfClearanceAsync(url);

                downloader = new DownloaderService(userAgent, "Descargas", cfClearance);

                // Esperar que WebView2 esté listo
                await webView2Zonatmo.EnsureCoreWebView2Async();

                // Crear la cookie cf_clearance para el dominio
                var cookie = webView2Zonatmo.CoreWebView2.CookieManager.CreateCookie(
                    "cf_clearance",
                    cfClearance,
                    ".zonatmo.com",
                    "/"
                );

                // (Opcional) configura la expiración si sabes cuánto dura
                cookie.Expires = DateTime.Now.AddHours(1);

                // Inyectar la cookie
                webView2Zonatmo.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);

                // Navegar al sitio con la cookie activa
                webView2Zonatmo.CoreWebView2.Navigate(url);

                BtnDesCap.Enabled = true;
                BtnDesMan.Enabled = true;
                BtnDesCola.Enabled = true;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener cf_clearance: " + ex.Message);
                return false;
            }
        }


        private void ActProgCap(int valor)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(ActProgCap), valor);
            }
            else
            {
                PbrCapitulo.Value = valor;
            }
        }

        private void ActCapDes(string valor)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(ActProgCap), valor);
            }
            else
            {
                LblCapDescar.Text = valor;
            }
        }

        private async void BtnDesCap_Click(object sender, EventArgs e)
        {
            string url = TxtDesCap.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Por favor, ingresa la URL del capítulo a descargar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (downloader == null)
            {
                MessageBox.Show("El servicio de descarga no está listo. Espera a que Selenium obtenga la cookie.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var extractor = new TmoExtractor(downloader);
                await extractor.DescargarCapitulo(url, ActProgCap, ActCapDes);

                MessageBox.Show("Capítulo descargado con éxito.");
                PbrCapitulo.Value = 0; // Reiniciar progreso
                LblCapDescar.Text = ""; // Limpiar etiqueta de capítulo descargado
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al extraer imágenes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnDesMan_Click(object sender, EventArgs e)
        {
            try
            {
                TxtMangas.Text = TxtMangas.Text.Trim();

                double? capInicio = null;
                double? capFinal = null;

                if (!string.IsNullOrWhiteSpace(TxtCapInicio.Text))
                {
                    if (!double.TryParse(TxtCapInicio.Text.Trim(), out double inicio))
                    {
                        MessageBox.Show("El capítulo de inicio no es un número válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    capInicio = inicio;
                }

                if (!string.IsNullOrWhiteSpace(TxtCapFinal.Text))
                {
                    if (!double.TryParse(TxtCapFinal.Text.Trim(), out double final))
                    {
                        MessageBox.Show("El capítulo final no es un número válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    capFinal = final;
                }

                // Verificación adicional: si ambos están presentes, que el inicio no sea mayor que el final
                if (capInicio.HasValue && capFinal.HasValue && capInicio > capFinal)
                {
                    MessageBox.Show("El capítulo de inicio no puede ser mayor que el capítulo final.", "Rango inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var mangaService = new MangaService(downloader);
                var capitulos = await mangaService.ObtenerCapitulos(TxtMangas.Text, capInicio, capFinal);

                if (capitulos.Count == 0)
                {
                    MessageBox.Show("No se encontraron capítulos en el rango especificado.", "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    /*
                    var resumen = string.Join("\n\n", capitulos
                        .Take(10)
                        .Select(c => $"Título: {c.Nombre}\nURL: {c.UrlVer}"));

                    MessageBox.Show($"Se encontraron {capitulos.Count} capítulos.\n\nPrimeros encontrados:\n\n{resumen}",
                        "Capítulos encontrados", MessageBoxButtons.OK, MessageBoxIcon.Information);*/
                    var extractor = new TmoExtractor(downloader);
                    PbrManga.Minimum = 0;
                    PbrManga.Maximum = capitulos.Count;
                    PbrManga.Value = 0;

                    foreach (var item in capitulos)
                    {
                        var stopwatch = Stopwatch.StartNew();
                        PbrManga.Value += 1;
                        string urlCapituloIndividual = await ChapterParser.ObtenerUrlFinalConReferer(item.UrlVer, item.UrlPagina);
                        await extractor.DescargarCapitulo(urlCapituloIndividual, ActProgCap, ActCapDes);
                        stopwatch.Stop();
                        if (stopwatch.Elapsed.TotalSeconds < 20)
                        {
                            int tiempoEspera = (int)(20 - stopwatch.Elapsed.TotalSeconds) * 1000; // Convertir a milisegundos
                            await Task.Delay(1000); // Esperar al menos 1 segundo entre descargas
                        }

                    }
                    MessageBox.Show($"Se descargaron {capitulos.Count} capítulos.", "Descarga completa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    PbrManga.Value = 0; // Reiniciar progreso
                    PbrCapitulo.Value = 0; // Reiniciar progreso del capítulo
                    LblCapDescar.Text = ""; // Limpiar etiqueta de capítulo descargado
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en Form1:\n{ex.Message}", "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // Lánzala de nuevo si quieres que la app cierre
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            RutaTemporalHelper.LimpiarCarpetaTemporal();
        }

        private async void BtnCookies_Click(object sender, EventArgs e)
        {
            PnlWeb.Controls.Clear(); // Limpiar el panel antes de agregar el WebView2
            bool acce = await cfRun();
            if (acce)
            {
                MessageBox.Show("Cookie cf_clearance obtenida correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No se pudo obtener la cookie cf_clearance.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnDesCola_Click(object sender, EventArgs e)
        {
            string[] lineas = TxtCola.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int tam = lineas.Length;
            PbrCola.Maximum = tam;

            foreach (string linea in lineas)
            {
                string entrada = linea.Trim();
                if (string.IsNullOrWhiteSpace(entrada))
                    continue;
                
                Funciones_Extras EntradaParser = new Funciones_Extras();

                // Usa el nuevo método modular que separaste para extraer la info
                var (url, capInicio, capFinal) = EntradaParser.ParsearEntrada(entrada);

                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    MessageBox.Show($"URL inválida:\n{entrada}", "Error en cola", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                var stopwatch = Stopwatch.StartNew();

                try
                {
                    if (url.Contains("/viewer/"))
                    {
                        var extractor = new TmoExtractor(downloader);
                        await extractor.DescargarCapitulo(url, ActProgCap, ActCapDes);
                    }
                    else if (url.Contains("/library/"))
                    {
                        var mangaService = new MangaService(downloader);

                        var capitulos = await mangaService.ObtenerCapitulos(url, capInicio, capFinal);
                        var extractor = new TmoExtractor(downloader);
                        PbrManga.Minimum = 0;
                        PbrManga.Maximum = capitulos.Count;
                        PbrManga.Value = 0;

                        if (capitulos.Count == 0)
                        {
                            MessageBox.Show($"No se encontraron capítulos para la URL:\n{url}", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        foreach (var item in capitulos)
                        {
                            PbrManga.Value += 1;
                            string urlCapitulo = await ChapterParser.ObtenerUrlFinalConReferer(item.UrlVer, item.UrlPagina);
                            await extractor.DescargarCapitulo(urlCapitulo, ActProgCap, ActCapDes);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"URL no reconocida:\n{url}", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error procesando esta entrada:\n\n{entrada}\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                stopwatch.Stop();
                if (stopwatch.Elapsed.TotalSeconds < 20)
                {
                    await Task.Delay(1000); // Espera mínima
                }

                PbrCola.Value += 1;
            }

            MessageBox.Show("Descarga en cola completada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            PbrCola.Value = 0;
            PbrCapitulo.Value = 0;
            PbrManga.Value = 0;
            LblCapDescar.Text = "";
        }



        private void BtnAbrirDes_Click(object sender, EventArgs e)
        {
            string rutaDescargas = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Descargas");

            if (!Directory.Exists(rutaDescargas))
            {
                var resultado = MessageBox.Show(
                    "La carpeta de descargas no existe. ¿Deseas crearla?",
                    "Carpeta no encontrada",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (resultado == DialogResult.Yes)
                {
                    Directory.CreateDirectory(rutaDescargas);
                    System.Diagnostics.Process.Start("explorer.exe", rutaDescargas);
                }
                // Si dicen que no, no se hace nada
            }
            else
            {
                System.Diagnostics.Process.Start("explorer.exe", rutaDescargas);
            }
        }

        private void BtnOrdenarCaps_Click(object sender, EventArgs e)
        {
            Funciones_Extras funcionesExtras = new Funciones_Extras();
            funcionesExtras.AgruparCarpetasPorNombreBase("Descargas");
        }
    }
}
