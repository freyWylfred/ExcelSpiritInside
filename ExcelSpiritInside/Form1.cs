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
        private const string ModelUrl = "https://huggingface.co/Qwen/Qwen3-4B-GGUF/resolve/main/Qwen3-4B-Q4_K_M.gguf";
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

        private async void Form1_Load(object sender, EventArgs e)
        {
            await EnsureModelAsync();
        }

        private async Task EnsureModelAsync()
        {
            var modelPath = ModelPath;
            if (File.Exists(modelPath))
            {
                labelStatus.Text = "Model ready.";
                progressBarDownload.Value = progressBarDownload.Maximum;
                return;
            }

            var directory = Path.GetDirectoryName(modelPath)!;
            Directory.CreateDirectory(directory);

            buttonOk.Enabled = false;
            labelStatus.Text = "Downloading model...";

            var tempPath = modelPath + ".part";
            try
            {
                using var client = new HttpClient();
                client.Timeout = Timeout.InfiniteTimeSpan;
                using var response = await client.GetAsync(ModelUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var total = response.Content.Headers.ContentLength ?? -1L;
                progressBarDownload.Style = total > 0 ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;

                using var httpStream = await response.Content.ReadAsStreamAsync();
                using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[81920];
                    long read = 0;
                    int count;
                    while ((count = await httpStream.ReadAsync(buffer)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, count));
                        read += count;
                        if (total > 0)
                        {
                            var percent = (int)(read * 100 / total);
                            progressBarDownload.Value = Math.Min(percent, progressBarDownload.Maximum);
                            labelStatus.Text = $"Downloading model... {read / (1024 * 1024)} MB / {total / (1024 * 1024)} MB";
                        }
                    }
                }

                File.Move(tempPath, modelPath, true);
                progressBarDownload.Style = ProgressBarStyle.Blocks;
                progressBarDownload.Value = progressBarDownload.Maximum;
                labelStatus.Text = "Model ready.";
            }
            catch (Exception ex)
            {
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
                progressBarDownload.Style = ProgressBarStyle.Blocks;
                progressBarDownload.Value = 0;
                labelStatus.Text = "Model download failed.";
                MessageBox.Show($"Failed to download model: {ex.Message}", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonOk.Enabled = true;
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

        private void buttonCompare_Click(object sender, EventArgs e)
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

            try
            {
                var diffs = CompareSheets(textBoxExcel1.Text, textBoxExcel2.Text, textBoxSheet.Text, out string message);

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

                WriteDiffWorkbook(diffs, textBoxSheet.Text, saveDialog.FileName);
                textBoxResult.Text = $"{message}\r\n\r\nDiff saved to: {saveDialog.FileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Comparison failed: {ex.Message}", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static List<(string Address, string Value1, string Value2)> CompareSheets(string path1, string path2, string sheetName, out string message)
        {
            var diffs = new List<(string, string, string)>();

            using var wb1 = new XLWorkbook(path1);
            using var wb2 = new XLWorkbook(path2);

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

                    var v1 = ws1.Cell(r, c).GetString();
                    var v2 = ws2.Cell(r, c).GetString();
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

            if (!File.Exists(ModelPath))
            {
                MessageBox.Show("Model is not downloaded yet.", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            buttonInfer.Enabled = false;
            labelStatus.Text = "Inferring...";
            try
            {
                var prompt = $"Analyze the following Excel column '{textBoxColumn.Text.Trim()}' values and summarize their meaning:\r\n{columnText}\r\n\r\nAnswer:";
                textBoxResult.Text = await InferAsync(prompt);
                labelStatus.Text = "Inference done.";
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Inference failed.";
                MessageBox.Show($"Inference failed: {ex.Message}", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonInfer.Enabled = true;
            }
        }

        private async void buttonAsk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxPrompt.Text))
            {
                MessageBox.Show("Please enter an instruction.", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(ModelPath))
            {
                MessageBox.Show("Model is not downloaded yet.", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            buttonAsk.Enabled = false;
            labelStatus.Text = "Thinking...";
            try
            {
                var prompt = $"User: {textBoxPrompt.Text.Trim()}\r\nAssistant:";
                textBoxResult.Text = await InferAsync(prompt);
                labelStatus.Text = "Done.";
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Failed.";
                MessageBox.Show($"Request failed: {ex.Message}", "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonAsk.Enabled = true;
            }
        }

        private static string ReadColumn(string path, string sheetName, string column)
        {
            using var wb = new XLWorkbook(path);
            if (!wb.TryGetWorksheet(sheetName, out var ws))
            {
                throw new InvalidOperationException($"Sheet '{sheetName}' not found.");
            }

            var used = ws.RangeUsed();
            int lastRow = used?.LastRow().RowNumber() ?? 0;

            var sb = new StringBuilder();
            for (int r = 1; r <= lastRow; r++)
            {
                var value = ws.Cell(r, column).GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    sb.AppendLine(value);
                }
            }
            return sb.ToString();
        }

        private async Task<string> InferAsync(string prompt)
        {
            return await Task.Run(async () =>
            {
                var parameters = new ModelParams(ModelPath)
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
