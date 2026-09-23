package com.surakshaar.ui.admin

import android.Manifest
import android.content.pm.PackageManager
import android.os.Bundle
import android.util.Log
import android.widget.EditText
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import androidx.lifecycle.lifecycleScope
import com.google.ar.core.Frame
import com.google.ar.core.Plane
import com.google.ar.core.Point
import com.google.ar.core.TrackingState
import io.github.sceneview.ar.ARSceneView
import com.surakshaar.R
import com.surakshaar.data.api.ApiClient
import com.surakshaar.data.model.SaveSiteRequest
import com.surakshaar.data.model.SiteModel
import com.surakshaar.data.model.SiteSpatialData
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.data.repository.SiteStorage
import com.surakshaar.language.LanguageManager
import com.surakshaar.util.ThemeManager
import com.surakshaar.util.VoiceManager
import kotlinx.coroutines.CoroutineExceptionHandler
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.UUID

class SiteMapActivity : AppCompatActivity() {

    companion object {
        private const val TAG = "SiteMapActivity"
        private const val CAMERA_PERMISSION_CODE = 100
    }

    private val exceptionHandler = CoroutineExceptionHandler { _, throwable ->
        Log.e(TAG, "Coroutine error", throwable)
    }

    private lateinit var arSceneView: ARSceneView
    private lateinit var tvStatus: TextView
    private lateinit var tvPlanesCount: TextView
    private lateinit var tvAnchorsCount: TextView
    private lateinit var tvTracking: TextView
    private lateinit var tvBoundingBox: TextView
    private lateinit var tvSaveSummary: TextView
    private lateinit var tvRecordBtn: TextView
    private lateinit var etSiteName: EditText
    private lateinit var btnRecord: LinearLayout
    private lateinit var btnPlaceAnchor: LinearLayout

    private var isRecording = false
    private var trackingJob: Job? = null
    private var lastHudUpdateMs = 0L

    private val detectedPlaneIds = mutableSetOf<Long>()
    private val placedAnchors = mutableListOf<FloatArray>()
    private val anchorMarkerNodes = mutableListOf<io.github.sceneview.node.Node>()

    private val boundingMin = floatArrayOf(Float.MAX_VALUE, Float.MAX_VALUE, Float.MAX_VALUE)
    private val boundingMax = floatArrayOf(-Float.MAX_VALUE, -Float.MAX_VALUE, -Float.MAX_VALUE)

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        ThemeManager.applyTheme(this, SessionManager.theme)
        setContentView(R.layout.activity_site_map)
        VoiceManager.attachToActivity(this)
        SiteStorage.init(this)

        arSceneView = findViewById(R.id.arSceneView)
        tvStatus = findViewById(R.id.tvStatus)
        tvPlanesCount = findViewById(R.id.tvPlanesCount)
        tvAnchorsCount = findViewById(R.id.tvAnchorsCount)
        tvTracking = findViewById(R.id.tvTracking)
        tvBoundingBox = findViewById(R.id.tvBoundingBox)
        tvSaveSummary = findViewById(R.id.tvSaveSummary)
        tvRecordBtn = findViewById(R.id.tvRecordBtn)
        etSiteName = findViewById(R.id.etSiteName)
        btnRecord = findViewById(R.id.btnRecord)
        btnPlaceAnchor = findViewById(R.id.btnPlaceAnchor)
        val btnSave = findViewById<LinearLayout>(R.id.btnSave)

        findViewById<android.widget.Button>(R.id.backButton).setOnClickListener { finish() }
        findViewById<TextView>(R.id.titleText).text = LanguageManager.get("site_title")

        btnRecord.setOnClickListener { toggleRecording() }
        btnPlaceAnchor.setOnClickListener { placeAnchorAtCenter() }
        btnSave.setOnClickListener { saveSite() }

        if (ContextCompat.checkSelfPermission(this, Manifest.permission.CAMERA)
            != PackageManager.PERMISSION_GRANTED
        ) {
            ActivityCompat.requestPermissions(
                this, arrayOf(Manifest.permission.CAMERA), CAMERA_PERMISSION_CODE
            )
        } else {
            initArScene()
        }
    }

    override fun onRequestPermissionsResult(
        requestCode: Int, permissions: Array<out String>, grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        if (requestCode == CAMERA_PERMISSION_CODE &&
            grantResults.isNotEmpty() && grantResults[0] == PackageManager.PERMISSION_GRANTED
        ) {
            initArScene()
        } else {
            tvStatus.text = LanguageManager.get("site_no_permission")
        }
    }

    private fun initArScene() {
        try {
            arSceneView.planeRenderer.isVisible = true
            arSceneView.planeRenderer.isEnabled = true
            arSceneView.onSessionFailed = { e ->
                Log.e(TAG, "AR session failed", e)
                tvStatus.text = "AR init failed: ${e.message}"
            }
            arSceneView.onSessionCreated = {
                runOnUiThread {
                    tvStatus.text = LanguageManager.get("site_scanning")
                }
            }
            arSceneView.onSessionUpdated = { _, frame ->
                runOnUiThread { processFrame(frame) }
            }
            tvStatus.text = LanguageManager.get("site_scanning")
            startTrackingLoop()
            updateHud(force = true, tracking = null, planeTotal = detectedPlaneIds.size)
        } catch (e: Exception) {
            Log.e(TAG, "AR init failed", e)
            tvStatus.text = "AR init failed: ${e.message}"
        }
    }

    private fun startTrackingLoop() {
        trackingJob?.cancel()
        trackingJob = lifecycleScope.launch(exceptionHandler) {
            while (isActive) {
                try {
                    val frame = arSceneView.frame
                    if (frame != null) {
                        processFrame(frame)
                    } else {
                        runOnUiThread {
                            tvTracking.text = "Tracking: —"
                        }
                    }
                } catch (e: Exception) {
                    Log.w(TAG, "Tracking error", e)
                }
                delay(200)
            }
        }
    }

    private fun processFrame(frame: Frame) {
        try {
            val camera = frame.camera
            val tracking = camera?.trackingState == TrackingState.TRACKING

            val updatedPlanes = frame.getUpdatedTrackables(Plane::class.java)
            var newCount = 0
            for (plane in updatedPlanes) {
                val id = plane.hashCode().toLong()
                if (detectedPlaneIds.add(id)) {
                    newCount++
                }
                if (tracking && isRecording) {
                    val center = plane.centerPose
                    updateBoundingBox(floatArrayOf(center.tx(), center.ty(), center.tz()))
                }
            }

            if (tracking && isRecording) {
                for (anchor in placedAnchors) {
                    updateBoundingBox(anchor)
                }
            }

            if (newCount > 0 || isRecording || System.currentTimeMillis() - lastHudUpdateMs > 400) {
                updateHud(
                    force = newCount > 0,
                    tracking = tracking,
                    planeTotal = detectedPlaneIds.size
                )
            }
        } catch (e: Exception) {
            Log.w(TAG, "Frame process error", e)
        }
    }

    private fun updateHud(force: Boolean, tracking: Boolean?, planeTotal: Int) {
        val now = System.currentTimeMillis()
        if (!force && now - lastHudUpdateMs < 200) return
        lastHudUpdateMs = now

        tvPlanesCount.text = "Planes: $planeTotal"
        tvAnchorsCount.text = "Anchors: ${placedAnchors.size}"
        tvTracking.text = when (tracking) {
            true -> "Tracking: OK"
            false -> "Tracking: lost"
            null -> "Tracking: —"
        }

        val hasBounds = detectedPlaneIds.isNotEmpty() || placedAnchors.isNotEmpty()
        if (hasBounds && boundingMin[0] <= boundingMax[0]) {
            val dx = (boundingMax[0] - boundingMin[0]).coerceAtLeast(0f)
            val dy = (boundingMax[1] - boundingMin[1]).coerceAtLeast(0f)
            val dz = (boundingMax[2] - boundingMin[2]).coerceAtLeast(0f)
            tvBoundingBox.text = String.format(Locale.US, "Bounds: %.1f × %.1f × %.1f m", dx, dy, dz)
        } else {
            tvBoundingBox.text = "Bounds: —"
        }

        tvSaveSummary.text =
            "Will save: $planeTotal planes · ${placedAnchors.size} anchors" +
                if (isRecording) " · REC" else ""
    }

    private fun toggleRecording() {
        isRecording = !isRecording
        if (isRecording) {
            tvRecordBtn.text = LanguageManager.get("site_stop_recording")
            btnRecord.setBackgroundColor(ContextCompat.getColor(this, R.color.danger))
            tvStatus.text = LanguageManager.get("site_scanning")
        } else {
            tvRecordBtn.text = LanguageManager.get("site_start_recording")
            btnRecord.setBackgroundColor(ContextCompat.getColor(this, R.color.primary))
            tvStatus.text = LanguageManager.get("site_ready")
        }
        updateHud(force = true, tracking = null, planeTotal = detectedPlaneIds.size)
    }

    private fun placeAnchorAtCenter() {
        val frame = arSceneView.frame ?: run {
            Toast.makeText(this, "AR not ready", Toast.LENGTH_SHORT).show()
            return
        }

        try {
            val camera = frame.camera ?: return
            if (camera.trackingState != TrackingState.TRACKING) {
                Toast.makeText(this, "No surfaces detected yet", Toast.LENGTH_SHORT).show()
                return
            }

            val viewWidth = arSceneView.width.toFloat()
            val viewHeight = arSceneView.height.toFloat()
            val hitResults = frame.hitTest(viewWidth / 2f, viewHeight / 2f)

            val hitResult = hitResults.firstOrNull { result ->
                val trackable = result.trackable
                trackable is Plane && trackable.isPoseInPolygon(result.hitPose)
            } ?: hitResults.firstOrNull { it.trackable is Point }

            if (hitResult != null) {
                val anchor = hitResult.createAnchor()
                val pose = hitResult.hitPose
                val pos = floatArrayOf(pose.tx(), pose.ty(), pose.tz())
                placedAnchors.add(pos)
                updateBoundingBox(pos)
                addAnchorMarker(pos)

                runOnUiThread {
                    updateHud(force = true, tracking = true, planeTotal = detectedPlaneIds.size)
                    Toast.makeText(
                        this,
                        "Anchor placed (${String.format("%.1f, %.1f, %.1f", pos[0], pos[1], pos[2])})",
                        Toast.LENGTH_SHORT
                    ).show()
                }
            } else {
                runOnUiThread {
                    Toast.makeText(this, "No surface at center", Toast.LENGTH_SHORT).show()
                }
            }
        } catch (e: Exception) {
            Log.e(TAG, "Anchor placement failed", e)
            runOnUiThread { Toast.makeText(this, "Error: ${e.message}", Toast.LENGTH_SHORT).show() }
        }
    }

    private fun updateBoundingBox(position: FloatArray) {
        for (i in 0..2) {
            if (position[i] < boundingMin[i]) boundingMin[i] = position[i]
            if (position[i] > boundingMax[i]) boundingMax[i] = position[i]
        }
    }

    private fun addAnchorMarker(position: FloatArray) {
        try {
            val node = io.github.sceneview.node.SphereNode(
                engine = arSceneView.engine,
                radius = 0.08f,
                materialInstance = arSceneView.materialLoader.createColorInstance(
                    android.graphics.Color.YELLOW
                )
            )
            node.position = dev.romainguy.kotlin.math.Float3(position[0], position[1], position[2])
            arSceneView.addChildNode(node)
            anchorMarkerNodes.add(node)
        } catch (e: Exception) {
            Log.w(TAG, "Anchor marker failed", e)
        }
    }

    private fun saveSite() {
        val siteName = etSiteName.text.toString().trim()
        if (siteName.isEmpty()) {
            etSiteName.error = LanguageManager.get("site_name_hint")
            return
        }

        val siteId = "SITE_${UUID.randomUUID().toString().take(8).uppercase(Locale.US)}"
        val now = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US).format(Date())
        val spatialData = SiteSpatialData(
            planePositions = placedAnchors.toList(),
            anchorPositions = placedAnchors.toList(),
            boundingBoxMin = boundingMin.copyOf(),
            boundingBoxMax = boundingMax.copyOf(),
            meshVertices = 0
        )

        val site = SiteModel(
            siteId = siteId,
            siteName = siteName,
            recordedByAdminId = SessionManager.userId,
            recordedAt = now,
            planesDetected = detectedPlaneIds.size,
            anchorsPlaced = placedAnchors.size,
            floorLevel = if (placedAnchors.isNotEmpty()) placedAnchors.map { it[1] }.average().toFloat() else 0f,
            spatialData = spatialData,
            isActive = true
        )

        SiteStorage.saveSite(site)

        val token = SessionManager.authHeader
        lifecycleScope.launch(exceptionHandler) {
            try {
                val request = SaveSiteRequest(
                    siteName = siteName,
                    anchorPosition = if (placedAnchors.isNotEmpty()) placedAnchors.last() else floatArrayOf(0f, 0f, 0f),
                    anchorRotation = floatArrayOf(0f, 0f, 0f, 1f),
                    anchorScale = floatArrayOf(1f, 1f, 1f),
                    scenarioPoints = emptyList(),
                    recordedAt = now
                )
                val response = ApiClient.api.saveSite(token, request)
                withContext(Dispatchers.Main) {
                    if (response.isSuccessful) {
                        Toast.makeText(this@SiteMapActivity, LanguageManager.get("site_saved"), Toast.LENGTH_SHORT).show()
                    } else {
                        Toast.makeText(this@SiteMapActivity, "Saved locally (server ${response.code()})", Toast.LENGTH_SHORT).show()
                    }
                }
            } catch (e: Exception) {
                withContext(Dispatchers.Main) {
                    Toast.makeText(this@SiteMapActivity, "Saved locally (offline)", Toast.LENGTH_SHORT).show()
                }
            }
        }

        Log.i(TAG, "Site saved: $siteId, planes=${detectedPlaneIds.size}, anchors=${placedAnchors.size}")
        finish()
    }

    override fun onDestroy() {
        trackingJob?.cancel()
        anchorMarkerNodes.clear()
        super.onDestroy()
    }
}
