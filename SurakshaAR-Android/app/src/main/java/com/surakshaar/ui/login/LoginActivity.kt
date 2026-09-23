package com.surakshaar.ui.login

import android.content.Intent
import android.graphics.RenderEffect
import android.graphics.Shader
import android.os.Build
import android.os.Bundle
import android.view.View
import android.view.inputmethod.EditorInfo
import android.widget.*
import androidx.activity.OnBackPressedCallback
import androidx.appcompat.app.AppCompatActivity
import androidx.lifecycle.lifecycleScope
import com.google.android.material.button.MaterialButton
import com.google.android.material.switchmaterial.SwitchMaterial
import com.google.android.material.textfield.TextInputEditText
import com.surakshaar.R
import com.surakshaar.data.api.ApiClient
import com.surakshaar.data.model.LoginRequest
import com.surakshaar.data.model.OtpSendRequest
import com.surakshaar.data.model.OtpVerifyRequest
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.language.LanguageManager
import com.surakshaar.ui.dashboard.DashboardActivity
import com.surakshaar.util.ThemeManager
import com.surakshaar.util.VoiceManager
import kotlinx.coroutines.launch
import org.json.JSONObject

class LoginActivity : AppCompatActivity() {

    private lateinit var userIdInput: TextInputEditText
    private lateinit var passwordInput: TextInputEditText
    private lateinit var loginButton: MaterialButton
    private lateinit var statusText: TextView
    private lateinit var rememberToggle: SwitchMaterial
    private lateinit var otpLoginButton: MaterialButton
    private lateinit var languageButton: TextView
    private lateinit var loginTitle: TextView
    private lateinit var loginSubtitle: TextView

    private lateinit var otpPanel: LinearLayout
    private lateinit var phoneInput: TextInputEditText
    private lateinit var sendOtpButton: MaterialButton
    private lateinit var otpInput: TextInputEditText
    private lateinit var verifyOtpButton: MaterialButton
    private lateinit var backToLoginButton: MaterialButton

    private lateinit var languagePanel: LinearLayout

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        ThemeManager.applyTheme(this, SessionManager.theme)
        setContentView(R.layout.activity_login)
        VoiceManager.attachToActivity(this)
        initViews()
        setupBlurEffect()
        setupListeners()
        applyTranslations()
        restoreRememberedId()
        VoiceManager.speak(loginTitle.text?.toString(), force = true)

        onBackPressedDispatcher.addCallback(this, object : OnBackPressedCallback(true) {
            override fun handleOnBackPressed() {
                if (otpPanel.visibility == View.VISIBLE) {
                    hideOtpPanel()
                } else if (languagePanel.visibility == View.VISIBLE) {
                    hideLanguagePanel()
                } else {
                    finish()
                }
            }
        })
    }

    private fun setupBlurEffect() {
        val background = findViewById<ImageView>(R.id.loginBackground)
        val overlay = findViewById<View>(R.id.blurOverlay)

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            try {
                val blurRadius = 6f
                background.setRenderEffect(
                    RenderEffect.createBlurEffect(blurRadius, blurRadius, Shader.TileMode.CLAMP)
                )
            } catch (e: Exception) {
                overlay.alpha = 0.50f
            }
        } else {
            overlay.alpha = 0.50f
        }
    }

    private fun initViews() {
        userIdInput = findViewById(R.id.userIdInput)
        passwordInput = findViewById(R.id.passwordInput)
        loginButton = findViewById(R.id.loginButton)
        statusText = findViewById(R.id.statusText)
        rememberToggle = findViewById(R.id.rememberToggle)
        otpLoginButton = findViewById(R.id.otpLoginButton)
        languageButton = findViewById(R.id.languageButton)
        loginTitle = findViewById(R.id.loginTitle)
        loginSubtitle = findViewById(R.id.loginSubtitle)

        otpPanel = findViewById(R.id.otpPanel)
        phoneInput = findViewById(R.id.phoneInput)
        sendOtpButton = findViewById(R.id.sendOtpButton)
        otpInput = findViewById(R.id.otpInput)
        verifyOtpButton = findViewById(R.id.verifyOtpButton)
        backToLoginButton = findViewById(R.id.backToLoginButton)

        languagePanel = findViewById(R.id.languagePanel)
    }

    private fun setupListeners() {
        loginButton.setOnClickListener { onIdLogin() }
        otpLoginButton.setOnClickListener { showOtpPanel() }
        sendOtpButton.setOnClickListener { onSendOtp() }
        verifyOtpButton.setOnClickListener { onVerifyOtp() }
        backToLoginButton.setOnClickListener { hideOtpPanel() }
        languageButton.setOnClickListener { showLanguagePanel() }

        passwordInput.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_DONE) {
                onIdLogin()
                true
            } else false
        }

        val langButtons = mapOf(
            R.id.langEnglish to "en",
            R.id.langHindi to "hi",
            R.id.langSantali to "sat",
            R.id.langMarathi to "mr",
            R.id.langTamil to "ta",
            R.id.langTelugu to "te",
            R.id.langKannada to "kn"
        )
        langButtons.forEach { (id, code) ->
            findViewById<MaterialButton>(id).setOnClickListener {
                LanguageManager.setLanguage(code)
                SessionManager.language = code
                VoiceManager.applyLanguage(code)
                languageButton.text = LanguageManager.getLanguageName(code)
                hideLanguagePanel()
                recreate()
            }
        }
    }

    private fun applyTranslations() {
        try {
            val t = LanguageManager
            loginTitle.text = t.get("login_title")
            loginSubtitle.text = t.get("login_subtitle")
            findViewById<com.google.android.material.textfield.TextInputLayout>(R.id.tilUserId).hint = t.get("login_user_id")
            findViewById<com.google.android.material.textfield.TextInputLayout>(R.id.tilPassword).hint = t.get("login_password")
            rememberToggle.text = t.get("login_remember")
            loginButton.text = t.get("login_button")
            otpLoginButton.text = t.get("login_otp_button")
            sendOtpButton.text = t.get("otp_send")
            verifyOtpButton.text = t.get("otp_verify")
            backToLoginButton.text = t.get("otp_back")
            languageButton.text = LanguageManager.getLanguageName(SessionManager.language.ifEmpty { LanguageManager.getCurrentLanguage() })
            findViewById<com.google.android.material.textfield.TextInputLayout>(R.id.tilPhone).hint = t.get("enter_phone")
            findViewById<com.google.android.material.textfield.TextInputLayout>(R.id.tilOtp).hint = t.get("otp_enter")
            statusText.text = ""
        } catch (e: Exception) {
            // Translation failed
        }
    }

    private fun restoreRememberedId() {
        val savedId = SessionManager.prefs.getString("SavedUserId", "") ?: ""
        if (savedId.isNotEmpty()) {
            userIdInput.setText(savedId)
            rememberToggle.isChecked = true
        }
    }

    private fun onIdLogin() {
        val userId = userIdInput.text.toString().trim()
        val password = passwordInput.text.toString().trim()
        val companyId = "" // Company resolved from user ID server-side

        if (userId.isEmpty()) {
            statusText.text = LanguageManager.get("enter_id")
            return
        }
        if (password.isEmpty()) {
            statusText.text = LanguageManager.get("login_password")
            return
        }

        statusText.text = LanguageManager.get("logging_in")
        loginButton.isEnabled = false

        lifecycleScope.launch {
            try {
                val response = ApiClient.api.login(LoginRequest(userId, password, companyId))
                if (response.isSuccessful) {
                    val body = response.body()
                    if (body != null && body.success) {
                        SessionManager.saveAuth(body)
                        SessionManager.saveOfflineCredentials(userId, password)
                        if (rememberToggle.isChecked) {
                            SessionManager.prefs.edit().putString("SavedUserId", userId).apply()
                        }
                        navigateToDashboard()
                    } else {
                        val failMsg = body?.message ?: LanguageManager.get("login_failed")
                        statusText.text = failMsg
                        VoiceManager.speak(failMsg, force = true)
                    }
                } else {
                    val errorMsg = parseErrorMessage(response.code(), response.errorBody()?.string())
                    statusText.text = errorMsg
                    VoiceManager.speak(errorMsg, force = true)
                }
            } catch (e: Exception) {
                offlineLogin(userId, password)
            }
            loginButton.isEnabled = true
        }
    }

    private fun parseErrorMessage(code: Int, errorBody: String?): String {
        val message = try {
            if (!errorBody.isNullOrBlank()) {
                JSONObject(errorBody).optString("message", "")
            } else ""
        } catch (e: Exception) {
            ""
        }

        return when (code) {
            400 -> {
                if (message.isNotEmpty()) message
                else LanguageManager.get("enter_id_password")
            }
            401 -> {
                when {
                    message.contains("pending admin confirmation", ignoreCase = true) ->
                        LanguageManager.get("account_pending")
                    message.contains("not found", ignoreCase = true) ->
                        LanguageManager.get("id_not_found")
                    message.contains("invalid password", ignoreCase = true) ->
                        LanguageManager.get("incorrect_password")
                    message.contains("not registered", ignoreCase = true) ->
                        LanguageManager.get("phone_not_registered")
                    message.isNotEmpty() -> message
                    else -> LanguageManager.get("invalid_credentials")
                }
            }
            403 -> LanguageManager.get("access_denied")
            404 -> LanguageManager.get("request_failed")
            500 -> LanguageManager.get("server_error")
            in 501..599 -> LanguageManager.get("server_error")
            in 402..499 -> LanguageManager.get("request_failed")
            0 -> LanguageManager.get("cannot_reach_server")
            else -> LanguageManager.get("error_try_again")
        }
    }

    private fun offlineLogin(userId: String, password: String) {
        if (SessionManager.verifyOfflineCredentials(userId, password)) {
            val offlineMsg = LanguageManager.get("offline_using_saved")
            statusText.text = offlineMsg
            VoiceManager.speak(offlineMsg, force = true)
            if (SessionManager.userId.isEmpty()) {
                SessionManager.userId = userId
                SessionManager.userName = userId
                SessionManager.role = "worker"
            }
            if (rememberToggle.isChecked) {
                SessionManager.prefs.edit().putString("SavedUserId", userId).apply()
            }
            navigateToDashboard()
        } else {
            val offlineMsg = LanguageManager.get("cannot_reach_login")
            statusText.text = offlineMsg
            VoiceManager.speak(offlineMsg, force = true)
        }
    }

    private fun onSendOtp() {
        val phone = phoneInput.text.toString().trim()
        if (phone.isEmpty()) {
            statusText.text = LanguageManager.get("enter_phone")
            return
        }
        statusText.text = LanguageManager.get("sending_otp")
        lifecycleScope.launch {
            try {
                val response = ApiClient.api.sendOtp(OtpSendRequest(phone))
                if (response.isSuccessful) {
                    statusText.text = LanguageManager.get("otp_sent")
                } else {
                    val errorMsg = parseErrorMessage(response.code(), response.errorBody()?.string())
                    statusText.text = errorMsg
                }
            } catch (e: Exception) {
                statusText.text = LanguageManager.get("cannot_reach_server")
            }
        }
    }

    private fun onVerifyOtp() {
        val phone = phoneInput.text.toString().trim()
        val otp = otpInput.text.toString().trim()
        val companyId = "" // Company resolved from user ID server-side
        if (phone.isEmpty() || otp.isEmpty()) {
            statusText.text = LanguageManager.get("enter_phone_otp")
            return
        }
        statusText.text = LanguageManager.get("verifying")
        lifecycleScope.launch {
            try {
                val response = ApiClient.api.verifyOtp(OtpVerifyRequest(phone, otp, companyId))
                if (response.isSuccessful) {
                    val body = response.body()
                    if (body != null && body.success) {
                        SessionManager.saveAuth(body)
                        navigateToDashboard()
                    } else {
                        statusText.text = body?.message ?: LanguageManager.get("verification_failed")
                    }
                } else {
                    val errorMsg = parseErrorMessage(response.code(), response.errorBody()?.string())
                    statusText.text = errorMsg
                }
            } catch (e: Exception) {
                statusText.text = LanguageManager.get("cannot_reach_server")
            }
        }
    }

    private fun showOtpPanel() {
        otpPanel.visibility = View.VISIBLE
    }

    private fun hideOtpPanel() {
        otpPanel.visibility = View.GONE
    }

    private fun showLanguagePanel() {
        languagePanel.visibility = View.VISIBLE
    }

    private fun hideLanguagePanel() {
        languagePanel.visibility = View.GONE
    }

    private fun navigateToDashboard() {
        startActivity(Intent(this, DashboardActivity::class.java))
        finish()
    }
}
