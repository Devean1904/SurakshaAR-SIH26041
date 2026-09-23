package com.surakshaar.ui.training

import android.Manifest
import android.content.Intent
import android.content.pm.PackageManager
import android.graphics.Color
import android.os.Bundle
import android.os.CountDownTimer
import android.util.Log
import android.view.View
import android.view.WindowManager
import android.widget.Button
import android.widget.LinearLayout
import android.widget.ProgressBar
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import androidx.lifecycle.lifecycleScope
import com.google.ar.core.Frame
import com.google.ar.core.Plane
import com.google.ar.core.TrackingState
import com.surakshaar.R
import com.surakshaar.data.api.ApiClient
import com.surakshaar.data.model.*
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.data.repository.TrainingDataStore
import com.surakshaar.language.LanguageManager
import com.surakshaar.ui.assessment.AssessmentActivity
import com.surakshaar.util.ThemeManager
import com.surakshaar.util.VoiceManager
import io.github.sceneview.ar.ARSceneView
import io.github.sceneview.node.CubeNode
import io.github.sceneview.node.Node
import io.github.sceneview.node.SphereNode
import dev.romainguy.kotlin.math.Float3
import kotlinx.coroutines.launch

class ARTrainingActivity : AppCompatActivity() {

    companion object {
        private const val TAG = "ARTrainingActivity"
        private const val CAMERA_PERMISSION_CODE = 1001
        private const val MARKER_ACTION_PREFIX = "action:"
        private const val MARKER_HAZARD_PREFIX = "hazard:"
    }

    private lateinit var rootFrame: View
    private lateinit var hudOverlay: LinearLayout
    private lateinit var arSceneView: ARSceneView
    private lateinit var arStatusText: TextView
    private lateinit var hazardTypeText: TextView
    private lateinit var hazardLevelText: TextView
    private lateinit var timerText: TextView
    private lateinit var timerProgress: ProgressBar
    private lateinit var scoreText: TextView
    private lateinit var penaltyText: TextView
    private lateinit var instructionText: TextView
    private lateinit var actionButtonContainer: LinearLayout

    private lateinit var alertPanel: LinearLayout
    private lateinit var alertText: TextView

    private lateinit var criticalFailPanel: LinearLayout
    private lateinit var criticalFailTitle: TextView
    private lateinit var criticalFailMessage: TextView
    private lateinit var criticalFailScore: TextView
    private lateinit var retryButton: Button
    private lateinit var viewResultsButton: Button

    private lateinit var evacuationPanel: LinearLayout
    private lateinit var evacuationText: TextView

    private lateinit var resultsPanel: LinearLayout
    private lateinit var resultsTitle: TextView
    private lateinit var resultsActions: TextView
    private lateinit var resultsQuestions: TextView
    private lateinit var resultsTotal: TextView
    private lateinit var resultsMessage: TextView
    private lateinit var viewCertificateButton: Button
    private lateinit var retryResultsButton: Button
    private lateinit var homeButton: Button

    private lateinit var extinguishButton: Button
    private lateinit var evacuateButton: Button
    private lateinit var reportButton: Button

    private var gameState = GameState.PLAYING
    private var score = 0
    private var penalty = 0
    private var currentDomain = ""
    private var currentModuleId = ""
    private var currentScenarioId = ""
    private var completedActions = mutableListOf<String>()
    private var gameTimer: CountDownTimer? = null
    private var escalationTimers = mutableListOf<CountDownTimer>()
    private var totalDurationMs = 180000L
    private var timeRemainingMs = totalDurationMs
    private var actionScore = 0
    private var questionScore = 0
    private var attemptId: String? = null
    private var currentScenario: ScenarioConfig? = null
    private val alertHandler = android.os.Handler(mainLooper)
    private val evacuationHandler = android.os.Handler(mainLooper)

    private val hazardMarkers = mutableMapOf<String, Node>()
    private val actionMarkers = mutableMapOf<String, Node>()
    private val placedMarkerIds = mutableSetOf<String>()
    private var planeCount = 0
    private var markersPlaced = false
    private var arReady = false

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        try {
            ThemeManager.applyTheme(this, SessionManager.theme)
            setContentView(R.layout.activity_ar_training)
            VoiceManager.attachToActivity(this)
            window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)

            requestCameraPermission()
            initViews()
            setupArScene()
            applyTranslations()
            loadDomain()
            loadScenario()
            setupListeners()
            startGame()
        } catch (e: Exception) {
            Log.e(TAG, "Failed to start training", e)
            Toast.makeText(
                this,
                LanguageManager.get("training_error") + ": ${e.message}",
                Toast.LENGTH_LONG
            ).show()
            finish()
        }
    }

    private fun requestCameraPermission() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.CAMERA)
            != PackageManager.PERMISSION_GRANTED
        ) {
            ActivityCompat.requestPermissions(
                this,
                arrayOf(Manifest.permission.CAMERA),
                CAMERA_PERMISSION_CODE
            )
        }
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        if (requestCode == CAMERA_PERMISSION_CODE) {
            if (grantResults.isEmpty() || grantResults[0] != PackageManager.PERMISSION_GRANTED) {
                val permMsg = LanguageManager.get("camera_permission_required")
                arStatusText.text = permMsg
                Toast.makeText(
                    this,
                    permMsg,
                    Toast.LENGTH_LONG
                ).show()
            }
        }
    }

    private fun initViews() {
        rootFrame = findViewById(R.id.rootFrame)
        hudOverlay = findViewById(R.id.hudOverlay)
        arSceneView = findViewById(R.id.arSceneView)
        arStatusText = findViewById(R.id.arStatusText)
        hazardTypeText = findViewById(R.id.hazardTypeText)
        hazardLevelText = findViewById(R.id.hazardLevelText)
        timerText = findViewById(R.id.timerText)
        timerProgress = findViewById(R.id.timerProgress)
        scoreText = findViewById(R.id.scoreText)
        penaltyText = findViewById(R.id.penaltyText)
        instructionText = findViewById(R.id.instructionText)
        actionButtonContainer = findViewById(R.id.actionButtonContainer)

        alertPanel = findViewById(R.id.alertPanel)
        alertText = findViewById(R.id.alertText)

        criticalFailPanel = findViewById(R.id.criticalFailPanel)
        criticalFailTitle = findViewById(R.id.criticalFailTitle)
        criticalFailMessage = findViewById(R.id.criticalFailMessage)
        criticalFailScore = findViewById(R.id.criticalFailScore)
        retryButton = findViewById(R.id.retryButton)
        viewResultsButton = findViewById(R.id.viewResultsButton)

        evacuationPanel = findViewById(R.id.evacuationPanel)
        evacuationText = findViewById(R.id.evacuationText)

        resultsPanel = findViewById(R.id.resultsPanel)
        resultsTitle = findViewById(R.id.resultsTitle)
        resultsActions = findViewById(R.id.resultsActions)
        resultsQuestions = findViewById(R.id.resultsQuestions)
        resultsTotal = findViewById(R.id.resultsTotal)
        resultsMessage = findViewById(R.id.resultsMessage)
        viewCertificateButton = findViewById(R.id.viewCertificateButton)
        retryResultsButton = findViewById(R.id.retryResultsButton)
        homeButton = findViewById(R.id.homeButton)

        extinguishButton = findViewById(R.id.extinguishButton)
        evacuateButton = findViewById(R.id.evacuateButton)
        reportButton = findViewById(R.id.reportButton)
    }

    private fun setupArScene() {
        try {
            arSceneView.planeRenderer.isVisible = true
            arSceneView.planeRenderer.isEnabled = true

            arSceneView.onSessionCreated = {
                runOnUiThread {
                    arReady = true
                    arStatusText.text = LanguageManager.get("ar_ready")
                }
            }

            arSceneView.onSessionFailed = { e ->
                Log.e(TAG, "AR session failed", e)
                runOnUiThread {
                    arReady = false
                    arStatusText.text = LanguageManager.get("ar_unavailable")
                }
            }

            arSceneView.onSessionUpdated = { _, frame ->
                runOnUiThread { processArFrame(frame) }
            }

            arSceneView.setOnGestureListener(
                onSingleTapConfirmed = { _, node ->
                    handleMarkerTap(node)
                }
            )
        } catch (e: Exception) {
            Log.e(TAG, "AR setup failed", e)
            arStatusText.text = LanguageManager.get("ar_setup_failed")
        }
    }

    private fun processArFrame(frame: Frame) {
        try {
            val camera = frame.camera
            if (camera.trackingState != TrackingState.TRACKING) {
                arStatusText.text = "Looking for surfaces…"
                return
            }

            val planes = frame.getUpdatedTrackables(Plane::class.java)
            var newPlanes = 0
            val seen = mutableSetOf<Long>()
            for (plane in planes) {
                val id = plane.hashCode().toLong()
                if (seen.add(id) && plane.trackingState == TrackingState.TRACKING) {
                    newPlanes++
                }
            }
            if (newPlanes > 0) {
                planeCount += newPlanes
            }

            if (!markersPlaced && (planeCount > 0 || frame.hasHit)) {
                placeScenarioMarkers(frame)
            }

            updateArStatus()
        } catch (e: Exception) {
            Log.w(TAG, "AR frame error", e)
        }
    }

    private val Frame.hasHit: Boolean
        get() = try {
            val w = arSceneView.width.toFloat()
            val h = arSceneView.height.toFloat()
            if (w <= 0f || h <= 0f) false
            else hitTest(w / 2f, h / 2f).isNotEmpty()
        } catch (e: Exception) {
            false
        }

    private fun updateArStatus() {
        val remaining = actionMarkers.size
        arStatusText.text = buildString {
            append("Surfaces: $planeCount")
            append(" · Hazards: ${hazardMarkers.size}")
            append(" · Actions left: $remaining")
        }
    }

    private fun placeScenarioMarkers(frame: Frame) {
        val scenario = currentScenario ?: return
        if (markersPlaced) return

        val hitPose = resolveHitPose(frame) ?: return
        val origin = floatArrayOf(hitPose.tx(), hitPose.ty(), hitPose.tz())

        hazardMarkers.clear()
        actionMarkers.clear()
        placedMarkerIds.clear()
        arSceneView.clearChildNodes()

        scenario.hazardZones.forEachIndexed { index, zone ->
            val id = MARKER_HAZARD_PREFIX + zone.zoneId
            if (!placedMarkerIds.add(id)) return@forEachIndexed
            val local = placeOffset(zone.position, index, count = scenario.hazardZones.size)
            val pos = floatArrayOf(
                origin[0] + local[0],
                origin[1] + local[1].coerceAtLeast(0.05f),
                origin[2] + local[2]
            )
            val color = parseColor(zone.riskColor, Color.RED)
            val radius = (zone.radius * 0.15f).coerceIn(0.15f, 0.6f)
            val node = createHazardNode(radius, color, id)
            node.position = Float3(pos[0], pos[1], pos[2])
            arSceneView.addChildNode(node)
            hazardMarkers[id] = node
        }

        scenario.requiredActions.forEachIndexed { index, action ->
            val id = MARKER_ACTION_PREFIX + action.actionId
            if (!placedMarkerIds.add(id)) return@forEachIndexed
            val local = placeOffset(
                floatArrayOf(
                    ((index % 3) - 1) * 0.7f,
                    0.15f,
                    -0.9f - (index / 3) * 0.7f
                ),
                index,
                count = scenario.requiredActions.size
            )
            val pos = floatArrayOf(
                origin[0] + local[0],
                origin[1] + local[1].coerceAtLeast(0.3f),
                origin[2] + local[2]
            )
            val color = if (action.isCritical) Color.parseColor("#FFD600") else Color.parseColor("#00E676")
            val size = (action.interactionRadius * 0.12f).coerceIn(0.12f, 0.35f)
            val node = createActionNode(size, color, id)
            node.position = Float3(pos[0], pos[1], pos[2])
            arSceneView.addChildNode(node)
            actionMarkers[id] = node
        }

        markersPlaced = hazardMarkers.isNotEmpty() || actionMarkers.isNotEmpty()
        updateArStatus()
    }

    private fun resolveHitPose(frame: Frame): com.google.ar.core.Pose? {
        return try {
            val w = arSceneView.width.toFloat()
            val h = arSceneView.height.toFloat()
            if (w <= 0f || h <= 0f) return null
            val hits = frame.hitTest(w / 2f, h / 2f)
            hits.firstOrNull { result ->
                val trackable = result.trackable
                trackable is Plane && trackable.isPoseInPolygon(result.hitPose)
            }?.hitPose ?: hits.firstOrNull()?.hitPose
        } catch (e: Exception) {
            null
        }
    }

    private fun placeOffset(base: FloatArray, index: Int, count: Int): FloatArray {
        val bx = if (base.size > 0) base[0] else 0f
        val by = if (base.size > 1) base[1] else 0f
        val bz = if (base.size > 2) base[2] else 0f
        val spread = if (count > 1) index * 0.35f else 0f
        return floatArrayOf(
            (bx * 0.4f) + spread,
            (by * 0.5f),
            (bz * 0.4f) - 1.2f
        )
    }

    private fun createHazardNode(radius: Float, color: Int, name: String): Node {
        val node = SphereNode(
            engine = arSceneView.engine,
            radius = radius,
            materialInstance = arSceneView.materialLoader.createColorInstance(color)
        )
        node.name = name
        node.isTouchable = true
        return node
    }

    private fun createActionNode(size: Float, color: Int, name: String): Node {
        val node = CubeNode(
            engine = arSceneView.engine,
            size = Float3(size, size, size),
            materialInstance = arSceneView.materialLoader.createColorInstance(color)
        )
        node.name = name
        node.isTouchable = true
        node.onSingleTapConfirmed = {
            val actionId = name.removePrefix(MARKER_ACTION_PREFIX)
            performAction(actionId)
            true
        }
        return node
    }

    private fun parseColor(hex: String?, fallback: Int): Int {
        return try {
            if (hex.isNullOrBlank()) fallback else Color.parseColor(hex)
        } catch (e: Exception) {
            fallback
        }
    }

    private fun handleMarkerTap(node: Node?) {
        val name = node?.name ?: return
        if (name.startsWith(MARKER_ACTION_PREFIX)) {
            val actionId = name.removePrefix(MARKER_ACTION_PREFIX)
            performAction(actionId)
            removeActionMarker(actionId)
        } else if (name.startsWith(MARKER_HAZARD_PREFIX)) {
            instructionText.text =
                LanguageManager.get("hazard_zone_hint")
        }
    }

    private fun removeActionMarker(actionId: String) {
        val id = MARKER_ACTION_PREFIX + actionId
        actionMarkers.remove(id)?.let { arSceneView.removeChildNode(it) }
        updateArStatus()
    }

    private fun applyTranslations() {
        try {
            val t = LanguageManager
            extinguishButton.text = t.get("extinguish")
            evacuateButton.text = t.get("evacuate")
            reportButton.text = t.get("report")
            scoreText.text = t.get("score_label") + " $score"
            instructionText.text = t.get("use_action_buttons")
            retryButton.text = t.get("retry")
            retryResultsButton.text = t.get("retry")
            homeButton.text = t.get("home_title")
            viewCertificateButton.text = t.get("view_certificate")
            viewResultsButton.text = t.get("view_results")
            criticalFailTitle.text = t.get("scenario_failed")
            evacuationText.text = t.get("escalation_evacuate")
            alertText.text = t.get("escalation_warning_short")
        } catch (e: Exception) {
            Log.e(TAG, "Translation failed", e)
        }
    }

    private fun loadDomain() {
        currentDomain = intent.getStringExtra("domain") ?: "fire_safety"
        mapDomainToIds()
    }

    private fun mapDomainToIds() {
        when (currentDomain) {
            "fire_safety", "fire", "fire_explosion" -> {
                currentModuleId = "fire-safety-101"
                currentScenarioId = "fire-1"
            }
            "gas_leak", "gas", "chemical" -> {
                currentModuleId = "gas-leak-101"
                currentScenarioId = "gas-1"
            }
            "machinery_loto", "machinery" -> {
                currentModuleId = "machinery-101"
                currentScenarioId = "machinery-1"
            }
            "electrical_hazards", "electrical" -> {
                currentModuleId = "electrical-101"
                currentScenarioId = "electrical-1"
            }
            "heights_fall", "heights" -> {
                currentModuleId = "heights-101"
                currentScenarioId = "heights-1"
            }
            else -> {
                currentModuleId = "fire-safety-101"
                currentScenarioId = "fire-1"
            }
        }
    }

    private fun loadScenario() {
        applyScenario(TrainingDataStore.getScenario(currentModuleId, currentScenarioId))
        loadRemoteScenarioIfNeeded()
        recordTrainingStart()
    }

    private fun loadRemoteScenarioIfNeeded() {
        if (!SessionManager.isAuthenticated) return

        lifecycleScope.launch {
            try {
                val response = ApiClient.api.getTrainingModule(
                    SessionManager.authHeader,
                    currentModuleId
                )
                if (!response.isSuccessful) return@launch
                val module = response.body() ?: return@launch
                val remote = module.scenarios.firstOrNull { it.scenarioId == currentScenarioId }
                    ?: module.scenarios.firstOrNull()
                    ?: return@launch

                escalationTimers.forEach { it.cancel() }
                escalationTimers.clear()
                applyScenario(remote)
                if (gameState == GameState.PLAYING) {
                    startEscalationTimers()
                }
            } catch (e: Exception) {
                Log.w(TAG, "Remote scenario unavailable; using local data", e)
            }
        }
    }

    private fun applyScenario(scenario: ScenarioConfig?) {
        currentScenario = scenario
        if (scenario != null) {
            hazardTypeText.text = scenario.hazardType
            hazardLevelText.text = String.format(
                LanguageManager.get("level_n"),
                scenario.maxEscalationLevel
            )
            instructionText.text = scenario.scenarioName
            totalDurationMs = 180000L
            setupEscalationTimers(scenario.escalationEvents)
            markersPlaced = false
            hazardMarkers.clear()
            actionMarkers.clear()
            placedMarkerIds.clear()
            try {
                arSceneView.clearChildNodes()
            } catch (e: Exception) {
                Log.w(TAG, "Clear markers failed", e)
            }
        }
    }

    private fun recordTrainingStart() {
        if (!SessionManager.isAuthenticated) return

        lifecycleScope.launch {
            try {
                val response = ApiClient.api.startTraining(
                    SessionManager.authHeader,
                    TrainingStartRequest(
                        employeeId = SessionManager.userId,
                        moduleId = currentModuleId,
                        scenarioId = currentScenarioId
                    )
                )
                if (response.isSuccessful) {
                    attemptId = response.body()?.attemptId
                } else {
                    attemptId = queueOfflineAttemptStart()
                }
            } catch (e: Exception) {
                attemptId = queueOfflineAttemptStart()
            }
        }
    }

    private fun passThreshold(): Float {
        val t = TrainingDataStore.getModule(currentModuleId)?.passThreshold
        return if (t != null && t > 0f) t else 70f
    }

    private fun queueOfflineAttemptStart(): String {
        val localId = java.util.UUID.randomUUID().toString().replace("-", "").take(12).uppercase()
        val now = java.time.Instant.now().toString()
        val payload = org.json.JSONObject().apply {
            put("AttemptId", localId)
            put("EmployeeId", SessionManager.userId)
            put("ModuleId", currentModuleId)
            put("ScenarioId", currentScenarioId)
            put("StartTime", now)
            put("EndTime", "")
            put("ActionScore", 0)
            put("QuestionScore", 0)
            put("TotalScore", 0)
            put("Passed", false)
            put("EscalationLevel", 0)
            put("Status", "in_progress")
        }
        com.surakshaar.data.offline.OfflineQueue.upsertAttempt(payload.toString())
        return localId
    }

    private fun recordTrainingComplete(totalScore: Int, passed: Boolean) {
        if (!SessionManager.isAuthenticated) return

        val id = attemptId ?: queueOfflineAttemptStart()
        attemptId = id

        lifecycleScope.launch {
            try {
                val res = ApiClient.api.completeTraining(
                    SessionManager.authHeader,
                    TrainingCompleteRequest(
                        attemptId = id,
                        actionScore = actionScore,
                        questionScore = questionScore,
                        totalScore = totalScore,
                        passed = passed,
                        escalationLevel = 0
                    )
                )
                if (!res.isSuccessful) {
                    queueOfflineAttemptComplete(id, totalScore, passed)
                }
            } catch (e: Exception) {
                queueOfflineAttemptComplete(id, totalScore, passed)
            }
        }
    }

    private fun queueOfflineAttemptComplete(attemptIdValue: String, totalScore: Int, passed: Boolean) {
        val now = java.time.Instant.now().toString()
        val payload = org.json.JSONObject().apply {
            put("AttemptId", attemptIdValue)
            put("EmployeeId", SessionManager.userId)
            put("ModuleId", currentModuleId)
            put("ScenarioId", currentScenarioId)
            put("StartTime", now)
            put("EndTime", now)
            put("ActionScore", actionScore)
            put("QuestionScore", questionScore)
            put("TotalScore", totalScore)
            put("Passed", passed)
            put("EscalationLevel", 0)
            put("Status", "completed")
        }
        com.surakshaar.data.offline.OfflineQueue.upsertAttempt(payload.toString())
    }

    private fun setupEscalationTimers(events: List<EscalationEvent>?) {
        events?.forEach { event ->
            if (event.timeThreshold <= 0f) return@forEach
            val timer = object : CountDownTimer(event.timeThreshold.toLong() * 1000L, 1000L) {
                override fun onTick(millisUntilFinished: Long) {
                    // tick
                }

                override fun onFinish() {
                    handleEscalation(event)
                }
            }
            escalationTimers.add(timer)
        }
    }

    private fun setupListeners() {
        extinguishButton.setOnClickListener {
            if (gameState == GameState.PLAYING) {
                performAction("use-extinguisher")
            }
        }

        evacuateButton.setOnClickListener {
            if (gameState == GameState.PLAYING) {
                performAction("evacuate")
            }
        }

        reportButton.setOnClickListener {
            if (gameState == GameState.PLAYING) {
                performAction("activate-alarm")
            }
        }

        retryButton.setOnClickListener {
            resetGame()
        }

        viewResultsButton.setOnClickListener {
            showResults()
        }

        retryResultsButton.setOnClickListener {
            resetGame()
        }

        homeButton.setOnClickListener {
            finish()
        }

        viewCertificateButton.setOnClickListener {
            val intent = Intent(this, AssessmentActivity::class.java).apply {
                putExtra("moduleId", currentModuleId)
                putExtra("scenarioId", currentScenarioId)
                putExtra("attemptId", attemptId ?: "")
                putExtra("actionScore", actionScore)
                putExtra("questionScore", questionScore)
                putExtra("totalScore", score)
                putExtra("passed", score >= passThreshold())
            }
            startActivity(intent)
        }
    }

    private fun startGame() {
        gameState = GameState.PLAYING
        showHUD()
        startGameTimer()
        startEscalationTimers()
        updateScoreDisplay()
    }

    private fun startGameTimer() {
        gameTimer = object : CountDownTimer(totalDurationMs, 1000L) {
            override fun onTick(millisUntilFinished: Long) {
                timeRemainingMs = millisUntilFinished
                val seconds = millisUntilFinished / 1000
                val minutes = seconds / 60
                val secs = seconds % 60
                timerText.text = String.format("%02d:%02d", minutes, secs)

                val progress = (millisUntilFinished.toFloat() / totalDurationMs * 100).toInt()
                timerProgress.progress = progress
            }

            override fun onFinish() {
                handleTimeUp()
            }
        }.start()
    }

    private fun startEscalationTimers() {
        escalationTimers.forEach { it.start() }
    }

    private fun handleEscalation(event: EscalationEvent) {
        if (gameState != GameState.PLAYING) return

        escalateHazards(event)

        when {
            event.triggerAutoFail -> {
                showCriticalFail(event.warningText)
            }
            event.level >= 3 -> {
                showEvacuation()
            }
            event.scorePenalty > 0 -> {
                applyPenalty((event.scorePenalty * 100).toInt())
                showAlert(event.warningText)
            }
            else -> {
                showAlert(event.warningText)
            }
        }
    }

    private fun escalateHazards(event: EscalationEvent) {
        val growth = event.hazardGrowthMultiplier.coerceIn(0.5f, 3f)
        hazardMarkers.values.forEach { node ->
            val base = node.scale
            node.scale = Float3(
                (base.x * growth).coerceAtLeast(0.5f),
                (base.y * growth).coerceAtLeast(0.5f),
                (base.z * growth).coerceAtLeast(0.5f)
            )
        }
        if (event.level >= 2) {
            instructionText.text = event.warningText
        }
    }

    private fun performAction(action: String) {
        if (completedActions.contains(action)) return

        completedActions.add(action)
        removeActionMarker(action)

        val scenario = currentScenario
        val actionResult = scenario?.requiredActions?.find {
            it.actionId == action || it.actionName.lowercase().replace(" ", "") == action
        }

        if (actionResult != null) {
            score = minOf(100, score + actionResult.scoreValue)
            instructionText.text = actionResult.completionFeedback
        } else {
            score = minOf(100, score + 10)
            instructionText.text =
                LanguageManager.get("action_completed") + ": $action"
        }

        updateScoreDisplay()

        val matchedActions = completedActions.filter { completed ->
            scenario?.requiredActions?.any { it.actionId == completed } == true
        }
        if (matchedActions.size >= (scenario?.requiredActions?.size ?: 0)) {
            finishGame()
        }
    }

    private fun applyPenalty(points: Int) {
        penalty += points
        score = maxOf(0, score - points)
        updateScoreDisplay()
    }

    private fun updateScoreDisplay() {
        scoreText.text = LanguageManager.get("score_label") + " $score"
        penaltyText.text = "-$penalty"
    }

    private fun handleTimeUp() {
        if (gameState == GameState.PLAYING) {
            finishGame()
        }
    }

    private fun finishGame() {
        gameState = GameState.COMPLETED
        gameTimer?.cancel()
        escalationTimers.forEach { it.cancel() }
        actionScore = score
        recordTrainingComplete(score, score >= passThreshold())
        showResults()
    }

    private fun showHUD() {
        hudOverlay.visibility = View.VISIBLE
        alertPanel.visibility = View.GONE
        criticalFailPanel.visibility = View.GONE
        evacuationPanel.visibility = View.GONE
        resultsPanel.visibility = View.GONE
    }

    private fun showAlert(message: String) {
        alertText.text = message
        alertPanel.visibility = View.VISIBLE

        alertHandler.removeCallbacksAndMessages(null)
        alertHandler.postDelayed({
            alertPanel.visibility = View.GONE
        }, 2000)
    }

    private fun showCriticalFail(message: String? = null) {
        gameState = GameState.FAILED
        gameTimer?.cancel()
        escalationTimers.forEach { it.cancel() }

        hudOverlay.visibility = View.GONE
        alertPanel.visibility = View.GONE
        evacuationPanel.visibility = View.GONE
        resultsPanel.visibility = View.GONE
        criticalFailPanel.visibility = View.VISIBLE

        criticalFailMessage.text = message
            ?: LanguageManager.get("scenario_failed_message")
        criticalFailScore.text = LanguageManager.get("score_label") + " $score"
        recordTrainingComplete(score, false)
    }

    private fun showEvacuation() {
        hudOverlay.visibility = View.GONE
        alertPanel.visibility = View.GONE
        criticalFailPanel.visibility = View.GONE
        resultsPanel.visibility = View.GONE
        evacuationPanel.visibility = View.VISIBLE

        evacuationHandler.removeCallbacksAndMessages(null)
        evacuationHandler.postDelayed({
            evacuationPanel.visibility = View.GONE
            showHUD()
        }, 3000)
    }

    private fun showResults() {
        gameState = GameState.COMPLETED
        gameTimer?.cancel()
        escalationTimers.forEach { it.cancel() }

        hudOverlay.visibility = View.GONE
        alertPanel.visibility = View.GONE
        criticalFailPanel.visibility = View.GONE
        evacuationPanel.visibility = View.GONE
        resultsPanel.visibility = View.VISIBLE

        val threshold = passThreshold()
        val passed = score >= threshold
        resultsTitle.setTextColor(
            if (passed) getColor(R.color.success) else getColor(R.color.danger)
        )
        try {
            resultsActions.text = LanguageManager.get("assess_actions") + " $actionScore"
            resultsQuestions.text = LanguageManager.get("assess_questions") + " $questionScore"
            resultsTotal.text = LanguageManager.get("assess_total") + " $score"
            resultsTitle.text = LanguageManager.get(if (passed) "passed" else "failed")
            resultsMessage.text = when {
                score >= 90 -> LanguageManager.get("assess_outstanding")
                passed -> LanguageManager.get("assess_good")
                score >= 50 -> LanguageManager.get("assess_improve")
                else -> LanguageManager.get("assess_failed")
            }
        } catch (e: Exception) {
            resultsActions.text = "Actions: $actionScore"
            resultsQuestions.text = "Questions: $questionScore"
            resultsTotal.text = "Total: $score"
            resultsTitle.text = if (passed) "PASSED" else "FAILED"
            resultsMessage.text = when {
                score >= 90 -> "Outstanding! You are well-prepared for emergencies."
                passed -> "Good job! You handled the scenario effectively."
                score >= 50 -> "Needs improvement. Review safety procedures."
                else -> "Failed. Please complete the training module again."
            }
        }
    }

    private fun resetGame() {
        gameTimer?.cancel()
        escalationTimers.forEach { it.cancel() }

        score = 0
        penalty = 0
        completedActions.clear()
        actionScore = 0
        questionScore = 0
        timeRemainingMs = totalDurationMs
        attemptId = null

        escalationTimers.clear()
        markersPlaced = false
        hazardMarkers.clear()
        actionMarkers.clear()
        placedMarkerIds.clear()
        try {
            arSceneView.clearChildNodes()
        } catch (e: Exception) {
            Log.w(TAG, "Clear markers failed", e)
        }

        timerProgress.progress = 100
        scoreText.text = LanguageManager.get("score_label") + " 0"
        penaltyText.text = "-0"

        loadScenario()
        startGame()
    }

    override fun onDestroy() {
        super.onDestroy()
        gameTimer?.cancel()
        escalationTimers.forEach { it.cancel() }
        alertHandler.removeCallbacksAndMessages(null)
        evacuationHandler.removeCallbacksAndMessages(null)
    }
}
