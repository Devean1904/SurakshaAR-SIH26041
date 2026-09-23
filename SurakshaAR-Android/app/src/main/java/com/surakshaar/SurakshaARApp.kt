package com.surakshaar

import android.app.Application
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.language.LanguageManager
import com.surakshaar.util.VoiceManager

class SurakshaARApp : Application() {
    override fun onCreate() {
        super.onCreate()
        SessionManager.init(this)
        LanguageManager.init(this)
        VoiceManager.init(this)
        VoiceManager.applyLanguage(LanguageManager.getCurrentLanguage())
    }
}
