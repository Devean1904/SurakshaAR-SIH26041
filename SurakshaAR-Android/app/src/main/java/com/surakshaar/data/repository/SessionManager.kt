package com.surakshaar.data.repository

import android.content.Context
import android.content.SharedPreferences
import com.surakshaar.data.model.*

object SessionManager {
    private const val PREF_NAME = "suraksha_session"

    lateinit var prefs: SharedPreferences
        private set

    fun init(context: Context) {
        prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE)
    }

    var token: String
        get() = prefs.getString("Token", "") ?: ""
        set(v) = prefs.edit().putString("Token", v).apply()

    var userId: String
        get() = prefs.getString("UserId", "") ?: ""
        set(v) = prefs.edit().putString("UserId", v).apply()

    var userName: String
        get() = prefs.getString("UserName", "") ?: ""
        set(v) = prefs.edit().putString("UserName", v).apply()

    var role: String
        get() = prefs.getString("Role", "worker") ?: "worker"
        set(v) = prefs.edit().putString("Role", v).apply()

    var companyId: String
        get() = prefs.getString("CompanyId", "") ?: ""
        set(v) = prefs.edit().putString("CompanyId", v).apply()

    var theme: String
        get() = prefs.getString("Theme", "dark") ?: "dark"
        set(v) = prefs.edit().putString("Theme", v).apply()

    var language: String
        get() = prefs.getString("Language", "en") ?: "en"
        set(v) = prefs.edit().putString("Language", v).apply()

    var screenReader: Boolean
        get() = prefs.getBoolean("ScreenReader", false)
        set(v) = prefs.edit().putBoolean("ScreenReader", v).apply()

    var speakOnTap: Boolean
        get() = prefs.getBoolean("SpeakOnTap", true)
        set(v) = prefs.edit().putBoolean("SpeakOnTap", v).apply()

    var modulesCompleted: Int
        get() = prefs.getInt("ModulesCompleted", 0)
        set(v) = prefs.edit().putInt("ModulesCompleted", v).apply()

    var certCount: Int
        get() = prefs.getInt("CertCount", 0)
        set(v) = prefs.edit().putInt("CertCount", v).apply()

    var dayStreak: Int
        get() = prefs.getInt("DayStreak", 7)
        set(v) = prefs.edit().putInt("DayStreak", v).apply()

    fun saveAuth(response: AuthResponse) {
        token = response.token ?: ""
        userId = response.userId ?: ""
        role = response.role ?: "worker"
        userName = response.name ?: ""
        theme = response.theme ?: "dark"
        language = response.language ?: "en"
        val responseCompanyId = response.companyId
        if (!responseCompanyId.isNullOrEmpty()) {
            companyId = responseCompanyId
        }
    }

    fun saveOfflineCredentials(userId: String, password: String) {
        prefs.edit()
            .putString("OfflineUserId", userId)
            .putString("OfflinePassword", password)
            .apply()
    }

    fun verifyOfflineCredentials(userId: String, password: String): Boolean {
        if (!hasOfflineCredentials()) return false
        val savedId = prefs.getString("OfflineUserId", "") ?: ""
        val savedPw = prefs.getString("OfflinePassword", "") ?: ""
        return savedId == userId && savedPw == password
    }

    fun hasOfflineCredentials(): Boolean {
        return (prefs.getString("OfflineUserId", "") ?: "").isNotEmpty()
    }

    fun clear() {
        prefs.edit().clear().apply()
    }

    val isAuthenticated: Boolean get() = token.isNotEmpty()
    val authHeader: String get() = "Bearer $token"
}
