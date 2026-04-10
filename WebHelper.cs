using ImageMagick;
using System.IO;
using System;

namespace RipMD.Helpers
{
    public static class WebPHelper
    {
        public static void ConvertToJpeg(string inputWebP, string outputJpeg)
        {
            using var image = new MagickImage(inputWebP);
            image.Format = MagickFormat.Jpeg;
            image.Write(outputJpeg);
        }
    }

    public static class RutaTemporalHelper
    {
        private static readonly string CarpetaRaiz = Path.Combine(Path.GetTempPath(), "RipMD");

        // El constructor estático se asegura de que la carpeta exista la primera vez que se usa la clase.
        static RutaTemporalHelper()
        {
            if (!Directory.Exists(CarpetaRaiz))
                Directory.CreateDirectory(CarpetaRaiz);
        }

        public static string CrearRutaTemporal(string nombreArchivoConExtension)
        {
            // CORRECCIÓN: Nos aseguramos de que la carpeta exista CADA VEZ que pedimos una ruta.
            // Esto evita el error si se borró previamente.
            if (!Directory.Exists(CarpetaRaiz))
            {
                Directory.CreateDirectory(CarpetaRaiz);
            }
            return Path.Combine(CarpetaRaiz, nombreArchivoConExtension);
        }

        public static void LimpiarCarpetaTemporal()
        {
            try
            {
                if (Directory.Exists(CarpetaRaiz))
                    Directory.Delete(CarpetaRaiz, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al limpiar carpeta temporal: " + ex.Message);
            }
        }

        public static string CarpetaTemporal => CarpetaRaiz;
    }
}