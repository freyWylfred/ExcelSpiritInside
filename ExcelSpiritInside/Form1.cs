using System.IO;
using System.Net.Http;
using System.Text;
using ClosedXML.Excel;
using LLama;
using LLama.Common;

namespace ExcelSpiritInside
{
    public partial class Form1 : Form
    {
        private static readonly string[] ModelUrls =
        {
            "https://huggingface.co/Qwen/Qwen3-4B-GGUF/resolve/main/Qwen3-4B-Q4_K_M.gguf",
            "https://huggingface.co/unsloth/Qwen3-4B-GGUF/resolve/main/Qwen3-4B-Q4_K_M.gguf",
            "https://huggingface.co/bartowski/Qwen_Qwen3-4B-GGUF/resolve/main/Qwen_Qwen3-4B-Q4_K_M.gguf"
        };
        private const string ModelFileName = "Qwen3-4B-Q4_K_M.gguf";

        public string ExcelPath1 => textBoxExcel1.Text;
        public string ExcelPath2 => textBoxExcel2.Text;

        public string ModelPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ExcelSpiritInside", "models", ModelFileName);

        public Form1()
        {
            InitializeComponent();
            ApplyModernStyle();
            Load += Form1_Load;
        }

        private void ApplyModernStyle()
        {
            var background = Color.FromArgb(245, 246, 250);
            var surface = Color.White;
            var text = Color.FromArgb(33, 37, 41);
            var accent = Color.FromArgb(0, 120, 212);
            var accentHover = Color.FromArgb(0, 99, 177);
            var secondary = Color.FromArgb(233, 236, 239);
            var secondaryText = Color.FromArgb(52, 58, 64);
            var baseFont = new Font("Segoe UI", 10F, FontStyle.Regular);

            Font = baseFont;
            BackColor = background;
            ForeColor = text;

            StyleControls(Controls, text, surface);

            labelStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            labelStatus.ForeColor = Color.FromArgb(108, 117, 125);

            textBoxResult.BackColor = Color.FromArgb(248, 249, 250);
            textBoxResult.Font = new Font("Consolas", 10F, FontStyle.Regular);

            StyleButton(buttonCompare, accent, accentHover, Color.White);
            StyleButton(buttonInfer, accent, accentHover, Color.White);
            StyleButton(buttonAsk, accent, accentHover, Color.White);
            StyleButton(buttonOk, accent, accentHover, Color.White);
            StyleButton(buttonBrowse1, secondary, Color.FromArgb(214, 219, 223), secondaryText);
            StyleButton(buttonBrowse2, secondary, Color.FromArgb(214, 219, 223), secondaryText);
        }

        private static void StyleControls(Control.ControlCollection controls, Color text, Color surface)
        {
            foreach (Control control in controls)
            {
                switch (control)
                {
                    case Label label:
                        label.ForeColor = text;
                        label.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
                        break;
                    case TextBox tb:
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        tb.BackColor = surface;
                        tb.ForeColor = text;
                        break;
                    case TableLayoutPanel:
                    case FlowLayoutPanel:
                        control.BackColor = Color.Transparent;
                        break;
                }

                if (control.HasChildren)
                {
                    StyleControls(control.Controls, text, surface);
                }
            }
        }

        private static void StyleButton(Button button, Color back, Color hover, Color fore)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = hover;
            button.FlatAppearance.MouseDownBackColor = hover;
            button.BackColor = back;
            button.ForeColor = fore;
            button.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            await EnsureModelAsync();
        }

        private bool isModelReady;
        private bool isBusy;

        private static bool IsValidModelFile(string path)
        {
            try
            {
                var info = new FileInfo(path);
                if (!info.Exists || info.Length < 100L * 1024 * 1024)
                {
                    return false;
                }

                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var magic = new byte[4];
                return fs.Read(magic, 0, 4) == 4
                    && magic[0] == (byte)'G' && magic[1] == (byte)'G' && magic[2] == (byte)'U' && magic[3] == (byte)'F';
            }
            catch
            {
                return false;
            }
        }

        private void SetBusy(bool busy, string status)
        {
            isBusy = busy;
            labelStatus.Text = status;
            UseWaitCursor = busy;

            if (busy)
            {
                progressBarDownload.Style = ProgressBarStyle.Marquee;
                progressBarDownload.MarqueeAnimationSpeed = 30;
            }
            else
            {
                progressBarDownload.Style = ProgressBarStyle.Blocks;
                progressBarDownload.MarqueeAnimationSpeed = 0;
                progressBarDownload.Value = isModelReady ? progressBarDownload.Maximum : 0;
            }

            buttonBrowse1.Enabled = !busy;
            buttonBrowse2.Enabled = !busy;
            buttonCompare.Enabled = !busy;
            buttonInfer.Enabled = !busy;
            buttonAsk.Enabled = !busy;
            buttonOk.Enabled = !busy;
            textBoxExcel1.Enabled = !busy;
            textBoxExcel2.Enabled = !busy;
            textBoxSheet.Enabled = !busy;
            textBoxColumn.Enabled = !busy;
            textBoxPrompt.Enabled = !busy;
            Application.DoEvents();
        }

        private async Task<bool> EnsureModelReadyForInferenceAsync()
        {
            if (isModelReady && IsValidModelFile(ModelPath))
            {
                return true;
            }

            var answer = MessageBox.Show(
                "The local model is not available yet. Download it now? (about 2.5 GB)",
                "Excel Spirit Inside", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes)
            {
                return false;
            }

            await EnsureModelAsync();
            return isModelReady;
        }

        private async Task EnsureModelAsync()
        {
            if (isBusy)
            {
                return;
            }

            var modelPath = ModelPath;
            if (IsValidModelFile(modelPath))
            {
                isModelReady = true;
                labelStatus.Text = "Model ready.";
                progressBarDownload.Style = ProgressBarStyle.Blocks;
                progressBarDownload.Value = progressBarDownload.Maximum;
                return;
            }

            if (File.Exists(modelPath))
            {
                try { File.Delete(modelPath); } catch { }
            }

            var directory = Path.GetDirectoryName(modelPath)!;
            Directory.CreateDirectory(directory);

            isModelReady = false;
            SetBusy(true, "Downloading model...");

            var tempPath = modelPath + ".part";
            var errors = new StringBuilder();
            try
            {
                using var client = new HttpClient();
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ExcelSpiritInside/1.1");

                bool downloaded = false;
                for (int i = 0; i < ModelUrls.Length && !downloaded; i++)
                {
                    var url = ModelUrls[i];
                    try
                    {
                        labelStatus.Text = $"Downloading model (source {i + 1}/{ModelUrls.Length})...";
                        await DownloadModelFileAsync(client, url, tempPath);
                        downloaded = true;
                    }
                    catch (Exception ex)
                    {
                        errors.AppendLine($"- {url}: {ex.Message}");
                        if (File.Exists(tempPath))
                        {
                            try { File.Delete(tempPath); } catch { }
                        }
                    }
                }

                if (!downloaded)
                {
                    throw new IOException("All download sources failed:\r\n" + errors);
                }

                File.Move(tempPath, modelPath, true);

                if (!IsValidModelFile(modelPath))
                {
                    try { File.Delete(modelPath); } catch { }
                    throw new InvalidDataException("Downloaded file is not a valid GGUF model.");
                }

                isModelReady = true;
                SetBusy(false, "Model ready.");
            }
            catch (Exception ex)
            {
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
                isModelReady = false;
                SetBusy(false, "Model download failed.");
                MessageBox.Show(
                    $"Failed to download model: {ex.Message}\r\n\r\nIf you are behind a proxy or firewall, allow access to huggingface.co, or manually place the file at:\r\n{modelPath}",
                    "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DownloadModelFileAsync(HttpClient client, string url, string tempPath)
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (mediaType.Contains("html", StringComparison.OrdinalIgnoreCase) || mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Server returned {mediaType} instead of a model file (possible proxy or rate limit page).");
            }

            var total = response.Content.Headers.ContentLength ?? -1L;
            if (total > 0)
            {
                progressBarDownload.Style = ProgressBarStyle.Blocks;
                progressBarDownload.MarqueeAnimationSpeed = 0;
                progressBarDownload.Value = 0;
            }

            using var httpStream = await response.Content.ReadAsStreamAsync();
            long read = 0;
            bool headerChecked = false;
            using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var buffer = new byte[81920];
                int count;
                while ((count = await httpStream.ReadAsync(buffer)) > 0)
                {
                    if (!headerChecked)
                    {
                        if (count < 4)
                        {
                            var extra = await httpStream.ReadAsync(buffer.AsMemory(count, buffer.Length - count));
                            count += extra;
                        }
                        headerChecked = true;
                        if (count < 4 || buffer[0] != (byte)'G' || buffer[1] != (byte)'G' || buffer[2] != (byte)'U' || buffer[3] != (byte)'F')
                        {
                            var preview = Encoding.ASCII.GetString(buffer, 0, Math.Min(count, 64)).Replace("\r", " ").Replace("\n", " ");
                            throw new InvalidDataException($"Response is not a GGUF file (starts with: \"{preview}\").");
                        }
                    }

                    await fileStream.WriteAsync(buffer.AsMemory(0, count));
                    read += count;
                    if (total > 0)
                    {
                        var percent = (int)(read * 100 / total);
                        progressBarDownload.Value = Math.Min(percent, progressBarDownload.Maximum);
                        labelStatus.Text = $"Downloading model... {read / (1024 * 1024)} MB / {total / (1024 * 1024)} MB";
                    }
                    else
                    {
                        labelStatus.Text = $"Downloading model... {read / (1024 * 1024)} MB";
                    }
                }
            }

            if (total > 0 && read != total)
            {
                throw new IOException($"Download incomplete ({read} of {total} bytes).");
            }
        }

        private void buttonBrowse1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxExcel1.Text = openFileDialog.FileName;
            }
        }

        private void buttonBrowse2_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxExcel2.Text = openFileDialog.FileName;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxExcel1.Text) || string.IsNullOrWhiteSpace(textBoxExcel2.Text))
            {
                MessageBox.Show("Please select both Excel files.", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private async void buttonCompare_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxExcel1.Text) || string.IsNullOrWhiteSpace(textBoxExcel2.Text))
            {
                MessageBox.Show("Please select both Excel files.", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(textBoxExcel1.Text) || !File.Exists(textBoxExcel2.Text))
            {
                MessageBox.Show("One or both Excel files do not exist.", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxSheet.Text))
            {
                MessageBox.Show("Please enter a sheet name.", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isBusy)
            {
                return;
            }

            try
            {
                SetBusy(true, "Comparing workbooks...");
                var path1 = textBoxExcel1.Text;
                var path2 = textBoxExcel2.Text;
                var sheet = textBoxSheet.Text;
                string message = string.Empty;
                var diffs = await Task.Run(() => CompareSheets(path1, path2, sheet, out message));
                SetBusy(false, diffs.Count == 0 ? "Comparison done." : $"Comparison done: {diffs.Count} difference(s).");

                if (diffs.Count == 0)
                {
                    textBoxResult.Text = message;
                    return;
                }

                using var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = "Diff.xlsx"
                };
                if (saveDialog.ShowDialog() != DialogResult.OK)
                {
                    textBoxResult.Text = message;
                    return;
                }

                SetBusy(true, "Writing diff workbook...");
                var output = saveDialog.FileName;
                await Task.Run(() => WriteDiffWorkbook(diffs, sheet, output));
                SetBusy(false, "Diff saved.");
                textBoxResult.Text = $"{message}\r\n\r\nDiff saved to: {output}";
            }
            catch (Exception ex)
            {
                SetBusy(false, "Comparison failed.");
                MessageBox.Show($"Comparison failed: {ex.Message}", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static XLWorkbook OpenWorkbookSafe(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found: {path}", path);
            }

            try
            {
                return new XLWorkbook(path);
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException or KeyNotFoundException or InvalidOperationException or NullReferenceException)
            {
                // ClosedXML fails on workbooks with broken relationships (images, comments, drawings)
                // or out-of-range style indices. Sanitize a temporary copy and retry.
            }

            var tempDir = Path.Combine(Path.GetTempPath(), "ExcelSpiritInside");
            Directory.CreateDirectory(tempDir);
            var tempPath = Path.Combine(tempDir, Guid.NewGuid().ToString("N") + ".xlsx");
            try
            {
                File.Copy(path, tempPath, true);
                WorkbookSanitizer.Sanitize(tempPath);

                using var ms = new MemoryStream(File.ReadAllBytes(tempPath));
                return new XLWorkbook(ms);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    $"Unable to open '{Path.GetFileName(path)}'. The workbook contains elements ClosedXML cannot read (e.g. broken image or comment links). " +
                    "Open it in Excel, use 'Save As' to create a new .xlsx, and try again.\r\nDetails: {ex.Message}", ex);
            }
            finally
            {
                try { File.Delete(tempPath); } catch { }
            }
        }

        private static List<(string Address, string Value1, string Value2)> CompareSheets(string path1, string path2, string sheetName, out string message)
        {
            var diffs = new List<(string, string, string)>();

            using var wb1 = OpenWorkbookSafe(path1);
            using var wb2 = OpenWorkbookSafe(path2);

            if (!wb1.TryGetWorksheet(sheetName, out var ws1))
            {
                message = $"Sheet '{sheetName}' not found in file 1.";
                return diffs;
            }
            if (!wb2.TryGetWorksheet(sheetName, out var ws2))
            {
                message = $"Sheet '{sheetName}' not found in file 2.";
                return diffs;
            }

            var range1 = ws1.RangeUsed();
            var range2 = ws2.RangeUsed();

            int rows = Math.Max(range1?.RowCount() ?? 0, range2?.RowCount() ?? 0);
            int cols = Math.Max(range1?.ColumnCount() ?? 0, range2?.ColumnCount() ?? 0);

            var sb = new StringBuilder();

            for (int r = 1; r <= rows; r++)
            {
                if (ws1.Row(r).IsHidden || ws2.Row(r).IsHidden)
                {
                    continue;
                }

                for (int c = 1; c <= cols; c++)
                {
                    if (ws1.Column(c).IsHidden || ws2.Column(c).IsHidden)
                    {
                        continue;
                    }

                    var v1 = GetCellString(ws1.Cell(r, c));
                    var v2 = GetCellString(ws2.Cell(r, c));
                    if (!string.Equals(v1, v2, StringComparison.Ordinal))
                    {
                        var address = ws1.Cell(r, c).Address.ToStringRelative();
                        diffs.Add((address, v1, v2));
                        sb.AppendLine($"{address}: \"{v1}\" -> \"{v2}\"");
                    }
                }
            }

            message = diffs.Count == 0
                ? "No differences found."
                : $"{diffs.Count} difference(s) found:\r\n\r\n{sb}";
            return diffs;
        }

        private async void buttonInfer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxExcel1.Text) || !File.Exists(textBoxExcel1.Text))
            {
                MessageBox.Show("Please select a valid Excel File 1.", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxSheet.Text))
            {
                MessageBox.Show("Please enter a sheet name.", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxColumn.Text))
            {
                MessageBox.Show("Please enter a column to infer (e.g. A).", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isBusy || !await EnsureModelReadyForInferenceAsync())
            {
                return;
            }

            string columnText;
            try
            {
                columnText = ReadColumn(textBoxExcel1.Text, textBoxSheet.Text, textBoxColumn.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read column: {ex.Message}", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(columnText))
            {
                textBoxResult.Text = "The specified column is empty.";
                return;
            }

            SetBusy(true, "Loading model and inferring... this may take a while.");
            textBoxResult.Text = "Working...";
            try
            {
                var prompt = $"Analyze the following Excel column '{textBoxColumn.Text.Trim()}' values and summarize their meaning:\r\n{columnText}\r\n\r\nAnswer:";
                textBoxResult.Text = await InferAsync(prompt);
                SetBusy(false, "Inference done.");
            }
            catch (Exception ex)
            {
                textBoxResult.Text = string.Empty;
                SetBusy(false, "Inference failed.");
                MessageBox.Show($"Inference failed: {ex.Message}", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonAsk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxPrompt.Text))
            {
                MessageBox.Show("Please enter an instruction.", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isBusy || !await EnsureModelReadyForInferenceAsync())
            {
                return;
            }

            SetBusy(true, "Loading model and thinking... this may take a while.");
            textBoxResult.Text = "Working...";
            try
            {
                var prompt = $"User: {textBoxPrompt.Text.Trim()}\r\nAssistant:";
                textBoxResult.Text = await InferAsync(prompt);
                SetBusy(false, "Done.");
            }
            catch (Exception ex)
            {
                textBoxResult.Text = string.Empty;
                SetBusy(false, "Failed.");
                MessageBox.Show($"Request failed: {ex.Message}", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string ReadColumn(string path, string sheetName, string column)
        {
            using var wb = OpenWorkbookSafe(path);
            if (!wb.TryGetWorksheet(sheetName, out var ws))
            {
                throw new InvalidOperationException($"Sheet '{sheetName}' not found.");
            }

            var used = ws.RangeUsed();
            int lastRow = used?.LastRow().RowNumber() ?? 0;

            var sb = new StringBuilder();
            for (int r = 1; r <= lastRow; r++)
            {
                var value = GetCellString(ws.Cell(r, column));
                if (!string.IsNullOrWhiteSpace(value))
                {
                    sb.AppendLine(value);
                }
            }
            return sb.ToString();
        }

        private static string GetCellString(IXLCell cell)
        {
            if (cell.IsEmpty())
            {
                return string.Empty;
            }

            try
            {
                return cell.Value.ToString() ?? string.Empty;
            }
            catch
            {
                try
                {
                    return cell.GetString();
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        private async Task<string> InferAsync(string prompt)
        {
            var modelPath = ModelPath;
            if (!IsValidModelFile(modelPath))
            {
                isModelReady = false;
                throw new FileNotFoundException("The model file is missing or corrupted. Please download it again.", modelPath);
            }

            return await Task.Run(async () =>
            {
                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = 4096
                };

                using var weights = LLamaWeights.LoadFromFile(parameters);
                using var context = weights.CreateContext(parameters);
                var executor = new InteractiveExecutor(context);

                var inferenceParams = new InferenceParams
                {
                    MaxTokens = 512,
                    AntiPrompts = new List<string> { "\nUser:" }
                };

                var sb = new StringBuilder();
                await foreach (var token in executor.InferAsync(prompt, inferenceParams))
                {
                    sb.Append(token);
                }
                return sb.ToString();
            });
        }

        private static void WriteDiffWorkbook(List<(string Address, string Value1, string Value2)> diffs, string sheetName, string outputPath)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Diff");

            ws.Cell(1, 1).Value = "Sheet";
            ws.Cell(1, 2).Value = "Cell";
            ws.Cell(1, 3).Value = "File 1";
            ws.Cell(1, 4).Value = "File 2";
            ws.Row(1).Style.Font.Bold = true;

            int row = 2;
            foreach (var (address, v1, v2) in diffs)
            {
                ws.Cell(row, 1).Value = sheetName;
                ws.Cell(row, 2).Value = address;
                ws.Cell(row, 3).Value = v1;
                ws.Cell(row, 4).Value = v2;
                row++;
            }

            ws.Columns().AdjustToContents();
            wb.SaveAs(outputPath);
        }
    }
}
