# SurakshaAR — Development Workflow (Status)

Native Android + .NET backend path is complete for submission.

## Completed

- [x] Backend hardening (auth, validation, XSS, Swagger, CORS)
- [x] Android login (no company ID), dashboard, settings, profile
- [x] Real AR training (SceneView ARSceneView, markers, escalation)
- [x] Site mapping (planes, bounding box, anchors, save)
- [x] Assessment scoring aligned client/server (50/50, module threshold, ordered answers)
- [x] Certificates + QR + in-app verify + public `/verify/` page
- [x] QR scanner on dashboard (zxing IntentIntegrator)
- [x] Offline queue + flush + worker offline/sync APIs
- [x] Admin compliance endpoint + web Compliance tab
- [x] Hindi/Santali camera permission + LanguageManager result keys
- [x] Hindi/Santali + 7-language results/certificate localization (Assessment, AR training, Certificate screens; hi/sat complete)
- [x] Dead paths removed (earthquake/flood), junk folders/logs deleted
- [x] README + DEMO_SCRIPT
- [x] APK assembled (`app/build/outputs/apk/debug/app-debug.apk`, ~60 MB)
- [x] Backend green (0 warnings / 0 errors); health / verify / admin verified
- [x] No "Coming soon" stubs remain in app code
- [x] Root `.gitignore` added
- [x] Screen reader / TTS (VoiceManager, Settings toggle, tap-to-speak, Read Screen, Voice Packs)
- [x] Offline: persist training attempts + flush via `/worker/offline/{id}` + `/worker/online/sync`
- [x] Web admin certificates/attempts views + API
- [x] Grading/pass-threshold consistency (client/server/Unity + certificates re-check)
- [x] Seed Modules admin card removed; no "Coming soon"/QR stubs
- [x] Junk folders/logs cleaned (REF temp_*, JDK installer, Backend *.log)
- [x] README refreshed (admin certs/attempts, localization, status)
- [x] Demo DB wipe: all users (admin/manager/worker) + companies cleared from MongoDB

## Not applicable (Unity path abandoned)

- Unity scene automation, particle fire/gas sims, VoiceModule — superseded by native Kotlin app.

## Optional follow-ups

- [ ] Public git init / first commit (git + gh now installed: 2.55.0, 2.101.0)
- [ ] Release APK signing
- [ ] Instrumented tests / CI
