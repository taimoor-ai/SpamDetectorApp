using Microsoft.ML.Data;

namespace SpamDetectorApp.Models;

// ─────────────────────────────────────────────────────────────────────────────
//  INPUT  — raw CSV row loaded into ML.NET
// ─────────────────────────────────────────────────────────────────────────────

public class SmsInput
{
    [LoadColumn(0)]
    public string Label { get; set; } = string.Empty;   // "spam" | "ham"

    [LoadColumn(1)]
    public string Message { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────────────────────────────────────
//  PIPELINE DATA
// ─────────────────────────────────────────────────────────────────────────────

public class SmsData
{
    public bool IsSpam { get; set; }   // true = spam, false = ham

    public string Message { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────────────────────────────────────
//  PREDICTION OUTPUT  ✅ FIXED HERE
// ─────────────────────────────────────────────────────────────────────────────

public class SmsPrediction
{
    [ColumnName("PredictedLabel")]
    public bool IsSpam { get; set; }

    /// <summary>
    /// For binary classification, Probability is a SINGLE float (0–1)
    /// </summary>
    [ColumnName("Probability")]
    public float Probability { get; set; }   // ✅ FIXED (was float[])

    /// <summary>Raw score before sigmoid</summary>
    [ColumnName("Score")]
    public float Score { get; set; }

    // ── Derived helpers used by UI ─────────────────────────────────────────

    public string Label => IsSpam ? "SPAM" : "HAM";

    /// <summary>Confidence percentage (0–100)</summary>
    public float ConfidencePercent => Probability * 100f;

    /// <summary>Spam probability percentage</summary>
    public float SpamProbabilityPercent => Probability * 100f;
}

// ─────────────────────────────────────────────────────────────────────────────
//  HISTORY ENTRY  — DataGridView binding
// ─────────────────────────────────────────────────────────────────────────────

public record HistoryEntry(
    int No,
    string Timestamp,
    string Preview,
    string Result,
    string Confidence
);

// ─────────────────────────────────────────────────────────────────────────────
//  TRAINING RESULT
// ─────────────────────────────────────────────────────────────────────────────

public class TrainingResult
{
    public double Accuracy { get; init; }
    public double AUC { get; init; }
    public double F1Score { get; init; }
    public double Precision { get; init; }
    public double Recall { get; init; }
    public int SampleCount { get; init; }
    public string ModelPath { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }

    public string AccuracyPct => $"{Accuracy * 100:F1}%";
    public string AUCPct => $"{AUC * 100:F1}%";
    public string F1Pct => $"{F1Score * 100:F1}%";
    public string PrecisionPct => $"{Precision * 100:F1}%";
    public string RecallPct => $"{Recall * 100:F1}%";
}