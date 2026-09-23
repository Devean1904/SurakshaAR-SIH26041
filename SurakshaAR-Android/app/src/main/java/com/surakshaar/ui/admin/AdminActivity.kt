package com.surakshaar.ui.admin

import android.os.Bundle
import android.util.Log
import android.view.View
import android.widget.*
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import androidx.lifecycle.lifecycleScope
import com.google.android.material.button.MaterialButton
import com.google.android.material.textfield.TextInputEditText
import com.surakshaar.R
import com.surakshaar.data.api.ApiClient
import com.surakshaar.data.model.ConfirmWorkerRequest
import com.surakshaar.data.model.RemoveUserRequest
import com.surakshaar.data.model.AddManagerRequest
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.data.repository.SiteStorage
import com.surakshaar.language.LanguageManager
import com.surakshaar.util.ThemeManager
import com.surakshaar.util.VoiceManager
import kotlinx.coroutines.CoroutineExceptionHandler
import kotlinx.coroutines.launch

class AdminActivity : AppCompatActivity() {

    companion object {
        private const val TAG = "AdminActivity"
    }

    private val exceptionHandler = CoroutineExceptionHandler { _, throwable ->
        Log.e(TAG, "Coroutine error", throwable)
    }

    private lateinit var tabWorkers: LinearLayout
    private lateinit var tabManagers: LinearLayout
    private lateinit var tabAnalytics: LinearLayout
    private lateinit var tabSites: LinearLayout
    private lateinit var contentFrame: LinearLayout
    private lateinit var backButton: Button
    private lateinit var titleText: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        ThemeManager.applyTheme(this, SessionManager.theme)
        setContentView(R.layout.activity_admin)
        VoiceManager.attachToActivity(this)

        backButton = findViewById(R.id.backButton)
        titleText = findViewById(R.id.titleText)
        tabWorkers = findViewById(R.id.tabWorkers)
        tabManagers = findViewById(R.id.tabManagers)
        tabAnalytics = findViewById(R.id.tabAnalytics)
        tabSites = findViewById(R.id.tabSites)
        contentFrame = findViewById(R.id.contentFrame)

        backButton.setOnClickListener { finish() }
        titleText.text = LanguageManager.get("admin_panel")

        val initialTab = intent.getStringExtra("tab") ?: "workers"
        setupTabs(initialTab)
    }

    private fun setupTabs(initialTab: String) {
        selectTab(initialTab)
        tabWorkers.setOnClickListener { selectTab("workers") }
        tabManagers.setOnClickListener { selectTab("managers") }
        tabAnalytics.setOnClickListener { selectTab("analytics") }
        tabSites.setOnClickListener { selectTab("sites") }
    }

    private fun selectTab(tab: String) {
        val tabs = listOf(tabWorkers, tabManagers, tabAnalytics, tabSites)
        tabs.forEach { it.setBackgroundColor(getColor(R.color.surface)) }
        val tabTextViews = listOf(
            tabWorkers.findViewById<TextView>(R.id.tabWorkersLabel),
            tabManagers.findViewById<TextView>(R.id.tabManagersLabel),
            tabAnalytics.findViewById<TextView>(R.id.tabAnalyticsLabel),
            tabSites.findViewById<TextView>(R.id.tabSitesLabel)
        )
        tabTextViews.forEach { it?.setTextColor(getColor(R.color.white)) }

        when (tab) {
            "workers" -> {
                tabWorkers.setBackgroundColor(getColor(R.color.primary))
                tabTextViews[0]?.text = LanguageManager.get("stat_workers")
                tabTextViews[0]?.paint?.isFakeBoldText = true
                loadWorkers()
            }
            "managers" -> {
                tabManagers.setBackgroundColor(getColor(R.color.primary))
                tabTextViews[1]?.text = LanguageManager.get("stat_managers")
                tabTextViews[1]?.paint?.isFakeBoldText = true
                loadManagers()
            }
            "analytics" -> {
                tabAnalytics.setBackgroundColor(getColor(R.color.primary))
                tabTextViews[2]?.text = LanguageManager.get("admin_reports")
                tabTextViews[2]?.paint?.isFakeBoldText = true
                loadAnalytics()
            }
            "sites" -> {
                tabSites.setBackgroundColor(getColor(R.color.primary))
                tabTextViews[3]?.text = LanguageManager.get("admin_site_mapping")
                tabTextViews[3]?.paint?.isFakeBoldText = true
                loadSites()
            }
        }
    }

    // ── WORKERS TAB ──

    private fun loadWorkers() {
        contentFrame.removeAllViews()
        val progress = ProgressBar(this)
        contentFrame.addView(progress)
        val token = SessionManager.authHeader

        lifecycleScope.launch(exceptionHandler) {
            try {
                val pendingResponse = ApiClient.api.getPendingWorkers(token)
                val allResponse = ApiClient.api.getWorkers(token)
                progress.visibility = View.GONE

                if (pendingResponse.isSuccessful) {
                    val pending = pendingResponse.body() ?: emptyList()
                    if (pending.isNotEmpty()) {
                        val header = TextView(this@AdminActivity).apply {
                            text = "⏳ ${LanguageManager.get("stat_pending")} (${pending.size})"
                            setTextColor(getColor(R.color.warning))
                            textSize = 15f
                            setPadding(16, 16, 16, 8)
                        }
                        contentFrame.addView(header)
                        pending.forEach { worker ->
                            contentFrame.addView(createWorkerCard(worker.userId, worker.name, worker.phoneNumber, true, worker.enrolledByManagerId))
                        }
                    }
                }

                if (allResponse.isSuccessful) {
                    val allWorkers = allResponse.body() ?: emptyList()
                    val active = allWorkers.filter { !it.awaitingAdminConfirmation }
                    if (active.isNotEmpty()) {
                        val header = TextView(this@AdminActivity).apply {
                            text = "✓ ${LanguageManager.get("stat_workers")} (${active.size})"
                            setTextColor(getColor(R.color.success))
                            textSize = 15f
                            setPadding(16, 24, 16, 8)
                        }
                        contentFrame.addView(header)
                        active.forEach { worker ->
                            contentFrame.addView(createWorkerCard(worker.userId, worker.name, worker.phoneNumber, false, worker.enrolledByManagerId))
                        }
                    }
                    if (pendingResponse.isSuccessful && (pendingResponse.body()?.size ?: 0) == 0 && active.isEmpty()) {
                        showEmpty("No workers found")
                    }
                }
            } catch (e: Exception) {
                progress.visibility = View.GONE
                showEmpty("Error: ${e.message}")
            }
        }
    }

    private fun createWorkerCard(userId: String, name: String, phone: String, pending: Boolean, managerId: String): View {
        val card = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(32, 20, 32, 20)
            setBackgroundColor(getColor(R.color.surface))
            val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
            p.bottomMargin = 8
            layoutParams = p
        }

        card.addView(TextView(this).apply {
            text = name
            setTextColor(getColor(R.color.white))
            textSize = 16f
            paint.isFakeBoldText = true
        })

        card.addView(TextView(this).apply {
            text = "ID: $userId"
            setTextColor(getColor(R.color.text_secondary))
            textSize = 12f
        })

        card.addView(TextView(this).apply {
            text = "Phone: $phone"
            setTextColor(getColor(R.color.text_secondary))
            textSize = 12f
        })

        if (managerId.isNotEmpty()) {
            card.addView(TextView(this).apply {
                text = "Manager: $managerId"
                setTextColor(getColor(R.color.text_muted))
                textSize = 11f
            })
        }

        val statusText = TextView(this).apply {
            text = if (pending) "⏳ Pending approval" else "✓ Active"
            setTextColor(if (pending) getColor(R.color.warning) else getColor(R.color.success))
            textSize = 13f
            setPadding(0, 4, 0, 0)
        }
        card.addView(statusText)

        val btnRow = LinearLayout(this).apply {
            orientation = LinearLayout.HORIZONTAL
            val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
            p.topMargin = 10
            layoutParams = p
        }

        if (pending) {
            btnRow.addView(MaterialButton(this).apply {
                text = "✓ Approve"
                setTextColor(getColor(R.color.white))
                setBackgroundColor(getColor(R.color.success))
                val p = LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WRAP_CONTENT, 1f)
                p.marginEnd = 8
                layoutParams = p
                setOnClickListener { confirmWorker(userId) }
            })
        }

        btnRow.addView(MaterialButton(this).apply {
            text = "👤 Profile"
            setTextColor(getColor(R.color.white))
            setBackgroundColor(getColor(R.color.primary))
            val p = LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WRAP_CONTENT, 1f)
            p.marginEnd = if (pending) 8 else 0
            layoutParams = p
            setOnClickListener { showWorkerProfile(userId, name, phone, managerId) }
        })

        btnRow.addView(MaterialButton(this).apply {
            text = "✕ Remove"
            setTextColor(getColor(R.color.white))
            setBackgroundColor(getColor(R.color.danger))
            val p = LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WRAP_CONTENT, 1f)
            layoutParams = p
            setOnClickListener { removeWorker(userId) }
        })

        card.addView(btnRow)
        return card
    }

    // ── WORKER PROFILE (matches manager profile style + analytics) ──

    private fun showWorkerProfile(userId: String, name: String, phone: String, managerId: String) {
        contentFrame.removeAllViews()
        val progress = ProgressBar(this)
        contentFrame.addView(progress)
        val token = SessionManager.authHeader

        lifecycleScope.launch(exceptionHandler) {
            try {
                val histResponse = ApiClient.api.getTrainingHistory(token, userId)
                progress.visibility = View.GONE

                val backBtn = MaterialButton(this@AdminActivity).apply {
                    text = "← Back to Workers"
                    setTextColor(getColor(R.color.white))
                    setBackgroundColor(getColor(R.color.surface))
                    val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.WRAP_CONTENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                    p.bottomMargin = 12
                    layoutParams = p
                    setOnClickListener { loadWorkers() }
                }
                contentFrame.addView(backBtn)

                val title = TextView(this@AdminActivity).apply {
                    text = "Worker: $name ($userId)"
                    setTextColor(getColor(R.color.white))
                    textSize = 18f
                    paint.isFakeBoldText = true
                    setPadding(16, 8, 16, 8)
                }
                contentFrame.addView(title)

                val infoCard = LinearLayout(this@AdminActivity).apply {
                    orientation = LinearLayout.VERTICAL
                    setPadding(32, 20, 32, 20)
                    setBackgroundColor(getColor(R.color.surface))
                    val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                    p.bottomMargin = 12
                    layoutParams = p
                }
                infoCard.addView(TextView(this@AdminActivity).apply {
                    text = "Phone: $phone"
                    setTextColor(getColor(R.color.text_secondary))
                    textSize = 13f
                })
                if (managerId.isNotEmpty()) {
                    infoCard.addView(TextView(this@AdminActivity).apply {
                        text = "Enrolled by: $managerId"
                        setTextColor(getColor(R.color.text_secondary))
                        textSize = 13f
                    })
                }
                contentFrame.addView(infoCard)

                val history = if (histResponse.isSuccessful) histResponse.body() ?: emptyList() else emptyList()
                val totalAttempts = history.size
                val passed = history.count { it.passed }
                val avgScore = if (totalAttempts > 0) history.map { it.totalScore }.average().toInt() else 0
                val passRate = if (totalAttempts > 0) (passed * 100 / totalAttempts) else 0

                val perfCard = LinearLayout(this@AdminActivity).apply {
                    orientation = LinearLayout.VERTICAL
                    setPadding(32, 20, 32, 20)
                    setBackgroundColor(getColor(R.color.surface))
                    val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                    p.bottomMargin = 12
                    layoutParams = p
                }

                perfCard.addView(TextView(this@AdminActivity).apply {
                    text = "Performance"
                    setTextColor(getColor(R.color.white))
                    textSize = 15f
                    paint.isFakeBoldText = true
                    setPadding(0, 0, 0, 8)
                })

                val statsRow = LinearLayout(this@AdminActivity).apply {
                    orientation = LinearLayout.HORIZONTAL
                    layoutParams = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                }
                statsRow.addView(createStatBadge("Attempts", totalAttempts.toString(), R.color.primary))
                statsRow.addView(createStatBadge("Passed", passed.toString(), R.color.success))
                statsRow.addView(createStatBadge("Avg Score", "$avgScore%", R.color.warning))
                perfCard.addView(statsRow)

                contentFrame.addView(perfCard)

                if (history.isNotEmpty()) {
                    val histHeader = TextView(this@AdminActivity).apply {
                        text = "Training History"
                        setTextColor(getColor(R.color.text_secondary))
                        textSize = 15f
                        setPadding(16, 16, 16, 8)
                    }
                    contentFrame.addView(histHeader)

                    history.take(10).forEach { attempt ->
                        val attemptCard = LinearLayout(this@AdminActivity).apply {
                            orientation = LinearLayout.VERTICAL
                            setPadding(24, 12, 24, 12)
                            setBackgroundColor(getColor(R.color.surface))
                            val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                            p.bottomMargin = 6
                            layoutParams = p
                        }

                        attemptCard.addView(TextView(this@AdminActivity).apply {
                            text = attempt.moduleId
                            setTextColor(getColor(R.color.white))
                            textSize = 13f
                            paint.isFakeBoldText = true
                        })

                        attemptCard.addView(TextView(this@AdminActivity).apply {
                            text = "Score: ${attempt.totalScore}% | ${if (attempt.passed) "✓ Passed" else "✕ Failed"}"
                            setTextColor(if (attempt.passed) getColor(R.color.success) else getColor(R.color.danger))
                            textSize = 12f
                        })

                        contentFrame.addView(attemptCard)
                    }
                } else {
                    contentFrame.addView(TextView(this@AdminActivity).apply {
                        text = "No training history yet"
                        setTextColor(getColor(R.color.text_secondary))
                        textSize = 14f
                        gravity = android.view.Gravity.CENTER
                        setPadding(0, 24, 0, 24)
                    })
                }
            } catch (e: Exception) {
                progress.visibility = View.GONE
                showEmpty("Error loading profile: ${e.message}")
            }
        }
    }

    private fun confirmWorker(workerId: String) {
        val token = SessionManager.authHeader
        lifecycleScope.launch(exceptionHandler) {
            try {
                val response = ApiClient.api.confirmWorker(token, ConfirmWorkerRequest(workerId))
                if (response.isSuccessful) {
                    Toast.makeText(this@AdminActivity, "Worker $workerId approved", Toast.LENGTH_SHORT).show()
                    loadWorkers()
                } else {
                    Toast.makeText(this@AdminActivity, "Failed: ${response.code()}", Toast.LENGTH_SHORT).show()
                }
            } catch (e: Exception) {
                Toast.makeText(this@AdminActivity, "Error: ${e.message}", Toast.LENGTH_SHORT).show()
            }
        }
    }

    private fun removeWorker(userId: String) {
        AlertDialog.Builder(this)
            .setTitle("Remove Worker")
            .setMessage("Are you sure you want to remove worker $userId?")
            .setPositiveButton("Remove") { _, _ ->
                val token = SessionManager.authHeader
                lifecycleScope.launch(exceptionHandler) {
                    try {
                        val response = ApiClient.api.removeWorker(token, RemoveUserRequest(userId))
                        if (response.isSuccessful) {
                            Toast.makeText(this@AdminActivity, "Worker $userId removed", Toast.LENGTH_SHORT).show()
                            loadWorkers()
                        } else {
                            Toast.makeText(this@AdminActivity, "Failed: ${response.code()}", Toast.LENGTH_SHORT).show()
                        }
                    } catch (e: Exception) {
                        Toast.makeText(this@AdminActivity, "Error: ${e.message}", Toast.LENGTH_SHORT).show()
                    }
                }
            }
            .setNegativeButton("Cancel", null)
            .show()
    }

    // ── MANAGERS TAB ──

    private fun loadManagers() {
        contentFrame.removeAllViews()
        val progress = ProgressBar(this)
        contentFrame.addView(progress)
        val token = SessionManager.authHeader

        lifecycleScope.launch(exceptionHandler) {
            try {
                val managersResponse = ApiClient.api.getManagers(token)
                val workersResponse = ApiClient.api.getWorkers(token)
                progress.visibility = View.GONE

                val addBtn = MaterialButton(this@AdminActivity).apply {
                    text = "+ Add New Manager"
                    setTextColor(getColor(R.color.white))
                    setBackgroundColor(getColor(R.color.success))
                    val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                    p.bottomMargin = 16
                    layoutParams = p
                    setOnClickListener { showAddManagerForm() }
                }
                contentFrame.addView(addBtn)

                if (managersResponse.isSuccessful) {
                    val managers = managersResponse.body() ?: emptyList()
                    val allWorkers = if (workersResponse.isSuccessful) workersResponse.body() ?: emptyList() else emptyList()

                    if (managers.isEmpty()) {
                        showEmpty("No managers found")
                        return@launch
                    }

                    managers.forEach { manager ->
                        val managerWorkers = allWorkers.filter { it.enrolledByManagerId == manager.userId }
                        val activeCount = managerWorkers.count { it.isActive && !it.awaitingAdminConfirmation }
                        val pendingCount = managerWorkers.count { it.awaitingAdminConfirmation }
                        contentFrame.addView(createManagerCard(manager.userId, manager.name, manager.phoneNumber, managerWorkers.size, activeCount, pendingCount))
                    }
                }
            } catch (e: Exception) {
                progress.visibility = View.GONE
                showEmpty("Error: ${e.message}")
            }
        }
    }

    private fun createManagerCard(userId: String, name: String, phone: String, totalWorkers: Int, activeWorkers: Int, pendingWorkers: Int): View {
        val card = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(32, 20, 32, 20)
            setBackgroundColor(getColor(R.color.surface))
            val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
            p.bottomMargin = 12
            layoutParams = p
        }

        card.addView(TextView(this).apply {
            text = name
            setTextColor(getColor(R.color.white))
            textSize = 16f
            paint.isFakeBoldText = true
        })

        card.addView(TextView(this).apply {
            text = "ID: $userId | Phone: $phone"
            setTextColor(getColor(R.color.text_secondary))
            textSize = 12f
        })

        val statsRow = LinearLayout(this).apply {
            orientation = LinearLayout.HORIZONTAL
            val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
            p.topMargin = 10
            layoutParams = p
        }
        statsRow.addView(createStatBadge("Total", totalWorkers.toString(), R.color.primary))
        statsRow.addView(createStatBadge("Active", activeWorkers.toString(), R.color.success))
        statsRow.addView(createStatBadge("Pending", pendingWorkers.toString(), R.color.warning))
        card.addView(statsRow)

        val btnRow = LinearLayout(this).apply {
            orientation = LinearLayout.HORIZONTAL
            val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
            p.topMargin = 10
            layoutParams = p
        }

        btnRow.addView(MaterialButton(this).apply {
            text = "👤 Profile"
            setTextColor(getColor(R.color.white))
            setBackgroundColor(getColor(R.color.primary))
            val p = LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WRAP_CONTENT, 1f)
            p.marginEnd = 8
            layoutParams = p
            setOnClickListener { showManagerAnalytics(userId, name) }
        })

        btnRow.addView(MaterialButton(this).apply {
            text = "✕ Remove"
            setTextColor(getColor(R.color.white))
            setBackgroundColor(getColor(R.color.danger))
            val p = LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WRAP_CONTENT, 1f)
            layoutParams = p
            setOnClickListener { removeManager(userId) }
        })

        card.addView(btnRow)
        return card
    }

    private fun showManagerAnalytics(managerId: String, managerName: String) {
        contentFrame.removeAllViews()
        val progress = ProgressBar(this)
        contentFrame.addView(progress)
        val token = SessionManager.authHeader

        lifecycleScope.launch(exceptionHandler) {
            try {
                val workersResponse = ApiClient.api.getWorkers(token)
                progress.visibility = View.GONE

                val backBtn = MaterialButton(this@AdminActivity).apply {
                    text = "← Back to Managers"
                    setTextColor(getColor(R.color.white))
                    setBackgroundColor(getColor(R.color.surface))
                    val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.WRAP_CONTENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                    p.bottomMargin = 12
                    layoutParams = p
                    setOnClickListener { loadManagers() }
                }
                contentFrame.addView(backBtn)

                val title = TextView(this@AdminActivity).apply {
                    text = "Manager: $managerName ($managerId)"
                    setTextColor(getColor(R.color.white))
                    textSize = 18f
                    setPadding(16, 16, 16, 8)
                    paint.isFakeBoldText = true
                }
                contentFrame.addView(title)

                if (workersResponse.isSuccessful) {
                    val myWorkers = (workersResponse.body() ?: emptyList()).filter { it.enrolledByManagerId == managerId }

                    val summaryCard = LinearLayout(this@AdminActivity).apply {
                        orientation = LinearLayout.VERTICAL
                        setPadding(32, 20, 32, 20)
                        setBackgroundColor(getColor(R.color.surface))
                        val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                        p.bottomMargin = 12
                        layoutParams = p
                    }

                    val statsRow = LinearLayout(this@AdminActivity).apply {
                        orientation = LinearLayout.HORIZONTAL
                        layoutParams = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                    }
                    statsRow.addView(createStatBadge("Total", myWorkers.size.toString(), R.color.primary))
                    statsRow.addView(createStatBadge("Active", myWorkers.count { it.isActive && !it.awaitingAdminConfirmation }.toString(), R.color.success))
                    statsRow.addView(createStatBadge("Pending", myWorkers.count { it.awaitingAdminConfirmation }.toString(), R.color.warning))
                    summaryCard.addView(statsRow)
                    contentFrame.addView(summaryCard)

                    if (myWorkers.isEmpty()) {
                        showEmpty("No workers under this manager")
                        return@launch
                    }

                    val workerHeader = TextView(this@AdminActivity).apply {
                        text = "Worker Performance"
                        setTextColor(getColor(R.color.text_secondary))
                        textSize = 15f
                        setPadding(16, 16, 16, 8)
                    }
                    contentFrame.addView(workerHeader)

                    myWorkers.forEach { worker ->
                        val workerCard = LinearLayout(this@AdminActivity).apply {
                            orientation = LinearLayout.VERTICAL
                            setPadding(32, 16, 32, 16)
                            setBackgroundColor(getColor(R.color.surface))
                            val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                            p.bottomMargin = 8
                            layoutParams = p
                        }

                        workerCard.addView(TextView(this@AdminActivity).apply {
                            text = "${worker.name} (${worker.userId})"
                            setTextColor(getColor(R.color.white))
                            textSize = 14f
                        })

                        val statusText = TextView(this@AdminActivity).apply {
                            text = "Loading performance data..."
                            setTextColor(getColor(R.color.text_secondary))
                            textSize = 12f
                        }
                        workerCard.addView(statusText)
                        contentFrame.addView(workerCard)

                        lifecycleScope.launch(exceptionHandler) {
                            try {
                                val histResponse = ApiClient.api.getTrainingHistory(token, worker.userId)
                                if (histResponse.isSuccessful) {
                                    val history = histResponse.body() ?: emptyList()
                                    val completed = history.count { it.passed }
                                    val total = history.size
                                    val avgScore = if (total > 0) history.map { it.totalScore }.average().toInt() else 0
                                    statusText.text = "Attempts: $total | Passed: $completed | Avg Score: $avgScore%"
                                } else {
                                    statusText.text = "No training data"
                                }
                            } catch (e: Exception) {
                                statusText.text = "Could not load data"
                            }
                        }
                    }
                }
            } catch (e: Exception) {
                progress.visibility = View.GONE
                showEmpty("Error: ${e.message}")
            }
        }
    }

    private fun showAddManagerForm() {
        val formLayout = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(48, 32, 48, 16)
        }

        val idInput = TextInputEditText(this).apply { hint = "Manager ID (e.g. M001)" }
        val nameInput = TextInputEditText(this).apply { hint = "Full Name" }
        val phoneInput = TextInputEditText(this).apply { hint = "Phone Number" }
        val passInput = TextInputEditText(this).apply { hint = "Password" }

        listOf(idInput, nameInput, phoneInput, passInput).forEach { formLayout.addView(it) }

        AlertDialog.Builder(this)
            .setTitle("Add New Manager")
            .setView(formLayout)
            .setPositiveButton("Add") { _, _ ->
                val userId = idInput.text.toString().trim()
                val name = nameInput.text.toString().trim()
                val phone = phoneInput.text.toString().trim()
                val pass = passInput.text.toString().trim()
                if (userId.isEmpty() || name.isEmpty() || pass.isEmpty()) {
                    Toast.makeText(this, "ID, Name, and Password required", Toast.LENGTH_SHORT).show()
                    return@setPositiveButton
                }
                addManager(userId, name, phone, pass)
            }
            .setNegativeButton("Cancel", null)
            .show()
    }

    private fun addManager(userId: String, name: String, phone: String, password: String) {
        val token = SessionManager.authHeader
        lifecycleScope.launch(exceptionHandler) {
            try {
                val response = ApiClient.api.addManager(token, AddManagerRequest(userId, name, phone, password))
                if (response.isSuccessful) {
                    Toast.makeText(this@AdminActivity, "Manager $userId added", Toast.LENGTH_SHORT).show()
                    loadManagers()
                } else {
                    Toast.makeText(this@AdminActivity, "Failed: ${response.code()}", Toast.LENGTH_SHORT).show()
                }
            } catch (e: Exception) {
                Toast.makeText(this@AdminActivity, "Error: ${e.message}", Toast.LENGTH_SHORT).show()
            }
        }
    }

    private fun removeManager(userId: String) {
        AlertDialog.Builder(this)
            .setTitle("Remove Manager")
            .setMessage("Remove manager $userId? Their workers will need reassignment.")
            .setPositiveButton("Remove") { _, _ ->
                val token = SessionManager.authHeader
                lifecycleScope.launch(exceptionHandler) {
                    try {
                        val response = ApiClient.api.removeManager(token, RemoveUserRequest(userId))
                        if (response.isSuccessful) {
                            Toast.makeText(this@AdminActivity, "Manager $userId removed", Toast.LENGTH_SHORT).show()
                            loadManagers()
                        } else {
                            Toast.makeText(this@AdminActivity, "Failed: ${response.code()}", Toast.LENGTH_SHORT).show()
                        }
                    } catch (e: Exception) {
                        Toast.makeText(this@AdminActivity, "Error: ${e.message}", Toast.LENGTH_SHORT).show()
                    }
                }
            }
            .setNegativeButton("Cancel", null)
            .show()
    }

    // ── ANALYTICS TAB ──

    private fun loadAnalytics() {
        contentFrame.removeAllViews()
        val progress = ProgressBar(this)
        contentFrame.addView(progress)
        val token = SessionManager.authHeader

        lifecycleScope.launch(exceptionHandler) {
            try {
                val workersResp = ApiClient.api.getWorkers(token)
                val managersResp = ApiClient.api.getManagers(token)
                val pendingResp = ApiClient.api.getPendingWorkers(token)
                progress.visibility = View.GONE

                val allWorkers = if (workersResp.isSuccessful) workersResp.body() ?: emptyList() else emptyList()
                val allManagers = if (managersResp.isSuccessful) managersResp.body() ?: emptyList() else emptyList()
                val pendingWorkers = if (pendingResp.isSuccessful) pendingResp.body() ?: emptyList() else emptyList()

                val title = TextView(this@AdminActivity).apply {
                    text = "System Overview"
                    setTextColor(getColor(R.color.white))
                    textSize = 18f
                    setPadding(16, 16, 16, 8)
                    paint.isFakeBoldText = true
                }
                contentFrame.addView(title)

                val summaryCard = LinearLayout(this@AdminActivity).apply {
                    orientation = LinearLayout.VERTICAL
                    setPadding(32, 20, 32, 20)
                    setBackgroundColor(getColor(R.color.surface))
                    val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                    p.bottomMargin = 12
                    layoutParams = p
                }

                val statsRow = LinearLayout(this@AdminActivity).apply {
                    orientation = LinearLayout.HORIZONTAL
                    layoutParams = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                }
                statsRow.addView(createStatBadge(LanguageManager.get("stat_workers"), allWorkers.size.toString(), R.color.primary))
                statsRow.addView(createStatBadge(LanguageManager.get("stat_managers"), allManagers.size.toString(), R.color.success))
                statsRow.addView(createStatBadge(LanguageManager.get("stat_pending"), pendingWorkers.size.toString(), R.color.warning))
                summaryCard.addView(statsRow)
                contentFrame.addView(summaryCard)

                val managerHeader = TextView(this@AdminActivity).apply {
                    text = "Manager Performance Overview"
                    setTextColor(getColor(R.color.text_secondary))
                    textSize = 15f
                    setPadding(16, 16, 16, 8)
                }
                contentFrame.addView(managerHeader)

                if (allManagers.isEmpty()) {
                    showEmpty("No managers registered yet")
                    return@launch
                }

                allManagers.forEach { manager ->
                    val managerWorkers = allWorkers.filter { it.enrolledByManagerId == manager.userId }
                    val activeCount = managerWorkers.count { it.isActive && !it.awaitingAdminConfirmation }
                    val card = LinearLayout(this@AdminActivity).apply {
                        orientation = LinearLayout.VERTICAL
                        setPadding(32, 16, 32, 16)
                        setBackgroundColor(getColor(R.color.surface))
                        val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                        p.bottomMargin = 8
                        layoutParams = p
                    }

                    card.addView(TextView(this@AdminActivity).apply {
                        text = manager.name
                        setTextColor(getColor(R.color.white))
                        textSize = 15f
                        paint.isFakeBoldText = true
                    })

                    val statsRow2 = LinearLayout(this@AdminActivity).apply {
                        orientation = LinearLayout.HORIZONTAL
                        val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                        p.topMargin = 8
                        layoutParams = p
                    }
                    statsRow2.addView(createStatBadge("Workers", "${managerWorkers.size}", R.color.primary))
                    statsRow2.addView(createStatBadge("Active", "$activeCount", R.color.success))
                    statsRow2.addView(createStatBadge("Pending", "${managerWorkers.count { it.awaitingAdminConfirmation }}", R.color.warning))
                    card.addView(statsRow2)

                    val perfText = TextView(this@AdminActivity).apply {
                        text = "Loading training performance..."
                        setTextColor(getColor(R.color.text_secondary))
                        textSize = 12f
                        setPadding(0, 8, 0, 0)
                    }
                    card.addView(perfText)
                    contentFrame.addView(card)

                    lifecycleScope.launch(exceptionHandler) {
                        try {
                            var totalAttempts = 0
                            var totalPassed = 0
                            var totalScore = 0
                            var scoreCount = 0
                            managerWorkers.forEach { worker ->
                                val hist = ApiClient.api.getTrainingHistory(token, worker.userId)
                                if (hist.isSuccessful) {
                                    val history = hist.body() ?: emptyList()
                                    totalAttempts += history.size
                                    totalPassed += history.count { it.passed }
                                    history.forEach { totalScore += it.totalScore; scoreCount++ }
                                }
                            }
                            val avgScore = if (scoreCount > 0) totalScore / scoreCount else 0
                            val passRate = if (totalAttempts > 0) (totalPassed * 100 / totalAttempts) else 0
                            perfText.text = "Training: $totalAttempts attempts | $totalPassed passed | Pass rate: $passRate% | Avg score: $avgScore%"
                        } catch (e: Exception) {
                            perfText.text = "Could not load performance data"
                        }
                    }
                }
            } catch (e: Exception) {
                progress.visibility = View.GONE
                showEmpty("Error: ${e.message}")
            }
        }
    }

    // ── SITES TAB ──

    private fun loadSites() {
        contentFrame.removeAllViews()

        val addBtn = MaterialButton(this).apply {
            text = "+ Record New Site"
            setTextColor(getColor(R.color.white))
            setBackgroundColor(getColor(R.color.success))
            val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
            p.bottomMargin = 16
            layoutParams = p
            setOnClickListener {
                try {
                    startActivity(android.content.Intent(this@AdminActivity, SiteMapActivity::class.java))
                } catch (e: Exception) {
                    Toast.makeText(this@AdminActivity, "Could not open site mapping", Toast.LENGTH_SHORT).show()
                }
            }
        }
        contentFrame.addView(addBtn)

        val sites = SiteStorage.getAllSites()
        if (sites.isEmpty()) {
            showEmpty("No sites recorded yet. Tap the button above to record a site.")
            return
        }

        val header = TextView(this).apply {
            text = "Recorded Sites (${sites.size})"
            setTextColor(getColor(R.color.white))
            textSize = 15f
            setPadding(16, 8, 16, 8)
            paint.isFakeBoldText = true
        }
        contentFrame.addView(header)

        sites.forEach { site ->
            val card = LinearLayout(this).apply {
                orientation = LinearLayout.VERTICAL
                setPadding(32, 20, 32, 20)
                setBackgroundColor(getColor(R.color.surface))
                val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                p.bottomMargin = 12
                layoutParams = p
            }

            card.addView(TextView(this).apply {
                text = site.siteName
                setTextColor(getColor(R.color.white))
                textSize = 16f
                paint.isFakeBoldText = true
            })

            card.addView(TextView(this).apply {
                text = "ID: ${site.siteId}"
                setTextColor(getColor(R.color.text_secondary))
                textSize = 12f
            })

            card.addView(TextView(this).apply {
                text = "Recorded: ${site.recordedAt}"
                setTextColor(getColor(R.color.text_secondary))
                textSize = 12f
            })

            val statsRow = LinearLayout(this).apply {
                orientation = LinearLayout.HORIZONTAL
                val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                p.topMargin = 10
                layoutParams = p
            }
            statsRow.addView(createStatBadge("Planes", site.planesDetected.toString(), R.color.primary))
            statsRow.addView(createStatBadge("Anchors", site.anchorsPlaced.toString(), R.color.success))
            statsRow.addView(createStatBadge("Floor", String.format("%.1fm", site.floorLevel), R.color.warning))
            card.addView(statsRow)

            val removeBtn = MaterialButton(this).apply {
                text = "Delete Site"
                setTextColor(getColor(R.color.white))
                setBackgroundColor(getColor(R.color.danger))
                val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                p.topMargin = 10
                layoutParams = p
                setOnClickListener {
                    AlertDialog.Builder(this@AdminActivity)
                        .setTitle("Delete Site")
                        .setMessage("Delete '${site.siteName}'? This cannot be undone.")
                        .setPositiveButton("Delete") { _, _ ->
                            SiteStorage.deleteSite(site.siteId)
                            loadSites()
                        }
                        .setNegativeButton("Cancel", null)
                        .show()
                }
            }
            card.addView(removeBtn)

            contentFrame.addView(card)
        }
    }

    // ── HELPERS ──

    private fun createStatBadge(label: String, value: String, colorRes: Int): View {
        val layout = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(16, 8, 16, 8)
            setBackgroundColor(getColor(colorRes))
            val p = LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WRAP_CONTENT, 1f)
            p.marginEnd = 8
            layoutParams = p
        }
        layout.addView(TextView(this).apply {
            text = value
            setTextColor(getColor(R.color.white))
            textSize = 18f
            paint.isFakeBoldText = true
            gravity = android.view.Gravity.CENTER
        })
        layout.addView(TextView(this).apply {
            text = label
            setTextColor(getColor(R.color.white))
            textSize = 11f
            gravity = android.view.Gravity.CENTER
        })
        return layout
    }

    private fun showEmpty(message: String) {
        contentFrame.addView(TextView(this).apply {
            text = message
            setTextColor(getColor(R.color.text_secondary))
            textSize = 16f
            gravity = android.view.Gravity.CENTER
            val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
            p.topMargin = 48
            layoutParams = p
        })
    }
}
