package com.surakshaar.ui.certificate

import android.content.ContentValues
import android.content.Intent
import android.content.pm.PackageManager
import android.graphics.Bitmap
import android.graphics.Color
import android.os.Build
import android.os.Bundle
import android.os.Environment
import android.provider.MediaStore
import android.widget.Button
import android.widget.ImageView
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import androidx.lifecycle.lifecycleScope
import com.google.zxing.BarcodeFormat
import com.google.zxing.qrcode.QRCodeWriter
import com.surakshaar.R
import com.surakshaar.data.api.ApiClient
import com.surakshaar.data.model.CertificateGenerateRequest
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.util.ThemeManager
import com.surakshaar.util.VoiceManager
import kotlinx.coroutines.launch
import java.io.File
import java.io.FileOutputStream

class CertificateActivity : AppCompatActivity() {

    private lateinit var employeeName: TextView
    private lateinit var employeeId: TextView
    private lateinit var certificateTitle: TextView
    private lateinit var moduleName: TextView
    private lateinit var scoreDisplay: TextView
    private lateinit var statusBadge: TextView
    private lateinit var issueDate: TextView
    private lateinit var qrCodeImage: ImageView
    private lateinit var certificateId: TextView
    private lateinit var blockchainTx: TextView
    private lateinit var downloadButton: Button
    private lateinit var shareButton: Button
    private lateinit var verifyButton: Button

    private var certificateData: String = ""
    private var qrBitmap: Bitmap? = null
    private var sharePayload: String = ""

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        ThemeManager.applyTheme(this, SessionManager.theme)
        setContentView(R.layout.activity_certificate)
        VoiceManager.attachToActivity(this)

        initViews()
        applyTranslations()
        loadCertificateData()
        setupButtons()
        if (VoiceManager.isEnabled()) {
            VoiceManager.speak(
                com.surakshaar.language.LanguageManager.get("cert_voice_ready") + " " + moduleName.text,
                force = true
            )
        }
    }

    private fun applyTranslations() {
        try {
            val t = com.surakshaar.language.LanguageManager
            certificateTitle.text = t.get("cert_title")
            downloadButton.text = t.get("cert_download")
            shareButton.text = t.get("cert_share")
            verifyButton.text = t.get("qr_verify_title")
        } catch (e: Exception) {
        }
    }

    private fun initViews() {
        employeeName = findViewById(R.id.employeeName)
        employeeId = findViewById(R.id.employeeId)
        certificateTitle = findViewById(R.id.certificateTitle)
        moduleName = findViewById(R.id.moduleName)
        scoreDisplay = findViewById(R.id.scoreDisplay)
        statusBadge = findViewById(R.id.statusBadge)
        issueDate = findViewById(R.id.issueDate)
        qrCodeImage = findViewById(R.id.qrCodeImage)
        certificateId = findViewById(R.id.certificateId)
        blockchainTx = findViewById(R.id.blockchainTx)
        downloadButton = findViewById(R.id.downloadButton)
        shareButton = findViewById(R.id.shareButton)
        verifyButton = findViewById(R.id.verifyButton)
    }

    private fun loadCertificateData() {
        val empId = intent.getStringExtra("employeeId") ?: SessionManager.userId
        val moduleId = intent.getStringExtra("moduleId") ?: "Unknown"
        val userName = SessionManager.userName.ifEmpty { empId }

        if (!SessionManager.isAuthenticated) {
            showServerError(com.surakshaar.language.LanguageManager.get("cert_auth_required"))
            return
        }

        lifecycleScope.launch {
            try {
                val moduleNameStr = getModuleName(moduleId)
                val response = ApiClient.api.generateCertificate(
                    SessionManager.authHeader,
                    CertificateGenerateRequest(
                        employeeId = empId,
                        employeeName = userName,
                        moduleId = moduleId,
                        moduleName = moduleNameStr
                    )
                )
                val cert = if (response.isSuccessful) response.body() else null
                if (cert != null) {
                    displayCertificate(
                        certId = cert.certificateId,
                        empId = empId,
                        empName = userName,
                        moduleId = moduleId,
                        totalScore = cert.score,
                        passed = cert.passed,
                        signature = cert.signature,
                        blockchainHash = cert.blockchainTx,
                        qrPayload = cert.qrPayload,
                        issuedAt = cert.issuedAt
                    )
                } else {
                    val err = try {
                        response.errorBody()?.string().orEmpty()
                    } catch (e: Exception) {
                        ""
                    }
                    showServerError(
                        parseCertError(response.code(), err)
                    )
                }
            } catch (e: Exception) {
                showServerError(com.surakshaar.language.LanguageManager.get("cert_server_unreachable"))
            }
        }
    }

    private fun parseCertError(code: Int, body: String): String {
        val message = try {
            val json = org.json.JSONObject(body)
            json.optString("message", "")
        } catch (e: Exception) {
            ""
        }
        return when {
            message.isNotEmpty() -> message
            code == 401 -> com.surakshaar.language.LanguageManager.get("cert_err_401")
            code == 403 -> com.surakshaar.language.LanguageManager.get("cert_err_403")
            code == 404 -> com.surakshaar.language.LanguageManager.get("cert_err_404")
            else -> com.surakshaar.language.LanguageManager.get("cert_err_generic") + " (error $code)."
        }
    }

    private fun showServerError(message: String) {
        val t = com.surakshaar.language.LanguageManager
        employeeName.text = t.get("cert_employee_label") + " " + SessionManager.userName.ifEmpty { SessionManager.userId }
        employeeId.text = t.get("cert_id_label") + " " + SessionManager.userId
        moduleName.text = getModuleName(intent.getStringExtra("moduleId") ?: "")
        scoreDisplay.text = "—"
        statusBadge.text = t.get("cert_unavailable")
        statusBadge.setBackgroundColor(getColor(R.color.danger))
        statusBadge.setTextColor(Color.WHITE)
        issueDate.text = message
        certificateId.text = "—"
        blockchainTx.text = t.get("cert_local_hash_chain")
        qrCodeImage.setImageDrawable(null)
        downloadButton.isEnabled = false
        shareButton.isEnabled = false
        verifyButton.isEnabled = false
        Toast.makeText(this, message, Toast.LENGTH_LONG).show()
    }

    private fun displayCertificate(
        certId: String,
        empId: String,
        empName: String,
        moduleId: String,
        totalScore: Int,
        passed: Boolean,
        signature: String,
        blockchainHash: String,
        qrPayload: String,
        issuedAt: String
    ) {
        certificateData = qrPayload
        sharePayload = qrPayload

        val t = com.surakshaar.language.LanguageManager
        employeeName.text = t.get("cert_employee_label") + " $empName"
        employeeId.text = t.get("cert_id_label") + " $empId"
        moduleName.text = getModuleName(moduleId)
        scoreDisplay.text = t.get("cert_score_label") + " $totalScore/100"

        if (passed) {
            scoreDisplay.setTextColor(getColor(R.color.success))
            statusBadge.text = t.get("passed")
            statusBadge.setBackgroundColor(getColor(R.color.success))
        } else {
            scoreDisplay.setTextColor(getColor(R.color.danger))
            statusBadge.text = t.get("failed")
            statusBadge.setBackgroundColor(getColor(R.color.danger))
        }

        statusBadge.setTextColor(Color.WHITE)
        issueDate.text = t.get("cert_issued") + " $issuedAt"
        certificateId.text = certId
        val displayTx = blockchainHash.removePrefix("LOCAL-")
        blockchainTx.text = if (blockchainHash.startsWith("LOCAL-")) {
            t.get("cert_tx_local") + " ${displayTx.take(4)}...${displayTx.takeLast(4)}"
        } else {
            t.get("cert_tx") + " ${blockchainHash.take(4)}...${blockchainHash.takeLast(4)}"
        }

        qrBitmap = generateQrBitmap(certificateData)
        qrCodeImage.setImageBitmap(qrBitmap)
    }

    private fun getModuleName(moduleId: String): String {
        val t = com.surakshaar.language.LanguageManager
        return when (moduleId) {
            "fire-safety-101" -> t.get("module_fire")
            "gas-leak-101" -> t.get("module_gas")
            "machinery-101" -> t.get("module_machinery")
            "electrical-101" -> t.get("module_electrical")
            "heights-101" -> t.get("module_heights")
            else -> moduleId.replace("_", " ").replaceFirstChar { it.uppercase() }
        }
    }

    private fun generateQrBitmap(content: String): Bitmap? {
        return try {
            val writer = QRCodeWriter()
            val bitMatrix = writer.encode(content, BarcodeFormat.QR_CODE, 512, 512)
            val width = bitMatrix.width
            val height = bitMatrix.height
            val bitmap = Bitmap.createBitmap(width, height, Bitmap.Config.RGB_565)

            for (x in 0 until width) {
                for (y in 0 until height) {
                    bitmap.setPixel(x, y, if (bitMatrix[x, y]) Color.BLACK else Color.WHITE)
                }
            }
            bitmap
        } catch (e: Exception) {
            e.printStackTrace()
            null
        }
    }

    private fun setupButtons() {
        downloadButton.setOnClickListener { downloadCertificate() }
        shareButton.setOnClickListener { shareCertificate() }
        verifyButton.setOnClickListener { verifyCertificate() }
    }

    private fun downloadCertificate() {
        val bitmap = qrBitmap ?: return
        val filename = "SurakshaAR_Certificate_${certificateId.text}.png"

        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
                val contentValues = ContentValues().apply {
                    put(MediaStore.Downloads.DISPLAY_NAME, filename)
                    put(MediaStore.Downloads.MIME_TYPE, "image/png")
                    put(MediaStore.Downloads.RELATIVE_PATH, Environment.DIRECTORY_DOWNLOADS)
                }

                val uri = contentResolver.insert(MediaStore.Downloads.EXTERNAL_CONTENT_URI, contentValues)
                uri?.let {
                    contentResolver.openOutputStream(it)?.use { outputStream ->
                        bitmap.compress(Bitmap.CompressFormat.PNG, 100, outputStream)
                    }
                    Toast.makeText(this, com.surakshaar.language.LanguageManager.get("cert_downloaded_toast"), Toast.LENGTH_SHORT).show()
                }
            } else {
                if (ContextCompat.checkSelfPermission(this, android.Manifest.permission.WRITE_EXTERNAL_STORAGE)
                    != PackageManager.PERMISSION_GRANTED) {
                    ActivityCompat.requestPermissions(this,
                        arrayOf(android.Manifest.permission.WRITE_EXTERNAL_STORAGE), 1002)
                    Toast.makeText(this, com.surakshaar.language.LanguageManager.get("cert_grant_storage"), Toast.LENGTH_SHORT).show()
                    return
                }
                val downloadsDir = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS)
                val file = File(downloadsDir, filename)
                FileOutputStream(file).use { outputStream ->
                    bitmap.compress(Bitmap.CompressFormat.PNG, 100, outputStream)
                }
                Toast.makeText(this, com.surakshaar.language.LanguageManager.get("cert_downloaded_toast"), Toast.LENGTH_SHORT).show()
            }
        } catch (e: Exception) {
            e.printStackTrace()
            Toast.makeText(this, com.surakshaar.language.LanguageManager.get("cert_download_failed"), Toast.LENGTH_SHORT).show()
        }
    }

    private fun shareCertificate() {
        val t = com.surakshaar.language.LanguageManager
        val shareIntent = Intent(Intent.ACTION_SEND).apply {
            type = "text/plain"
            putExtra(Intent.EXTRA_SUBJECT, t.get("cert_share_subject"))
            putExtra(Intent.EXTRA_TEXT, sharePayload)
        }
        startActivity(Intent.createChooser(shareIntent, t.get("cert_share_chooser")))
    }

    private fun verifyCertificate() {
        val t = com.surakshaar.language.LanguageManager
        if (sharePayload.isEmpty()) {
            Toast.makeText(this, t.get("cert_verify_none"), Toast.LENGTH_SHORT).show()
            return
        }

        lifecycleScope.launch {
            try {
                val response = ApiClient.api.verifyCertificate(sharePayload)
                if (response.isSuccessful) {
                    val result = response.body()
                    if (result != null && result.valid) {
                        Toast.makeText(
                            this@CertificateActivity,
                            t.get("cert_verified_toast") + "\n" + t.get("cert_id_label") + " ${result.certificateId}\n" + t.get("cert_score_label") + " ${result.score}",
                            Toast.LENGTH_LONG
                        ).show()
                    } else {
                        Toast.makeText(
                            this@CertificateActivity,
                            t.get("cert_not_found_local_chain"),
                            Toast.LENGTH_SHORT
                        ).show()
                    }
                } else {
                    Toast.makeText(this@CertificateActivity, t.get("cert_verify_unavailable"), Toast.LENGTH_SHORT).show()
                }
            } catch (e: Exception) {
                Toast.makeText(this@CertificateActivity, t.get("cert_verify_offline"), Toast.LENGTH_SHORT).show()
            }
        }
    }
}
