using ImageMagick;

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

        static RutaTemporalHelper()
        {
            if (!Directory.Exists(CarpetaRaiz))
                Directory.CreateDirectory(CarpetaRaiz);
        }

        public static string CrearRutaTemporal(string nombreArchivoConExtension)
        {
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
                // Si falla, no se rompe la app, solo lo ignoramos o lo puedes loguear si quieres.
                Console.WriteLine("Error al limpiar carpeta temporal: " + ex.Message);
            }
        }

        public static string CarpetaTemporal => CarpetaRaiz;
    }
}
