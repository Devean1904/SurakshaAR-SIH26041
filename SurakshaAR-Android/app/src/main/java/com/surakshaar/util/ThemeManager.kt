package com.surakshaar.util

import android.app.Activity
import android.os.Build
import android.view.Window
import android.view.WindowManager
import androidx.appcompat.app.AppCompatDelegate

object ThemeManager {

    fun applyTheme(activity: Activity, theme: String) {
        val nightMode = when (theme.lowercase()) {
            "light" -> AppCompatDelegate.MODE_NIGHT_NO
            "dark" -> AppCompatDelegate.MODE_NIGHT_YES
            else -> AppCompatDelegate.MODE_NIGHT_FOLLOW_SYSTEM
        }
        AppCompatDelegate.setDefaultNightMode(nightMode)

        val window: Window = activity.window
        window.addFlags(WindowManager.LayoutParams.FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS)

        when (theme.lowercase()) {
            "light" -> {
                window.decorView.setBackgroundColor(android.graphics.Color.parseColor("#FFFFFFFF"))
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                    window.navigationBarColor = android.graphics.Color.parseColor("#FFF1F5F9")
                    window.statusBarColor = android.graphics.Color.parseColor("#FFF1F5F9")
                }
            }
            else -> {
                window.decorView.setBackgroundColor(android.graphics.Color.parseColor("#FF0D0E12"))
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                    window.navigationBarColor = android.graphics.Color.parseColor("#FF1A1C24")
                    window.statusBarColor = android.graphics.Color.parseColor("#FF1A1C24")
                }
            }
        }

    }

    fun parseColor(hex: String): Int {
        return android.graphics.Color.parseColor(hex)
    }
}
