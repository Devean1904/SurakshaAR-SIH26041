# SurakshaAR Demo Script (≈4–5 minutes)

Target: screen-record Android emulator/device + browser. Keep cuts tight.

---

## 0. Hook (0:00–0:15)

**On camera / voiceover:**  
“Mining safety training is still classroom-based. SurakshaAR puts AR hazard scenarios, graded assessments, and verifiable certificates in every worker’s pocket.”

Show the repo briefly: Android app + .NET backend + web admin.

---

## 1. Login & localization (0:15–0:45)

1. Launch app → login screen (title **SurakshaAR**, no company ID field).
2. Switch language spinner to **हिन्दी** or **ᱥᱟᱱᱛᱟᱞᱤ** → labels update.
3. Sign in as worker.

**Callout:** “Worker ID only — company is resolved server-side from the account.”

---

## 2. AR training (0:45–1:45)

1. Tap **Fire & Explosion** (or any domain) card.
2. Grant camera permission if prompted.
3. Scan a flat surface → plane mesh appears → hazard spheres + action cubes spawn.
4. Tap a green action marker (or use Extinguish / Evacuate / Report buttons).
5. Let escalation fire once (warning banner) if time allows.
6. Finish required actions → results panel with score.

**Callout:** “Live ARCore planes, scenario markers from the training config, timed escalation with score penalties.”

---

## 3. Assessment & certificate (1:45–2:45)

1. From results, open **assessment**.
2. Answer questions (green/red feedback).
3. Submit → server grades → **PASSED** with 50/50 action+question score and module threshold.
4. Open **certificate** → QR + score + hash-chain TX.
5. Tap **Verify** in-app → “Certificate verified”.

**Callout:** “Server re-grades answers; certificates require a server-side pass.”

---

## 4. QR verify (2:45–3:15)

1. Dashboard → **QR Scan** → scan the certificate QR (or another device’s QR).
2. Dialog shows VALID + employee + module + score.
3. Browser: open `http://<host>:5000/verify/` → paste payload → **VALID** badge.

**Callout:** “Anyone can verify a certificate without an account.”

---

## 5. Offline sync (3:15–3:40)

1. Enable airplane mode (or kill backend).
2. Complete an assessment → toast “Saved offline…”.
3. Reconnect / restart backend → return to dashboard → “Synced N offline item(s)”.

**Callout:** “Failed submits queue locally and flush on next session.”

---

## 6. Manager / Admin (3:40–4:30)

**Web admin** (`/admin/`):
1. Login as admin.
2. Overview stats → Workers / Managers / Pending / Escalations.
3. **Compliance** tab → pass rate, by-module table, recent certificates & assessments.
4. Confirm a pending worker if present.

**In-app admin cards** (optional cut): whole-card tap opens Users / Reports / Site Mapping.

**Callout:** “Compliance is one click — pass rates, certificates, and failed attempts per module.”

---

## 7. Close (4:30–4:50)

Show: APK path, `dotnet run` backend, `/verify/` page, README.

**Line:** “SurakshaAR — AR training, graded assessments, offline-first, verifiable certificates.”

---

## Recording checklist

- [ ] Emulator or device at 1080p, brightness up
- [ ] Backend running (`dotnet run` in `Backend/`)
- [ ] Mongo running
- [ ] Admin credentials ready
- [ ] Network security: use `10.0.2.2` (emulator) or LAN IP build flag
- [ ] Hide notifications / personal data
- [ ] Total length ≤ 5 minutes
