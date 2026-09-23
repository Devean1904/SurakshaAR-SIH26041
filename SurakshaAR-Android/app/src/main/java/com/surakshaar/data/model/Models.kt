package com.surakshaar.data.model

import com.google.gson.annotations.SerializedName

enum class GameState { PLAYING, COMPLETED, FAILED }

// ── Auth ──
data class LoginRequest(
    @SerializedName("UserId") val userId: String,
    @SerializedName("Password") val password: String,
    @SerializedName("CompanyId") val companyId: String = ""
)

data class OtpSendRequest(@SerializedName("PhoneNumber") val phoneNumber: String)

data class OtpVerifyRequest(
    @SerializedName("PhoneNumber") val phoneNumber: String,
    @SerializedName("Otp") val otp: String,
    @SerializedName("CompanyId") val companyId: String = ""
)

data class AuthResponse(
    val success: Boolean,
    val token: String?,
    val userId: String?,
    val role: String?,
    val name: String?,
    val theme: String?,
    val language: String?,
    val companyId: String? = null,
    val message: String?
)

// ── Training Module ──
data class TrainingModule(
    val moduleId: String,
    val title: String,
    val description: String,
    val safetyDomain: String,
    val difficultyLevel: Int,
    val timeLimitSeconds: Float,
    val passThreshold: Float,
    val scenarios: List<ScenarioConfig>,
    val questions: List<AssessmentQuestion>,
    val requiredEquipment: List<String>
)

data class ScenarioConfig(
    val scenarioId: String,
    val moduleId: String,
    val hazardType: String,
    val scenarioName: String,
    val initialDangerLevel: Float,
    val escalationIntervalSeconds: Float,
    val maxEscalationLevel: Int,
    val hazardZones: List<HazardZone>,
    val requiredActions: List<RequiredAction>,
    val escalationEvents: List<EscalationEvent>
)

data class HazardZone(
    val zoneId: String,
    val zoneName: String,
    val riskColor: String,
    val position: FloatArray,
    val radius: Float,
    val dangerLevel: Float
)

data class RequiredAction(
    val actionId: String,
    val actionName: String,
    val description: String,
    val completionFeedback: String,
    val interactionRadius: Float,
    val timeLimitSeconds: Float,
    val scoreValue: Int,
    val isCritical: Boolean
)

data class EscalationEvent(
    val level: Int,
    val levelName: String,
    val timeThreshold: Float,
    val scorePenalty: Float,
    val visualEffect: String,
    val audioEffect: String,
    val warningText: String,
    val showEvacuationArrows: Boolean,
    val triggerAutoFail: Boolean,
    val hazardGrowthMultiplier: Float
)

data class AssessmentQuestion(
    val questionId: String,
    val moduleId: String,
    val questionText: String,
    val options: List<String>,
    val correctAnswerIndex: Int? = null,
    val scoreValue: Int,
    val explanation: String,
    val difficulty: String
)

// ── Certificate ──
data class CertificateData(
    val certificateId: String,
    val employeeId: String,
    val employeeName: String,
    val moduleId: String,
    val moduleName: String,
    val score: Int,
    val passed: Boolean,
    val issuedAt: String,
    val signature: String,
    val blockchainTx: String,
    val qrPayload: String,
    val qrCodeBase64: String? = null
)

data class CertificateGenerateRequest(
    @SerializedName("EmployeeId") val employeeId: String,
    @SerializedName("EmployeeName") val employeeName: String,
    @SerializedName("ModuleId") val moduleId: String,
    @SerializedName("ModuleName") val moduleName: String
)

data class CertificateGenerateResponse(
    val certificateId: String,
    val employeeId: String,
    val moduleId: String,
    val score: Int,
    val passed: Boolean,
    val signature: String,
    val blockchainTx: String,
    val qrPayload: String,
    val issuedAt: String
)

// ── Assessment ──
data class AssessmentResult(
    val moduleId: String,
    val actionScore: Int,
    val questionScore: Int,
    val totalScore: Int,
    val passed: Boolean,
    val answers: List<Int>,
    val totalQuestions: Int,
    val correctAnswers: Int
)

data class AssessmentSubmitRequest(
    @SerializedName("EmployeeId") val employeeId: String,
    @SerializedName("ModuleId") val moduleId: String,
    @SerializedName("AttemptId") val attemptId: String,
    @SerializedName("ActionScore") val actionScore: Int,
    @SerializedName("QuestionScore") val questionScore: Int,
    @SerializedName("TotalScore") val totalScore: Int,
    @SerializedName("Passed") val passed: Boolean,
    @SerializedName("Answers") val answers: List<Int>
)

data class AssessmentSubmitResponse(
    val assessmentId: String,
    val totalScore: Int,
    val passed: Boolean,
    val message: String
)

// ── Training ──
data class TrainingStartRequest(
    @SerializedName("EmployeeId") val employeeId: String,
    @SerializedName("ModuleId") val moduleId: String,
    @SerializedName("ScenarioId") val scenarioId: String
)

data class TrainingStartResponse(
    val attemptId: String,
    val message: String
)

data class TrainingCompleteRequest(
    @SerializedName("AttemptId") val attemptId: String,
    @SerializedName("ActionScore") val actionScore: Int,
    @SerializedName("QuestionScore") val questionScore: Int,
    @SerializedName("TotalScore") val totalScore: Int,
    @SerializedName("Passed") val passed: Boolean,
    @SerializedName("EscalationLevel") val escalationLevel: Int
)

data class TrainingAttemptModel(
    val attemptId: String,
    val employeeId: String,
    val moduleId: String,
    val scenarioId: String,
    val startTime: String,
    val endTime: String,
    val actionScore: Int,
    val questionScore: Int,
    val totalScore: Int,
    val passed: Boolean,
    val escalationLevel: Int,
    val status: String
)

// ── Manager / Admin Models ──
data class WorkerInfo(
    val userId: String,
    val name: String,
    val phoneNumber: String,
    val awaitingAdminConfirmation: Boolean,
    val enrolledByManagerId: String = "",
    val isActive: Boolean = true,
    val createdAt: String = ""
)

data class ManagerInfo(
    val userId: String,
    val name: String,
    val phoneNumber: String,
    val createdAt: String,
    val assignedWorkerIds: List<String>
)

data class AdminInfo(
    val userId: String,
    val name: String,
    val phoneNumber: String,
    val createdAt: String
)

data class AddWorkerRequest(
    @SerializedName("UserId") val userId: String,
    @SerializedName("Name") val name: String,
    @SerializedName("PhoneNumber") val phoneNumber: String,
    @SerializedName("Password") val password: String
)

data class AddManagerRequest(
    @SerializedName("UserId") val userId: String,
    @SerializedName("Name") val name: String,
    @SerializedName("PhoneNumber") val phoneNumber: String,
    @SerializedName("Password") val password: String
)

data class ConfirmWorkerRequest(
    @SerializedName("WorkerId") val workerId: String
)

data class RemoveUserRequest(
    @SerializedName("UserId") val userId: String
)

data class WorkerListResponse(
    val userId: String,
    val name: String,
    val phoneNumber: String,
    val awaitingAdminConfirmation: Boolean,
    val isActive: Boolean
)

data class ManagerStatsResponse(
    val totalWorkers: Int = 0,
    val activeWorkers: Int = 0,
    val pendingWorkers: Int = 0,
    val inactiveWorkers: Int = 0
)

data class AssignEscalationRequest(
    @SerializedName("ReportId") val reportId: String,
    @SerializedName("AssignedTo") val assignedTo: String
)

data class ResolveEscalationRequest(
    @SerializedName("ReportId") val reportId: String
)

// ── Site Mapping ──
data class SiteModel(
    val siteId: String,
    val siteName: String,
    val recordedByAdminId: String,
    val recordedAt: String,
    val planesDetected: Int,
    val anchorsPlaced: Int,
    val floorLevel: Float,
    val spatialData: SiteSpatialData,
    val isActive: Boolean = true
)

data class SiteSpatialData(
    val planePositions: List<FloatArray>,
    val anchorPositions: List<FloatArray>,
    val boundingBoxMin: FloatArray,
    val boundingBoxMax: FloatArray,
    val meshVertices: Int = 0
)

data class SaveSiteRequest(
    @SerializedName("SiteName") val siteName: String,
    @SerializedName("AnchorPosition") val anchorPosition: FloatArray = floatArrayOf(0f, 0f, 0f),
    @SerializedName("AnchorRotation") val anchorRotation: FloatArray = floatArrayOf(0f, 0f, 0f, 1f),
    @SerializedName("AnchorScale") val anchorScale: FloatArray = floatArrayOf(1f, 1f, 1f),
    @SerializedName("ScenarioPoints") val scenarioPoints: List<Any> = emptyList(),
    @SerializedName("RecordedAt") val recordedAt: String = ""
)
