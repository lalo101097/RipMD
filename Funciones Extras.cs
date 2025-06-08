using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RipMD.Services
{
    internal class Funciones_Extras
    {
        public void AgruparCarpetasPorNombreBase(string carpetaBase)
        {
            try
            {
                if (!Directory.Exists(carpetaBase))
                {
                    MessageBox.Show("La carpeta especificada no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var carpetas = Directory.GetDirectories(carpetaBase);
                var grupos = new Dictionary<string, List<string>>();
                var regex = new Regex(@"(.+?)\s+Cap[ií]tulo", RegexOptions.IgnoreCase);

                foreach (var carpeta in carpetas)
                {
                    var nombre = Path.GetFileName(carpeta);
                    var match = regex.Match(nombre);
                    if (match.Success)
                    {
                        string baseNombre = match.Groups[1].Value.Trim();
                        if (!grupos.ContainsKey(baseNombre))
                            grupos[baseNombre] = new List<string>();
                        grupos[baseNombre].Add(carpeta);
                    }
                }

                int totalMovidas = 0;

                foreach (var grupo in grupos)
                {
                    string destinoGrupo = Path.Combine(carpetaBase, grupo.Key);
                    Directory.CreateDirectory(destinoGrupo);

                    foreach (var carpetaOriginal in grupo.Value)
                    {
                        string nombreCarpeta = Path.GetFileName(carpetaOriginal);
                        string nuevoDestino = Path.Combine(destinoGrupo, nombreCarpeta);

                        if (!Directory.Exists(nuevoDestino))
                        {
                            Directory.Move(carpetaOriginal, nuevoDestino);
                            totalMovidas++;
                        }
                    }
                }

                MessageBox.Show($"Se movieron {totalMovidas} carpetas en {grupos.Count} grupos.", "Agrupado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agrupar carpetas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public (string url, double? capInicio, double? capFinal) ParsearEntrada(string entrada)
        {
            string url = entrada.Trim();
            double? desde = null;
            double? hasta = null;

            // Soporta coma, espacio, y guion dentro de URL
            // Busca último espacio o coma válido para dividir entrada de URL y rango
            int espacioIdx = entrada.LastIndexOf(' ');
            int comaIdx = entrada.LastIndexOf(',');

            int sepIdx = Math.Max(espacioIdx, comaIdx);

            if (sepIdx > 0 && sepIdx < entrada.Length - 1)
            {
                string posibleUrl = entrada.Substring(0, sepIdx).Trim();
                string posibleRango = entrada.Substring(sepIdx + 1).Trim();

                string[] partes = posibleRango.Split('-');

                if (partes.Length == 2)
                {
                    if (double.TryParse(partes[0], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                        desde = d;
                    if (double.TryParse(partes[1], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out double h))
                        hasta = h;
                }
                else if (double.TryParse(posibleRango, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out double único))
                {
                    desde = único;
                    hasta = único;
                }

                url = posibleUrl;
            }

            // También soporta formato tipo: url,,5.5 o url,,2
            if (entrada.Contains(",,"))
            {
                var partes = entrada.Split(new[] { ",," }, StringSplitOptions.None);
                if (partes.Length == 2)
                {
                    url = partes[0].Trim();
                    if (double.TryParse(partes[1], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out double único))
                    {
                        desde = único;
                        hasta = único;
                    }
                }
            }

            return (url, desde, hasta);
        }

    }
}
