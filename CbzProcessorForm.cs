using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RipMD
{
    public partial class CbzProcessorForm : Form
    {
        // Valores por defecto
        private const int DefaultWidth = 1072;
        private const int DefaultHeight = 1448;
        private const int DefaultQuality = 85;
        private const string OutputFolder = "CBZ_Procesados";

        private List<string> selectedFiles = new List<string>();

        public CbzProcessorForm()
        {
            InitializeComponent();
        }

        private void CbzProcessorForm_Load(object sender, EventArgs e)
        {
            txtWidth.Text = DefaultWidth.ToString();
            txtHeight.Text = DefaultHeight.ToString();
            txtQuality.Text = DefaultQuality.ToString();
        }

        private void btnSelectFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Archivos CBZ|*.cbz";
                openFileDialog.Multiselect = true;
                openFileDialog.Title = "Seleccionar archivos CBZ";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFiles.Clear();
                    selectedFiles.AddRange(openFileDialog.FileNames);

                    // Mostrar archivos seleccionados
                    listBoxFiles.Items.Clear();
                    foreach (var file in selectedFiles)
                    {
                        listBoxFiles.Items.Add(Path.GetFileName(file));
                    }

                    lblStatus.Text = $"{selectedFiles.Count} archivo(s) seleccionado(s)";
                }
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("Selecciona al menos un archivo CBZ", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtWidth.Text, out int maxWidth) ||
                !int.TryParse(txtHeight.Text, out int maxHeight) ||
                !int.TryParse(txtQuality.Text, out int quality))
            {
                MessageBox.Show("Por favor, introduce valores numéricos válidos", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Deshabilitar controles durante el procesamiento
            SetControlsEnabled(false);

            // Procesar en segundo plano
            backgroundWorker.RunWorkerAsync(new ProcessingParams
            {
                Files = selectedFiles.ToArray(),
                MaxWidth = maxWidth,
                MaxHeight = maxHeight,
                Quality = quality,
                ForceQuality = chkForceQuality.Checked
            });
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var parameters = (ProcessingParams)e.Argument;
            int processed = 0;

            foreach (var file in parameters.Files)
            {
                if (backgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    ProcessCBZ(file, parameters.MaxWidth, parameters.MaxHeight,
                              parameters.Quality, parameters.ForceQuality);

                    processed++;
                    int progress = (int)((processed / (double)parameters.Files.Length) * 100);
                    backgroundWorker.ReportProgress(progress, Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    backgroundWorker.ReportProgress(-1, $"Error procesando {Path.GetFileName(file)}: {ex.Message}");
                }
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage >= 0)
            {
                progressBar.Value = e.ProgressPercentage;
                lblStatus.Text = $"Procesando: {e.UserState}";
            }
            else
            {
                // Mostrar error
                MessageBox.Show(e.UserState.ToString(), "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetControlsEnabled(true);

            if (e.Cancelled)
            {
                lblStatus.Text = "Procesamiento cancelado";
            }
            else if (e.Error != null)
            {
                lblStatus.Text = $"Error: {e.Error.Message}";
                MessageBox.Show($"Error durante el procesamiento: {e.Error.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                lblStatus.Text = "Procesamiento completado";
                MessageBox.Show($"Se procesaron {selectedFiles.Count} archivos CBZ\n\n" +
                               $"Los archivos procesados se encuentran en:\n{Path.Combine(Directory.GetCurrentDirectory(), OutputFolder)}",
                    "Procesamiento completado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                progressBar.Value = 0;
            }
        }

        private void ProcessCBZ(string inputCbz, int maxWidth, int maxHeight, int quality, bool forceQuality)
        {
            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), OutputFolder);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string outputCbz = Path.Combine(outputDir, Path.GetFileName(inputCbz));

            using (ZipArchive inputArchive = ZipFile.OpenRead(inputCbz))
            using (FileStream outputStream = new FileStream(outputCbz, FileMode.Create))
            using (ZipArchive outputArchive = new ZipArchive(outputStream, ZipArchiveMode.Create))
            {
                foreach (ZipArchiveEntry entry in inputArchive.Entries)
                {
                    using (Stream entryStream = entry.Open())
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        entryStream.CopyTo(memoryStream);
                        byte[] data = memoryStream.ToArray();

                        string extension = Path.GetExtension(entry.Name).ToLower();
                        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                        {
                            data = ProcessImage(data, maxWidth, maxHeight, quality, forceQuality);
                        }

                        ZipArchiveEntry newEntry = outputArchive.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                        using (Stream newEntryStream = newEntry.Open())
                        {
                            newEntryStream.Write(data, 0, data.Length);
                        }
                    }
                }
            }
        }

        private byte[] ProcessImage(byte[] imgBytes, int maxWidth, int maxHeight, int quality, bool forceQuality)
        {
            using (MemoryStream inputStream = new MemoryStream(imgBytes))
            using (Image img = Image.FromStream(inputStream))
            {
                int width = img.Width;
                int height = img.Height;
                bool needsProcessing = false;

                // MODIFICACIÓN: Solo reducir 2px si está justo al máximo
                if (width == maxWidth && height == maxHeight)
                {
                    width -= 2;
                    height -= 2;
                    needsProcessing = true;
                }

                // Si no necesita procesamiento y no se fuerza la calidad, devolver los bytes originales
                if (!needsProcessing && !forceQuality)
                {
                    return imgBytes;
                }

                using (Bitmap processedImage = new Bitmap(width, height))
                using (Graphics graphics = Graphics.FromImage(processedImage))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(img, 0, 0, width, height);

                    // Convertir a JPEG con la calidad especificada
                    using (MemoryStream outputStream = new MemoryStream())
                    {
                        var encoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                        encoderParameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                            System.Drawing.Imaging.Encoder.Quality, quality);

                        var jpegCodec = GetEncoderInfo("image/jpeg");
                        processedImage.Save(outputStream, jpegCodec, encoderParameters);

                        return outputStream.ToArray();
                    }
                }
            }
        }

        private System.Drawing.Imaging.ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            foreach (System.Drawing.Imaging.ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == mimeType)
                    return codec;
            }
            return null;
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnSelectFiles.Enabled = enabled;
            btnProcess.Enabled = enabled;
            btnCancel.Enabled = !enabled;
            txtWidth.Enabled = enabled;
            txtHeight.Enabled = enabled;
            txtQuality.Enabled = enabled;
            chkForceQuality.Enabled = enabled;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy)
            {
                backgroundWorker.CancelAsync();
            }
        }

        private struct ProcessingParams
        {
            public string[] Files { get; set; }
            public int MaxWidth { get; set; }
            public int MaxHeight { get; set; }
            public int Quality { get; set; }
            public bool ForceQuality { get; set; }
        }
    }
}