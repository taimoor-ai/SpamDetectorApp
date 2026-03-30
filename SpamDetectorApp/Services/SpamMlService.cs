using Microsoft.ML;
using Microsoft.ML.Data;
using SpamDetectorApp.Models;
using System.Reflection;

namespace SpamDetectorApp.Services;

/// <summary>
/// Core ML.NET service.  Responsible for:
///   1. Loading training data (embedded CSV resource)
///   2. Building and training the ML pipeline
///   3. Evaluating the model via cross-validation
///   4. Saving / loading the model to/from disk
///   5. Running single-message predictions
///
/// ─── PIPELINE OVERVIEW ───────────────────────────────────────────────────────
///
///   Raw CSV text
///     │
///     ▼  MapValueToKey  (string "spam"/"ham" → bool IsSpam key)
///     │
///     ▼  FeaturizeText  (TF-IDF + char/word n-grams → float[] Features)
///        · Tokenises the message into individual words
///        · Computes term-frequency × inverse-document-frequency weights
///        · Produces a dense numeric vector the classifier can consume
///     │
///     ▼  FastTree Binary Classifier
///        · Gradient-Boosted Decision Trees (GBDT)
///        · 100 trees, 20 leaves per tree, learning rate 0.2
///        · Outputs calibrated probability and binary label
///     │
///     ▼  SmsPrediction { IsSpam, Probability[2], Score }
///
/// ─────────────────────────────────────────────────────────────────────────────
/// </summary>
public sealed class SpamMlService : IDisposable
{
    // ── Private state ──────────────────────────────────────────────────────
    private readonly MLContext _mlContext;
    private PredictionEngine<SmsData, SmsPrediction>? _engine;
    private readonly string _modelPath;
    private readonly object _lock = new();

    // ── Public state ───────────────────────────────────────────────────────
    public bool IsReady => _engine is not null;
    public TrainingResult? LastResult { get; private set; }

    // ── Events ─────────────────────────────────────────────────────────────
    public event EventHandler<string>? StatusChanged;

    public SpamMlService()
    {
        _mlContext = new MLContext(seed: 42); // fixed seed → reproducible results
        _modelPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "MLModels", "spam_model.zip");
        Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  TRAINING
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Trains the model from the embedded CSV resource.
    /// Runs cross-validation, saves the final model to disk,
    /// and returns evaluation metrics.
    /// </summary>
    public TrainingResult Train()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // ── Step 1: Load training data ────────────────────────────────────
        Report("Loading training data...");
        var smsRows = LoadEmbeddedCsv();
        Report($"Loaded {smsRows.Count} labeled SMS messages.");

        // Convert SmsInput list to ML.NET IDataView
        // IDataView is ML.NET's lazy, columnar data representation
        var rawView = _mlContext.Data.LoadFromEnumerable(smsRows);

        // ── Step 2: Convert string label → bool ───────────────────────────
        // ML.NET binary classifiers need a boolean "Label" column.
        // We achieve this with a custom mapping action.
        Report("Preprocessing labels...");
        var mappingPipeline = _mlContext.Transforms.CustomMapping<SmsInput, LabelBool>(
            (input, output) => output.IsSpam = input.Label.Trim().ToLower() == "spam",
            contractName: "SpamLabelMap");

        var mappedView = mappingPipeline.Fit(rawView).Transform(rawView);

        // Now project to SmsData (IsSpam + Message)
        var dataView = _mlContext.Data.LoadFromEnumerable(
            _mlContext.Data.CreateEnumerable<SmsInput>(rawView, reuseRowObject: false)
                .Select(row => new SmsData
                {
                    IsSpam = row.Label.Trim().ToLower() == "spam",
                    Message = row.Message
                }));

        // Shuffle rows to prevent ordering bias
        dataView = _mlContext.Data.ShuffleRows(dataView, seed: 42);

        // ── Step 3: Build ML pipeline ─────────────────────────────────────
        Report("Building ML pipeline...");
        var pipeline = BuildPipeline();

        // ── Step 4: 5-fold cross-validation ──────────────────────────────
        // Cross-validation is more reliable than a single train/test split
        // because every sample gets to be in the test set exactly once.
        Report("Running 5-fold cross-validation...");
        var cvResults = _mlContext.BinaryClassification.CrossValidate(
            dataView, pipeline, numberOfFolds: 5, labelColumnName: "Label");

        var accuracy = cvResults.Average(r => r.Metrics.Accuracy);
        var auc = cvResults.Average(r => r.Metrics.AreaUnderRocCurve);
        var f1 = cvResults.Average(r => r.Metrics.F1Score);
        var precision = cvResults.Average(r => r.Metrics.PositivePrecision);
        var recall = cvResults.Average(r => r.Metrics.PositiveRecall);

        // ── Step 5: Train final model on ALL data ─────────────────────────
        Report("Training final model on full dataset...");
        var finalModel = pipeline.Fit(dataView);

        // ── Step 6: Save model ────────────────────────────────────────────
        Report("Saving model to disk...");
        _mlContext.Model.Save(finalModel, dataView.Schema, _modelPath);

        // ── Step 7: Load prediction engine ────────────────────────────────
        lock (_lock)
        {
            _engine = _mlContext.Model.CreatePredictionEngine<SmsData, SmsPrediction>(finalModel);
        }

        sw.Stop();
        LastResult = new TrainingResult
        {
            Accuracy = accuracy,
            AUC = auc,
            F1Score = f1,
            Precision = precision,
            Recall = recall,
            SampleCount = smsRows.Count,
            ModelPath = _modelPath,
            Duration = sw.Elapsed
        };

        Report($"Training complete — Accuracy: {accuracy * 100:F1}%");
        return LastResult;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  LOAD EXISTING MODEL
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads a previously trained model from the .zip file on disk.
    /// Fast path — no retraining needed.
    /// </summary>
    public void LoadSavedModel()
    {
        if (!File.Exists(_modelPath))
            throw new FileNotFoundException(
                $"No saved model found at:\n{_modelPath}\n\nPlease train the model first.");

        Report("Loading saved model from disk...");
        var model = _mlContext.Model.Load(_modelPath, out _);
        lock (_lock)
        {
            _engine = _mlContext.Model.CreatePredictionEngine<SmsData, SmsPrediction>(model);
        }
        Report("Model loaded successfully.");
    }

    /// <summary>
    /// Ensures the model is ready: loads from disk if possible, otherwise trains.
    /// Called on application startup.
    /// </summary>
    public TrainingResult? EnsureReady()
    {
        if (IsReady) return LastResult;
        if (File.Exists(_modelPath))
        {
            LoadSavedModel();
            return null; // no new metrics — loaded existing model
        }
        return Train();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PREDICTION
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Runs inference on a single SMS message.
    /// Thread-safe via locking.
    /// </summary>
    /// <param name="messageText">Raw SMS text from user input</param>
    public SmsPrediction Predict(string messageText)
    {
        if (!IsReady)
            throw new InvalidOperationException(
                "Model is not ready. Please train or load a model first.");

        if (string.IsNullOrWhiteSpace(messageText))
            return new SmsPrediction
            {
                IsSpam = false,
                Probability = 0f,   // ✅ FIXED
                Score = -99f
            };

        var input = new SmsData { Message = messageText.Trim() };
        lock (_lock)
        {
            return _engine!.Predict(input);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVATE HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the two-stage ML pipeline:
    ///   FeaturizeText → FastTree binary classifier
    /// </summary>
    private IEstimator<ITransformer> BuildPipeline()
    {
        return _mlContext.Transforms
            // Map bool IsSpam → "Label" column expected by binary classifiers
            .Conversion.ConvertType(
                outputColumnName: "Label",
                inputColumnName: nameof(SmsData.IsSpam),
                outputKind: DataKind.Boolean)

            // TF-IDF text featurisation:
            //   • Splits message into word unigrams and bigrams
            //   • Weights each token by inverse document frequency
            //   • Outputs a dense float vector called "Features"
            .Append(_mlContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features",
                inputColumnName: nameof(SmsData.Message)))

            // FastTree (Gradient Boosted Decision Trees) binary classifier.
            // Chosen because:
            //   ✓ Handles sparse TF-IDF vectors efficiently
            //   ✓ Produces calibrated probabilities
            //   ✓ Fast training, faster inference
            //   ✓ No GPU or special hardware required
            .Append(_mlContext.BinaryClassification.Trainers.FastTree(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: 20,
                numberOfTrees: 100,
                minimumExampleCountPerLeaf: 5,
                learningRate: 0.2));
    }

    /// <summary>
    /// Reads the embedded sms_train.csv resource and returns typed rows.
    /// Using an embedded resource means the CSV ships inside the .exe
    /// and cannot be accidentally deleted.
    /// </summary>
    private static List<SmsInput> LoadEmbeddedCsv()
    {
        var assembly = Assembly.GetExecutingAssembly();
        // Resource name pattern: AssemblyName.Folder.FileName
        var resourceName = "SpamDetectorApp.Resources.sms_train.csv";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' not found.\n" +
                "Ensure sms_train.csv has Build Action = Embedded Resource.");

        using var reader = new StreamReader(stream);
        var rows = new List<SmsInput>();
        bool isHeader = true;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (isHeader) { isHeader = false; continue; } // skip header row

            // CSV format: label,message
            // The message itself can contain commas so we split only on the first comma
            var commaIndex = line.IndexOf(',');
            if (commaIndex < 0) continue;

            rows.Add(new SmsInput
            {
                Label = line[..commaIndex].Trim(),
                Message = line[(commaIndex + 1)..].Trim()
            });
        }

        return rows;
    }

    private void Report(string message) =>
        StatusChanged?.Invoke(this, message);

    public void Dispose() { /* MLContext has no IDisposable */ }
}

// ── Internal helper type for the CustomMapping transform ──────────────────────
/// <summary>Helper class required by ML.NET's CustomMapping transform</summary>
internal sealed class LabelBool { public bool IsSpam { get; set; } }