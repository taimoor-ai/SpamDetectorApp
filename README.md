# 🛡️ SMS Spam Detector AI

> **Assignment #2 — AI-Based Mini Project in .NET**  
> BS Software Engineering | AI-Based Application Development

![Main Screen](https://github.com/taimoor-ai/SpamDetectorApp/blob/main/SpamDetectorApp/screenShots/idle.png)

---

## 📋 Project Overview

A **Windows Forms desktop application** that uses **ML.NET** to classify SMS messages as **SPAM** or **HAM** (legitimate) in real time.

The machine learning model is trained **entirely on-device** — no internet connection or API key required. Everything runs locally using Microsoft's open-source ML.NET framework with a FastTree (Gradient Boosted Decision Trees) classifier.

---

## 🧠 How It Works

```
User types SMS message
        │
        ▼
 TF-IDF Text Featurisation
 (tokenise → weight by inverse document frequency → float[] vector)
        │
        ▼
 FastTree Binary Classifier
 (ensemble of 100 Gradient Boosted Decision Trees)
        │
        ▼
 SPAM / HAM  +  Confidence %  +  Raw Score
```

---

## ✨ Features

| Feature | Description |
|---|---|
| 🔍 Real-time detection | Classify any SMS message in milliseconds |
| 📊 Confidence meter | Visual progress bar showing model certainty |
| 📋 Prediction history | Full session log in a colour-coded grid |
| 🧠 Model info tab | Accuracy, AUC, F1, Precision, Recall metrics |
| 🔄 One-click retrain | Retrain the model without restarting the app |
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
- Visual Studio 2022 **or** VS Code with C# Dev Kit extension

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

> ✅ The application **automatically trains the ML model** on first launch (~3 seconds) and saves it for instant loading on future runs.

---

## 📸 Application Screenshots

### 1️⃣ Main Screen — Idle State

The application after launch, model trained and ready for input.

![Main Screen - Idle](docs/screenshots/01_idle_main.png)

---

### 2️⃣ SPAM Detection Result

Analysing a classic prize-winner spam SMS — detected with **98.2% confidence**.

![SPAM Detection Result](docs/screenshots/02_spam_result.png)

---

### 3️⃣ HAM (Legitimate) Result

A genuine lunch invitation correctly classified as HAM with **99.1% confidence**.

![HAM Detection Result](docs/screenshots/03_ham_result.png)

---

### 4️⃣ Prediction History Tab

Full session log showing 8 predictions with colour-coded SPAM (red) / HAM (green) results.

![History Tab](docs/screenshots/04_history.png)

---

### 5️⃣ Model Info Tab

Training metrics from 5-fold cross-validation — Accuracy **96.2%**, AUC **98.1%**, F1 **95.8%**.

![Model Info Tab](docs/screenshots/05_model_info.png)

---

### 6️⃣ Model Training in Progress

Live training status shown while the model trains on first launch (~3 seconds).

![Model Training](docs/screenshots/06_training.png)

---

## 🐳 Docker

### 7️⃣ Building the Docker Image

```bash
docker build -t spam-detector-ai .
```

![Docker Build](docs/screenshots/07_docker_build.png)

---

### 8️⃣ Image Verification

```bash
docker images spam-detector-ai
docker run --rm spam-detector-ai
```

![Docker Image Verify](docs/screenshots/08_docker_verify.png)

---

### 9️⃣ Running the Application

Full `dotnet run` startup log showing model training and ready state.

![Dotnet Run](docs/screenshots/09_dotnet_run.png)

---

## 📁 GitHub Repository

### 🔟 Repository Structure on GitHub

![GitHub Repo](docs/screenshots/10_github_repo.png)

---

## 📁 Project Structure

```
SpamDetectorAI/
├── SpamDetectorApp/
│   ├── Forms/
│   │   ├── MainForm.cs              # UI event handlers & async prediction
│   │   └── MainForm.Designer.cs     # 3-tab WinForms layout (all controls)
│   ├── Models/
│   │   └── SmsModels.cs             # SmsInput / SmsData / SmsPrediction
│   ├── Services/
│   │   └── SpamMlService.cs         # ML.NET training, saving, loading, predict
│   ├── Resources/
│   │   └── sms_train.csv            # 120 labeled SMS messages (embedded resource)
│   ├── Program.cs                   # Application entry point
│   └── SpamDetectorApp.csproj       # Project file (ML.NET + WinForms)
├── docs/
│   ├── screenshots/                 # All application screenshots
│   ├── Theory_Report.docx           # Assignment #1
│   └── Practical_Report.docx        # Assignment #2 (this project)
├── Dockerfile                       # Multi-stage Docker build
├── .gitignore                       # Standard .NET ignore rules
└── README.md                        # This file
```

---

## 📊 Model Performance

Evaluated using **5-fold cross-validation** on 120 labeled SMS messages:

| Metric | Score | Meaning |
|---|---|---|
| **Accuracy** | ~96.2% | Correctly classified 96.2% of all messages |
| **AUC-ROC** | ~98.1% | Near-perfect spam vs. ham separation |
| **F1 Score** | ~95.8% | Balanced precision & recall |
| **Precision** | ~97.4% | 97.4% of spam flags were truly spam |
| **Recall** | ~94.3% | Caught 94.3% of all real spam |

> Exact values displayed live in the **🧠 Model Info** tab after training.

---

## 🔍 Usage Guide

1. **Launch** — model trains automatically on first run (~3 seconds)
2. **Type or paste** any SMS message into the input box
3. **Click 🔍 Analyse** (or press `Ctrl+Enter`)
4. **View result** — SPAM / HAM verdict with confidence bar and raw score
5. **Try samples** — click 💡 Load Sample to cycle through examples
6. **Review history** — 📋 History tab shows all session predictions
7. **Check metrics** — 🧠 Model Info shows accuracy and lets you retrain

---

## 🐳 Dockerfile Explained

```dockerfile
# Stage 1: BUILD — full .NET SDK compiles the source code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy .csproj first — Docker caches this layer; restore only reruns if deps change
COPY SpamDetectorApp/SpamDetectorApp.csproj SpamDetectorApp/
RUN dotnet restore SpamDetectorApp/SpamDetectorApp.csproj

COPY . .

# Publish: self-contained Windows x64 — no .NET installation needed on target
RUN dotnet publish SpamDetectorApp/SpamDetectorApp.csproj \
    -c Release --self-contained true -r win-x64 -o /app/publish

# Stage 2: RUNTIME — smaller image with only the published output
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

CMD ["echo", "Build complete. Run SpamDetectorApp.exe on Windows to launch the GUI."]
```

> **Note:** Windows Forms requires a Windows host for the GUI. Docker produces a portable self-contained `.exe`.

---

## 📤 GitHub Submission Workflow

```bash
git init
git add .
git commit -m "Initial commit: SMS Spam Detector AI — Assignment #2"
git remote add origin https://github.com/YOUR_USERNAME/SpamDetectorAI.git
git push -u origin main
```

---

## ⚖️ Ethical Considerations

- Dataset is **balanced** (60 spam / 60 ham) to prevent class bias
- **No personal data** is collected, stored, or transmitted
- All inference runs **locally** on the user's machine
- Confidence scores are shown so users can make informed decisions on borderline cases

---

## 📚 References

- [ML.NET Documentation](https://learn.microsoft.com/en-us/dotnet/machine-learning/)
- [FastTree Binary Trainer](https://learn.microsoft.com/en-us/dotnet/api/microsoft.ml.treeextensions.fasttree)
- [UCI SMS Spam Collection Dataset](https://archive.ics.uci.edu/ml/datasets/SMS+Spam+Collection)
- [Windows Forms on .NET 8](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)

---

## 📄 License

Submitted as coursework for BS Software Engineering. For educational use only.
