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
            findViewById<TextView>(R.id.profileRole)?.text = "Role: ${role.replaceFirstChar { it.uppercase() }}"
            findViewById<TextView>(R.id.profileUserId)?.text = userId
            findViewById<TextView>(R.id.profileInitials)?.text = userName.firstOrNull()?.uppercase() ?: "W"
            findViewById<TextView>(R.id.profilePhone)?.text = "Not set"
            findViewById<TextView>(R.id.profileLanguage)?.text = LanguageManager.getLanguageName(SessionManager.language.ifEmpty { "en" })
            findViewById<TextView>(R.id.profileTheme)?.text = SessionManager.theme.replaceFirstChar { it.uppercase() }
            findViewById<TextView>(R.id.profileModulesCount)?.text = SessionManager.modulesCompleted.toString()
            findViewById<TextView>(R.id.profileCertsCount)?.text = SessionManager.certCount.toString()
        } catch (e: Exception) {
            Log.e(TAG, "ProfileActivity failed", e)
            Toast.makeText(this, "Profile error: ${e.message}", Toast.LENGTH_SHORT).show()
            finish()
        }
    }
}
