namespace ExcelSpiritInside
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel layoutRoot;
        private FlowLayoutPanel panelOptions;
        private FlowLayoutPanel panelActions;
        private FlowLayoutPanel panelFooter;
        private Label labelExcel1;
        private TextBox textBoxExcel1;
        private Button buttonBrowse1;
        private Label labelExcel2;
        private TextBox textBoxExcel2;
        private Button buttonBrowse2;
        private Button buttonOk;
        private OpenFileDialog openFileDialog;
        private ProgressBar progressBarDownload;
        private Label labelStatus;
        private Label labelSheet;
        private TextBox textBoxSheet;
        private Button buttonCompare;
        private TextBox textBoxResult;
        private Label labelColumn;
        private TextBox textBoxColumn;
        private Button buttonInfer;
        private Label labelPrompt;
        private TextBox textBoxPrompt;
        private Button buttonAsk;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            layoutRoot = new TableLayoutPanel();
            panelOptions = new FlowLayoutPanel();
            panelActions = new FlowLayoutPanel();
            panelFooter = new FlowLayoutPanel();
            labelExcel1 = new Label();
            textBoxExcel1 = new TextBox();
            buttonBrowse1 = new Button();
            labelExcel2 = new Label();
            textBoxExcel2 = new TextBox();
            buttonBrowse2 = new Button();
            buttonOk = new Button();
            openFileDialog = new OpenFileDialog();
            progressBarDownload = new ProgressBar();
            labelStatus = new Label();
            labelSheet = new Label();
            textBoxSheet = new TextBox();
            buttonCompare = new Button();
            textBoxResult = new TextBox();
            labelColumn = new Label();
            textBoxColumn = new TextBox();
            buttonInfer = new Button();
            labelPrompt = new Label();
            textBoxPrompt = new TextBox();
            buttonAsk = new Button();
            layoutRoot.SuspendLayout();
            panelOptions.SuspendLayout();
            panelActions.SuspendLayout();
            panelFooter.SuspendLayout();
            SuspendLayout();
            // 
            // layoutRoot
            // 
            layoutRoot.ColumnCount = 3;
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layoutRoot.Dock = DockStyle.Fill;
            layoutRoot.Name = "layoutRoot";
            layoutRoot.Padding = new Padding(16);
            layoutRoot.RowCount = 8;
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutRoot.Controls.Add(labelExcel1, 0, 0);
            layoutRoot.Controls.Add(textBoxExcel1, 1, 0);
            layoutRoot.Controls.Add(buttonBrowse1, 2, 0);
            layoutRoot.Controls.Add(labelExcel2, 0, 1);
            layoutRoot.Controls.Add(textBoxExcel2, 1, 1);
            layoutRoot.Controls.Add(buttonBrowse2, 2, 1);
            layoutRoot.Controls.Add(progressBarDownload, 0, 2);
            layoutRoot.Controls.Add(labelStatus, 0, 3);
            layoutRoot.Controls.Add(panelOptions, 0, 4);
            layoutRoot.Controls.Add(panelActions, 2, 4);
            layoutRoot.Controls.Add(labelPrompt, 0, 5);
            layoutRoot.Controls.Add(textBoxPrompt, 1, 5);
            layoutRoot.Controls.Add(buttonAsk, 2, 5);
            layoutRoot.Controls.Add(textBoxResult, 0, 6);
            layoutRoot.Controls.Add(panelFooter, 0, 7);
            layoutRoot.SetColumnSpan(progressBarDownload, 3);
            layoutRoot.SetColumnSpan(labelStatus, 3);
            layoutRoot.SetColumnSpan(panelOptions, 2);
            layoutRoot.SetColumnSpan(textBoxResult, 3);
            layoutRoot.SetColumnSpan(panelFooter, 3);
            // 
            // labelExcel1
            // 
            labelExcel1.Anchor = AnchorStyles.Left;
            labelExcel1.AutoSize = true;
            labelExcel1.Margin = new Padding(0, 0, 12, 8);
            labelExcel1.Name = "labelExcel1";
            labelExcel1.Text = "Excel File 1";
            // 
            // textBoxExcel1
            // 
            textBoxExcel1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBoxExcel1.Margin = new Padding(0, 0, 12, 8);
            textBoxExcel1.Name = "textBoxExcel1";
            // 
            // buttonBrowse1
            // 
            buttonBrowse1.AutoSize = true;
            buttonBrowse1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonBrowse1.Margin = new Padding(0, 0, 0, 8);
            buttonBrowse1.MinimumSize = new Size(110, 32);
            buttonBrowse1.Name = "buttonBrowse1";
            buttonBrowse1.Padding = new Padding(12, 4, 12, 4);
            buttonBrowse1.Text = "Browse...";
            buttonBrowse1.Click += buttonBrowse1_Click;
            // 
            // labelExcel2
            // 
            labelExcel2.Anchor = AnchorStyles.Left;
            labelExcel2.AutoSize = true;
            labelExcel2.Margin = new Padding(0, 0, 12, 8);
            labelExcel2.Name = "labelExcel2";
            labelExcel2.Text = "Excel File 2";
            // 
            // textBoxExcel2
            // 
            textBoxExcel2.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBoxExcel2.Margin = new Padding(0, 0, 12, 8);
            textBoxExcel2.Name = "textBoxExcel2";
            // 
            // buttonBrowse2
            // 
            buttonBrowse2.AutoSize = true;
            buttonBrowse2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonBrowse2.Margin = new Padding(0, 0, 0, 8);
            buttonBrowse2.MinimumSize = new Size(110, 32);
            buttonBrowse2.Name = "buttonBrowse2";
            buttonBrowse2.Padding = new Padding(12, 4, 12, 4);
            buttonBrowse2.Text = "Browse...";
            buttonBrowse2.Click += buttonBrowse2_Click;
            // 
            // progressBarDownload
            // 
            progressBarDownload.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            progressBarDownload.Height = 8;
            progressBarDownload.Margin = new Padding(0, 4, 0, 4);
            progressBarDownload.Name = "progressBarDownload";
            progressBarDownload.Style = ProgressBarStyle.Continuous;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Margin = new Padding(0, 0, 0, 12);
            labelStatus.Name = "labelStatus";
            // 
            // panelOptions
            // 
            panelOptions.Anchor = AnchorStyles.Left;
            panelOptions.AutoSize = true;
            panelOptions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelOptions.Margin = new Padding(0, 0, 12, 8);
            panelOptions.Name = "panelOptions";
            panelOptions.WrapContents = false;
            panelOptions.Controls.Add(labelSheet);
            panelOptions.Controls.Add(textBoxSheet);
            panelOptions.Controls.Add(labelColumn);
            panelOptions.Controls.Add(textBoxColumn);
            // 
            // labelSheet
            // 
            labelSheet.Anchor = AnchorStyles.Left;
            labelSheet.AutoSize = true;
            labelSheet.Margin = new Padding(0, 0, 8, 0);
            labelSheet.Name = "labelSheet";
            labelSheet.Text = "Sheet Name";
            // 
            // textBoxSheet
            // 
            textBoxSheet.Anchor = AnchorStyles.Left;
            textBoxSheet.Margin = new Padding(0, 0, 24, 0);
            textBoxSheet.Name = "textBoxSheet";
            textBoxSheet.Width = 220;
            // 
            // labelColumn
            // 
            labelColumn.Anchor = AnchorStyles.Left;
            labelColumn.AutoSize = true;
            labelColumn.Margin = new Padding(0, 0, 8, 0);
            labelColumn.Name = "labelColumn";
            labelColumn.Text = "Infer Column";
            // 
            // textBoxColumn
            // 
            textBoxColumn.Anchor = AnchorStyles.Left;
            textBoxColumn.Margin = new Padding(0);
            textBoxColumn.Name = "textBoxColumn";
            textBoxColumn.Width = 80;
            // 
            // panelActions
            // 
            panelActions.Anchor = AnchorStyles.Right;
            panelActions.AutoSize = true;
            panelActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelActions.Margin = new Padding(0, 0, 0, 8);
            panelActions.Name = "panelActions";
            panelActions.WrapContents = false;
            panelActions.Controls.Add(buttonCompare);
            panelActions.Controls.Add(buttonInfer);
            // 
            // buttonCompare
            // 
            buttonCompare.AutoSize = true;
            buttonCompare.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonCompare.Margin = new Padding(0, 0, 8, 0);
            buttonCompare.MinimumSize = new Size(110, 32);
            buttonCompare.Name = "buttonCompare";
            buttonCompare.Padding = new Padding(12, 4, 12, 4);
            buttonCompare.Text = "Compare";
            buttonCompare.Click += buttonCompare_Click;
            // 
            // buttonInfer
            // 
            buttonInfer.AutoSize = true;
            buttonInfer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonInfer.Margin = new Padding(0);
            buttonInfer.MinimumSize = new Size(110, 32);
            buttonInfer.Name = "buttonInfer";
            buttonInfer.Padding = new Padding(12, 4, 12, 4);
            buttonInfer.Text = "Infer";
            buttonInfer.Click += buttonInfer_Click;
            // 
            // labelPrompt
            // 
            labelPrompt.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            labelPrompt.AutoSize = true;
            labelPrompt.Margin = new Padding(0, 6, 12, 8);
            labelPrompt.Name = "labelPrompt";
            labelPrompt.Text = "Instruction";
            // 
            // textBoxPrompt
            // 
            textBoxPrompt.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBoxPrompt.Height = 64;
            textBoxPrompt.Margin = new Padding(0, 0, 12, 8);
            textBoxPrompt.Multiline = true;
            textBoxPrompt.Name = "textBoxPrompt";
            textBoxPrompt.ScrollBars = ScrollBars.Vertical;
            // 
            // buttonAsk
            // 
            buttonAsk.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            buttonAsk.AutoSize = true;
            buttonAsk.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonAsk.Margin = new Padding(0, 0, 0, 8);
            buttonAsk.MinimumSize = new Size(110, 32);
            buttonAsk.Name = "buttonAsk";
            buttonAsk.Padding = new Padding(12, 4, 12, 4);
            buttonAsk.Text = "Ask";
            buttonAsk.Click += buttonAsk_Click;
            // 
            // textBoxResult
            // 
            textBoxResult.Dock = DockStyle.Fill;
            textBoxResult.Margin = new Padding(0, 4, 0, 12);
            textBoxResult.Multiline = true;
            textBoxResult.Name = "textBoxResult";
            textBoxResult.ReadOnly = true;
            textBoxResult.ScrollBars = ScrollBars.Both;
            textBoxResult.WordWrap = false;
            // 
            // panelFooter
            // 
            panelFooter.Anchor = AnchorStyles.Right;
            panelFooter.AutoSize = true;
            panelFooter.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelFooter.FlowDirection = FlowDirection.RightToLeft;
            panelFooter.Margin = new Padding(0);
            panelFooter.Name = "panelFooter";
            panelFooter.WrapContents = false;
            panelFooter.Controls.Add(buttonOk);
            // 
            // buttonOk
            // 
            buttonOk.AutoSize = true;
            buttonOk.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonOk.Margin = new Padding(0);
            buttonOk.MinimumSize = new Size(110, 32);
            buttonOk.Name = "buttonOk";
            buttonOk.Padding = new Padding(12, 4, 12, 4);
            buttonOk.Text = "OK";
            buttonOk.Click += buttonOk_Click;
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|All Files (*.*)|*.*";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(960, 640);
            Controls.Add(layoutRoot);
            MinimumSize = new Size(760, 520);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Excel Spirit Inside";
            layoutRoot.ResumeLayout(false);
            layoutRoot.PerformLayout();
            panelOptions.ResumeLayout(false);
            panelOptions.PerformLayout();
            panelActions.ResumeLayout(false);
            panelActions.PerformLayout();
            panelFooter.ResumeLayout(false);
            panelFooter.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}
