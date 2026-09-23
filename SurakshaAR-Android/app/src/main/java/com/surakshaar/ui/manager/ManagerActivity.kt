package com.surakshaar.ui.manager

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
import com.surakshaar.data.model.AddWorkerRequest
import com.surakshaar.data.model.RemoveUserRequest
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.language.LanguageManager
import com.surakshaar.util.ThemeManager
import com.surakshaar.util.VoiceManager
import kotlinx.coroutines.CoroutineExceptionHandler
import kotlinx.coroutines.launch

class ManagerActivity : AppCompatActivity() {

    companion object {
        private const val TAG = "ManagerActivity"
    }

    private val exceptionHandler = CoroutineExceptionHandler { _, throwable ->
        Log.e(TAG, "Coroutine error", throwable)
    }

    private lateinit var tabMyWorkers: LinearLayout
    private lateinit var tabAddWorker: LinearLayout
    private lateinit var tabAssignTraining: LinearLayout
    private lateinit var tabProgress: LinearLayout
    private lateinit var contentFrame: LinearLayout
    private lateinit var backButton: Button
    private lateinit var titleText: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        ThemeManager.applyTheme(this, SessionManager.theme)
        setContentView(R.layout.activity_manager)
        VoiceManager.attachToActivity(this)

        backButton = findViewById(R.id.backButton)
        titleText = findViewById(R.id.titleText)
        tabMyWorkers = findViewById(R.id.tabMyWorkers)
        tabAddWorker = findViewById(R.id.tabAddWorker)
        tabAssignTraining = findViewById(R.id.tabAssignTraining)
        tabProgress = findViewById(R.id.tabProgress)
        contentFrame = findViewById(R.id.contentFrame)

        backButton.setOnClickListener { finish() }
        titleText.text = LanguageManager.get("manager_panel")
        applyTabTranslations()

        val initialTab = when (intent.getStringExtra("tab")) {
            "add_worker", "addWorker" -> "addWorker"
            "assign", "assignTraining" -> "assignTraining"
            "progress", "scenarios" -> "progress"
            else -> "myWorkers"
        }
        selectTab(initialTab)
        tabMyWorkers.setOnClickListener { selectTab("myWorkers") }
        tabAddWorker.setOnClickListener { selectTab("addWorker") }
        tabAssignTraining.setOnClickListener { selectTab("assignTraining") }
        tabProgress.setOnClickListener { selectTab("progress") }
    }

    private fun applyTabTranslations() {
        try {
            val labels = listOf(
                R.id.tabMyWorkersLabel to "tab_my_workers",
                R.id.tabAddWorkerLabel to "tab_add_worker",
                R.id.tabAssignTrainingLabel to "tab_assign_training",
                R.id.tabProgressLabel to "tab_progress"
            )
            labels.forEach { (id, key) ->
                findViewById<TextView>(id)?.text = LanguageManager.get(key)
            }
        } catch (_: Exception) {}
    }

    private fun selectTab(tab: String) {
        val tabs = listOf(tabMyWorkers, tabAddWorker, tabAssignTraining, tabProgress)
        tabs.forEach { it.setBackgroundColor(getColor(R.color.surface)) }
        when (tab) {
            "myWorkers" -> { tabMyWorkers.setBackgroundColor(getColor(R.color.primary)); loadMyWorkers() }
            "addWorker" -> { tabAddWorker.setBackgroundColor(getColor(R.color.primary)); loadAddWorkerForm() }
            "assignTraining" -> { tabAssignTraining.setBackgroundColor(getColor(R.color.primary)); loadAssignTraining() }
            "progress" -> { tabProgress.setBackgroundColor(getColor(R.color.primary)); loadProgress() }
        }
    }

    // ── MY WORKERS TAB ──

    private fun loadMyWorkers() {
        contentFrame.removeAllViews()
        val progress = ProgressBar(this)
        contentFrame.addView(progress)
        val token = SessionManager.authHeader

        lifecycleScope.launch(exceptionHandler) {
            try {
                val workersResponse = ApiClient.api.getMyWorkers(token)
                progress.visibility = View.GONE

                if (workersResponse.isSuccessful) {
                    val workers = workersResponse.body() ?: emptyList()
                    if (workers.isEmpty()) {
                        showEmpty(LanguageManager.get("no_workers_yet"))
                        return@launch
                    }

                    val pending = workers.filter { it.awaitingAdminConfirmation }
                    val active = workers.filter { !it.awaitingAdminConfirmation }

                    if (pending.isNotEmpty()) {
                        val header = TextView(this@ManagerActivity).apply {
                            text = "${LanguageManager.get("pending_approval")} (${pending.size})"
                            setTextColor(getColor(R.color.warning))
                            textSize = 15f
                            setPadding(16, 16, 16, 8)
                        }
                        contentFrame.addView(header)
                        pending.forEach { w ->
                            contentFrame.addView(createWorkerCard(w.userId, w.name, w.phoneNumber, true))
                        }
                    }

                    if (active.isNotEmpty()) {
                        val header = TextView(this@ManagerActivity).apply {
                            text = "${LanguageManager.get("active_status")} (${active.size})"
                            setTextColor(getColor(R.color.success))
                            textSize = 15f
                            setPadding(16, if (pending.isNotEmpty()) 24 else 16, 16, 8)
                        }
                        contentFrame.addView(header)
                        active.forEach { w ->
                            contentFrame.addView(createWorkerCard(w.userId, w.name, w.phoneNumber, false))
                        }
                    }
                } else {
                    showEmpty(LanguageManager.get("failed_load_workers"))
                }
            } catch (e: Exception) {
                progress.visibility = View.GONE
                showEmpty(LanguageManager.get("error_try_again"))
            }
        }
    }

    private fun createWorkerCard(userId: String, name: String, phone: String, pending: Boolean): View {
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
            text = "${LanguageManager.get("id_label")}: $userId | ${LanguageManager.get("phone_label")}: $phone"
            setTextColor(getColor(R.color.text_secondary))
            textSize = 12f
        })

        val statusText = TextView(this).apply {
            text = if (pending) "⏳ ${LanguageManager.get("awaiting_approval")}" else "✓ ${LanguageManager.get("active_status")}"
            setTextColor(if (pending) getColor(R.color.warning) else getColor(R.color.success))
            textSize = 13f
            setPadding(0, 4, 0, 0)
        }
        card.addView(statusText)

        if (!pending) {
            val btnRow = LinearLayout(this).apply {
                orientation = LinearLayout.HORIZONTAL
                val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                p.topMargin = 10
                layoutParams = p
            }

            btnRow.addView(MaterialButton(this).apply {
                text = LanguageManager.get("remove")
                setTextColor(getColor(R.color.white))
                setBackgroundColor(getColor(R.color.danger))
                val p = LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WRAP_CONTENT, 1f)
                layoutParams = p
                setOnClickListener { removeWorker(userId, name) }
            })

            card.addView(btnRow)
        }

        return card
    }

    private fun removeWorker(userId: String, name: String) {
        AlertDialog.Builder(this)
            .setTitle(LanguageManager.get("remove_worker"))
            .setMessage("${LanguageManager.get("remove")} $name ($userId)?")
            .setPositiveButton(LanguageManager.get("remove")) { _, _ ->
                val token = SessionManager.authHeader
                lifecycleScope.launch(exceptionHandler) {
                    try {
                        val response = ApiClient.api.removeManagerWorker(token, RemoveUserRequest(userId))
                        if (response.isSuccessful) {
                            Toast.makeText(this@ManagerActivity, LanguageManager.get("worker_removed"), Toast.LENGTH_SHORT).show()
                            loadMyWorkers()
                        } else {
                            val errMsg = response.errorBody()?.string() ?: LanguageManager.get("failed_to_load")
                            Toast.makeText(this@ManagerActivity, errMsg, Toast.LENGTH_SHORT).show()
                        }
                    } catch (e: Exception) {
                        Toast.makeText(this@ManagerActivity, LanguageManager.get("error_try_again"), Toast.LENGTH_SHORT).show()
                    }
                }
            }
            .setNegativeButton(LanguageManager.get("cancel"), null)
            .show()
    }

    // ── ADD WORKER TAB ──

    private fun loadAddWorkerForm() {
        contentFrame.removeAllViews()

        val title = TextView(this).apply {
            text = LanguageManager.get("manager_add_worker")
            setTextColor(getColor(R.color.white))
            textSize = 16f
            setPadding(16, 16, 16, 8)
        }
        contentFrame.addView(title)

        val card = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(32, 24, 32, 24)
            setBackgroundColor(getColor(R.color.surface))
            val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
            p.marginStart = 16; p.marginEnd = 16
            layoutParams = p
        }

        val idInput = TextInputEditText(this).apply { hint = LanguageManager.get("manager_worker_id"); id = View.generateViewId() }
        val nameInput = TextInputEditText(this).apply { hint = LanguageManager.get("manager_worker_name"); id = View.generateViewId() }
        val phoneInput = TextInputEditText(this).apply { hint = LanguageManager.get("manager_worker_phone"); id = View.generateViewId() }
        val passInput = TextInputEditText(this).apply { hint = LanguageManager.get("login_password_hint"); id = View.generateViewId() }

        val fields = listOf(idInput, nameInput, phoneInput, passInput)
        fields.forEach { card.addView(it) }

        val infoText = TextView(this).apply {
            text = LanguageManager.get("manager_worker_info")
            setTextColor(getColor(R.color.text_secondary))
            textSize = 12f
            setPadding(0, 12, 0, 0)
        }
        card.addView(infoText)

        val submitBtn = MaterialButton(this).apply {
            text = LanguageManager.get("manager_add_worker")
            setTextColor(getColor(R.color.white))
            setBackgroundColor(getColor(R.color.success))
            val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
            p.topMargin = 20
            layoutParams = p
        }

        submitBtn.setOnClickListener {
            val userId = idInput.text.toString().trim()
            val name = nameInput.text.toString().trim()
            val phone = phoneInput.text.toString().trim()
            val pass = passInput.text.toString().trim()

            if (userId.isEmpty() || name.isEmpty() || pass.isEmpty()) {
                Toast.makeText(this, LanguageManager.get("manager_worker_validation"), Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            addWorker(userId, name, phone, pass) {
                fields.forEach { it.setText("") }
            }
        }
        card.addView(submitBtn)
        contentFrame.addView(card)
    }

    private fun addWorker(userId: String, name: String, phone: String, password: String, onDone: () -> Unit) {
        val token = SessionManager.authHeader
        lifecycleScope.launch(exceptionHandler) {
            try {
                val response = ApiClient.api.addWorker(token, AddWorkerRequest(userId, name, phone, password))
                if (response.isSuccessful) {
                    Toast.makeText(this@ManagerActivity, LanguageManager.get("manager_worker_added"), Toast.LENGTH_SHORT).show()
                    onDone()
                } else {
                    val errMsg = response.errorBody()?.string() ?: LanguageManager.get("failed_code")
                    Toast.makeText(this@ManagerActivity, LanguageManager.get("request_failed_short"), Toast.LENGTH_SHORT).show()
                }
            } catch (e: Exception) {
                Toast.makeText(this@ManagerActivity, LanguageManager.get("error_try_again"), Toast.LENGTH_SHORT).show()
            }
        }
    }

    // ── ASSIGN TRAINING TAB ──

    private fun loadAssignTraining() {
        contentFrame.removeAllViews()
        val progress = ProgressBar(this)
        contentFrame.addView(progress)
        val token = SessionManager.authHeader

        lifecycleScope.launch(exceptionHandler) {
            try {
                val workersResponse = ApiClient.api.getMyWorkers(token)
                progress.visibility = View.GONE

                if (!workersResponse.isSuccessful) {
                    showEmpty(LanguageManager.get("failed_load_workers"))
                    return@launch
                }

                val activeWorkers = (workersResponse.body() ?: emptyList()).filter { !it.awaitingAdminConfirmation && it.isActive }

                if (activeWorkers.isEmpty()) {
                    showEmpty(LanguageManager.get("no_active_workers_assign"))
                    return@launch
                }

                val title = TextView(this@ManagerActivity).apply {
                    text = LanguageManager.get("manager_assign_training")
                    setTextColor(getColor(R.color.white))
                    textSize = 16f
                    setPadding(16, 16, 16, 8)
                }
                contentFrame.addView(title)

                val scenarios = listOf(
                    "fire_evacuation" to LanguageManager.get("scenario_fire"),
                    "chemical_spill" to LanguageManager.get("scenario_chemical"),
                    "first_aid" to LanguageManager.get("scenario_first_aid"),
                    "equipment_safety" to LanguageManager.get("scenario_equipment"),
                    "electrical_safety" to LanguageManager.get("scenario_electrical")
                )

                activeWorkers.forEach { worker ->
                    val card = LinearLayout(this@ManagerActivity).apply {
                        orientation = LinearLayout.VERTICAL
                        setPadding(32, 16, 32, 16)
                        setBackgroundColor(getColor(R.color.surface))
                        val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                        p.bottomMargin = 8
                        layoutParams = p
                    }

                    card.addView(TextView(this@ManagerActivity).apply {
                        text = "${worker.name} (${worker.userId})"
                        setTextColor(getColor(R.color.white))
                        textSize = 15f
                        paint.isFakeBoldText = true
                    })

                    val btnRow = LinearLayout(this@ManagerActivity).apply {
                        orientation = LinearLayout.HORIZONTAL
                        val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                        p.topMargin = 10
                        layoutParams = p
                    }

                    scenarios.forEach { (scenarioId, label) ->
                        btnRow.addView(MaterialButton(this@ManagerActivity).apply {
                            text = label
                            textSize = 11f
                            setTextColor(getColor(R.color.white))
                            setBackgroundColor(getColor(R.color.primary))
                            val p = LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WRAP_CONTENT, 1f)
                            p.marginEnd = 4
                            layoutParams = p
                            setOnClickListener { assignScenario(worker.userId, scenarioId, label) }
                        })
                    }

                    card.addView(btnRow)
                    contentFrame.addView(card)
                }
            } catch (e: Exception) {
                progress.visibility = View.GONE
                showEmpty(LanguageManager.get("error_try_again"))
            }
        }
    }

    private fun assignScenario(workerId: String, scenarioId: String, scenarioName: String) {
        val token = SessionManager.authHeader
        val request = mapOf(
            "WorkerId" to workerId,
            "ScenarioType" to scenarioId,
            "Title" to scenarioName,
            "RequiredScore" to 80
        )
        lifecycleScope.launch(exceptionHandler) {
            try {
                val response = ApiClient.api.assignScenario(token, request)
                if (response.isSuccessful) {
                    val msg = LanguageManager.get("assigned_to")
                        .replace("%1\$s", scenarioName)
                        .replace("%2\$s", workerId)
                    Toast.makeText(this@ManagerActivity, msg, Toast.LENGTH_SHORT).show()
                } else {
                    Toast.makeText(this@ManagerActivity, LanguageManager.get("request_failed_short"), Toast.LENGTH_SHORT).show()
                }
            } catch (e: Exception) {
                Toast.makeText(this@ManagerActivity, LanguageManager.get("error_try_again"), Toast.LENGTH_SHORT).show()
            }
        }
    }

    // ── PROGRESS TAB ──

    private fun loadProgress() {
        contentFrame.removeAllViews()
        val progress = ProgressBar(this)
        contentFrame.addView(progress)
        val token = SessionManager.authHeader

        lifecycleScope.launch(exceptionHandler) {
            try {
                val workersResponse = ApiClient.api.getMyWorkers(token)
                progress.visibility = View.GONE

                if (!workersResponse.isSuccessful) {
                    showEmpty(LanguageManager.get("failed_load_workers"))
                    return@launch
                }

                val activeWorkers = (workersResponse.body() ?: emptyList()).filter { !it.awaitingAdminConfirmation && it.isActive }

                if (activeWorkers.isEmpty()) {
                    showEmpty(LanguageManager.get("no_active_workers"))
                    return@launch
                }

                val title = TextView(this@ManagerActivity).apply {
                    text = LanguageManager.get("manager_progress")
                    setTextColor(getColor(R.color.white))
                    textSize = 16f
                    setPadding(16, 16, 16, 8)
                }
                contentFrame.addView(title)

                activeWorkers.forEach { worker ->
                    val card = LinearLayout(this@ManagerActivity).apply {
                        orientation = LinearLayout.VERTICAL
                        setPadding(32, 16, 32, 16)
                        setBackgroundColor(getColor(R.color.surface))
                        val p = LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT)
                        p.bottomMargin = 8
                        layoutParams = p
                    }

                    card.addView(TextView(this@ManagerActivity).apply {
                        text = "${worker.name} (${worker.userId})"
                        setTextColor(getColor(R.color.white))
                        textSize = 15f
                        paint.isFakeBoldText = true
                    })

                    val statusText = TextView(this@ManagerActivity).apply {
                        text = LanguageManager.get("load_history")
                        setTextColor(getColor(R.color.text_secondary))
                        textSize = 12f
                        setPadding(0, 8, 0, 0)
                    }
                    card.addView(statusText)
                    contentFrame.addView(card)

                    lifecycleScope.launch(exceptionHandler) {
                        try {
                            val histResponse = ApiClient.api.getTrainingHistory(token, worker.userId)
                            if (histResponse.isSuccessful) {
                                val history = histResponse.body() ?: emptyList()
                                val completed = history.count { it.passed }
                                val total = history.size
                                val avgScore = if (total > 0) history.map { it.totalScore }.average().toInt() else 0
                                val lastAttempt = history.maxByOrNull { it.startTime }
                                val lastText = if (lastAttempt != null) "${LanguageManager.get("last_attempt")}: ${lastAttempt.scenarioId}" else LanguageManager.get("no_attempts_yet")
                                statusText.text = LanguageManager.get("scenario_stats")
                                    .replace("%1\$d", total.toString())
                                    .replace("%2\$d", completed.toString())
                                    .replace("%3\$d", avgScore.toString())
                                    .replace("%4\$s", lastText)
                            } else {
                                statusText.text = LanguageManager.get("no_training_data")
                            }
                        } catch (e: Exception) {
                            statusText.text = LanguageManager.get("failed_to_load")
                        }
                    }
                }
            } catch (e: Exception) {
                progress.visibility = View.GONE
                showEmpty(LanguageManager.get("error_try_again"))
            }
        }
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
