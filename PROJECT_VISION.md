# SurakshaAR - Project Vision & Status

> **"Real Hazards. Real Decisions. Safer Tomorrow."**

AR-Based Vocational Training Simulator for Industrial Safety in Jharkhand's Mining & Manufacturing Sector

**Last updated:** September 23, 2026 · **Problem:** SIH 26041 · **Team:** IMMUTABLE-A (153165)

---

## 1. Project Overview

**SurakshaAR** (Hindi *Suraksha* = Safety) is a mobile AR-based vocational training and safety certification platform for industrial workers in Jharkhand, India.

### The Problem

- Jharkhand is India's leading mineral-producing state
- Coal mines, steel plants, and mica units employ hundreds of thousands of workers
- Many are young tribal recruits with no prior industrial exposure
- **48 fatal mine accidents** recorded in Jharkhand in 2022–23
- Large share involving workers with **under 30 days of orientation**
- Classroom-based safety training retention rates **below 20% after one week**
- Live drills are operationally disruptive
- VR headset simulators are inaccessible to small-scale mines
- No standardized digital training platform exists in regional languages

### The Solution

A mobile AR app that:

- Trains workers using **real-world AR simulations** (fire, gas leaks, machinery)
- Runs on **mid-range Android phones** (no headset required)
- Works in **7 languages** (English, Hindi, Santali, Marathi, Tamil, Telugu, Kannada)
- Supports **offline** assessment/training sync when the device reconnects
- Issues **verifiable QR certificates** (server-anchored hash + local chain)
- Uses a **QR scanner** for checkpoint / certificate verification flows

---

## 2. Vision Statement

### How the app should feel

**Login & language**

- Dark, professional glass-card login
- Worker ID + password only (company resolved server-side)
- Language selector on the login screen; labels update immediately
- Voice reads titles and key actions (VoiceManager TTS)

**AR training**

- Camera plane detection → hazard/action markers in the real environment
- Timed escalation on inaction (penalties, auto-fail at critical)
- Score panel at completion; pass threshold default **70%**

**Certificates**

- Pass → QR certificate with payload `SURAKSHA:{certId}:{sha256}`
- In-app scan + public web `/verify/` page
- Server re-checks pass status before generation

**Roles**

- Worker → dashboard, AR, assessment, certificate, QR, settings
- Manager → roster, assign training, progress, escalations
- Admin → users, compliance, sites, certificates, attempts (app + web)

---

## 3. Problem Statement (SIH 26041)

| Field | Value |
|-------|--------|
| **ID** | 26041 |
| **Title** | AR-Based Vocational Training Simulator for Industrial Safety in Jharkhand's Mining & Manufacturing Sector |
| **Organization** | Government of Jharkhand |
| **Department** | Department of Higher & Technical Education |
| **Category** | Software |
| **Theme** | Smart Education |

### Minimum submission requirements

| Requirement | Status |
|-------------|--------|
| AR training modules (≥2 of 5 domains) | Done — domain cards seeded server-side |
| Assessment engine | Done — server-graded, 50/50 action+question |
| Certificates (QR generate + verify) | Done — API + in-app + web `/verify/` |
| Languages Hindi + Santali | Done — plus EN/MR/TA/TE/KN (7 total) |
| Offline | Done — local queue + `/worker/online/sync` |
| Admin dashboard (web) | Done — `Backend/wwwroot/admin` |
| Delivery APK + demo + GitHub | Done — release `v1.0-sih2026` |

### Safety domains (seeded modules)

1. **Fire & Explosion Response**
2. **Gas Leak & Confined Space Protocol**
3. **Machinery Safety**
4. **Electrical Safety**
5. **Working at Heights**

---

## 4. Who we're building for

| Role | Interface | Key features |
|------|-----------|--------------|
| **Worker** | Worker dashboard | AR training, assessments, certificates, progress |
| **Manager** | Manager panel | Worker roster, assign training, escalations |
| **Admin** | Admin panel (app + web) | Users, sites, compliance, certificates, attempts |

### Worker profile

- Young tribal recruits in Jharkhand; limited digital literacy
- Hindi, Santali, or other regional languages
- Mid-range Android phones (minSdk 28)
- Often limited/no connectivity after initial session

### Key behaviors

1. Worker enters Worker ID + password → backend resolves role → direct landing
2. Open a domain card → AR session (or assessment offline path)
3. Act within time limits → inaction causes escalation
4. Complete training → assessment → certificate if passed
5. Offline submissions flush when the device is back online

---

## 5. Tech stack (current — native path)

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Client** | Kotlin + Android (API 28+) | Primary app |
| **AR** | ARCore / SceneView 2.1 | Plane detection, markers |
| **HTTP** | Retrofit + OkHttp | API |
| **QR** | zxing IntentIntegrator | Scan certificates / checkpoints |
| **TTS** | Android TTS (VoiceManager) | Screen reader, 7-lang with sat→hi→en fallback |
| **Backend** | ASP.NET Core 8.0 | REST API, auth, business logic |
| **Database** | MongoDB (Driver 2.28.0) | Users, modules, certificates, attempts |
| **Auth** | JWT + BCrypt | Secure authentication |
| **Shared** | `Shared/` class library | DTOs used by Backend |
| **Web admin** | Static HTML/JS (`wwwroot/admin`) | Compliance, certs, attempts |
| **Public verify** | `wwwroot/verify` | Anonymous QR payload check |

> **Legacy:** Unity 6 prototype was removed from the repository. All product work is on the Kotlin + .NET path.

---

## 6. System architecture

```
+-------------------------------------------------+
|                 FIELD / DEVICE                   |
|  Android app (Kotlin)                            |
|  - Login / roles / 7 languages / TTS             |
|  - AR training (SceneView + markers)             |
|  - Assessment + offline queue                    |
|  - Certificate QR view + scanner                 |
+-----------------------+-------------------------+
                        | HTTPS REST/JSON
                        v
+-------------------------------------------------+
|              BACKEND (ASP.NET Core 8.0)          |
|  Auth (JWT) · Training · Assessment · Admin      |
|  Certificate generate/verify · Offline sync      |
|  Escalations · Site mapping                      |
+-----------------------+-------------------------+
                        |
          +-------------+-------------+
          v                           v
   MongoDB (primary)          Local hash-chain
   users, modules,            (certificate tx)
   certs, attempts
          |
          v
   Web admin + /verify (static under wwwroot)
```

---

## 7. Screen map (implemented)

### Worker

| Screen | Purpose |
|--------|---------|
| Login | Worker ID + password, language panel, OTP path |
| Dashboard | Domain cards, status, QR scan, offline cues |
| Site Map | Planes / anchors / save |
| AR Training | Plane detection, markers, escalation, score |
| Assessment | Server-graded MCQs |
| Results | Pass/fail breakdown |
| Certificate | QR certificate + verify |
| Profile / Settings | Account, language, voice, theme |

### Manager

| Screen | Purpose |
|--------|---------|
| My Workers | Roster + status |
| Add Worker | Create worker |
| Assign Training | Assign modules |
| Progress | Scores / attempts / last attempt |

### Admin (app + web)

| Area | Purpose |
|------|---------|
| Workers / Managers | Approve, list, add |
| Analytics | Attempt stats |
| Sites | Mapping / site IDs |
| Compliance | Pass rates |
| Certificates / Attempts | Lists + public verify link |

---

## 8. AR training flow

```
1. OPEN DOMAIN CARD
   → camera permission if needed
        |
2. AR SESSION
   → plane detection → tap/place markers
   → hazard (red) + action (green) markers from scenario config
        |
3. RESPOND
   correct action within time → score +=
   ignore/delay → escalation (penalties → auto-fail)
        |
4. END SCENARIO
   → results panel (action score)
   → assessment questions (server-graded)
        |
5. CERTIFICATE (if pass ≥ threshold)
   → QR payload SURAKSHA:{id}:{hash}
   → verify in-app or /verify/
```

---

## 9. Escalation system

**Escalation is a consequence of inaction, not random events.**

| Level | Behavior | Score |
|-------|----------|-------|
| Normal | Green / ready | 0% penalty |
| Minor | Yellow warning | −10% |
| Moderate | Orange + urgent cue | −25% |
| Severe | Red + critical cue | −50% |
| Critical | Auto-fail | Failed |

Configured per scenario in backend training modules; Android `ARTrainingActivity` applies penalties and `triggerAutoFail` at critical.

---

## 10. Assessment engine

| Component | Current behavior |
|-----------|------------------|
| Questions | From `/api/assessment/{moduleId}/questions` |
| Weighting | **50% actions + 50% questions** (client and server aligned) |
| Pass threshold | Module threshold, default **70** |
| Grading | Server authoritative on submit |
| Offline | Local queue → flush via `/worker/online/sync` |

---

## 11. Certificate system

### Generation

1. Worker passes (client flag + server total ≥ threshold)
2. Backend builds cert record (id, worker, module, score, date)
3. Payload `SURAKSHA:{certId}:{sha256}` + HMAC signature
4. Local hash-chain transaction on device; MongoDB store on server
5. QR shown on certificate screen

### Verification

| Method | Internet | How |
|--------|----------|-----|
| In-app QR scan | Yes (API) | `GET /api/certificate/verify/{payload}` |
| Web `/verify/` | Yes | Same anonymous endpoint |
| Offline payload format | Format-only | Signature/hash structure still parseable |

> Polygon testnet anchoring is a follow-up; current chain is local + server HMAC.

---

## 12. Voice navigation & localization

| Language | Code | UI strings | TTS |
|----------|------|------------|-----|
| English | en | Yes | Android TTS |
| Hindi | hi | Yes | Android TTS |
| Santali | sat | Yes | sat → hi-IN → en fallback |
| Marathi | mr | Yes | Android TTS |
| Tamil | ta | Yes | Android TTS |
| Telugu | te | Yes | Android TTS |
| Kannada | kn | Yes | Android TTS |

- All main screens localized via `LanguageManager` (`t(key, en, hi, sat, mr, ta, te, kn)`)
- Settings: voice toggle, Read Screen, tap-to-speak
- Default language: Santali

---

## 13. Training modules

Seeded on backend startup (examples):

| Module | Domain |
|--------|--------|
| Fire & Explosion | Exit ID, extinguisher, evacuation |
| Gas Leak / Confined Space | Hazard zone, PPE, buddy system |
| Machinery | LOTO, e-stop, guards |
| Electrical | Isolation, insulated tools |
| Heights | Harness, anchors, fall protection |

Admin can list/assign; manager assigns to workers; worker completes via AR + assessment.

---

## 14. Current project state

### Works (shipping)

| Area | Status |
|------|--------|
| Backend API + JWT + Swagger | Working |
| MongoDB modules / users / certs / attempts | Working |
| Android login, dashboard, settings, profile | Working |
| AR training + escalation + scoring | Working |
| Assessment client/server aligned | Working |
| Certificates + QR + web verify | Working |
| Offline sync endpoints + client queue | Working |
| Manager + Admin panels (app) | Working |
| Web admin (workers, compliance, certs, attempts) | Working |
| 7-language UI + Santali TTS chain | Working |
| GitHub repo + release APK | Working |

### Optional / follow-up

| Item | Priority |
|------|----------|
| Release keystore signing | High for store; OK for demo |
| CI / instrumented tests | Medium |
| Polygon on-chain anchor | Medium |
| Production HTTPS / secrets management | High if deployed beyond demo |

### Removed from repo (intentional)

| Item | Reason |
|------|--------|
| `UnityAR/` | Legacy prototype; superseded by Kotlin app |
| `REF/` | Reference docs / mockups not part of build |
| Root `SurakshaAR.apk` | Distributed via GitHub Releases instead |

---

## 15. Project file structure

```
SurakshaAR/
├── SurakshaAR.slnx
├── Backend/                 # ASP.NET Core 8 API
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Controllers/
│   ├── Services/
│   ├── Config/
│   ├── wwwroot/admin/       # Web admin
│   ├── wwwroot/verify/      # Public QR verify
│   ├── start-server.bat
│   └── open-firewall.bat
├── Shared/
│   └── Models.cs            # Shared DTOs
├── AdminCLI/
│   └── Program.cs           # Optional console admin
├── SurakshaAR-Android/      # Kotlin app (primary)
│   └── app/src/main/
│       ├── java/com/surakshaar/
│       │   ├── ui/          # Activities (login, dashboard, AR, …)
│       │   ├── data/        # ApiClient, models, repo, session
│       │   ├── language/    # LanguageManager (7 langs)
│       │   └── util/        # VoiceManager, ThemeManager
│       └── res/             # Layouts, drawables, color states
├── Proposal/                # SIH proposal HTML + PDF
├── README.md
├── DEMO_SCRIPT.md
├── WORKFLOW.md
└── PROJECT_VISION.md
```

---

## 16. Key file reference

| File | Purpose |
|------|---------|
| `Backend/Program.cs` | Entry, JWT, Swagger, seed |
| `Backend/Controllers/*.cs` | Auth, admin, manager, worker, cert, assessment |
| `Shared/Models.cs` | Domain DTOs |
| `SurakshaAR-Android/.../ApiClient.kt` | Retrofit base URL (`BuildConfig.API_BASE_URL`) |
| `.../language/LanguageManager.kt` | 7-language keys |
| `.../util/VoiceManager.kt` | TTS + sat→hi→en chain |
| `.../ui/login/LoginActivity.kt` | Login + OTP + language panel |
| `.../ui/training/ARTrainingActivity.kt` | AR session, markers, escalation |
| `.../ui/assessment/AssessmentActivity.kt` | Assessment UI + submit |
| `Backend/wwwroot/admin/` | Web admin |
| `Backend/wwwroot/verify/` | Public certificate verify |

---

## 17. Reference

| Document | Location |
|----------|----------|
| Proposal | `Proposal/SurakshaAR_Proposal.html` (+ PDF) |
| Demo script | `DEMO_SCRIPT.md` |
| Status checklist | `WORKFLOW.md` |
| README / run instructions | `README.md` |
| GitHub | https://github.com/Devean1904/SurakshaAR-SIH26041 |
| Release APK | https://github.com/Devean1904/SurakshaAR-SIH26041/releases/tag/v1.0-sih2026 |

---

*Project: SurakshaAR — SIH Problem Statement 26041*
