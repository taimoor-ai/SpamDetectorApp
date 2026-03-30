using SpamDetectorApp.Models;
using SpamDetectorApp.Services;

namespace SpamDetectorApp.Forms;

/// <summary>
/// Main application window.
///
/// Responsibilities:
///   • Manage the lifecycle of SpamMlService (train / load / predict)
///   • Handle all user interactions (analyse button, clear, sample load)
///   • Render prediction results with colour-coded feedback
///   • Maintain the prediction history DataGridView
///   • Display model evaluation metrics on the Model Info tab
///
/// Threading model:
///   Training is CPU-intensive (can take 2–5 seconds).
///   All ML work is offloaded to Task.Run() to keep the UI responsive.
///   Results are marshalled back to the UI thread via this.Invoke().
/// </summary>
public partial class MainForm : Form
{
    // ── Dependencies ───────────────────────────────────────────────────────
    private readonly SpamMlService _mlService;

    // ── State ──────────────────────────────────────────────────────────────
    private int _predictionCount = 0;
    private bool _isBusy = false;

    // ── Sample messages ────────────────────────────────────────────────────
    private static readonly string[] SpamSamples =
    {
        "WINNER!! As a valued customer you have been selected to receive a 900 prize reward! To claim call 09061701461. Claim code KL341 valid 12 hours only.",
        "FREE entry in 2 a wkly comp to win FA Cup final tkts! Text FA to 87121 to receive entry. Standard text rates apply.",
        "URGENT: Your mobile has been awarded a complimentary Nokia N95 handset. To claim call 09064019788 before midnight tonight!",
        "Congratulations! You've won a 2-week holiday to Ibiza. Call 0906 501 1905 to claim your prize. Cost 150p/min."
    };

    private static readonly string[] HamSamples =
    {
        "Hey, are you coming to the team lunch on Friday? We're going to Nando's at 1pm. Let me know!",
        "Your appointment with Dr. Ahmed is confirmed for Thursday at 10:30am at the health centre on Park Street.",
        "Can you pick up some milk and bread on your way home please? We are almost out and I forgot at Tesco.",
        "The project meeting has been moved to 3pm in conference room B. Please bring your laptop and the latest report."
    };

    private readonly Random _rng = new();
    private bool _nextSampleIsSpam = true; // alternate between spam/ham samples

    // ──────────────────────────────────────────────────────────────────────
    //  CONSTRUCTOR & STARTUP
    // ──────────────────────────────────────────────────────────────────────

    public MainForm()
    {
        InitializeComponent();

        _mlService = new SpamMlService();
        _mlService.StatusChanged += OnMlStatusChanged;

        WireEvents();

        // Kick off model initialisation on a background thread
        // so the form appears immediately while the model loads
        Task.Run(InitialiseModel);
    }

    /// <summary>
    /// Loads or trains the model asynchronously on startup.
    /// Updates the status bar and Model Info tab when done.
    /// </summary>
    private async Task InitialiseModel()
    {
        SetBusy(true, "Initialising model...");
        try
        {
            // EnsureReady() returns TrainingResult if it trained, null if it loaded
            var result = await Task.Run(() => _mlService.EnsureReady());

            this.Invoke(() =>
            {
                if (result != null)
                    UpdateModelInfoTab(result, justTrained: true);
                else
                    UpdateModelInfoTab(null, justTrained: false);

                SetStatus("✅  Model ready — enter an SMS message to begin.");
            });
        }
        catch (Exception ex)
        {
            this.Invoke(() =>
            {
                SetStatus($"❌  Error: {ex.Message}");
                MessageBox.Show(
                    $"Model initialisation failed:\n\n{ex.Message}",
                    "Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            });
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    //  EVENT WIRING
    // ──────────────────────────────────────────────────────────────────────

    private void WireEvents()
    {
        btnAnalyse.Click += BtnAnalyse_Click;
        btnClear.Click += BtnClear_Click;
        btnPasteSample.Click += BtnPasteSample_Click;
        btnRetrain.Click += BtnRetrain_Click;
        btnClearHistory.Click += BtnClearHistory_Click;

        rtbMessage.TextChanged += RtbMessage_TextChanged;

        // Allow Ctrl+Enter to trigger analysis
        rtbMessage.KeyDown += (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnAnalyse_Click(s, e);
            }
        };
    }

    // ──────────────────────────────────────────────────────────────────────
    //  ANALYSE BUTTON
    // ──────────────────────────────────────────────────────────────────────

    private async void BtnAnalyse_Click(object? sender, EventArgs e)
    {
        var text = rtbMessage.Text.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            ShowInputError("Please enter an SMS message to analyse.");
            return;
        }

        if (text.Length < 5)
        {
            ShowInputError("Message is too short (minimum 5 characters).");
            return;
        }

        if (!_mlService.IsReady)
        {
            SetStatus("⏳  Model is still loading — please wait a moment.");
            return;
        }

        SetBusy(true, "Analysing message...");

        try
        {
            // Run prediction on background thread — keeps UI responsive
            var prediction = await Task.Run(() => _mlService.Predict(text));
            DisplayResult(prediction, text);
            AddToHistory(prediction, text);
        }
        catch (Exception ex)
        {
            SetStatus($"❌  Prediction failed: {ex.Message}");
            MessageBox.Show(ex.Message, "Prediction Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            SetBusy(false);
            SetStatus(
                _predictionCount == 1
                    ? "✅  Analysis complete."
                    : $"✅  Analysis complete — {_predictionCount} messages checked this session.");
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    //  DISPLAY RESULT
    // ──────────────────────────────────────────────────────────────────────

    private void DisplayResult(SmsPrediction prediction, string originalMessage)
    {
        pnlResult.Visible = true;

        // ── Badge ──────────────────────────────────────────────────────────
        if (prediction.IsSpam)
        {
            lblResultIcon.Text = "🚨";
            lblResultLabel.Text = "SPAM";
            lblResultLabel.ForeColor = Color.FromArgb(255, 75, 75);
            lblResultSummary.Text = "This message has been classified as SPAM.";
            pnlResult.BackColor = Color.FromArgb(40, 20, 22);
            pnlProgressFill.BackColor = Color.FromArgb(220, 50, 50);
        }
        else
        {
            lblResultIcon.Text = "✅";
            lblResultLabel.Text = "HAM";
            lblResultLabel.ForeColor = Color.FromArgb(60, 200, 120);
            lblResultSummary.Text = "This message appears to be legitimate (HAM).";
            pnlResult.BackColor = Color.FromArgb(18, 38, 26);
            pnlProgressFill.BackColor = Color.FromArgb(40, 180, 100);
        }

        // ── Confidence bar ─────────────────────────────────────────────────
        lblConfidenceValue.Text = $"{prediction.ConfidencePercent:F1}%";
        int fillWidth = (int)(pnlProgressTrack.Width * (prediction.ConfidencePercent / 100f));
        pnlProgressFill.Width = Math.Max(4, fillWidth);

        // ── Metric labels ──────────────────────────────────────────────────
        lblSpamProbValue.Text = $"{prediction.SpamProbabilityPercent:F2}%";
        lblScoreValue.Text = $"{prediction.Score:F4}  (positive = spam tendency)";

        // ── Analysed message preview ───────────────────────────────────────
        rtbAnalysedMsg.Text = originalMessage;

        // Scroll result into view
        pnlResult.Focus();
    }

    // ──────────────────────────────────────────────────────────────────────
    //  HISTORY
    // ──────────────────────────────────────────────────────────────────────

    private void AddToHistory(SmsPrediction prediction, string message)
    {
        _predictionCount++;

        var preview = message.Length > 70
            ? message[..70] + "..."
            : message;

        var row = dgvHistory.Rows.Add(
            _predictionCount,
            DateTime.Now.ToString("HH:mm:ss"),
            preview,
            prediction.Label,
            $"{prediction.ConfidencePercent:F1}%"
        );

        // Colour-code the result cell
        var resultCell = dgvHistory.Rows[row].Cells["Result"];
        resultCell.Style.ForeColor =
            prediction.IsSpam
                ? Color.FromArgb(255, 90, 90)
                : Color.FromArgb(60, 200, 120);
        resultCell.Style.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

        // Scroll to newest entry
        dgvHistory.FirstDisplayedScrollingRowIndex = dgvHistory.RowCount - 1;
        dgvHistory.ClearSelection();
        dgvHistory.Rows[row].Selected = true;

        lblHistoryCount.Text =
            $"  {_predictionCount} message{(_predictionCount == 1 ? "" : "s")} analysed this session";
    }

    private void BtnClearHistory_Click(object? sender, EventArgs e)
    {
        if (dgvHistory.RowCount == 0) return;
        if (MessageBox.Show("Clear all history entries?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            dgvHistory.Rows.Clear();
            _predictionCount = 0;
            lblHistoryCount.Text = "  History cleared.";
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    //  RETRAIN
    // ──────────────────────────────────────────────────────────────────────

    private async void BtnRetrain_Click(object? sender, EventArgs e)
    {
        if (_isBusy) return;

        if (MessageBox.Show(
                "Retrain the model from scratch?\n\nThis will take a few seconds.",
                "Confirm Retrain",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes) return;

        SetBusy(true, "Training model...");
        lblTrainStatus.Text = "⏳ Training in progress...";

        try
        {
            var result = await Task.Run(() => _mlService.Train());
            this.Invoke(() =>
            {
                UpdateModelInfoTab(result, justTrained: true);
                SetStatus($"✅  Retraining complete — Accuracy: {result.AccuracyPct}");
            });
        }
        catch (Exception ex)
        {
            this.Invoke(() =>
            {
                lblTrainStatus.Text = $"❌ Training failed: {ex.Message}";
                SetStatus($"❌  Training failed.");
            });
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    //  MODEL INFO TAB UPDATE
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Populates the Model Info tab with training metrics.
    /// Called after training or loading a model.
    /// </summary>
    private void UpdateModelInfoTab(TrainingResult? result, bool justTrained)
    {
        if (result != null)
        {
            lblModelStatus.Text = "✅  Model Status: Ready";
            lblModelStatus.ForeColor = Color.FromArgb(60, 200, 120);

            SetInfoValue("Training set:", $"{result.SampleCount} labeled SMS messages");
            SetInfoValue("Accuracy:", result.AccuracyPct);
            SetInfoValue("AUC-ROC:", result.AUCPct);
            SetInfoValue("F1 Score:", result.F1Pct);
            SetInfoValue("Precision:", result.PrecisionPct);
            SetInfoValue("Recall:", result.RecallPct);
            SetInfoValue("Train time:", $"{result.Duration.TotalSeconds:F1}s");
            SetInfoValue("Model file:", result.ModelPath);

            lblTrainStatus.Text =
                justTrained
                    ? $"✅  Model trained successfully at {DateTime.Now:HH:mm:ss}"
                    : $"✅  Model loaded from disk at {DateTime.Now:HH:mm:ss}";
        }
        else
        {
            // Model was loaded from disk, no new metrics
            lblModelStatus.Text = "✅  Model Status: Loaded from disk";
            lblModelStatus.ForeColor = Color.FromArgb(100, 180, 255);
            lblTrainStatus.Text = $"✅  Existing model loaded at {DateTime.Now:HH:mm:ss}";
        }
    }

    private void SetInfoValue(string key, string value)
    {
        var controlName = "val_" + key.Replace(" ", "_").Replace("%", "");
        var ctrl = tblModelInfo.Controls.Find(controlName, false);
        if (ctrl.Length > 0) ctrl[0].Text = value;
    }

    // ──────────────────────────────────────────────────────────────────────
    //  SAMPLE MESSAGES
    // ──────────────────────────────────────────────────────────────────────

    private void BtnPasteSample_Click(object? sender, EventArgs e)
    {
        string sample;
        if (_nextSampleIsSpam)
            sample = SpamSamples[_rng.Next(SpamSamples.Length)];
        else
            sample = HamSamples[_rng.Next(HamSamples.Length)];

        _nextSampleIsSpam = !_nextSampleIsSpam;
        rtbMessage.Text = sample;
        rtbMessage.Focus();
        rtbMessage.SelectionStart = rtbMessage.Text.Length;

        SetStatus(_nextSampleIsSpam
            ? "💡  Loaded a HAM sample — click Analyse to test."
            : "💡  Loaded a SPAM sample — click Analyse to test.");
    }

    // ──────────────────────────────────────────────────────────────────────
    //  OTHER EVENTS
    // ──────────────────────────────────────────────────────────────────────

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        rtbMessage.Clear();
        pnlResult.Visible = false;
        rtbMessage.Focus();
    }

    private void RtbMessage_TextChanged(object? sender, EventArgs e)
    {
        int len = rtbMessage.Text.Length;
        lblCharCount.Text = $"{len} / 2000";
        lblCharCount.ForeColor = len > 1800
            ? Color.FromArgb(255, 120, 60)
            : Color.FromArgb(90, 110, 160);
    }

    private void OnMlStatusChanged(object? sender, string message)
    {
        // Event fires on background thread — marshal to UI thread
        if (this.IsHandleCreated)
            this.Invoke(() => SetStatus($"  ⏳  {message}"));
    }

    // ──────────────────────────────────────────────────────────────────────
    //  UI HELPERS
    // ──────────────────────────────────────────────────────────────────────

    private void SetBusy(bool busy, string? statusText = null)
    {
        if (!this.IsHandleCreated) return;
        this.Invoke(() =>
        {
            _isBusy = busy;
            progressBar.Visible = busy;
            btnAnalyse.Enabled = !busy;
            btnRetrain.Enabled = !busy;
            if (statusText != null) SetStatus($"  {statusText}");
        });
    }

    private void SetStatus(string text)
    {
        if (this.IsHandleCreated)
            this.Invoke(() => lblStatus.Text = $"  {text}");
    }

    private void ShowInputError(string message)
    {
        MessageBox.Show(message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        rtbMessage.Focus();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _mlService.Dispose();
        base.OnFormClosed(e);
    }
}