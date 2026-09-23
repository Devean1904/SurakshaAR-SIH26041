package com.surakshaar.ui.dashboard

import android.content.Intent
import android.graphics.Color
import android.os.Bundle
import android.util.Log
import android.view.View
import android.widget.AdapterView
import android.widget.ArrayAdapter
import android.widget.ImageButton
import android.widget.LinearLayout
import android.widget.Spinner
import android.widget.TextView
import android.widget.Toast
import androidx.activity.OnBackPressedCallback
import androidx.appcompat.app.AppCompatActivity
import androidx.cardview.widget.CardView
import androidx.lifecycle.lifecycleScope
import com.google.android.material.button.MaterialButton
import com.surakshaar.R
import com.surakshaar.data.api.ApiClient
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.language.LanguageManager
import com.surakshaar.ui.training.ARTrainingActivity
import com.surakshaar.ui.login.LoginActivity
import com.surakshaar.ui.settings.SettingsActivity
import com.surakshaar.ui.settings.ProfileActivity
import com.surakshaar.ui.certificate.CertificateActivity
import com.surakshaar.ui.admin.AdminActivity
import com.surakshaar.ui.admin.SiteMapActivity
import com.surakshaar.ui.manager.ManagerActivity
import com.surakshaar.util.ThemeManager
import com.surakshaar.util.VoiceManager
import com.google.zxing.integration.android.IntentIntegrator
import kotlinx.coroutines.CoroutineExceptionHandler
import kotlinx.coroutines.launch

class DashboardActivity : AppCompatActivity() {

    companion object {
        private const val TAG = "DashboardActivity"
    }

    private val exceptionHandler = CoroutineExceptionHandler { _, throwable ->
        Log.e(TAG, "Coroutine error in dashboard", throwable)
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        onBackPressedDispatcher.addCallback(this, object : OnBackPressedCallback(true) {
            override fun handleOnBackPressed() { finish() }
        })

        var errorMsg: String? = null

        try {
            ThemeManager.applyTheme(this, SessionManager.theme)
        } catch (e: Exception) {
            Log.e(TAG, "Theme apply failed", e)
            errorMsg = "Theme: ${e.javaClass.simpleName}: ${e.message}"
        }

        if (errorMsg == null) {
            try {
                setContentView(R.layout.activity_dashboard)
                VoiceManager.attachToActivity(this)
            } catch (e: Exception) {
                Log.e(TAG, "setContentView failed", e)
                errorMsg = "Layout: ${e.javaClass.simpleName}: ${e.message}"
            }
        }

        if (errorMsg != null) {
            val tv = TextView(this).apply {
                text = "ERROR:\n$errorMsg\n\nLog: $TAG"
                setTextColor(Color.RED)
                textSize = 14f
                setPadding(32, 32, 32, 32)
            }
            setContentView(tv)
            return
        }

        val role = SessionManager.role.ifEmpty { "worker" }
        val userName = SessionManager.userName.ifEmpty { "Worker" }

        try {
            setupWelcomeSection(userName, role)
        } catch (e: Exception) {
            Log.e(TAG, "Welcome section failed", e)
        }

        try {
            setupStatCards(role)
        } catch (e: Exception) {
            Log.e(TAG, "Stat cards failed", e)
        }

        try {
            setupLanguageSpinner()
        } catch (e: Exception) {
            Log.e(TAG, "Language spinner failed", e)
        }

        try {
            applyTranslations()
        } catch (e: Exception) {
            Log.e(TAG, "Translations failed", e)
        }

        try {
            setupRoleSections(role)
        } catch (e: Exception) {
            Log.e(TAG, "Role sections failed", e)
        }

        try {
            setupBottomNav(role)
        } catch (e: Exception) {
            Log.e(TAG, "Bottom nav failed", e)
        }

        try {
            fetchDashboardData(role)
        } catch (e: Exception) {
            Log.e(TAG, "Dashboard data fetch failed", e)
        }

        try {
            flushOfflineQueue()
        } catch (e: Exception) {
            Log.e(TAG, "Offline flush failed", e)
        }

        try {
            VoiceManager.speak(findViewById<TextView>(R.id.tvWelcome)?.text?.toString(), force = true)
        } catch (_: Exception) {
        }
    }

    private fun flushOfflineQueue() {
        if (!SessionManager.isAuthenticated) return
        val pending = com.surakshaar.data.offline.OfflineQueue.pendingCount()
        lifecycleScope.launch(exceptionHandler) {
            if (pending == 0) {
                com.surakshaar.data.offline.OfflineSync.prefetchOfflineData()
                return@launch
            }
            val ok = com.surakshaar.data.offline.OfflineSync.flushAll()
            if (ok) {
                com.surakshaar.data.offline.OfflineSync.prefetchOfflineData()
                Toast.makeText(
                    this@DashboardActivity,
                    LanguageManager.get("synced_items"),
                    Toast.LENGTH_SHORT
                ).show()
            }
        }
    }

    private fun setupWelcomeSection(userName: String, role: String) {
        try {
            val tvWelcome = findViewById<TextView>(R.id.tvWelcome)
            val greetingKey = when (role.lowercase()) {
                "admin", "manager" -> "greeting_manager"
                else -> "greeting_worker"
            }
            val greetingTemplate = LanguageManager.get(greetingKey)
            tvWelcome.text = greetingTemplate.replace("Worker", userName).replace("Manager", userName)

            val tvRoleBadge = findViewById<TextView>(R.id.tvRoleBadge)
            tvRoleBadge?.text = role.replaceFirstChar { it.uppercase() }
        } catch (e: Exception) {
            Log.e(TAG, "Welcome section failed", e)
        }
    }

    private fun setupStatCards(role: String) {
        try {
            val statLabel1 = findViewById<TextView>(R.id.statModulesLabel)
            val statLabel2 = findViewById<TextView>(R.id.statCertsLabel)
            val statLabel3 = findViewById<TextView>(R.id.statStreakLabel)

            when (role.lowercase()) {
                "admin" -> {
                    statLabel1.text = "Workers"
                    statLabel2.text = "Managers"
                    statLabel3.text = "Pending"
                }
                "manager" -> {
                    statLabel1.text = "My Workers"
                    statLabel2.text = "Completed"
                    statLabel3.text = "Pending"
                }
                else -> {
                    statLabel1.text = "Modules"
                    statLabel2.text = "Certificates"
                    statLabel3.text = "Day Streak"
                }
            }
        } catch (e: Exception) {
            Log.e(TAG, "Stat cards failed", e)
        }
    }

    private fun setupRoleSections(role: String) {
        val adminPanel = findViewById<View>(R.id.adminPanel)
        val managerPanel = findViewById<View>(R.id.managerPanel)
        val workerSections = findViewById<View>(R.id.workerSections)

        when (role.lowercase()) {
            "admin" -> {
                adminPanel?.visibility = View.VISIBLE
                managerPanel?.visibility = View.GONE
                workerSections?.visibility = View.GONE
                setupAdminButtons()
            }
            "manager" -> {
                adminPanel?.visibility = View.GONE
                managerPanel?.visibility = View.VISIBLE
                workerSections?.visibility = View.GONE
                setupManagerButtons()
            }
            else -> {
                adminPanel?.visibility = View.GONE
                managerPanel?.visibility = View.GONE
                workerSections?.visibility = View.VISIBLE
                setupDomainCards()
                setupQuickActions()
            }
        }
    }

    private fun setupDomainCards() {
        try {
            val domains = mapOf(
                R.id.cardFireExplosion to "fire_explosion",
                R.id.cardGasLeak to "gas_leak",
                R.id.cardMachineryLoto to "machinery_loto",
                R.id.cardElectricalHazards to "electrical_hazards",
                R.id.cardHeightsFall to "heights_fall"
            )

            domains.forEach { (cardId, domain) ->
                findViewById<CardView>(cardId)?.setOnClickListener {
                    try {
                        val intent = Intent(this, ARTrainingActivity::class.java)
                        intent.putExtra("domain", domain)
                        startActivity(intent)
                    } catch (e: Exception) {
                        Log.e(TAG, "Failed to launch training for $domain", e)
                        Toast.makeText(this, "Could not start training: ${e.message}", Toast.LENGTH_SHORT).show()
                    }
                }
            }
        } catch (e: Exception) {
            Log.e(TAG, "Domain cards failed", e)
        }
    }

    private fun setupQuickActions() {
        try {
            findViewById<MaterialButton>(R.id.btnResumeTraining)?.setOnClickListener {
                try {
                    val intent = Intent(this, ARTrainingActivity::class.java)
                    intent.putExtra("domain", "resume")
                    startActivity(intent)
                } catch (e: Exception) {
                    Log.e(TAG, "Failed to launch resume training", e)
                    Toast.makeText(this, "Could not start training: ${e.message}", Toast.LENGTH_SHORT).show()
                }
            }

            findViewById<MaterialButton>(R.id.btnViewCertificates)?.setOnClickListener {
                try {
                    startActivity(Intent(this, CertificateActivity::class.java))
                } catch (e: Exception) {
                    Toast.makeText(this, "Could not open certificates: ${e.message}", Toast.LENGTH_SHORT).show()
                }
            }

            findViewById<MaterialButton>(R.id.btnQrScan)?.setOnClickListener {
                launchQrScanner()
            }

            findViewById<MaterialButton>(R.id.btnSettings)?.setOnClickListener {
                try {
                    startActivity(Intent(this, SettingsActivity::class.java))
                } catch (e: Exception) {
                    Log.e(TAG, "Failed to launch settings", e)
                    Toast.makeText(this, "Could not open settings: ${e.message}", Toast.LENGTH_SHORT).show()
                }
            }
        } catch (e: Exception) {
            Log.e(TAG, "Quick actions failed", e)
        }
    }

    private fun setupTopBarButtons() {
        try {
            findViewById<ImageButton>(R.id.btnNotifications)?.setOnClickListener {
                Toast.makeText(this, LanguageManager.get("no_new_notifications"), Toast.LENGTH_SHORT).show()
            }

            findViewById<ImageButton>(R.id.btnTopSettings)?.setOnClickListener {
                try {
                    startActivity(Intent(this, SettingsActivity::class.java))
                } catch (e: Exception) {
                    Log.e(TAG, "Failed to launch settings", e)
                }
            }

            findViewById<ImageButton>(R.id.btnProfile)?.setOnClickListener {
                try {
                    startActivity(Intent(this, ProfileActivity::class.java))
                } catch (e: Exception) {
                    Log.e(TAG, "Failed to launch profile", e)
                }
            }
        } catch (e: Exception) {
            Log.e(TAG, "Top bar buttons failed", e)
        }
    }

    private fun setupBottomNav(role: String) {
        setupTopBarButtons()

        val bottomNav = findViewById<LinearLayout>(R.id.bottomNavBar)
        if (role.lowercase() == "admin") {
            bottomNav?.visibility = View.GONE
            return
        }

        val navHome = findViewById<LinearLayout>(R.id.navHome)
        val navTraining = findViewById<LinearLayout>(R.id.navTraining)
        val navCerts = findViewById<LinearLayout>(R.id.navCerts)
        val navProfile = findViewById<LinearLayout>(R.id.navProfile)

        val trainingLabel = navTraining?.findViewById<TextView>(R.id.navTrainingLabel)

        when (role.lowercase()) {
            "admin" -> {
                trainingLabel?.text = LanguageManager.get("admin_reports")
            }
            "manager" -> {
                trainingLabel?.text = LanguageManager.get("workers")
            }
            else -> {
                trainingLabel?.text = LanguageManager.get("nav_training")
            }
        }

        navHome?.setOnClickListener {
            findViewById<android.widget.ScrollView>(R.id.scrollView)?.smoothScrollTo(0, 0)
        }

        navTraining?.setOnClickListener {
            when (role.lowercase()) {
                "admin" -> {
                    try {
                        startActivity(Intent(this, AdminActivity::class.java).putExtra("tab", "analytics"))
                    } catch (e: Exception) {
                        Log.e(TAG, "Failed to launch admin analytics", e)
                    }
                }
                "manager" -> {
                    try {
                        startActivity(Intent(this, ManagerActivity::class.java))
                    } catch (e: Exception) {
                        Log.e(TAG, "Failed to launch manager panel", e)
                    }
                }
                else -> {
                    try {
                        startActivity(Intent(this, ARTrainingActivity::class.java))
                    } catch (e: Exception) {
                        Log.e(TAG, "Failed to launch training", e)
                    }
                }
            }
        }

        navCerts?.setOnClickListener {
            try {
                startActivity(Intent(this, CertificateActivity::class.java))
            } catch (e: Exception) {
                Log.e(TAG, "Failed to launch certificates", e)
            }
        }

        navProfile?.setOnClickListener {
            try {
                startActivity(Intent(this, ProfileActivity::class.java))
            } catch (e: Exception) {
                Log.e(TAG, "Failed to launch profile", e)
            }
        }
    }

    private fun setupAdminButtons() {
        try {
            val openWorkers = View.OnClickListener {
                try {
                    startActivity(Intent(this, AdminActivity::class.java).putExtra("tab", "workers"))
                } catch (e: Exception) {
                    Toast.makeText(this, "Could not open workers: ${e.message}", Toast.LENGTH_SHORT).show()
                }
            }
            val openManagers = View.OnClickListener {
                try {
                    startActivity(Intent(this, AdminActivity::class.java).putExtra("tab", "managers"))
                } catch (e: Exception) {
                    Toast.makeText(this, "Could not open managers: ${e.message}", Toast.LENGTH_SHORT).show()
                }
            }
            val openReports = View.OnClickListener {
                try {
                    startActivity(Intent(this, AdminActivity::class.java).putExtra("tab", "analytics"))
                } catch (e: Exception) {
                    Toast.makeText(this, "Could not open reports", Toast.LENGTH_SHORT).show()
                }
            }
            val openSiteMapping = View.OnClickListener {
                try {
                    startActivity(Intent(this, SiteMapActivity::class.java))
                } catch (e: Exception) {
                    Toast.makeText(this, "Could not open site mapping", Toast.LENGTH_SHORT).show()
                }
            }

            findViewById<ImageButton>(R.id.btnAdminUsers)?.setOnClickListener(openWorkers)
            findViewById<CardView>(R.id.cardAdminUsers)?.setOnClickListener(openWorkers)
            findViewById<ImageButton>(R.id.btnAdminManagers)?.setOnClickListener(openManagers)
            findViewById<CardView>(R.id.cardAdminManagers)?.setOnClickListener(openManagers)
            findViewById<ImageButton>(R.id.btnAdminReports)?.setOnClickListener(openReports)
            findViewById<CardView>(R.id.cardAdminReports)?.setOnClickListener(openReports)
            findViewById<ImageButton>(R.id.btnAdminSiteMapping)?.setOnClickListener(openSiteMapping)
            findViewById<CardView>(R.id.cardAdminSiteMapping)?.setOnClickListener(openSiteMapping)
        } catch (e: Exception) {
            Log.e(TAG, "Admin buttons failed", e)
        }
    }

    private fun setupManagerButtons() {
        try {
            findViewById<ImageButton>(R.id.btnManagerWorkers)?.setOnClickListener {
                try {
                    startActivity(Intent(this, ManagerActivity::class.java).putExtra("tab", "myWorkers"))
                } catch (e: Exception) {
                    Toast.makeText(this, LanguageManager.get("error_try_again"), Toast.LENGTH_SHORT).show()
                }
            }
            findViewById<ImageButton>(R.id.btnManagerAddWorker)?.setOnClickListener {
                try {
                    startActivity(Intent(this, ManagerActivity::class.java).putExtra("tab", "addWorker"))
                } catch (e: Exception) {
                    Toast.makeText(this, LanguageManager.get("error_try_again"), Toast.LENGTH_SHORT).show()
                }
            }
            findViewById<ImageButton>(R.id.btnManagerAssign)?.setOnClickListener {
                try {
                    startActivity(Intent(this, ManagerActivity::class.java).putExtra("tab", "assignTraining"))
                } catch (e: Exception) {
                    Toast.makeText(this, LanguageManager.get("error_try_again"), Toast.LENGTH_SHORT).show()
                }
            }
            findViewById<ImageButton>(R.id.btnManagerReports)?.setOnClickListener {
                try {
                    startActivity(Intent(this, ManagerActivity::class.java).putExtra("tab", "progress"))
                } catch (e: Exception) {
                    Toast.makeText(this, LanguageManager.get("error_try_again"), Toast.LENGTH_SHORT).show()
                }
            }
            findViewById<ImageButton>(R.id.btnManagerEscalations)?.setOnClickListener {
                loadManagerEscalations()
            }
            findViewById<ImageButton>(R.id.btnManagerScenarios)?.setOnClickListener {
                try {
                    startActivity(Intent(this, ManagerActivity::class.java).putExtra("tab", "progress"))
                } catch (e: Exception) {
                    Toast.makeText(this, LanguageManager.get("error_try_again"), Toast.LENGTH_SHORT).show()
                }
            }
        } catch (e: Exception) {
            Log.e(TAG, "Manager buttons failed", e)
        }
    }

    private fun launchQrScanner() {
        try {
            val integrator = IntentIntegrator(this)
            integrator.setDesiredBarcodeFormats(IntentIntegrator.QR_CODE)
            integrator.setPrompt(LanguageManager.get("qr_scan_prompt"))
            integrator.setBeepEnabled(true)
            integrator.setOrientationLocked(false)
            integrator.initiateScan()
        } catch (e: Exception) {
            Log.e(TAG, "QR scanner failed to launch", e)
            Toast.makeText(this, "QR scanner unavailable: ${e.message}", Toast.LENGTH_SHORT).show()
        }
    }

    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
        val result = IntentIntegrator.parseActivityResult(requestCode, resultCode, data)
        if (result != null) {
            if (result.contents.isNullOrEmpty()) {
                Toast.makeText(this, LanguageManager.get("qr_scan_prompt"), Toast.LENGTH_SHORT).show()
            } else {
                verifyScannedCertificate(result.contents)
            }
        } else {
            super.onActivityResult(requestCode, resultCode, data)
        }
    }

    private fun verifyScannedCertificate(payload: String) {
        lifecycleScope.launch(exceptionHandler) {
            try {
                val response = ApiClient.api.verifyCertificate(payload)
                val body = response.body()
                if (response.isSuccessful && body != null && body.valid) {
                    val message = buildString {
                        appendLine(LanguageManager.get("qr_valid"))
                        appendLine("ID: ${body.certificateId}")
                        appendLine("${body.employeeName} — ${body.moduleName}")
                        append("${LanguageManager.get("assess_total")} ${body.score}")
                    }
                    androidx.appcompat.app.AlertDialog.Builder(this@DashboardActivity)
                        .setTitle(LanguageManager.get("qr_verify_title"))
                        .setMessage(message)
                        .setPositiveButton(android.R.string.ok, null)
                        .show()
                } else {
                    androidx.appcompat.app.AlertDialog.Builder(this@DashboardActivity)
                        .setTitle(LanguageManager.get("qr_verify_title"))
                        .setMessage(LanguageManager.get("qr_invalid"))
                        .setPositiveButton(android.R.string.ok, null)
                        .show()
                }
            } catch (e: Exception) {
                Toast.makeText(
                    this@DashboardActivity,
                    LanguageManager.get("verification_failed"),
                    Toast.LENGTH_LONG
                ).show()
            }
        }
    }

    private fun loadAdminReports() {
        val token = SessionManager.authHeader
        lifecycleScope.launch(exceptionHandler) {
            try {
                val response = ApiClient.api.getWorkers(token)
                if (response.isSuccessful) {
                    val workers = response.body() ?: emptyList()
                    val pending = workers.count { it.awaitingAdminConfirmation }
                    val active = workers.count { it.isActive }
                    val msg = "Workers: $active active, $pending pending admin confirmation"
                    Toast.makeText(this@DashboardActivity, msg, Toast.LENGTH_LONG).show()
                }
            } catch (e: Exception) {
                Log.e(TAG, "Failed to load reports", e)
                Toast.makeText(this@DashboardActivity, LanguageManager.get("failed_to_load"), Toast.LENGTH_SHORT).show()
            }
        }
    }

    private fun loadAdminSites() {
        val token = SessionManager.authHeader
        lifecycleScope.launch(exceptionHandler) {
            try {
                val response = ApiClient.api.getAdminSites(token)
                if (response.isSuccessful) {
                    val sites = response.body() ?: emptyList()
                    Toast.makeText(this@DashboardActivity, "Sites loaded: ${sites.size}", Toast.LENGTH_SHORT).show()
                }
            } catch (e: Exception) {
                Toast.makeText(this@DashboardActivity, LanguageManager.get("failed_to_load"), Toast.LENGTH_SHORT).show()
            }
        }
    }

    private fun loadManagerEscalations() {
        val token = SessionManager.authHeader
        lifecycleScope.launch(exceptionHandler) {
            try {
                val response = ApiClient.api.getManagerEscalations(token)
                if (response.isSuccessful) {
                    val escalations = response.body() ?: emptyList()
                    Toast.makeText(this@DashboardActivity, "Escalations: ${escalations.size}", Toast.LENGTH_SHORT).show()
                }
            } catch (e: Exception) {
                Toast.makeText(this@DashboardActivity, LanguageManager.get("failed_to_load"), Toast.LENGTH_SHORT).show()
            }
        }
    }

    private fun setupLanguageSpinner() {
        try {
            val spinner = findViewById<Spinner>(R.id.spinnerLanguage)
            val languageNames = LanguageManager.getSupportedLanguages().map { LanguageManager.getLanguageName(it) }.toTypedArray()
            val languageCodes = LanguageManager.getSupportedLanguages()
            val adapter = ArrayAdapter(this, android.R.layout.simple_spinner_item, languageNames)
            adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
            spinner.adapter = adapter

            val currentLang = SessionManager.language.ifEmpty { LanguageManager.getCurrentLanguage() }
            val currentIndex = languageCodes.indexOf(currentLang)
            if (currentIndex >= 0) {
                spinner.setSelection(currentIndex)
            }

            var lastSelectedIndex = currentIndex
            spinner.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
                override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                    if (position == lastSelectedIndex) return
                    lastSelectedIndex = position
                    val selectedLang = languageCodes[position]
                    LanguageManager.setLanguage(selectedLang)
                    SessionManager.language = selectedLang
                    VoiceManager.applyLanguage(selectedLang)
                    recreate()
                }

                override fun onNothingSelected(parent: AdapterView<*>?) {}
            }
        } catch (e: Exception) {
            Log.e(TAG, "Language spinner failed", e)
        }
    }

    private fun applyTranslations() {
        try {
            val t = LanguageManager
            val userName = SessionManager.userName.ifEmpty { "Worker" }
            val role = SessionManager.role.ifEmpty { "worker" }
            val greetingKey = when (role.lowercase()) {
                "admin", "manager" -> "greeting_manager"
                else -> "greeting_worker"
            }
            val greetingTemplate = t.get(greetingKey)
            findViewById<TextView>(R.id.tvWelcome)?.text = greetingTemplate.replace("Worker", userName).replace("Manager", userName)
            findViewById<TextView>(R.id.statModulesLabel)?.text = when (role.lowercase()) {
                "admin" -> t.get("stat_workers")
                "manager" -> t.get("stat_my_workers")
                else -> t.get("stat_modules")
            }
            findViewById<TextView>(R.id.statCertsLabel)?.text = when (role.lowercase()) {
                "admin" -> t.get("stat_managers")
                "manager" -> t.get("stat_completed")
                else -> t.get("stat_certs")
            }
            findViewById<TextView>(R.id.statStreakLabel)?.text = when (role.lowercase()) {
                "admin" -> t.get("stat_pending")
                "manager" -> t.get("stat_pending")
                else -> t.get("stat_streak")
            }
            findViewById<TextView>(R.id.trainingDomainsHeader)?.text = t.get("training_domains")
            findViewById<MaterialButton>(R.id.btnResumeTraining)?.text = t.get("resume_training")
            findViewById<MaterialButton>(R.id.btnViewCertificates)?.text = t.get("view_certs")
            findViewById<MaterialButton>(R.id.btnQrScan)?.text = t.get("qr_scan")
            findViewById<MaterialButton>(R.id.btnSettings)?.text = t.get("settings")
            findViewById<TextView>(R.id.quickActionsHeader)?.text = t.get("quick_actions")
            findViewById<TextView>(R.id.adminPanelHeader)?.text = t.get("admin_panel")
            findViewById<TextView>(R.id.managerPanelHeader)?.text = t.get("manager_panel")
            findViewById<TextView>(R.id.adminUsersLabel)?.text = t.get("admin_manage_users")
            findViewById<TextView>(R.id.adminUsersDesc)?.text = t.get("admin_manage_users_desc")
            findViewById<TextView>(R.id.adminManagersLabel)?.text = t.get("admin_manage_managers")
            findViewById<TextView>(R.id.adminManagersDesc)?.text = t.get("admin_manage_managers_desc")
            findViewById<TextView>(R.id.adminReportsLabel)?.text = t.get("admin_reports")
            findViewById<TextView>(R.id.adminReportsDesc)?.text = t.get("admin_reports_desc")
            findViewById<TextView>(R.id.adminSiteMappingLabel)?.text = t.get("admin_site_mapping")
            findViewById<TextView>(R.id.adminSiteMappingDesc)?.text = t.get("admin_site_mapping_desc")
            findViewById<TextView>(R.id.managerMyWorkersLabel)?.text = t.get("manager_my_workers")
            findViewById<TextView>(R.id.managerAddWorkerLabel)?.text = t.get("manager_add_worker")
            findViewById<TextView>(R.id.managerAssignLabel)?.text = t.get("manager_assign_training")
            findViewById<TextView>(R.id.managerProgressLabel)?.text = t.get("manager_progress")
            findViewById<TextView>(R.id.managerEscalationsLabel)?.text = t.get("manager_escalations")
            findViewById<TextView>(R.id.managerScenariosLabel)?.text = t.get("manager_scenarios")
            findViewById<TextView>(R.id.domainTitleFire)?.text = t.get("domain_fire")
            findViewById<TextView>(R.id.domainTitleGas)?.text = t.get("domain_gas")
            findViewById<TextView>(R.id.domainTitleMachinery)?.text = t.get("domain_machinery")
            findViewById<TextView>(R.id.domainTitleElectrical)?.text = t.get("domain_electrical")
            findViewById<TextView>(R.id.domainTitleHeights)?.text = t.get("domain_heights")
            findViewById<TextView>(R.id.domainDescFire)?.text = t.get("domain_fire_desc")
            findViewById<TextView>(R.id.domainDescGas)?.text = t.get("domain_gas_desc")
            findViewById<TextView>(R.id.domainDescMachinery)?.text = t.get("domain_machinery_desc")
            findViewById<TextView>(R.id.domainDescElectrical)?.text = t.get("domain_electrical_desc")
            findViewById<TextView>(R.id.domainDescHeights)?.text = t.get("domain_heights_desc")
            findViewById<TextView>(R.id.adminControlCenterLabel)?.text = t.get("admin_control_center")
            findViewById<TextView>(R.id.navHomeLabel)?.text = t.get("nav_home")
            findViewById<TextView>(R.id.navCertsLabel)?.text = t.get("nav_certs")
            findViewById<TextView>(R.id.navProfileLabel)?.text = t.get("nav_profile")
            findViewById<TextView>(R.id.tvStatus)?.text = t.get("online")
        } catch (e: Exception) {
            Log.e(TAG, "Translation apply failed", e)
        }
    }

    private fun fetchDashboardData(role: String) {
        if (!SessionManager.isAuthenticated) return

        val token = SessionManager.authHeader
        val userId = SessionManager.userId

        lifecycleScope.launch(exceptionHandler) {
            try {
                when (role.lowercase()) {
                    "worker" -> {
                        val historyResponse = ApiClient.api.getTrainingHistory(token, userId)
                        if (historyResponse.isSuccessful) {
                            val history = historyResponse.body() ?: emptyList()
                            val completed = history.count { it.passed }
                            val totalAttempts = history.size
                            findViewById<TextView>(R.id.tvModulesCount)?.text = totalAttempts.toString()
                            findViewById<TextView>(R.id.tvCertsCount)?.text = completed.toString()

                            val recentDays = history
                                .mapNotNull {
                                    try {
                                        val dateStr = it.startTime?.take(10) ?: return@mapNotNull null
                                        if (dateStr.length < 10) return@mapNotNull null
                                        java.text.SimpleDateFormat("yyyy-MM-dd", java.util.Locale.US).parse(dateStr)
                                    } catch (e: Exception) {
                                        null
                                    }
                                }
                                .filter { it.time > System.currentTimeMillis() - 7 * 24 * 60 * 60 * 1000 }
                                .map { it.toInstant().atZone(java.time.ZoneId.systemDefault()).toLocalDate() }
                                .distinct()
                                .size
                            findViewById<TextView>(R.id.tvStreakCount)?.text = recentDays.toString()
                        }
                    }
                    "manager" -> {
                        loadManagerStats(token)
                    }
                    "admin" -> {
                        loadAdminStats(token)
                    }
                }
            } catch (e: Exception) {
                Log.w(TAG, "Could not fetch dashboard data (offline or server error)", e)
            }
        }
    }

    private fun loadManagerStats(token: String) {
        lifecycleScope.launch(exceptionHandler) {
            try {
                val workersResponse = ApiClient.api.getMyWorkers(token)
                if (workersResponse.isSuccessful) {
                    val workers = workersResponse.body() ?: emptyList()
                    findViewById<TextView>(R.id.tvModulesCount)?.text = workers.size.toString()
                    val pending = workers.count { it.awaitingAdminConfirmation }
                    findViewById<TextView>(R.id.tvStreakCount)?.text = pending.toString()

                    var completed = 0
                    workers.forEach { worker ->
                        try {
                            val histResponse = ApiClient.api.getTrainingHistory(token, worker.userId)
                            if (histResponse.isSuccessful) {
                                completed += (histResponse.body()?.count { it.passed } ?: 0)
                            }
                        } catch (_: Exception) {}
                    }
                    findViewById<TextView>(R.id.tvCertsCount)?.text = completed.toString()
                }
            } catch (e: Exception) {
                Log.w(TAG, "Could not load manager stats", e)
            }
        }
    }

    private fun loadAdminStats(token: String) {
        lifecycleScope.launch(exceptionHandler) {
            try {
                val workersResponse = ApiClient.api.getWorkers(token)
                if (workersResponse.isSuccessful) {
                    val workers = workersResponse.body() ?: emptyList()
                    findViewById<TextView>(R.id.tvModulesCount)?.text = workers.size.toString()
                }
                val managersResponse = ApiClient.api.getManagers(token)
                if (managersResponse.isSuccessful) {
                    val managers = managersResponse.body() ?: emptyList()
                    findViewById<TextView>(R.id.tvCertsCount)?.text = managers.size.toString()
                }
                val pendingResponse = ApiClient.api.getPendingWorkers(token)
                if (pendingResponse.isSuccessful) {
                    val pending = pendingResponse.body() ?: emptyList()
                    findViewById<TextView>(R.id.tvStreakCount)?.text = pending.size.toString()
                }
            } catch (e: Exception) {
                Log.w(TAG, "Could not load admin stats", e)
            }
        }
    }
}
