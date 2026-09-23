# SurakshaAR — Multi-Issue Fix Plan

Fix all reported issues in the SurakshaAR React+Capacitor+Android app.

## User Review Required

> [!IMPORTANT]
> The Android build error shown in the screenshots says `dataBinding = true` but the file on disk shows `dataBinding = false`. The screenshots also show the build was run from Android Studio, not CLI. This discrepancy means Android Studio may be caching stale Gradle files. The fix will: (1) set `dataBinding = false` explicitly + disable it fully, (2) upgrade Gradle to 9.0.0 which is required for Java 25 + AGP 8.7.3.

> [!WARNING]
> The screen reader in the Navbar is a "read current screen aloud" button — a **separate** feature from the TalkBack/Screen Reader **toggle** in Settings. The user wants:
> - **Settings Screen Reader toggle** = TalkBack accessibility mode (1st tap = announce, 2nd tap = activate — standard Android TalkBack UX)  
> - **Navbar "Read" button** = instant read-aloud of current page content
> Both need to work correctly.

## Open Questions

> [!IMPORTANT]
> **Voice model downloads**: The user asked to "download all language voice models". Web Speech API uses OS-installed TTS voices — they cannot be bundled in a web/Capacitor app. For Android, the app will open Android TTS settings to let the user download voices. For the web version, we'll add a "Check Voice Availability" feature that tests each language and shows which voices are installed.

## Proposed Changes

### 1. Android Gradle Build Fix

#### [MODIFY] [build.gradle.kts (app)](file:///E:/SIH/android/app/build.gradle.kts)
- Set `dataBinding = false` explicitly (already false but Android Studio may cache stale state)
- Remove `ksp` block from `defaultConfig` (Room schema arg can go in top-level)
- Upgrade `compileSdk` and `targetSdk` from 34 → 35 for Java 25 compatibility

#### [MODIFY] [gradle-wrapper.properties](file:///E:/SIH/android/gradle/wrapper/gradle-wrapper.properties)  
- Upgrade Gradle from 8.11.1 → **9.0.0** — this is required because Java 25 (JBR from Android Studio) needs Gradle 9.x to parse the version string

#### [MODIFY] [gradle.properties](file:///E:/SIH/android/gradle.properties)
- Add `android.defaults.buildfeatures.databinding=false` to disable data binding globally
- Add `android.defaults.buildfeatures.viewbinding=true`
- Remove `org.gradle.tooling.parallel=true` (deprecated in Gradle 9)

#### [MODIFY] [settings.gradle.kts](file:///E:/SIH/android/settings.gradle.kts)
- Update foojay-resolver-convention to 1.0.0 (already correct)

### 2. Screen Reader Fix (Navbar "Read" button)

**Problem**: The `handleScreenReader` in Navbar sets `isReadingScreen` to true, calls `speechService.speak()`, then resets via `setTimeout` after 8 seconds — but speech may end before or after. The speaking listener isn't wired up, so the button gets stuck in "Stop" state or never reads.

#### [MODIFY] [Navbar.tsx](file:///E:/SIH/src/components/layout/Navbar.tsx)
- Wire `speechService.setSpeakingListener()` to properly track speaking state
- Reset `isReadingScreen` based on speech end event, not a hardcoded timer
- On language switch from Navbar dropdown: if TalkBack is enabled, announce in new language (via AppContext greeting effect — already exists, just needs `isTalkBackEnabled` to be consumed)

### 3. Screen Reader TalkBack Mode — Double-Tap Behavior

**Problem**: The TalkBack toggle in Settings enables screen reader mode, but there's no double-tap interaction logic. Per the spec: 1st tap = announce what the element is, 2nd tap = activate it.

#### [MODIFY] [SettingsScreen.tsx](file:///E:/SIH/src/components/common/SettingsScreen.tsx)
- When `isTalkBackEnabled`, wrap each interactive element with a `useTalkBack` hook pattern:
  - Single tap → speak the element's label (aria-label)
  - Double-tap (within 400ms) → execute the action
- Add `data-talkback-label` attributes to all buttons
- Announce language greeting on language change (already in AppContext, just need to ensure the greeting fires in the **new** language, not the old one)

#### [NEW] [useTalkBack.ts](file:///E:/SIH/src/hooks/useTalkBack.ts)
- Custom hook: `useTalkBack(label, action, lang)` → returns `onClick` handler
- Manages first/second tap timing state with a ref
- Speaks label on first tap, calls action on second tap

### 4. Light Theme Fix

**Problem**: `setTheme` in AppContext does call `document.documentElement.setAttribute('data-theme', newTheme)`, but the theme is only applied on explicit call. On **initial load**, the stored theme is read from localStorage but `data-theme` is never set on `document.documentElement` — so the page always renders in dark mode until a theme button is clicked.

#### [MODIFY] [AppContext.tsx](file:///E:/SIH/src/context/AppContext.tsx)
- In the `theme` state initializer, immediately apply `data-theme` to `document.documentElement` on mount via `useEffect`

#### [MODIFY] [tokens.css](file:///E:/SIH/src/styles/tokens.css)
- The light theme `body` selector uses `[data-theme="light"] body` but the `background-color` isn't set there (it's only set on `html, body` in index.css using `var(--bg-deep)`)
- Since `--bg-deep` is already overridden in `[data-theme="light"]`, this should work — but `html` background (not just `body`) also needs the override
- Add explicit `background-color` to `html` element in light theme

### 5. Remove Non-Functioning / Unused Buttons

After review, these buttons/features exist but are broken or redundant:

| Element | Location | Status | Action |
|---|---|---|---|
| Voice Toggle (Mic) button | Navbar | Duplicates Settings toggle | Remove from Navbar |
| Sound Toggle (Volume) button | Navbar | Duplicates Settings toggle | Remove from Navbar |  
| Navbar "Read" button | Navbar | Keep but fix | Fix speaking listener |
| `isSyncing` state | Navbar | Set to true but never reset | Remove/fix |

#### [MODIFY] [Navbar.tsx](file:///E:/SIH/src/components/layout/Navbar.tsx)
- Remove the standalone Voice (Mic/MicOff) toggle button — it's duplicated in Settings
- Remove the standalone Sound (Volume) toggle button — it's duplicated in Settings  
- Clean up `isSyncing` state (only used to style the sync button, never actually set to true now)

### 6. Language Voice Model Download Guidance

#### [MODIFY] [SettingsScreen.tsx](file:///E:/SIH/src/components/common/SettingsScreen.tsx)
- Add a "Download Voice Models" section that shows availability of each language TTS voice
- For each language, check if `window.speechSynthesis.getVoices()` has a matching voice
- Show ✓ (available) or ⬇ (not installed) badge per language
- On Android (Capacitor), provide a button that opens Android TTS settings via deep link
- On web, show a note that voices are managed by the OS/browser

### 7. Language Greeting on Switch (Screen Reader)

**Current behavior**: The greeting effect fires when `language` changes AND `isTalkBackEnabled` is true AND `loginUser` is not null. The greeting message is in the OLD language (the state variable).

**Bug**: `setLanguage(langKey)` is called, which triggers the effect. But the greeting text is built using `greetingTranslations[language]` — and React batches state, so `language` in the effect is the **new** language ✓ (it's a dep). This is actually correct. But `speechService.speak(greeting, language, ...)` uses the correct new language code.

The real issue is the language selector in **Settings** uses `setLanguage(langKey)` then announces in the **old** language (line 268: `speechService.speak(msg, language, ...)`). The `language` variable at that point is still the old value (closure).

#### [MODIFY] [SettingsScreen.tsx](file:///E:/SIH/src/components/common/SettingsScreen.tsx)
- Fix language button `onClick`: announce using `langKey` (the new language), not `language` (old)
- The full greeting is already handled by AppContext `useEffect` — remove the duplicate `speechService.speak` from the button handler to avoid double-speaking

---

## Verification Plan

### Automated Tests
```bash
cd E:\SIH
npx vite build
```

### Manual Verification
1. **Light theme**: Refresh browser, confirm light theme persists from localStorage
2. **Screen reader**: Enable TalkBack, switch language → hears greeting in new language; navigate to any button → 1st tap reads label, 2nd tap activates
3. **Navbar Read button**: Click → speech starts; click again → speech stops; speech ends automatically and button resets
4. **Android build**: Run `gradlew.bat assembleDebug` from `E:\SIH\android` with Java 25 JAVA_HOME
5. **Removed buttons**: Confirm Voice and Sound buttons no longer in Navbar (accessible via Settings only)
6. **Voice model check**: Open Settings → Language section shows voice availability indicators
