package com.surakshaar.data.offline

import com.google.gson.Gson
import com.surakshaar.data.model.AssessmentSubmitRequest
import com.surakshaar.data.repository.SessionManager
import org.json.JSONArray
import org.json.JSONObject

object OfflineQueue {

    private const val KEY_ASSESSMENTS = "pending_assessments"
    private const val KEY_SCENARIOS = "pending_scenarios"
    private const val KEY_ESCALATIONS = "pending_escalations"
    private const val KEY_ATTEMPTS = "pending_attempts"
    private const val MAX_QUEUE = 50
    private val gson = Gson()

    fun enqueueAssessment(request: AssessmentSubmitRequest) {
        val arr = readArray(KEY_ASSESSMENTS)
        arr.put(JSONObject(gson.toJson(request)))
        while (arr.length() > MAX_QUEUE) arr.remove(0)
        writeArray(KEY_ASSESSMENTS, arr)
    }

    fun pendingAssessments(): List<AssessmentSubmitRequest> {
        val arr = readArray(KEY_ASSESSMENTS)
        val out = mutableListOf<AssessmentSubmitRequest>()
        for (i in 0 until arr.length()) {
            try {
                out.add(gson.fromJson(arr.getJSONObject(i).toString(), AssessmentSubmitRequest::class.java))
            } catch (_: Exception) {
            }
        }
        return out
    }

    fun removeAssessment(request: AssessmentSubmitRequest) {
        val arr = readArray(KEY_ASSESSMENTS)
        val keep = JSONArray()
        val key = gson.toJson(request)
        for (i in 0 until arr.length()) {
            if (arr.getJSONObject(i).toString() != key) keep.put(arr.getJSONObject(i))
        }
        writeArray(KEY_ASSESSMENTS, keep)
    }

    fun clearAssessments() {
        writeArray(KEY_ASSESSMENTS, JSONArray())
    }

    fun enqueueScenario(scenarioJson: String) {
        val arr = readArray(KEY_SCENARIOS)
        arr.put(JSONObject(scenarioJson))
        while (arr.length() > MAX_QUEUE) arr.remove(0)
        writeArray(KEY_SCENARIOS, arr)
    }

    fun pendingScenarioJsons(): List<String> {
        val arr = readArray(KEY_SCENARIOS)
        val out = mutableListOf<String>()
        for (i in 0 until arr.length()) out.add(arr.getJSONObject(i).toString())
        return out
    }

    fun clearScenarios() {
        writeArray(KEY_SCENARIOS, JSONArray())
    }

    fun enqueueEscalation(escalationJson: String) {
        val arr = readArray(KEY_ESCALATIONS)
        arr.put(JSONObject(escalationJson))
        while (arr.length() > MAX_QUEUE) arr.remove(0)
        writeArray(KEY_ESCALATIONS, arr)
    }

    fun pendingEscalationJsons(): List<String> {
        val arr = readArray(KEY_ESCALATIONS)
        val out = mutableListOf<String>()
        for (i in 0 until arr.length()) out.add(arr.getJSONObject(i).toString())
        return out
    }

    fun clearEscalations() {
        writeArray(KEY_ESCALATIONS, JSONArray())
    }

    /** Upsert a pending training attempt keyed by attemptId (start and/or complete). */
    fun upsertAttempt(attemptJson: String) {
        val obj = try {
            JSONObject(attemptJson)
        } catch (_: Exception) {
            return
        }
        val id = obj.optString("AttemptId", obj.optString("attemptId", ""))
        if (id.isEmpty()) return

        val arr = readArray(KEY_ATTEMPTS)
        val keep = JSONArray()
        for (i in 0 until arr.length()) {
            val existing = arr.getJSONObject(i)
            val existingId = existing.optString("AttemptId", existing.optString("attemptId", ""))
            if (existingId != id) keep.put(existing)
        }

        val merged = try {
            val prior = findAttemptObject(keep, id)
            if (prior != null) mergeAttempt(prior, obj) else obj
        } catch (_: Exception) {
            obj
        }
        keep.put(merged)
        while (keep.length() > MAX_QUEUE) keep.remove(0)
        writeArray(KEY_ATTEMPTS, keep)
    }

    fun pendingAttemptJsons(): List<String> {
        val arr = readArray(KEY_ATTEMPTS)
        val out = mutableListOf<String>()
        for (i in 0 until arr.length()) out.add(arr.getJSONObject(i).toString())
        return out
    }

    fun clearAttempts() {
        writeArray(KEY_ATTEMPTS, JSONArray())
    }

    fun pendingCount(): Int {
        return readArray(KEY_ASSESSMENTS).length() +
            readArray(KEY_SCENARIOS).length() +
            readArray(KEY_ESCALATIONS).length() +
            readArray(KEY_ATTEMPTS).length()
    }

    private fun findAttemptObject(arr: JSONArray, attemptId: String): JSONObject? {
        for (i in 0 until arr.length()) {
            val o = arr.getJSONObject(i)
            val id = o.optString("AttemptId", o.optString("attemptId", ""))
            if (id == attemptId) return o
        }
        return null
    }

    private fun mergeAttempt(prior: JSONObject, next: JSONObject): JSONObject {
        val merged = JSONObject(prior.toString())
        val names = next.names() ?: return next
        for (i in 0 until names.length()) {
            val key = names.getString(i)
            if (next.has(key) && !next.isNull(key)) {
                val value = next.get(key)
                val isEmptyString = value is String && value.isEmpty()
                val isZeroScore = (key.equals("ActionScore", true) || key.equals("QuestionScore", true) ||
                    key.equals("TotalScore", true) || key.equals("EscalationLevel", true)) &&
                    (value as? Int ?: 0) == 0
                val isFalse = key.equals("Passed", true) && value is Boolean && !value
                val isBlankTime = (key.equals("EndTime", true)) && isEmptyString
                val isPendingStatus = key.equals("Status", true) && (value as? String) == "in_progress"
                if (isEmptyString || isZeroScore || isFalse || isBlankTime || isPendingStatus) continue
                merged.put(key, value)
            }
        }
        if (!merged.has("Status") || merged.optString("Status") == "in_progress") {
            if (next.optString("Status") == "completed") merged.put("Status", "completed")
        }
        return merged
    }

    private fun readArray(key: String): JSONArray {
        val raw = SessionManager.prefs.getString(key, "") ?: ""
        if (raw.isEmpty()) return JSONArray()
        return try {
            JSONArray(raw)
        } catch (_: Exception) {
            JSONArray()
        }
    }

    private fun writeArray(key: String, arr: JSONArray) {
        SessionManager.prefs.edit().putString(key, arr.toString()).apply()
    }
}
