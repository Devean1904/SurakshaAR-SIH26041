package com.surakshaar.util

import android.app.Activity
import android.content.Context
import android.content.Intent
import android.provider.Settings
import android.speech.tts.TextToSpeech
import android.util.Log
import android.view.MotionEvent
import android.view.View
import android.view.ViewGroup
import android.view.Window
import android.widget.Button
import android.widget.CheckBox
import android.widget.ImageButton
import android.widget.RadioButton
import android.widget.Switch
import android.widget.TextView
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.language.LanguageManager
import java.util.Locale
import java.util.concurrent.atomic.AtomicBoolean

/**
 * In-app screen reader for users who cannot read the UI.
 * Speaks labels on tap and can read the whole screen aloud (EN/HI/SAT + more via Android TTS).
 */
object VoiceManager : TextToSpeech.OnInitListener {

    private const val TAG = "VoiceManager"

    private var tts: TextToSpeech? = null
    private val ready = AtomicBoolean(false)
    private var currentLocale: Locale = Locale.ENGLISH
    private var lastSpoken: String = ""
    private var lastSpokenAtMs: Long = 0L

    fun init(context: Context) {
        if (tts != null) return
        tts = TextToSpeech(context.applicationContext, this)
        applyLanguage(LanguageManager.getCurrentLanguage())
    }

    override fun onInit(status: Int) {
        if (status == TextToSpeech.SUCCESS) {
            ready.set(true)
            applyLanguage(LanguageManager.getCurrentLanguage())
            Log.i(TAG, "TTS ready locale=$currentLocale")
        } else {
            Log.e(TAG, "TTS init failed status=$status")
        }
    }

    fun isEnabled(): Boolean = SessionManager.screenReader

    fun setEnabled(enabled: Boolean) {
        SessionManager.screenReader = enabled
        if (enabled) {
            speak(LanguageManager.get("screen_reader_on"), force = true)
        } else {
            stop()
        }
    }

    fun applyLanguage(langCode: String) {
        val locale = localeFor(langCode)
        currentLocale = locale
        val engine = tts ?: return
        val result = engine.setLanguage(locale)
        if (result == TextToSpeech.LANG_MISSING_DATA || result == TextToSpeech.LANG_NOT_SUPPORTED) {
            val fallback = when (langCode) {
                "sat" -> Locale("hi", "IN")
                else -> Locale.ENGLISH
            }
            engine.setLanguage(fallback)
            currentLocale = fallback
        }
        engine.language = currentLocale
        engine.setSpeechRate(0.95f)
    }

    private fun localeFor(langCode: String): Locale = when (langCode) {
        "en" -> Locale("en", "IN")
        "hi" -> Locale("hi", "IN")
        "sat" -> Locale("hi", "IN") // Santali rarely has a system TTS voice; Hindi is closest
        "mr" -> Locale("mr", "IN")
        "ta" -> Locale("ta", "IN")
        "te" -> Locale("te", "IN")
        "kn" -> Locale("kn", "IN")
        else -> Locale.ENGLISH
    }

    fun speak(text: String?, force: Boolean = false) {
        val clean = text?.trim().orEmpty()
        if (clean.isEmpty()) return
        if (!force && !isEnabled()) return
        val engine = tts ?: return
        if (!ready.get()) return

        val now = System.currentTimeMillis()
        if (!force && clean == lastSpoken && now - lastSpokenAtMs < 900) return
        lastSpoken = clean
        lastSpokenAtMs = now

        engine.stop()
        engine.speak(clean, TextToSpeech.QUEUE_FLUSH, null, "suraksha-${now}")
    }

    fun stop() {
        tts?.stop()
        lastSpoken = ""
    }

    fun isVoiceAvailable(): Boolean = ready.get()

    fun openSystemVoiceSettings(context: Context) {
        val intents = listOf(
            Intent("android.settings.TTS_SETTINGS"),
            Intent(Settings.ACTION_SETTINGS),
            Intent("android.settings.TTS_SETTINGS")
        )
        for (i in intents) {
            try {
                i.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
                context.startActivity(i)
                return
            } catch (_: Exception) {
            }
        }
    }

    /** Wrap the activity window so taps speak the control label when the screen reader is on. */
    fun attachToActivity(activity: Activity) {
        val window = activity.window ?: return
        val original = window.callback ?: return
        if (original is SpeakWindowCallback) return
        window.callback = SpeakWindowCallback(original, activity)
    }

    /** Speak visible text on the screen (title, labels, buttons). */
    fun readScreen(activity: Activity, force: Boolean = true) {
        if (!force && !isEnabled()) return
        val root = activity.window?.decorView ?: return
        val parts = linkedSetOf<String>()
        collectReadable(root, parts, limit = 18)
        if (parts.isEmpty()) {
            speak(activity.title?.toString() ?: "Screen", force = true)
        } else {
            speak(parts.joinToString(". "), force = true)
        }
    }

    private fun collectReadable(view: View, out: MutableSet<String>, depth: Int = 0, limit: Int = 18) {
        if (out.size >= limit || depth > 24 || view.visibility != View.VISIBLE) return
        if (view.alpha < 0.1f) return

        when (view) {
            is TextView -> {
                val t = view.text?.toString()?.trim().orEmpty()
                if (t.isNotEmpty() && t.length < 160 && !t.startsWith("http")) out.add(t)
            }
        }

        if (view is ViewGroup) {
            for (i in 0 until view.childCount) {
                collectReadable(view.getChildAt(i), out, depth + 1, limit)
            }
        }
    }

    private fun speakableViewLabel(view: View): String? {
        val desc = view.contentDescription?.toString()?.trim().orEmpty()
        if (desc.isNotEmpty()) return desc
        return when (view) {
            is TextView -> view.text?.toString()?.trim()?.takeIf { it.isNotEmpty() }
            else -> null
        }
    }

    private class SpeakWindowCallback(
        private val original: Window.Callback,
        private val activity: Activity
    ) : Window.Callback by original {

        override fun dispatchTouchEvent(event: MotionEvent): Boolean {
            if (event.action == MotionEvent.ACTION_UP && VoiceManager.isEnabled()) {
                try {
                    val hit = hitTest(activity.window.decorView, event.rawX, event.rawY)
                    val label = hit?.let { speakableViewLabel(it) }
                    if (!label.isNullOrBlank()) {
                        VoiceManager.speak(label, force = true)
                    }
                } catch (_: Exception) {
                }
            }
            return original.dispatchTouchEvent(event)
        }

        private fun hitTest(view: View, screenX: Float, screenY: Float): View? {
            if (view.visibility != View.VISIBLE || view.alpha < 0.1f) return null
            if (view is ViewGroup) {
                for (i in view.childCount - 1 downTo 0) {
                    hitTest(view.getChildAt(i), screenX, screenY)?.let { return it }
                }
            }
            if (!isInteractive(view)) return null
            val loc = IntArray(2)
            view.getLocationOnScreen(loc)
            val x = screenX - loc[0]
            val y = screenY - loc[1]
            return if (x >= 0 && x < view.width && y >= 0 && y < view.height) view else null
        }

        private fun isInteractive(view: View): Boolean =
            view.isClickable || view.isLongClickable || view is Button ||
                view is ImageButton || view is CheckBox || view is RadioButton ||
                view is Switch || view is android.widget.CompoundButton
    }
}
