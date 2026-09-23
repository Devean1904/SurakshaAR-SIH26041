package com.surakshaar.data.api

import com.surakshaar.data.model.*
import retrofit2.Response
import retrofit2.http.*

interface ApiService {

    // ── Auth ──
    @POST("auth/login")
    suspend fun login(@Body request: LoginRequest): Response<AuthResponse>

    @POST("auth/otp/send")
    suspend fun sendOtp(@Body request: OtpSendRequest): Response<AuthResponse>

    @POST("auth/otp/verify")
    suspend fun verifyOtp(@Body request: OtpVerifyRequest): Response<AuthResponse>

    // ── Training ──
    @GET("training/modules")
    suspend fun getTrainingModules(
        @Header("Authorization") token: String
    ): Response<List<TrainingModule>>

    @GET("training/modules/{id}")
    suspend fun getTrainingModule(
        @Header("Authorization") token: String,
        @Path("id") moduleId: String
    ): Response<TrainingModule>

    @POST("training/start")
    suspend fun startTraining(
        @Header("Authorization") token: String,
        @Body request: TrainingStartRequest
    ): Response<TrainingStartResponse>

    @POST("training/complete")
    suspend fun completeTraining(
        @Header("Authorization") token: String,
        @Body request: TrainingCompleteRequest
    ): Response<GenericMessageResponse>

    @GET("training/history/{employeeId}")
    suspend fun getTrainingHistory(
        @Header("Authorization") token: String,
        @Path("employeeId") employeeId: String
    ): Response<List<TrainingAttemptModel>>

    // ── Assessment ──
    @GET("assessment/{moduleId}/questions")
    suspend fun getQuestions(
        @Header("Authorization") token: String,
        @Path("moduleId") moduleId: String
    ): Response<List<AssessmentQuestion>>

    @POST("assessment/submit")
    suspend fun submitAssessment(
        @Header("Authorization") token: String,
        @Body request: AssessmentSubmitRequest
    ): Response<AssessmentSubmitResponse>

    // ── Certificate ──
    @POST("certificate/generate")
    suspend fun generateCertificate(
        @Header("Authorization") token: String,
        @Body request: CertificateGenerateRequest
    ): Response<CertificateGenerateResponse>

    @GET("certificate/{id}")
    suspend fun getCertificate(
        @Header("Authorization") token: String,
        @Path("id") certificateId: String
    ): Response<CertificateData>

    @GET("certificate/verify/{qrPayload}")
    suspend fun verifyCertificate(
        @Path("qrPayload") qrPayload: String
    ): Response<CertificateVerifyResponse>

    // ── Manager APIs ──
    @GET("manager/workers")
    suspend fun getMyWorkers(
        @Header("Authorization") token: String
    ): Response<List<WorkerInfo>>

    @POST("manager/worker/add")
    suspend fun addWorker(
        @Header("Authorization") token: String,
        @Body request: AddWorkerRequest
    ): Response<GenericMessageResponse>

    @POST("manager/worker/remove")
    suspend fun removeManagerWorker(
        @Header("Authorization") token: String,
        @Body request: RemoveUserRequest
    ): Response<GenericMessageResponse>

    @GET("manager/stats")
    suspend fun getManagerStats(
        @Header("Authorization") token: String
    ): Response<ManagerStatsResponse>

    @POST("manager/scenario/assign")
    suspend fun assignScenario(
        @Header("Authorization") token: String,
        @Body request: Any
    ): Response<GenericMessageResponse>

    @GET("manager/scenarios/{workerId}")
    suspend fun getWorkerScenarios(
        @Header("Authorization") token: String,
        @Path("workerId") workerId: String
    ): Response<List<Any>>

    @GET("manager/escalations")
    suspend fun getManagerEscalations(
        @Header("Authorization") token: String
    ): Response<List<Any>>

    // ── Admin APIs ──
    @GET("admin/workers")
    suspend fun getWorkers(
        @Header("Authorization") token: String
    ): Response<List<WorkerInfo>>

    @GET("admin/managers")
    suspend fun getManagers(
        @Header("Authorization") token: String
    ): Response<List<ManagerInfo>>

    @GET("admin/admins")
    suspend fun getAdmins(
        @Header("Authorization") token: String
    ): Response<List<AdminInfo>>

    @POST("admin/manager/add")
    suspend fun addManager(
        @Header("Authorization") token: String,
        @Body request: AddManagerRequest
    ): Response<GenericMessageResponse>

    @POST("admin/manager/remove")
    suspend fun removeManager(
        @Header("Authorization") token: String,
        @Body request: RemoveUserRequest
    ): Response<GenericMessageResponse>

    @POST("admin/worker/confirm")
    suspend fun confirmWorker(
        @Header("Authorization") token: String,
        @Body request: ConfirmWorkerRequest
    ): Response<GenericMessageResponse>

    @POST("admin/worker/remove")
    suspend fun removeWorker(
        @Header("Authorization") token: String,
        @Body request: RemoveUserRequest
    ): Response<GenericMessageResponse>

    @GET("admin/workers/pending")
    suspend fun getPendingWorkers(
        @Header("Authorization") token: String
    ): Response<List<WorkerInfo>>

    @GET("admin/escalations")
    suspend fun getAdminEscalations(
        @Header("Authorization") token: String
    ): Response<List<Any>>

    @GET("admin/sites")
    suspend fun getAdminSites(
        @Header("Authorization") token: String
    ): Response<List<SiteModel>>

    @POST("admin/site/map")
    suspend fun saveSite(
        @Header("Authorization") token: String,
        @Body request: SaveSiteRequest
    ): Response<GenericMessageResponse>

    @POST("admin/escalation/assign")
    suspend fun assignEscalation(
        @Header("Authorization") token: String,
        @Body request: AssignEscalationRequest
    ): Response<GenericMessageResponse>

    @POST("admin/escalation/resolve")
    suspend fun resolveEscalation(
        @Header("Authorization") token: String,
        @Body request: ResolveEscalationRequest
    ): Response<GenericMessageResponse>

    @GET("auth/user/{userId}")
    suspend fun getUserProfile(
        @Header("Authorization") token: String,
        @Path("userId") userId: String
    ): Response<AuthResponse>

    // ── Offline / Sync ──
    @GET("worker/offline/{workerId}")
    suspend fun getOfflineData(
        @Header("Authorization") token: String,
        @Path("workerId") workerId: String
    ): Response<Map<String, Any>>

    @POST("worker/online/sync")
    suspend fun syncOnline(
        @Header("Authorization") token: String,
        @Body request: Map<String, Any>
    ): Response<Map<String, Any>>

    // ── Compliance ──
    @GET("admin/compliance")
    suspend fun getCompliance(
        @Header("Authorization") token: String
    ): Response<Map<String, Any>>

    @GET("admin/certificates")
    suspend fun getAdminCertificates(
        @Header("Authorization") token: String
    ): Response<List<Map<String, Any>>>

    @GET("admin/attempts")
    suspend fun getAdminAttempts(
        @Header("Authorization") token: String
    ): Response<List<Map<String, Any>>>

    @GET("training/modules")
    suspend fun getModuleCount(
        @Header("Authorization") token: String
    ): Response<List<TrainingModule>>
}

data class GenericMessageResponse(
    val message: String,
    val score: Int? = null
)

data class CertificateVerifyResponse(
    val valid: Boolean,
    val certificateId: String,
    val employeeName: String,
    val moduleName: String,
    val score: Int,
    val issuedAt: String
)
