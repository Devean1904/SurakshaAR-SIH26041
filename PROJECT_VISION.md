# SurakshaAR - Project Vision & Complete Status

> **"Real Hazards. Real Decisions. Safer Tomorrow."**

AR-Based Vocational Training Simulator for Industrial Safety in Jharkhand's Mining & Manufacturing Sector

---

## 1. Project Overview

**SurakshaAR** (from Hindi "Suraksha" meaning "Safety") is a mobile AR-based vocational training and safety certification platform for industrial workers in Jharkhand, India.

### The Problem

- Jharkhand is India's leading mineral-producing state
- Coal mines, steel plants, mica processing units employ hundreds of thousands of workers
- Many are young tribal recruits with no prior industrial exposure
- **48 fatal mine accidents** recorded in Jharkhand in 2022-23
- Large share involving workers with **under 30 days of orientation**
- Classroom-based safety training retention rates **below 20% after one week**
- Live drills are operationally disruptive
- VR headset simulators are inaccessible to small-scale mines
- No standardized digital training platform exists in regional languages

### The Solution

A mobile AR app that:

- Trains workers using **real-world AR simulations** (fire, gas leaks, machinery)
- Runs on **mid-range Android phones** (no headset required)
- Works in **7 regional languages** (Hindi, Santali, Marathi, Tamil, Telugu, Kannada, English)
- Provides **offline training** capability
- Issues **blockchain-verified certificates**
- Uses **QR-based checkpoint system** for physical training zones

---

## 2. Vision Statement

### How The Final App Should Feel

**When a worker opens the app:**

- Dark, professional theme with mining/industrial imagery
- Feels like a serious training tool, not a game
- Speaks their language (Hindi, Santali, or others)
- Voice navigation reads out every button they tap

**When a worker starts AR training:**

- Scans a QR code at a physical checkpoint
- Phone camera shows their real environment
- A 3D hazard appears in their actual workspace (fire, gas leak, etc.)
- HUD shows real-time danger level and timer
- They must take correct action (use extinguisher, evacuate, select PPE)
- If they ignore it, the situation escalates (Minor -> Moderate -> Severe -> Critical)
- Score and feedback at the end
- Certificate issued if they pass

**The reference demo (YouTube video) shows:**

- User scans room with phone camera
- 3D industrial machinery placed on physical floor
- Green gas cloud appears (methane leak simulation)
- Real-time gas concentration reading displayed
- Red warning alert: "CRITICAL METHANE LEAK DETECTED"
- Green AR arrows on ground showing evacuation route
- Safety instructions projected on screen
- Scorecard at completion with passed criteria and response time

**The UI should match the ChatGPT mockup images:**

- Dark theme with mining worker backgrounds
- Role-based landing pages (Worker, Manager, Admin)
- Cards-based layout with progress tracking
- Bottom navigation (Home, Training, Certificate, Profile)
- Professional, clean, accessible design

---

## 3. Problem Statement (SIH 26041)

**Problem Statement ID:** 26041
**Title:** AR-Based Vocational Training Simulator for Industrial Safety in Jharkhand's Mining & Manufacturing Sector
**Organization:** Government of Jharkhand
**Department:** Department of Higher & Technical Education
**Category:** Software
**Theme:** Smart Education

### Minimum Submission Requirements

| Requirement | Minimum |
|-------------|---------|
| AR Training Modules | At least **2** of 5 safety domains |
| Assessment Engine | Must score worker performance |
| Certificates | QR-based generation + verification |
| Languages | Hindi + Santali (mandatory) |
| Offline | Training must work without internet |
| Admin Dashboard | Web-based compliance view |
| Delivery | Working APK + demo video + GitHub repo |

### The 5 Safety Domains

1. **Fire & Explosion Response** - exit identification, extinguisher use, evacuation sequencing
2. **Gas Leak & Confined Space Protocol** - hazard zone recognition, PPE selection, buddy-system
3. **Machinery Safety** - lockout/tagout, emergency stop, guard inspection
4. **Electrical Safety** - live wire detection, insulated tools, isolation procedures
5. **Working at Heights** - harness inspection, anchor points, fall protection

---

## 4. Who We're Building For

### Roles

| Role | Interface | Key Features |
|------|-----------|--------------|
| **Worker** | Worker Home | AR training, assessments, certificates, progress tracking |
| **Manager** | Manager Home | Worker roster, compliance tracking, certificate verification, analytics |
| **Admin** | Admin Home | User management, site/module config, blockchain keys, system analytics |

### Worker Profile

- Young tribal recruits in Jharkhand
- Limited digital literacy
- Speak Hindi, Santali, or other regional languages
- Work in coal mines, steel plants, mica processing units
- Use mid-range Android phones (Android 10+)
- May have limited/no internet connectivity

### Key Behaviors

- Worker enters Employee ID + PIN -> Backend resolves role -> Direct landing (no role picker)
- Worker scans QR at checkpoint -> AR training starts
- Worker must act within time limit -> Inaction causes escalation
- Worker completes training -> Assessment -> Certificate (if passed)
- Everything works offline after initial download

---

## 5. Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Engine** | Unity 6000.6.2f1 | AR rendering, game logic, UI |
| **Language** | C# | All Unity scripting |
| **AR** | AR Foundation 6.6.2 + ARCore 6.6.2 | Cross-platform AR (Android) |
| **Backend** | ASP.NET Core 8.0 | REST API, authentication, business logic |
| **Database** | MongoDB (MongoDB.Driver 2.28.0) | User data, training content, certificates |
| **Auth** | JWT (HMAC-SHA256) + BCrypt | Secure authentication |
| **SMS** | Twilio | OTP delivery |
| **Blockchain** | Polygon Testnet | Certificate hash anchoring |
| **Certificate** | Ed25519 | Digital signature |
| **TTS** | Android Native TTS + Bundled Audio | Voice navigation |
| **Build Target** | Android (min SDK 29, ARM64, IL2CPP) | Mobile deployment |

---

## 6. System Architecture

```
+-------------------------------------------------+
|                    FIELD ENVIRONMENT              |
|  Mines  |  Steel Plants  |  Mica Units           |
|  QR Checkpoints placed at training zones         |
+-----------------------+-------------------------+
                        | Scan QR
                        v
+-------------------------------------------------+
|                 WORKER DEVICE (Android)           |
|                                                  |
|  +----------+  +----------+  +----------+       |
|  | Unity 6  |  |   AR     |  |  Voice   |       |
|  |  Engine  |  |Foundation|  |Navigation|       |
|  |  (C#)    |  |(ARCore)  |  |(7 langs) |       |
|  +----------+  +----------+  +----------+       |
|                                                  |
|  +--------------------------------------+       |
|  | Local Storage (Room / JSON)          |       |
|  | - Training progress  - Certificates  |       |
|  | - Offline cache      - Blockchain    |       |
|  +--------------------------------------+       |
+-----------------------+-------------------------+
                        | HTTPS REST/JSON (when online)
                        v
+-------------------------------------------------+
|              BACKEND (ASP.NET Core 8.0)           |
|                                                  |
|  +------------+  +------------+  +------------+  |
|  |   Auth     |  |  Training  |  |Certificate |  |
|  |  Service   |  |  Service   |  |  Service   |  |
|  | (JWT+OTP)  |  |(Modules,   |  |(Ed25519    |  |
|  |            |  | Scenarios) |  | Signing)   |  |
|  +------------+  +------------+  +------------+  |
|                                                  |
|  +------------+  +------------+  +------------+  |
|  |  User &    |  |  Analytics |  | Blockchain |  |
|  |  Site Mgmt |  | & Reports  |  |  Anchor    |  |
|  |            |  |            |  | (Polygon)  |  |
|  +------------+  +------------+  +------------+  |
+-----------------------+-------------------------+
                        |
                        v
+-------------------------------------------------+
|                    DATA LAYER                     |
|                                                  |
|  +----------------+  +----------------+         |
|  |   MongoDB      |  |Object Storage  |         |
|  |  (Primary DB)  |  |(3D Models,     |         |
|  |                |  | Audio, Assets) |         |
|  +----------------+  +----------------+         |
|                                                  |
|  +----------------+  +----------------+         |
|  | Polygon        |  | Ed25519        |         |
|  | Testnet        |  | Signing Key    |         |
|  | (Cert Hash)    |  | (Backend Only) |         |
|  +----------------+  +----------------+         |
+-------------------------------------------------+
```

---

## 7. Screen Map

### Worker Screens

| Screen | Purpose |
|--------|---------|
| Splash | Brand logo + loading |
| Login | Employee ID + PIN/password |
| Worker Home | Today's training, progress (X/5), certificates, settings |
| Site Map | Training locations with checkpoint list |
| QR Scanner | Scan checkpoint QR to start training |
| AR Training | Main AR simulation experience |
| Assessment | Randomized questions after scenario |
| Score Result | Pass/fail, breakdown, response time |
| Certificate | QR-verified certificate view |
| Settings | Language, voice, theme, accessibility |
| Offline Mode | Offline status + sync progress |

### Manager Screens

| Screen | Purpose |
|--------|---------|
| Manager Home | Worker roster, compliance overview |
| Worker Details | Individual worker progress |
| Certificate Verification | QR scan to verify certificates |
| Reports | Site-wise compliance analytics |

### Admin Screens

| Screen | Purpose |
|--------|---------|
| Admin Home | System overview, user count, analytics |
| User Management | Create/edit/delete users, assign roles |
| Site Management | Training sites, checkpoints, modules |
| Module Config | Training content, scenarios, questions |
| Blockchain Keys | Ed25519 key management |
| System Analytics | Training stats, completion rates |

---

## 8. AR Training Flow

```
1. SCAN CHECKPOINT QR
   Worker scans QR at physical training zone
   -> Identifies location, loads correct scenario
        |
        v
2. AR SESSION STARTS
   Camera feed + floor detection active
   3D hazard appears at real-world location
   (fire, gas leak, chemical spill, etc.)
        |
        v
3. HUD ACTIVATED
   - Hazard type + level indicator
   - Real-time metrics (gas %, temperature, etc.)
   - Countdown timer
   - Safety instructions text
   - Action buttons (Use Extinguisher / Evacuate / etc.)
        |
        v
4. WORKER RESPONDS

   IF correct action within time limit:
     -> Hazard resolves
     -> Green checkmark
     -> Good score

   IF worker ignores/delays:
     -> Escalation timer counts down
     -> Minor -> Moderate -> Severe -> Critical
     -> Score penalties at each level
     -> Auto-fail at Critical
        |
        v
5. SCENARIO ENDS
   -> Assessment screen appears
   -> Randomized questions from pool
   -> Action score + question score combined
   -> Pass/fail determination
        |
        v
6. CERTIFICATE (if passed)
   -> QR-verified certificate generated
   -> Ed25519 signed
   -> Hash anchored to Polygon testnet
   -> Stored locally + synced when online
```

---

## 9. Escalation System

### Design Principle

**Escalation is a consequence of inaction, not random events.**

If the worker takes correct action quickly -> no escalation.
If the worker ignores/delays -> situation worsens automatically.

### Escalation Levels

| Level | Time Elapsed | Visual | Audio | Score Penalty |
|-------|-------------|--------|-------|---------------|
| **Normal** | 0-15s | Green HUD | Voice: "Action required" | 0% |
| **Minor** | 15-30s | Yellow alert | Alarm sound | -10% |
| **Moderate** | 30-45s | Orange alert + flashing | Urgent alarm | -25% |
| **Severe** | 45-60s | Red alert + screen shake | Critical alarm | -50% |
| **Critical** | 60s+ | Red screen + auto-fail | Buzzer | FAILED |

### Scenario: Fire Training

```
0s    -> Fire appears, smoke visible
      -> HUD: "Fire detected! Use extinguisher!"
      -> Timer starts

15s   -> Worker hasn't moved
      -> Minor escalation: "Fire spreading!"
      -> Smoke increases, flames grow

30s   -> Worker still idle
      -> Moderate escalation: "Fire intensifying!"
      -> Room fills with smoke

45s   -> Worker still idle
      -> Severe escalation: "DANGER! Evacuate immediately!"
      -> Fire engulfs area

60s   -> Critical timeout
      -> "Scenario Failed - Fire too large"
      -> Score: 0/100
      -> Assessment still shown for learning
```

### Scenario: Gas Leak Training

```
0s    -> Green gas cloud appears near equipment
      -> HUD: "Methane detected! Level: 1.5%"
      -> Timer starts

15s   -> Worker hasn't moved
      -> Minor escalation: "Level rising! 2.8%"
      -> Gas cloud expands

30s   -> Worker still idle
      -> Moderate escalation: "Level: 4.2% - Danger zone!"
      -> Red perimeter appears

45s   -> Worker still idle
      -> Severe escalation: "CRITICAL LEVEL: 6.1%"
      -> Evacuation arrows appear

60s   -> Critical timeout
      -> "Scenario Failed - Evacuation required"
      -> Score: 0/100
```

---

## 10. Assessment Engine

### Structure

| Component | Details |
|-----------|---------|
| Questions per module | 5-10 (randomized from pool) |
| Question types | Multiple choice, action sequence, true/false |
| Action score | 60% of total (based on AR performance) |
| Question score | 40% of total (based on quiz) |
| Pass threshold | 70% overall |
| Retake policy | Allowed after 24 hours |

### Scoring Breakdown

```
Total Score = Action Score (60%) + Question Score (40%)

Action Score factors:
  - Response time (how fast did they act?)
  - Correct action (did they use the right tool?)
  - Escalation level reached (penalty for delay)
  - Safety protocol followed

Question Score factors:
  - Correct answers to randomized questions
  - No negative marking
  - All questions weighted equally
```

### Certificate Criteria

- Total score >= 70%
- All critical actions completed
- No Critical escalation reached
- All mandatory questions answered

---

## 11. Certificate System

### Generation Flow

```
Worker passes assessment
        |
        v
Backend generates certificate data
  - Certificate ID (unique)
  - Employee ID + Name
  - Module completed
  - Score + Date
  - QR code payload
        |
        v
Ed25519 signing service
  - Signs certificate data with private key
  - Private key stays on server (never in APK)
        |
        v
QR code generated
  - Contains certificate ID + hash
  - Verifiable offline via QR scan
        |
        v
Blockchain anchor (optional)
  - Hash submitted to Polygon testnet
  - Immutable public record
        |
        v
Certificate stored
  - Local on device (JSON)
  - MongoDB (server)
  - QR code image (PNG)
```

### Verification Methods

| Method | Requires Internet | How It Works |
|--------|------------------|--------------|
| QR Scan (offline) | No | Scan QR -> verify signature with embedded public key |
| Blockchain check | Yes | Scan QR -> check hash on Polygon testnet |
| Manual entry | Yes | Enter certificate ID -> lookup in MongoDB |

### Certificate Data Model

```json
{
  "certificateId": "CERT-2026-001234",
  "employeeId": "EMP00123",
  "employeeName": "Ramesh Kumar",
  "moduleId": "fire-safety-101",
  "moduleName": "Fire & Explosion Response",
  "score": 85,
  "passed": true,
  "issuedAt": "2026-09-20T10:30:00Z",
  "signature": "ed25519_signature_here",
  "blockchainTx": "0x...",
  "qrPayload": "SURAKSHA:CERT-2026-001234:HASH..."
}
```

---

## 12. Voice Navigation

### Languages

| Language | Code | TTS Source | Status |
|----------|------|-----------|--------|
| English | en | Android TTS | Working |
| Hindi | hi | Android TTS | Working |
| Santali | sat | Bundled Audio | Working |
| Marathi | mr | Android TTS | Needs building |
| Tamil | ta | Android TTS | Needs building |
| Telugu | te | Android TTS | Needs building |
| Kannada | kn | Android TTS | Needs building |

### Behavior

- Toggle on/off in Settings
- When ON: every tapped element is spoken in selected language
- Speech queue with debounce (prevents overlapping)
- Voice speed adjustable
- AR critical announcements spoken without changing visual state

### Example Voice Prompts

| Action | English | Hindi | Santali |
|--------|---------|-------|---------|
| Tap button | "Login button" | "Log in button" | "Log in button" |
| Fire detected | "Fire detected. Use extinguisher!" | "Fire detected! Use extinguisher!" | "Fire detected! Use extinguisher!" |
| Evacuate | "Evacuation route activated. Follow the path." | "Evacuation route activated. Follow the path." | "Evacuation route activated. Follow the path." |
| Passed | "Training complete. You passed!" | "Training complete. You passed!" | "Training complete. You passed!" |

### Screen Reader (TalkBack)

- Full Android TalkBack compatibility
- All interactive elements have content descriptions
- Double-tap interaction for accessibility
- ScreenReaderHook.cs handles click/enter events

---

## 13. Training Modules

### Module 1: Fire & Explosion Response

| Element | Details |
|---------|---------|
| Hazard | Fire with smoke, growing flames |
| 3D Assets | Fire extinguisher, exit signs, burning debris |
| Actions | Use extinguisher, evacuate, raise alarm |
| Escalation | Fire spreads if ignored |
| Assessment | 5 questions on fire safety |

### Module 2: Gas Leak & Confined Space

| Element | Details |
|---------|---------|
| Hazard | Green methane cloud, gas concentration HUD |
| 3D Assets | Gas detector, PPE, ventilation equipment |
| Actions | Detect leak, select PPE, activate ventilation, evacuate |
| Escalation | Gas level rises if ignored |
| Assessment | 5 questions on gas safety |

### Future Modules (Optional)

| Module | Hazard | Key Action |
|--------|--------|------------|
| Machinery Safety | Moving parts, entanglement | Lockout/tagout, emergency stop |
| Electrical Safety | Live wires, arc flash | Isolate power, insulated tools |
| Working at Heights | Fall risk | Harness inspection, anchor points |

---

## 14. Current Project State

### What EXISTS and WORKS

| Feature | File(s) | Status |
|---------|---------|--------|
| Login + OTP authentication | AuthController.cs, AuthService.cs, LoginManager.cs | Working |
| Role-based routing (no role picker) | UINavigator.cs, Program.cs | Working |
| Dark/Light theme | ThemeManager.cs | Working |
| Offline data sync | OfflineManager.cs | Working |
| Local SHA-256 blockchain | LocalBlockchain.cs | Working |
| AR plane detection | ARSessionSetup.cs | Working |
| AR anchor placement (async) | ARSiteMapper.cs | Working |
| Basic scenario prefab spawning | ARScenarioPlayer.cs | Working |
| Escalation reporting | EscalationManager.cs | Working |
| English localization (48 keys) | LanguageManager.cs, en.json | Working |
| Hindi localization (48 keys) | LanguageManager.cs, hi.json | Working |
| Santali localization (48 keys) | LanguageManager.cs, sat.json | Working |
| Android TTS voice module | VoiceModule.cs | Working |
| Screen reader hook | ScreenReaderHook.cs | Working |
| Settings screen | SettingsScreen.cs | Working |
| Backend API | Program.cs, Controllers/, Services/ | Working |
| MongoDB connection | MongoService.cs | Working |
| JWT authentication | Program.cs | Working |
| Swagger/OpenAPI | Program.cs | Working |
| SMS via Twilio | SmsService.cs | Working |
| Custom safety shader | SafetyHighlight.shader | Working |
| Prefab generator | PrefabGenerator.cs | Working |
| Android build config | AndroidBuildConfig.cs | Working |
| Unity project compiles | 0 errors, 0 warnings | Working |

### What is PARTIALLY Done

| Feature | Status | Missing |
|---------|--------|---------|
| Voice navigation | 3/7 languages | Marathi, Tamil, Telugu, Kannada |
| Scenario prefabs | Primitive shapes only | 3D models, particles |
| Blockchain | Local only | Polygon testnet integration |
| Escalation | Basic reporting | Timer-based escalation engine |
| UI | Basic panels | Full screens matching mockup |

### What DOES NOT Exist

| Feature | Priority |
|---------|----------|
| Unity scenes (LoginScene, MainScene) | Critical |
| Worker/Manager/Admin landing page UI | Critical |
| QR checkpoint scanner | Critical |
| AR fire simulation (particles, smoke, light) | Critical |
| AR gas leak simulation (green cloud, HUD) | Critical |
| Real-time HUD overlay | Critical |
| Evacuation route AR arrows | High |
| Timer-based escalation engine | Critical |
| Assessment engine (questions, scoring) | Critical |
| Certificate generation (QR + Ed25519) | Critical |
| Certificate verification | High |
| Polygon blockchain anchor | High |
| Web admin dashboard | Medium |
| 3D models (fire extinguisher, PPE, etc.) | High |
| Particle effects (fire, smoke, sparks) | High |
| Multiple training modules (2+) | Critical |
| Module config system (data-driven) | Medium |
| 4 more language translations | High |

---

## 15. What Needs Building

### Priority 1: Core (Must Have for Demo)

| Feature | Est. Time | Dependencies |
|---------|-----------|-------------|
| Create LoginScene + MainScene | 1 day | Unity Editor |
| Worker Landing Page UI | 1 day | MainScene |
| Manager Landing Page UI | 0.5 day | MainScene |
| Admin Landing Page UI | 0.5 day | MainScene |
| AR Fire Simulation | 2 days | MainScene, particles |
| AR Gas Leak Simulation | 2 days | MainScene, particles |
| Real-time HUD Overlay | 1 day | MainScene |
| Timer-based Escalation Engine | 2 days | HUD, scenario system |
| Assessment Engine | 2 days | Scenario system |
| Certificate Generation | 2 days | Backend, QR library |
| QR Checkpoint Scanner | 2 days | AR Foundation |
| **Subtotal** | **16 days** | |

### Priority 2: Important (Should Have)

| Feature | Est. Time | Dependencies |
|---------|-----------|-------------|
| Evacuation Route AR Arrows | 1 day | AR Foundation |
| Certificate Verification | 1 day | Certificate system |
| Polygon Blockchain Anchor | 2 days | Backend, Polygon API |
| 3D Models (fire extinguisher, PPE) | 2 days | 3D assets |
| Particle Effects (fire, smoke) | 1 day | Unity VFX |
| Voice Nav - 4 more languages | 2 days | Language files |
| Full TalkBack Accessibility | 1 day | Screen reader |
| Offline Module Download | 1 day | Backend, storage |
| **Subtotal** | **11 days** | |

### Priority 3: Nice to Have (Post-Demo)

| Feature | Est. Time | Dependencies |
|---------|-----------|-------------|
| Web Admin Dashboard | 3 days | Backend API |
| Module Config System | 2 days | Backend, MongoDB |
| Multiple Training Modules | 3 days | 3D assets, scenarios |
| Site Map Screen | 1 day | Backend, maps |
| **Subtotal** | **9 days** | |

### Total Estimated Effort

| Priority | Days |
|----------|------|
| Priority 1 (Core) | 16 |
| Priority 2 (Important) | 11 |
| Priority 3 (Nice to Have) | 9 |
| **Total** | **36 days** |

---

## 16. Project File Structure

```
E:\SAFE\
|
+-- SurakshaAR.slnx                    # .NET solution file
|
+-- Backend/                            # ASP.NET Core 8.0 API
|   +-- SurakshaAR.Backend.csproj
|   +-- Program.cs                      # Entry point, JWT, Swagger, DI
|   +-- appsettings.json                # MongoDB, JWT, Twilio config
|   +-- Config/
|   |   +-- MongoConfig.cs              # MongoDB configuration POCO
|   +-- Controllers/
|   |   +-- AuthController.cs           # Login, OTP, user endpoints
|   |   +-- AdminController.cs          # User/site/escalation management
|   |   +-- ManagerController.cs        # Worker assignment, scenarios
|   |   +-- WorkerController.cs         # View scenarios, report escalation
|   +-- Services/
|       +-- AuthService.cs              # JWT, registration, OTP
|       +-- BlockchainService.cs        # SHA-256 hash chain (server)
|       +-- EscalationService.cs        # Incident reports
|       +-- ScenarioService.cs          # Site mapping, scenarios
|       +-- OtpService.cs               # OTP generation
|       +-- MongoService.cs             # MongoDB connection
|       +-- SmsService.cs               # Twilio integration
|
+-- Shared/                             # Common models
|   +-- SurakshaAR.Shared.csproj
|   +-- Models.cs                       # User, Scenario, Escalation, etc.
|
+-- AdminCLI/                           # Console admin tool
|   +-- SurakshaAR.AdminCLI.csproj
|   +-- Program.cs                      # Create users, seed data
|
+-- UnityAR/                            # Unity 6 AR client
|   +-- Assets/
|   |   +-- Scripts/
|   |   |   +-- AppBootstrap.cs         # Singleton manager spawner
|   |   |   +-- AR/
|   |   |   |   +-- ARSessionSetup.cs  # AR lifecycle management
|   |   |   |   +-- ARSiteMapper.cs    # Tap-to-place anchors (async)
|   |   |   +-- Auth/
|   |   |   |   +-- LoginManager.cs    # Login/OTP UI
|   |   |   +-- Blockchain/
|   |   |   |   +-- LocalBlockchain.cs # Client-side SHA-256 chain
|   |   |   +-- Citification/
|   |   |   |   +-- CitificationManager.cs  # Zone safety display
|   |   |   +-- Escalation/
|   |   |   |   +-- EscalationManager.cs    # Report hazards
|   |   |   +-- Language/
|   |   |   |   +-- LanguageManager.cs      # EN/HI/SAT translations
|   |   |   |   +-- VoiceModule.cs          # Android TTS
|   |   |   +-- Offline/
|   |   |   |   +-- OfflineManager.cs       # Cache + sync
|   |   |   +-- Scenario/
|   |   |   |   +-- ARScenarioPlayer.cs     # Spawn prefabs at anchors
|   |   |   +-- UI/
|   |   |   |   +-- ThemeManager.cs         # Dark/Light theme
|   |   |   |   +-- UINavigator.cs          # Panel navigation
|   |   |   |   +-- SettingsScreen.cs       # Settings UI
|   |   |   |   +-- ScreenReaderHook.cs     # TalkBack accessibility
|   |   |   +-- Editor/
|   |   |       +-- PrefabGenerator.cs      # Generates 5 scenario prefabs
|   |   |       +-- AndroidBuildConfig.cs   # Build settings
|   |   |       +-- ProjectAutoSetup.cs     # First-time setup
|   |   |       +-- SceneSetup.cs           # Scene folder creation
|   |   |       +-- ARAutoSetup.cs          # AR config
|   |   |       +-- XRSetupScript.cs        # XR package config
|   |   |       +-- BuildScript.cs          # Build menu items
|   |   +-- Shaders/
|   |   |   +-- SafetyHighlight.shader      # Risk-level pulsing glow
|   |   +-- Plugins/Android/
|   |   |   +-- AndroidManifest.xml         # AR permissions
|   |   +-- StreamingAssets/Lang/
|   |   |   +-- en.json                     # English (48 keys)
|   |   |   +-- hi.json                     # Hindi (48 keys)
|   |   |   +-- sat.json                    # Santali (48 keys)
|   |   +-- Prefabs/
|   |   |   +-- Scenarios/                  # Generated at runtime
|   |   |   +-- Panels/                     # Generated at runtime
|   |   +-- Scenes/                         # EMPTY - needs creation
|   +-- Packages/
|   |   +-- manifest.json               # AR Foundation 6.6.2, ARCore 6.6.2
|   +-- ProjectSettings/
|       +-- ProjectSettings.asset        # Unity project settings
|
+-- REF/                                # Documentation
    +-- *.docx                          # SRS, Architecture, UI/UX docs
    +-- *.png                           # ChatGPT UI mockup images
    +-- implementation_plan.md          # Previous iteration notes
```

---

## 17. Key File Reference

### Most Important Files

| File | Purpose | Path |
|------|---------|------|
| Program.cs | Backend entry point, JWT, Swagger | Backend/Program.cs |
| Models.cs | All domain models | Shared/Models.cs |
| appsettings.json | MongoDB, JWT, Twilio config | Backend/appsettings.json |
| LoginManager.cs | Login/OTP UI logic | UnityAR/Assets/Scripts/Auth/LoginManager.cs |
| ARSessionSetup.cs | AR lifecycle | UnityAR/Assets/Scripts/AR/ARSessionSetup.cs |
| ARSiteMapper.cs | AR anchor placement | UnityAR/Assets/Scripts/AR/ARSiteMapper.cs |
| ARScenarioPlayer.cs | Scenario spawning | UnityAR/Assets/Scripts/Scenario/ARScenarioPlayer.cs |
| ThemeManager.cs | Dark/Light theme | UnityAR/Assets/Scripts/UI/ThemeManager.cs |
| UINavigator.cs | Panel navigation | UnityAR/Assets/Scripts/UI/UINavigator.cs |
| LanguageManager.cs | Translations | UnityAR/Assets/Scripts/Language/LanguageManager.cs |
| VoiceModule.cs | TTS | UnityAR/Assets/Scripts/Language/VoiceModule.cs |
| OfflineManager.cs | Offline sync | UnityAR/Assets/Scripts/Offline/OfflineManager.cs |
| LocalBlockchain.cs | Client blockchain | UnityAR/Assets/Scripts/Blockchain/LocalBlockchain.cs |
| SafetyHighlight.shader | Risk glow effect | UnityAR/Assets/Shaders/SafetyHighlight.shader |

### Reference Documents

| Document | Purpose | Path |
|----------|---------|------|
| SRS | Software Requirements Specification | REF/AR_Safety_Platform_SRS_...docx |
| Architecture | System architecture details | REF/AR_Safety_Platform_Architecture_...docx |
| UI/UX | Screen designs and flow | REF/AR_Safety_Platform_UIUX_...docx |
| UI Mockups | ChatGPT-generated designs | REF/ChatGPT Image Sep 18...png |
| Reference Video | YouTube demo concept | https://www.youtube.com/watch?v=nL7Elpg_EkE |

---

*Last updated: September 20, 2026*
*Project: SurakshaAR - SIH Problem Statement 26041*
