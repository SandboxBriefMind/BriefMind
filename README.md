# BriefMind

**BriefMind** is an AI-powered contract analysis and audit platform designed to evaluate legal agreements through adversarial multi-agent debate, local LLM inference, and cryptographic provenance tracking.

---

## Key Features

- **Document Ingestion & Character Offsets**: Extracts text, page locations, and text bounding boxes from PDF contracts using `PdfPig`.
- **Clause Segmentation & Cryptographic Hashing**: Breaks contracts into distinct clauses and calculates SHA-256 hashes (`ClauseHashSha256`, `FileHashSha256`) to ensure tamper verification.
- **Adversarial Legal Debate Engine**:
  - **Defender Agent**: Formulates commercial and legal justifications for clause provisions.
  - **Auditor Agent**: Identifies risks, one-sided liabilities, and ambiguities.
  - **Judge Verdict**: Computes a risk score (0–100), flags contested terms, and provides actionable redline recommendations.
- **Audit Verification & Provenance**: Generates an exportable `AuditVerificationBundle` cryptographically binding extracted clauses, LLM arguments, and risk verdicts.
- **Dual-Model Debate**: Configure distinct LLMs per role (e.g., `phi3:mini` for Defender/Judge vs. `qwen2.5:1.5b` or `qwen2.5:3b` for Auditor).
- **Local & Privacy-Conscious (RTX 1650 4GB Ready)**: Evaluates arguments sequentially via **Ollama**, allowing multiple models to run on 4GB VRAM without memory overflow.
- **Modern Interactive UI**: React 19 and TypeScript interface for inspecting extracted clauses, choosing models, running debates, and reviewing audit reports.

---

## Architecture & Tech Stack

| Layer | Technologies |
|---|---|
| **Backend** | .NET 10 (ASP.NET Core Web API), C# |
| **PDF Extraction** | `PdfPig` |
| **AI / Inference** | Local LLMs via `Ollama` (`phi3:mini`, `qwen2.5:1.5b`, `qwen2.5:3b`) |
| **Frontend** | React 19, TypeScript, Vite |
| **Security & Provenance** | SHA-256 hashing (`System.Security.Cryptography`) |

---

## Project Structure

```text
BriefMind/
├── BriefMind.Server/          # ASP.NET Core Web API (.NET 10)
│   ├── Api/                   # Controllers & DTOs
│   ├── Domain/                # Domain models (Clauses, Debates, Provenance)
│   ├── Infrastructure/        # Ollama HTTP client & DebateEngine
│   └── Services/              # PdfTextExtractor, ClauseSegmenter, ProvenanceService
├── briefmind.client/          # React 19 + TypeScript + Vite frontend
│   └── src/                   # Components, audit views, and API integration
└── README.md
```

---

## Prerequisites

1. **.NET 10 SDK**
2. **Node.js** (v18+ recommended) & **npm**
3. **Ollama** installed and running locally with models pulled:
   ```bash
   ollama pull phi3:mini
   ollama pull qwen2.5:1.5b
   ollama serve
   ```

---

## Getting Started

### 1. Run via Visual Studio
- Open `BriefMind.slnx` in Visual Studio 2026+.
- Set `BriefMind.Server` as the startup project and launch (`F5` or `Ctrl+F5`). The SPA proxy automatically starts the Vite client.

### 2. Run via Command Line

**Backend (`BriefMind.Server`)**:
```bash
cd BriefMind.Server
dotnet run
```
*API runs at `https://localhost:7234` or `http://localhost:5185`.*

**Frontend (`briefmind.client`)**:
```bash
cd briefmind.client
npm install
npm run dev
```
*Client runs at `https://localhost:54724`.*

---

## API Endpoints

- `POST /api/ContractAudit/upload`  
  Upload a contract PDF to extract text and segment clauses with SHA-256 hashes.
- `POST /api/ContractAudit/documents/{documentId}/audit`  
  Initiate multi-agent debate and risk scoring for all or targeted clauses.
- `GET /api/ContractAudit/bundles/{bundleId}/export`  
  Download the signed `AuditVerificationBundle` in JSON format.
