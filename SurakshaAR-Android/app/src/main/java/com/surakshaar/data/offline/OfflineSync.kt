package com.surakshaar.data.offline

import android.util.Log
import com.surakshaar.data.api.ApiClient
import com.surakshaar.data.repository.SessionManager
import com.google.gson.Gson
import com.surakshaar.data.model.AssessmentSubmitRequest
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

object OfflineSync {

    private const val TAG = "OfflineSync"
    private val gson = Gson()

    suspend fun flushAll(): Boolean = withContext(Dispatchers.IO) {
        if (!SessionManager.isAuthenticated) return@withContext false
        var anySuccess = false

        try {
            val assessments = OfflineQueue.pendingAssessments()
            for (req in assessments) {
                try {
                    val res = ApiClient.api.submitAssessment(SessionManager.authHeader, req)
                    if (res.isSuccessful) {
                        OfflineQueue.removeAssessment(req)
                        anySuccess = true
                    }
                } catch (e: Exception) {
                    Log.w(TAG, "Assessment flush failed", e)
                    break
                }
            }
        } catch (e: Exception) {
            Log.w(TAG, "Assessment flush error", e)
        }

        try {
            val scenarios = OfflineQueue.pendingScenarioJsons()
            val escalations = OfflineQueue.pendingEscalationJsons()
            val attempts = OfflineQueue.pendingAttemptJsons()
            if (scenarios.isNotEmpty() || escalations.isNotEmpty() || attempts.isNotEmpty()) {
                val body = mutableMapOf<String, Any>(
                    "completedScenarios" to scenarios.map {
                        parseMap(it)
                    },
                    "escalations" to escalations.map {
                        parseMap(it)
                    },
                    "trainingAttempts" to attempts.map {
                        parseMap(it)
                    }
                )
                val res = ApiClient.api.syncOnline(SessionManager.authHeader, body)
                if (res.isSuccessful) {
                    if (scenarios.isNotEmpty()) OfflineQueue.clearScenarios()
                    if (escalations.isNotEmpty()) OfflineQueue.clearEscalations()
                    if (attempts.isNotEmpty()) OfflineQueue.clearAttempts()
                    anySuccess = true
                }
            }
        } catch (e: Exception) {
            Log.w(TAG, "Scenario/escalation/attempt flush failed", e)
        }

        anySuccess
    }

    private fun parseMap(json: String): Map<String, Any> {
        return try {
            @Suppress("UNCHECKED_CAST")
            gson.fromJson(json, Map::class.java) as Map<String, Any>
        } catch (e: Exception) {
            emptyMap()
        }
    }

    suspend fun prefetchOfflineData(): Boolean = withContext(Dispatchers.IO) {
        if (!SessionManager.isAuthenticated) return@withContext false
        try {
            val res = ApiClient.api.getOfflineData(SessionManager.authHeader, SessionManager.userId)
            if (res.isSuccessful) {
                val body = res.body()
                if (body != null) {
                    SessionManager.prefs.edit()
                        .putString("offline_data", gson.toJson(body))
                        .putLong("offline_synced_at", System.currentTimeMillis())
                        .apply()
                    return@withContext true
                }
            }
        } catch (e: Exception) {
            Log.w(TAG, "Offline prefetch failed", e)
        }
        false
    }

    fun cachedOfflineData(): String? {
        return SessionManager.prefs.getString("offline_data", null)
    }

    fun lastSyncAt(): Long {
        return SessionManager.prefs.getLong("offline_synced_at", 0L)
    }
}
