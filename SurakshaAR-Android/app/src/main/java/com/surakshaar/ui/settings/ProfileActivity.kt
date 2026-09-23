package com.surakshaar.ui.settings

import android.os.Bundle
import android.util.Log
import android.widget.Button
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.surakshaar.R
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.language.LanguageManager
import com.surakshaar.util.ThemeManager
import com.surakshaar.util.VoiceManager

class ProfileActivity : AppCompatActivity() {

    companion object {
        private const val TAG = "ProfileActivity"
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        try {
            ThemeManager.applyTheme(this, SessionManager.theme)
            setContentView(R.layout.activity_profile)
            VoiceManager.attachToActivity(this)

            findViewById<Button>(R.id.backButton)?.setOnClickListener { finish() }

            val userName = SessionManager.userName.ifEmpty { "Worker" }
            val role = SessionManager.role.ifEmpty { "worker" }
            val userId = SessionManager.userId.ifEmpty { "" }

            findViewById<TextView>(R.id.profileName)?.text = userName
            findViewById<TextView>(R.id.profileRole)?.text = "${LanguageManager.get("role_label")}: ${role.replaceFirstChar { it.uppercase() }}"
            findViewById<TextView>(R.id.profileUserId)?.text = userId
            findViewById<TextView>(R.id.profileInitials)?.text = userName.firstOrNull()?.uppercase() ?: "W"
            findViewById<TextView>(R.id.profilePhone)?.text = LanguageManager.get("not_set")
            findViewById<TextView>(R.id.profileLanguage)?.text = LanguageManager.getLanguageName(SessionManager.language.ifEmpty { "en" })
            findViewById<TextView>(R.id.profileTheme)?.text = when (SessionManager.theme) {
                "dark" -> LanguageManager.get("dark_theme")
                else -> LanguageManager.get("light_theme")
            }
            findViewById<TextView>(R.id.profileModulesCount)?.text = SessionManager.modulesCompleted.toString()
            findViewById<TextView>(R.id.profileCertsCount)?.text = SessionManager.certCount.toString()
            findViewById<TextView>(R.id.profileTitle)?.text = LanguageManager.get("profile")
            findViewById<TextView>(R.id.accountInfoHeader)?.text = LanguageManager.get("profile_account_info")
            findViewById<TextView>(R.id.userIdLabel)?.text = LanguageManager.get("user_id_label")
            findViewById<TextView>(R.id.phoneLabel)?.text = LanguageManager.get("profile_phone_label")
            findViewById<TextView>(R.id.languageLabel)?.text = LanguageManager.get("settings_language")
            findViewById<TextView>(R.id.themeLabel)?.text = LanguageManager.get("settings_theme")
            findViewById<TextView>(R.id.statsHeader)?.text = LanguageManager.get("profile_training_stats")
            findViewById<TextView>(R.id.modulesLabel)?.text = LanguageManager.get("stat_modules")
            findViewById<TextView>(R.id.certsLabel)?.text = LanguageManager.get("stat_certs")
        } catch (e: Exception) {
            Log.e(TAG, "ProfileActivity failed", e)
            Toast.makeText(this, "Profile error: ${e.message}", Toast.LENGTH_SHORT).show()
            finish()
        }
    }
}
