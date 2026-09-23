package com.surakshaar.ui.settings

import android.content.Intent
import android.content.pm.PackageManager
import android.os.Bundle
import android.view.View
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import com.google.android.material.button.MaterialButton
import com.surakshaar.R
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.language.LanguageManager
import com.surakshaar.ui.login.LoginActivity
import com.surakshaar.util.ThemeManager
import com.surakshaar.util.VoiceManager

class SettingsActivity : AppCompatActivity() {

    private lateinit var backButton: Button
    private lateinit var languageSpinner: Spinner
    private lateinit var themeSwitch: Switch
    private lateinit var qualityLow: MaterialButton
    private lateinit var qualityMed: MaterialButton
    private lateinit var qualityUltra: MaterialButton
    private lateinit var screenReaderSwitch: Switch
    private lateinit var readScreenBtn: MaterialButton
    private lateinit var voiceDownloadBtn: MaterialButton
    private lateinit var voiceStatusText: TextView
    private lateinit var userIdText: TextView
    private lateinit var roleText: TextView
    private lateinit var versionText: TextView
    private lateinit var logoutButton: Button

    private val languages = LanguageManager.getSupportedLanguages().map { LanguageManager.getLanguageName(it) }.toTypedArray()
    private val languageCodes = LanguageManager.getSupportedLanguages()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        ThemeManager.applyTheme(this, SessionManager.theme)
        setContentView(R.layout.activity_settings)
        VoiceManager.attachToActivity(this)

        initViews()
        applyTranslations()
        setupLanguageSpinner()
        setupThemeSwitch()
        setupQualityButtons()
        setupScreenReader()
        loadUserInfo()
        loadVersion()
        setupLogout()
        setupBackButton()
    }

    private fun applyTranslations() {
        try {
            val t = LanguageManager
            logoutButton.text = t.get("logout_button")
            voiceDownloadBtn.text = t.get("voice_packs")
            readScreenBtn.text = t.get("read_screen")
            qualityLow.text = t.get("settings_quality_low")
            qualityMed.text = t.get("settings_quality_med")
            qualityUltra.text = t.get("settings_quality_ultra")
            findViewById<TextView>(R.id.settingsTitle)?.text = t.get("settings")
            findViewById<TextView>(R.id.languageHeader)?.text = t.get("settings_language")
            findViewById<TextView>(R.id.themeHeader)?.text = t.get("settings_theme")
            findViewById<TextView>(R.id.qualityHeader)?.text = t.get("settings_quality")
            findViewById<TextView>(R.id.aboutHeader)?.text = t.get("settings_about")
            findViewById<TextView>(R.id.darkModeLabel)?.text = t.get("settings_dark_mode")
            findViewById<TextView>(R.id.screenReaderHeader)?.text = t.get("screen_reader_title")
            findViewById<TextView>(R.id.screenReaderLabel)?.text = t.get("screen_reader_label")
            findViewById<TextView>(R.id.screenReaderHelp)?.text = t.get("screen_reader_help")
            val userId = SessionManager.userId
            val role = SessionManager.role
            userIdText.text = "${t.get("user_id_label")}: ${userId.ifEmpty { "W001" }}"
            roleText.text = "${t.get("role_label")}: ${role.replaceFirstChar { c -> c.uppercase() }}"
            findViewById<TextView>(R.id.userIdText)?.let {
                it.text = "${t.get("user_id_label")}: ${userId.ifEmpty { "W001" }}"
            }
        } catch (e: Exception) {
        }
    }

    private fun initViews() {
        backButton = findViewById(R.id.backButton)
        languageSpinner = findViewById(R.id.languageSpinner)
        themeSwitch = findViewById(R.id.themeSwitch)
        qualityLow = findViewById(R.id.qualityLow)
        qualityMed = findViewById(R.id.qualityMed)
        qualityUltra = findViewById(R.id.qualityUltra)
        screenReaderSwitch = findViewById(R.id.screenReaderSwitch)
        readScreenBtn = findViewById(R.id.readScreenBtn)
        voiceDownloadBtn = findViewById(R.id.voiceDownloadBtn)
        voiceStatusText = findViewById(R.id.voiceStatusText)
        userIdText = findViewById(R.id.userIdText)
        roleText = findViewById(R.id.roleText)
        versionText = findViewById(R.id.versionText)
        logoutButton = findViewById(R.id.logoutButton)
    }

    private fun setupBackButton() {
        backButton.setOnClickListener {
            finish()
        }
    }

    private fun setupLanguageSpinner() {
        val adapter = ArrayAdapter(this, android.R.layout.simple_spinner_dropdown_item, languages)
        languageSpinner.adapter = adapter

        val currentLangCode = LanguageManager.getCurrentLanguage()
        val currentIndex = languageCodes.indexOf(currentLangCode)
        if (currentIndex >= 0) {
            languageSpinner.setSelection(currentIndex)
        }

        languageSpinner.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                val selectedCode = languageCodes[position]
                if (selectedCode != LanguageManager.getCurrentLanguage()) {
                    LanguageManager.setLanguage(selectedCode)
                    SessionManager.language = selectedCode
                    VoiceManager.applyLanguage(selectedCode)
                    recreate()
                }
            }

            override fun onNothingSelected(parent: AdapterView<*>?) {}
        }
    }

    private fun setupThemeSwitch() {
        val currentTheme = SessionManager.theme
        themeSwitch.isChecked = currentTheme == "dark"

        themeSwitch.setOnCheckedChangeListener { _, isChecked ->
            val newTheme = if (isChecked) "dark" else "light"
            SessionManager.theme = newTheme
            ThemeManager.applyTheme(this, newTheme)
            recreate()
        }

        ThemeManager.applyTheme(this, SessionManager.theme)
    }

    private fun setupQualityButtons() {
        val prefs = getSharedPreferences("suraksha_settings", MODE_PRIVATE)
        val savedQuality = prefs.getString("quality", "medium") ?: "medium"

        updateQualitySelection(savedQuality)

        qualityLow.setOnClickListener {
            saveQuality("low")
            updateQualitySelection("low")
        }

        qualityMed.setOnClickListener {
            saveQuality("medium")
            updateQualitySelection("medium")
        }

        qualityUltra.setOnClickListener {
            saveQuality("ultra")
            updateQualitySelection("ultra")
        }
    }

    private fun saveQuality(quality: String) {
        val prefs = getSharedPreferences("suraksha_settings", MODE_PRIVATE)
        prefs.edit().putString("quality", quality).apply()
    }

    private fun updateQualitySelection(selected: String) {
        val buttons = mapOf(
            "low" to qualityLow,
            "medium" to qualityMed,
            "ultra" to qualityUltra
        )

        buttons.forEach { (key, button) ->
            if (key == selected) {
                button.setBackgroundColor(getColor(R.color.primary))
                button.setTextColor(getColor(R.color.white))
            } else {
                button.setBackgroundColor(getColor(R.color.card_bg))
                button.setTextColor(getColor(R.color.white))
            }
        }
    }

    private fun setupScreenReader() {
        screenReaderSwitch.isChecked = VoiceManager.isEnabled()
        updateVoiceStatus()

        screenReaderSwitch.setOnCheckedChangeListener { _, isChecked ->
            VoiceManager.setEnabled(isChecked)
            updateVoiceStatus()
        }

        readScreenBtn.setOnClickListener {
            if (!VoiceManager.isVoiceAvailable()) {
                VoiceManager.speak(LanguageManager.get("voice_unavailable"), force = true)
                Toast.makeText(this, LanguageManager.get("voice_unavailable"), Toast.LENGTH_LONG).show()
            } else {
                VoiceManager.readScreen(this, force = true)
            }
        }

        voiceDownloadBtn.setOnClickListener {
            VoiceManager.openSystemVoiceSettings(this)
        }
    }

    private fun updateVoiceStatus() {
        voiceStatusText.text = when {
            !VoiceManager.isVoiceAvailable() -> LanguageManager.get("voice_unavailable")
            VoiceManager.isEnabled() -> LanguageManager.get("screen_reader_on")
            else -> LanguageManager.get("screen_reader_off")
        }
    }

    private fun loadUserInfo() {
        val userId = SessionManager.userId
        val role = SessionManager.role
        val t = LanguageManager
        userIdText.text = "${t.get("user_id_label")}: ${userId.ifEmpty { "W001" }}"
        roleText.text = "${t.get("role_label")}: ${role.replaceFirstChar { it.uppercase() }}"
    }

    private fun loadVersion() {
        try {
            val packageInfo = packageManager.getPackageInfo(packageName, 0)
            val versionName = packageInfo.versionName ?: "1.0.0"
            versionText.text = "${LanguageManager.get("version_label")} $versionName"
        } catch (e: PackageManager.NameNotFoundException) {
            versionText.text = "${LanguageManager.get("version_label")} 1.0.0"
        }
    }

    private fun setupLogout() {
        logoutButton.setOnClickListener {
            SessionManager.clear()
            val intent = Intent(this, LoginActivity::class.java)
            intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
            startActivity(intent)
            finish()
        }
    }
}