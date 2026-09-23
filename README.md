# SurakshaAR

**SIH 2026 · Problem Statement `SIH26041` · Team `IMMUTABLE-A` · Team Code `153165`**

| | |
|--|--|
| **Problem** | AR-Based Vocational Safety Training Platform |
| **Theme** | Smart Education · Government of Jharkhand |
| **Tagline** | Real Hazards. Real Decisions. Safer Tomorrow. |

AR-based workplace safety training for mining operations. Workers complete AR hazard scenarios, pass assessments, and earn verifiable QR certificates.

**APK:** [GitHub Releases](https://github.com/Devean1904/SurakshaAR-SIH26041/releases/tag/v1.0-sih2026) (`SurakshaAR.apk`, ~59 MB)

**Repo:** https://github.com/Devean1904/SurakshaAR-SIH26041

## Stack

| Layer | Tech |
|-------|------|
| Android | Kotlin, ARCore / SceneView 2.1, Retrofit, zxing |
| Backend | .NET 8, MongoDB, JWT, Swagger |
| Web admin | Static HTML/JS under `Backend/wwwroot/admin` |
| Public verify | `Backend/wwwroot/verify` |

## Repository layout

```
SurakshaAR/
├── SurakshaAR-Android/   # Android app (primary deliverable)
├── Backend/              # ASP.NET Core API + admin/verify web UI
├── Shared/               # Shared DTOs
├── AdminCLI/             # Optional admin CLI
├── Proposal/             # Project proposal (HTML + PDF)
├── SurakshaAR.slnx       # .NET solution
├── README.md
├── DEMO_SCRIPT.md        # Demo video script
├── WORKFLOW.md           # Development status checklist
├── PROJECT_VISION.md     # Vision + current architecture
└── .gitignore
```

## Prerequisites

- JDK 17+ and Android SDK (API 35)
- .NET 8 SDK
- MongoDB on `localhost:27017`
- Gradle 8.13 (or Android Studio)

## Run the backend

```powershell
cd E:\SurakshaAR\Backend
dotnet run
```

- API base: `http://localhost:5000/api`
- Health: `http://localhost:5000/health`
- Admin dashboard: `http://localhost:5000/admin/`
- Certificate verify: `http://localhost:5000/verify/`
- Swagger (Development): `http://localhost:5000/swagger`

Training modules are seeded automatically on first startup.

For LAN access from a physical phone, open firewall port 5000 (`Backend\open-firewall.bat` as admin).

## Build the Android APK

```powershell
cd E:\SurakshaAR\SurakshaAR-Android

# Emulator default (10.0.2.2 → host)
gradle --no-daemon :app:assembleDebug

# Physical device on same Wi-Fi as the PC
gradle --no-daemon :app:assembleDebug -PsurakshaApiBaseUrl=http://<pc-ip>:5000/api/
```

APK output: `app/build/outputs/apk/debug/app-debug.apk`

Install:

```powershell
adb install -r app\build\outputs\apk\debug\app-debug.apk
```

> **Note:** The default base URL is `http://10.0.2.2:5000/api/` (emulator only). Rebuild with `-PsurakshaApiBaseUrl=http://<your-pc-lan-ip>:5000/api/` for a real device.

## Features

- **Worker dashboard** — domain training cards, certificates, QR scan, settings, language selector
- **AR training** — plane detection, hazard/action markers, escalation engine, scoring
- **Assessment** — server-graded MCQs, 50/50 action+question weighting, module pass threshold (default 70)
- **Certificates** — QR payload `SURAKSHA:{certId}:{sha256}`, HMAC signature, local hash-chain tx; server re-checks pass threshold
- **QR verify (in-app + web)** — `GET /api/certificate/verify/{payload}` is anonymous
- **Offline sync** — failed assessment submits and training attempts queue locally and flush on next dashboard open via `/worker/online/sync`; prefetch uses `/worker/offline/{id}`
- **Manager panel** — workers, add worker, assign training, progress
- **Admin panel (app + web)** — workers/managers, escalations, site mapping, compliance, certificates list, attempts list
- **Localization** — English, Hindi, Santali, Marathi, Tamil, Telugu, Kannada via `LanguageManager`; all main screens localized
- **Screen reader** — VoiceManager TTS, tap-to-speak, Read Screen, Voice Packs (Santali TTS falls back sat → hi-IN → en)

## Default roles

| Role | Capabilities |
|------|----------------|
| Worker | AR training, assessment, certificate, QR scan |
| Manager | Assign training, view worker progress, escalations |
| Admin | User management, compliance, certificates, attempts, sites, escalations |

## API highlights

```
POST /api/auth/login
GET  /api/training/modules
GET  /api/assessment/{moduleId}/questions
POST /api/assessment/submit
POST /api/certificate/generate
GET  /api/certificate/verify/{qrPayload}   # anonymous
GET  /api/worker/offline/{workerId}
POST /api/worker/online/sync
GET  /api/admin/compliance
GET  /api/admin/certificates
GET  /api/admin/attempts
```

## Web admin tabs

- Workers, Managers, Analytics
- Compliance — pass rates, certificates, attempts
- Certificates — filter, open public `/verify/?payload=...`
- Attempts — unified assessment + training attempt list

## Demo

See [DEMO_SCRIPT.md](DEMO_SCRIPT.md) for a step-by-step demo video script.

Status checklist: [WORKFLOW.md](WORKFLOW.md).

Architecture / vision: [PROJECT_VISION.md](PROJECT_VISION.md).

## Security notes

- JWT auth with role policies (`AdminOnly`, `ManagerOrAdmin`, `AuthenticatedUser`)
- Certificate generation re-checks pass status server-side
- Passwords stored hashed; secrets via config (do not commit real secrets)
- Network security config allows only emulator/localhost/LAN hosts

## License

Prototype / hackathon submission — not licensed for production use.
