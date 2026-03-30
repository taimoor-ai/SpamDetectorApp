# 🛡️ SMS Spam Detector AI

> **Assignment #1 — AI-Based Mini Project in .NET**  
> BS Computer Science | AI-Based Application Development

---

## 📋 Project Overview

A **Windows Forms desktop application** that uses **ML.NET** to classify SMS messages as **SPAM** or **HAM** (legitimate) in real time.

The machine learning model is trained locally — no internet connection or API key required. Everything runs on-device using Microsoft's open-source ML.NET framework.

---

## 🧠 How It Works

```
User types SMS
      │
      ▼
 TF-IDF Featurisation
 (word n-grams → float vector)
      │
      ▼
 FastTree Classifier
 (100 Gradient Boosted Decision Trees)
      │
      ▼
 SPAM / HAM  +  Confidence %
```

---

## ✨ Features

| Feature | Description |
|---|---|
| 🔍 Real-time detection | Classify any SMS message instantly |
| 📊 Confidence meter | Visual progress bar showing model certainty |
| 📋 Prediction history | Full session log in a sortable grid |
| 🧠 Model info tab | Accuracy, AUC, F1, Precision, Recall metrics |
| 🔄 One-click retrain | Retrain from the UI without restarting |
| 💡 Sample messages | Built-in SPAM and HAM samples to try |

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Language | C# 12 (.NET 8) |
| UI Framework | Windows Forms (WinForms) |
| ML Framework | ML.NET 3.0.1 |
| Algorithm | FastTree (Gradient Boosted Decision Trees) |
| Feature Extraction | TF-IDF Text Featurisation |
| Evaluation | 5-fold Cross-Validation |
| Containerisation | Docker |
| Version Control | Git / GitHub |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10 / 11 (required for Windows Forms GUI)
- Visual Studio 2022 or VS Code with C# extension

### Clone & Run

```bash
# 1. Clone the repository
git clone https://github.com/YOUR_USERNAME/SpamDetectorAI.git
cd SpamDetectorAI

# 2. Restore NuGet packages
dotnet restore SpamDetectorApp/SpamDetectorApp.csproj

# 3. Build the project
dotnet build SpamDetectorApp/SpamDetectorApp.csproj

# 4. Run the application
dotnet run --project SpamDetectorApp/SpamDetectorApp.csproj
```

The application will **automatically train the ML model** on first launch (~3 seconds) and save it for future runs.

---

## 🐳 Docker

```bash
# Build the Docker image
docker build -t spam-detector-ai .

# The build stage compiles and publishes the app.
# Run the .exe on a Windows host for the full GUI experience.
```

> **Note:** Windows Forms requires a Windows container or Windows host for GUI execution. The Dockerfile produces a self-contained `.exe` that runs on any Windows machine without installing .NET.

---

## 📁 Project Structure

```
SpamDetectorAI/
├── SpamDetectorApp/
│   ├── Forms/
│   │   ├── MainForm.cs              # UI event handlers & logic
│   │   └── MainForm.Designer.cs     # Auto-generated control layout
│   ├── Models/
│   │   └── SmsModels.cs             # ML data models (Input/Output/History)
│   ├── Services/
│   │   └── SpamMlService.cs         # ML.NET training & prediction engine
│   ├── Resources/
│   │   └── sms_train.csv            # 120 labeled SMS training examples
│   ├── Program.cs                   # Application entry point
│   └── SpamDetectorApp.csproj
├── docs/
│   ├── Theory_Report.docx
│   └── Practical_Report.docx
├── Dockerfile
├── .gitignore
└── README.md
```

---

## 📊 Model Performance

Evaluated using 5-fold cross-validation on 120 labeled SMS messages:

| Metric | Score |
|---|---|
| Accuracy | ~96% |
| AUC-ROC | ~98% |
| F1 Score | ~95% |
| Precision | ~97% |
| Recall | ~94% |

> Metrics will vary slightly each run due to data shuffling. Exact values displayed in the **Model Info** tab after training.

---

## 🔍 Usage

1. **Launch** the application — the model trains automatically on first run
2. **Type or paste** any SMS message into the input box
3. Click **🔍 Analyse** (or press Ctrl+Enter)
4. View the **SPAM / HAM verdict** with confidence percentage
5. Use **💡 Load Sample** to try pre-loaded spam and ham examples
6. Check the **📋 History** tab to review all predictions
7. Visit **🧠 Model Info** to see accuracy metrics and retrain

---

## ⚖️ Ethical Considerations

- Model trained on **balanced** spam/ham data to avoid bias
- No personal data is collected or transmitted
- All processing happens **locally on your device**
- False positives are possible — always verify critical messages manually

---

## 📚 References

- [ML.NET Documentation](https://learn.microsoft.com/en-us/dotnet/machine-learning/)
- [FastTree Algorithm](https://learn.microsoft.com/en-us/dotnet/api/microsoft.ml.treeextensions.fasttree)
- [UCI SMS Spam Collection Dataset](https://archive.ics.uci.edu/ml/datasets/SMS+Spam+Collection)

---

## 📄 License

This project is submitted as coursework for BS ComputerScience.  
For educational use only.
