namespace SpamDetectorApp.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    // ── Controls ───────────────────────────────────────────────────────────
    private TabControl tabControl;
    private TabPage tabDetect;
    private TabPage tabHistory;
    private TabPage tabModelInfo;

    // Detect tab
    private Panel pnlTop;
    private Label lblAppTitle;
    private Label lblSubtitle;
    private Panel pnlInput;
    private Label lblInputHeader;
    private Label lblInputHint;
    private RichTextBox rtbMessage;
    private Label lblCharCount;
    private FlowLayoutPanel pnlButtons;
    private Button btnAnalyse;
    private Button btnClear;
    private Button btnPasteSample;
    private Panel pnlResult;
    private Panel pnlResultBadge;
    private Label lblResultIcon;
    private Label lblResultLabel;
    private Label lblResultSummary;
    private TableLayoutPanel tblMetrics;
    private Label lblConfidenceTitle;
    private Label lblConfidenceValue;
    private Panel pnlProgressTrack;
    private Panel pnlProgressFill;
    private Label lblSpamProbTitle;
    private Label lblSpamProbValue;
    private Label lblScoreTitle;
    private Label lblScoreValue;
    private RichTextBox rtbAnalysedMsg;
    private Label lblStatus;
    private ProgressBar progressBar;

    // History tab
    private DataGridView dgvHistory;
    private Button btnClearHistory;
    private Label lblHistoryCount;

    // Model Info tab
    private TableLayoutPanel tblModelInfo;
    private Label lblModelStatus;
    private Button btnRetrain;
    private Label lblTrainStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();

        // ── Form ──────────────────────────────────────────────────────────
        this.Text = "SMS Spam Detector AI  –  Powered by ML.NET";
        this.Size = new Size(900, 760);
        this.MinimumSize = new Size(820, 680);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(18, 18, 30);
        this.Font = new Font("Segoe UI", 9.5f);
        this.FormBorderStyle = FormBorderStyle.Sizable;

        // ── TAB CONTROL ───────────────────────────────────────────────────
        tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10f, FontStyle.Regular),
            Padding = new Point(16, 6),
            Appearance = TabAppearance.Normal
        };

        tabDetect = new TabPage("  🔍  Detect Spam  ");
        tabHistory = new TabPage("  📋  History  ");
        tabModelInfo = new TabPage("  🧠  Model Info  ");

        foreach (var tab in new[] { tabDetect, tabHistory, tabModelInfo })
        {
            tab.BackColor = Color.FromArgb(22, 22, 36);
            tab.ForeColor = Color.White;
            tab.Padding = new Padding(12);
        }

        tabControl.Controls.AddRange(new Control[] { tabDetect, tabHistory, tabModelInfo });

        // ── STATUS BAR ────────────────────────────────────────────────────
        progressBar = new ProgressBar
        {
            Dock = DockStyle.Bottom,
            Height = 4,
            Style = ProgressBarStyle.Marquee,
            Visible = false
        };

        lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 28,
            BackColor = Color.FromArgb(12, 12, 20),
            ForeColor = Color.FromArgb(120, 140, 200),
            Text = "  ⏳ Initialising model...",
            Font = new Font("Segoe UI", 9f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0)
        };

        this.Controls.Add(tabControl);
        this.Controls.Add(lblStatus);
        this.Controls.Add(progressBar);

        BuildDetectTab();
        BuildHistoryTab();
        BuildModelInfoTab();

        this.ResumeLayout(false);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  DETECT TAB
    // ─────────────────────────────────────────────────────────────────────
    private void BuildDetectTab()
    {
        var outer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));   // header
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 230));  // input
        outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // result

        // ── App header ────────────────────────────────────────────────────
        pnlTop = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(26, 32, 58),
            Padding = new Padding(20, 14, 0, 0)
        };

        lblAppTitle = new Label
        {
            Text = "🛡️  SMS Spam Detector AI",
            Font = new Font("Segoe UI", 17f, FontStyle.Bold),
            ForeColor = Color.FromArgb(120, 180, 255),
            AutoSize = true,
            Location = new Point(20, 12)
        };

        lblSubtitle = new Label
        {
            Text = "Powered by ML.NET  •  FastTree Binary Classifier  •  TF-IDF Features",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(100, 120, 180),
            AutoSize = true,
            Location = new Point(22, 46)
        };

        pnlTop.Controls.Add(lblAppTitle);
        pnlTop.Controls.Add(lblSubtitle);

        // ── Input panel ───────────────────────────────────────────────────
        pnlInput = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 28, 44),
            Padding = new Padding(20, 12, 20, 8)
        };

        lblInputHeader = new Label
        {
            Text = "📨  Enter SMS Message",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 12)
        };

        lblInputHint = new Label
        {
            Text = "Paste or type any SMS message below and click Analyse",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(120, 140, 180),
            AutoSize = true,
            Location = new Point(22, 36)
        };

        rtbMessage = new RichTextBox
        {
            Location = new Point(20, 60),
            Size = new Size(820, 90),
            Font = new Font("Segoe UI", 10.5f),
            BackColor = Color.FromArgb(36, 36, 56),
            ForeColor = Color.FromArgb(220, 230, 255),
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            MaxLength = 2000
        };

        lblCharCount = new Label
        {
            Text = "0 / 2000",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(90, 110, 160),
            AutoSize = true,
            Location = new Point(20, 154)
        };

        pnlButtons = new FlowLayoutPanel
        {
            Location = new Point(20, 172),
            Size = new Size(820, 44),
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        btnAnalyse = CreateButton("🔍  Analyse", Color.FromArgb(37, 99, 235));
        btnClear = CreateButton("✖  Clear", Color.FromArgb(55, 65, 90));
        btnPasteSample = CreateButton("💡  Load Sample", Color.FromArgb(20, 100, 80));

        pnlButtons.Controls.AddRange(new Control[] { btnAnalyse, btnClear, btnPasteSample });
        pnlInput.Controls.AddRange(new Control[] {
            lblInputHeader, lblInputHint, rtbMessage, lblCharCount, pnlButtons });

        // ── Result panel ──────────────────────────────────────────────────
        pnlResult = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(22, 22, 36),
            Padding = new Padding(20, 10, 20, 10),
            Visible = false
        };

        // Badge row (icon + label + summary)
        pnlResultBadge = new Panel
        {
            Location = new Point(20, 10),
            Size = new Size(820, 60),
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        lblResultIcon = new Label
        {
            Text = "🚨",
            Font = new Font("Segoe UI Emoji", 24f),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        lblResultLabel = new Label
        {
            Text = "SPAM",
            Font = new Font("Segoe UI", 22f, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 80, 80),
            AutoSize = true,
            Location = new Point(52, 4)
        };

        lblResultSummary = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.FromArgb(180, 190, 220),
            AutoSize = true,
            Location = new Point(52, 38)
        };

        pnlResultBadge.Controls.AddRange(new Control[] {
            lblResultIcon, lblResultLabel, lblResultSummary });

        // Confidence bar
        lblConfidenceTitle = MakeMetricLabel("Confidence:", new Point(20, 80));
        lblConfidenceValue = MakeMetricValue("", new Point(140, 80));

        pnlProgressTrack = new Panel
        {
            Location = new Point(20, 100),
            Size = new Size(820, 10),
            BackColor = Color.FromArgb(50, 50, 70),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        pnlProgressFill = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(0, 10),
            BackColor = Color.FromArgb(37, 99, 235)
        };
        pnlProgressTrack.Controls.Add(pnlProgressFill);

        // Metric rows
        lblSpamProbTitle = MakeMetricLabel("Spam Probability:", new Point(20, 120));
        lblSpamProbValue = MakeMetricValue("", new Point(200, 120));
        lblScoreTitle = MakeMetricLabel("Raw Score:", new Point(20, 144));
        lblScoreValue = MakeMetricValue("", new Point(200, 144));

        // Analysed message preview
        var lblAnalysedHeader = MakeMetricLabel("Analysed message:", new Point(20, 172));

        rtbAnalysedMsg = new RichTextBox
        {
            Location = new Point(20, 192),
            Size = new Size(820, 60),
            Font = new Font("Segoe UI", 9.5f),
            BackColor = Color.FromArgb(30, 30, 48),
            ForeColor = Color.FromArgb(180, 190, 220),
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        pnlResult.Controls.AddRange(new Control[] {
            pnlResultBadge,
            lblConfidenceTitle, lblConfidenceValue,
            pnlProgressTrack,
            lblSpamProbTitle, lblSpamProbValue,
            lblScoreTitle,    lblScoreValue,
            lblAnalysedHeader, rtbAnalysedMsg
        });

        outer.Controls.Add(pnlTop, 0, 0);
        outer.Controls.Add(pnlInput, 0, 1);
        outer.Controls.Add(pnlResult, 0, 2);
        tabDetect.Controls.Add(outer);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  HISTORY TAB
    // ─────────────────────────────────────────────────────────────────────
    private void BuildHistoryTab()
    {
        lblHistoryCount = new Label
        {
            Text = "No predictions yet.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(120, 140, 200),
            Dock = DockStyle.Top,
            Height = 32,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0)
        };

        dgvHistory = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.FromArgb(22, 22, 36),
            GridColor = Color.FromArgb(40, 44, 66),
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            Font = new Font("Segoe UI", 9.5f),
            ColumnHeadersHeight = 36,
            RowTemplate = { Height = 30 }
        };

        // Style
        dgvHistory.DefaultCellStyle.BackColor = Color.FromArgb(28, 28, 44);
        dgvHistory.DefaultCellStyle.ForeColor = Color.FromArgb(200, 210, 240);
        dgvHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
        dgvHistory.DefaultCellStyle.SelectionForeColor = Color.White;
        dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 40, 70);
        dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(140, 180, 255);
        dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgvHistory.EnableHeadersVisualStyles = false;
        dgvHistory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(25, 26, 42);

        // Columns
        dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "No", HeaderText = "#", Width = 44, SortMode = DataGridViewColumnSortMode.NotSortable });
        dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Timestamp", HeaderText = "Time", Width = 80, SortMode = DataGridViewColumnSortMode.NotSortable });
        dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Preview", HeaderText = "Message Preview", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, SortMode = DataGridViewColumnSortMode.NotSortable });
        dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Result", HeaderText = "Result", Width = 80, SortMode = DataGridViewColumnSortMode.NotSortable });
        dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Confidence", HeaderText = "Confidence", Width = 90, SortMode = DataGridViewColumnSortMode.NotSortable });

        btnClearHistory = CreateButton("🗑  Clear History", Color.FromArgb(120, 40, 40));
        btnClearHistory.Dock = DockStyle.Bottom;
        btnClearHistory.Height = 36;

        tabHistory.Controls.Add(dgvHistory);
        tabHistory.Controls.Add(lblHistoryCount);
        tabHistory.Controls.Add(btnClearHistory);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  MODEL INFO TAB
    // ─────────────────────────────────────────────────────────────────────
    private void BuildModelInfoTab()
    {
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

        tblModelInfo = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(20)
        };
        tblModelInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        tblModelInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Model status label
        lblModelStatus = new Label
        {
            Text = "⏳  Model status: Initialising...",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(120, 180, 255),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 16)
        };

        void AddRow(string key, string value)
        {
            tblModelInfo.Controls.Add(new Label
            {
                Text = key,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 160, 220),
                AutoSize = true,
                Margin = new Padding(0, 4, 20, 4)
            });
            tblModelInfo.Controls.Add(new Label
            {
                Name = "val_" + key.Replace(" ", "_").Replace("%", ""),
                Text = value,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(210, 220, 255),
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 4)
            });
        }

        AddRow("Algorithm:", "FastTree (Gradient Boosted Decision Trees)");
        AddRow("Feature method:", "TF-IDF text featurisation (word n-grams)");
        AddRow("Evaluation:", "5-fold cross-validation");
        AddRow("Training set:", "—");
        AddRow("Accuracy:", "—");
        AddRow("AUC-ROC:", "—");
        AddRow("F1 Score:", "—");
        AddRow("Precision:", "—");
        AddRow("Recall:", "—");
        AddRow("Train time:", "—");
        AddRow("Model file:", "—");

        lblTrainStatus = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(100, 200, 120),
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0)
        };

        btnRetrain = CreateButton("🔄  Retrain Model", Color.FromArgb(37, 99, 235));
        btnRetrain.Margin = new Padding(0, 20, 0, 0);

        var infoPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(20, 20, 20, 0),
            BackColor = Color.Transparent
        };

        infoPanel.Controls.Add(lblModelStatus);
        infoPanel.Controls.Add(tblModelInfo);
        infoPanel.Controls.Add(lblTrainStatus);
        infoPanel.Controls.Add(btnRetrain);

        scroll.Controls.Add(infoPanel);
        tabModelInfo.Controls.Add(scroll);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────
    private static Button CreateButton(string text, Color backColor)
    {
        return new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(160, 38),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 10, 0),
            FlatAppearance = { BorderSize = 0 }
        };
    }

    private static Label MakeMetricLabel(string text, Point loc) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        ForeColor = Color.FromArgb(140, 160, 220),
        AutoSize = true,
        Location = loc
    };

    private static Label MakeMetricValue(string text, Point loc) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", 9.5f),
        ForeColor = Color.FromArgb(200, 215, 255),
        AutoSize = true,
        Location = loc
    };
}