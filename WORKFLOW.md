# SurakshaAR — Development Workflow (Status)

Native Android + .NET backend path is the shipping product. Unity prototype removed from the repository.

Last updated: September 23, 2026

## Completed

### Platform
- [x] Backend hardening (auth, validation, XSS, Swagger, CORS)
- [x] Backend green (0 warnings / 0 errors); health / verify / admin verified
- [x] MongoDB seed modules on first startup
- [x] Demo DB wipe capability (users + companies)
- [x] Root `.gitignore`; build outputs / secrets excluded

### Android app
- [x] Login (Worker ID only — company resolved server-side), dashboard, settings, profile
- [x] Real AR training (SceneView ARSceneView, markers, escalation)
- [x] Site mapping (planes, bounding box, anchors, save)
- [x] Assessment scoring aligned client/server (50/50, module threshold, ordered answers)
- [x] Certificates + QR + in-app verify + public `/verify/` page
- [x] QR scanner on dashboard (zxing IntentIntegrator)
- [x] Offline queue + flush + worker offline/sync APIs
- [x] Screen reader / TTS (VoiceManager, Settings toggle, tap-to-speak, Read Screen, Voice Packs)
- [x] Santali TTS locale chain (sat → hi-IN → en; re-apply on speak)
- [x] Full 7-language localization (EN/HI/SAT/MR/TA/TE/KN) on all main screens
- [x] Login input Material box styling (hint overlap fixed)
- [x] No "Coming soon" stubs remain in app code
- [x] APK assembled (`app/build/outputs/apk/debug/app-debug.apk`, ~59 MB)
- [x] Physical-device builds via `-PsurakshaApiBaseUrl=http://<lan-ip>:5000/api/`

### Web / ops
- [x] Admin compliance endpoint + web Compliance tab
- [x] Web admin certificates/attempts views + API
- [x] Firewall helper (`Backend/open-firewall.bat`) for port 5000

### Repo / delivery
- [x] README + DEMO_SCRIPT + PROJECT_VISION refreshed
- [x] Public GitHub repo: https://github.com/Devean1904/SurakshaAR-SIH26041
- [x] GitHub topics + description (SIH26041 / IMMUTABLE-A / 153165)
- [x] Release `v1.0-sih2026` with `SurakshaAR.apk` + `app-debug.apk`
- [x] Repo cleaned: root folder `SAFE` → `SurakshaAR`; UnityAR / REF / root APK removed

## Not applicable

- Unity scene automation, particle fire/gas sims, VoiceModule — superseded by native Kotlin app.
- Legacy Unity prototype and REF docs removed from repository.

## Optional follow-ups

- [ ] Release APK signing (keystore)
- [ ] Instrumented tests / CI
- [ ] Polygon on-chain certificate anchoring (currently local hash-chain + HMAC)
