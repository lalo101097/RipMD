using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace RipMD.Helpers
{
    public static class CfClearanceHelper
    {
        public static async Task<string> ObtenerCfClearanceAsync(string url)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless"); // Sin ventana visible
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--window-size=1920,1080");

            // Aquí es la clave: configurar el Service para ocultar consola
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;  // Esto oculta la consola de ChromeDriver

            using var driver = new ChromeDriver(service, options);

            driver.Navigate().GoToUrl(url);

            for (int i = 0; i < 15; i++)
            {
                var cookies = driver.Manage().Cookies.AllCookies;
                var cf = cookies.FirstOrDefault(c => c.Name == "cf_clearance");
                if (cf != null)
                {
                    return cf.Value;
                }
                await Task.Delay(1000);
            }

            throw new Exception("No se pudo obtener cf_clearance tras esperar 15 segundos.");
        }
    }
}
