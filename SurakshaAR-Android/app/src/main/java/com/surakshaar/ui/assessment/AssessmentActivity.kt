package com.surakshaar.ui.assessment

import android.content.Intent
import android.graphics.Color
import android.os.Bundle
import android.view.View
import android.widget.ProgressBar
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.core.content.ContextCompat
import androidx.lifecycle.lifecycleScope
import com.google.android.material.button.MaterialButton
import com.surakshaar.R
import com.surakshaar.data.api.ApiClient
import com.surakshaar.data.model.*
import com.surakshaar.data.repository.SessionManager
import com.surakshaar.data.repository.TrainingDataStore
import com.surakshaar.ui.certificate.CertificateActivity
import com.surakshaar.ui.dashboard.DashboardActivity
import com.surakshaar.util.ThemeManager
import com.surakshaar.util.VoiceManager
import com.surakshaar.language.LanguageManager
import kotlinx.coroutines.launch

class AssessmentActivity : AppCompatActivity() {

    private lateinit var questionPanel: View
    private lateinit var resultPanel: View
    private lateinit var questionNumberText: TextView
    private lateinit var questionProgressBar: ProgressBar
    private lateinit var questionText: TextView
    private lateinit var optionA: MaterialButton
    private lateinit var optionB: MaterialButton
    private lateinit var optionC: MaterialButton
    private lateinit var optionD: MaterialButton
    private lateinit var nextButton: MaterialButton
    private lateinit var resultTitleText: TextView
    private lateinit var actionScoreText: TextView
    private lateinit var questionScoreText: TextView
    private lateinit var totalScoreText: TextView
    private lateinit var resultMessageText: TextView
    private lateinit var viewCertificateButton: MaterialButton
    private lateinit var retryButton: MaterialButton
    private lateinit var homeButton: MaterialButton

    private lateinit var questions: List<AssessmentQuestion>
    private var currentQuestionIndex = 0
    private var questionScore = 0
    private var actionScore = 0
    private var moduleId = ""
    private var attemptId = ""
    private var answered = false
    private val selectedAnswers = mutableListOf<Int>()
    private var lastTotalScore = 0
    private var lastPassed = false

    private val optionButtons by lazy { listOf(optionA, optionB, optionC, optionD) }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        try {
            ThemeManager.applyTheme(this, SessionManager.theme)
            setContentView(R.layout.activity_assessment)
            VoiceManager.attachToActivity(this)

            moduleId = intent.getStringExtra("moduleId") ?: ""
            attemptId = intent.getStringExtra("attemptId") ?: ""
            actionScore = intent.getIntExtra("actionScore", 0)

            initViews()
            applyTranslations()
            loadQuestions()
        } catch (e: Exception) {
            Toast.makeText(
                this,
                LanguageManager.get("assess_error") + ": ${e.message}",
                Toast.LENGTH_LONG
            ).show()
            finish()
        }
    }

    private fun initViews() {
        questionPanel = findViewById(R.id.questionPanel)
        resultPanel = findViewById(R.id.resultPanel)
        questionNumberText = findViewById(R.id.questionNumberText)
        questionProgressBar = findViewById(R.id.questionProgressBar)
        questionText = findViewById(R.id.questionText)
        optionA = findViewById(R.id.optionA)
        optionB = findViewById(R.id.optionB)
        optionC = findViewById(R.id.optionC)
        optionD = findViewById(R.id.optionD)
        nextButton = findViewById(R.id.nextButton)
        resultTitleText = findViewById(R.id.resultTitleText)
        actionScoreText = findViewById(R.id.actionScoreText)
        questionScoreText = findViewById(R.id.questionScoreText)
        totalScoreText = findViewById(R.id.totalScoreText)
        resultMessageText = findViewById(R.id.resultMessageText)
        viewCertificateButton = findViewById(R.id.viewCertificateButton)
        retryButton = findViewById(R.id.retryButton)
        homeButton = findViewById(R.id.homeButton)

        nextButton.setOnClickListener { onNextClicked() }
        viewCertificateButton.setOnClickListener { onViewCertificateClicked() }
        retryButton.setOnClickListener { onRetryClicked() }
        homeButton.setOnClickListener { onHomeClicked() }
    }

    private fun applyTranslations() {
        try {
            val t = LanguageManager
            retryButton.text = t.get("retry")
            homeButton.text = t.get("home_title")
            viewCertificateButton.text = t.get("view_certificate")
            nextButton.text = t.get("next_button")
        } catch (e: Exception) {
        }
    }

    private fun loadQuestions() {
        if (SessionManager.isAuthenticated) {
            lifecycleScope.launch {
                try {
                    val response = ApiClient.api.getQuestions(SessionManager.authHeader, moduleId)
                    if (response.isSuccessful) {
                        val backendQuestions = response.body()
                        if (!backendQuestions.isNullOrEmpty()) {
                            // Preserve original module order so server-side answer grading matches indices.
                            questions = backendQuestions
                            displayQuestion()
                            return@launch
                        }
                    }
                } catch (e: Exception) {
                    // Fall through to local data
                }
                loadLocalQuestions()
            }
        } else {
            loadLocalQuestions()
        }
    }

    private fun loadLocalQuestions() {
        val module = TrainingDataStore.getModule(moduleId)
        // Keep original order for consistent scoring with server module data.
        questions = module?.questions?.take(5) ?: TrainingDataStore.getRandomQuestions(moduleId, 5)
        if (questions.isEmpty()) {
            Toast.makeText(this, LanguageManager.get("no_questions"), Toast.LENGTH_SHORT).show()
            finish()
        } else {
            displayQuestion()
        }
    }

    private fun displayQuestion() {
        if (currentQuestionIndex >= questions.size) {
            showResults()
            return
        }

        answered = false
        nextButton.isEnabled = false
        nextButton.text = if (currentQuestionIndex == questions.size - 1) {
            LanguageManager.get("finish")
        } else {
            LanguageManager.get("next_button")
        }

        val question = questions[currentQuestionIndex]
        questionNumberText.text = try {
            String.format(
                LanguageManager.get("question_of"),
                currentQuestionIndex + 1,
                questions.size
            )
        } catch (e: Exception) {
            "Question ${currentQuestionIndex + 1} of ${questions.size}"
        }
        questionText.text = question.questionText

        val progress = ((currentQuestionIndex + 1) * 100) / questions.size
        questionProgressBar.progress = progress

        val labels = listOf("A", "B", "C", "D")
        question.options.forEachIndexed { index, optionText ->
            optionButtons[index].text = "${labels[index]}. $optionText"
            resetOptionStyle(optionButtons[index])
            optionButtons[index].isEnabled = true
            optionButtons[index].setOnClickListener { onOptionSelected(index) }
        }

        if (VoiceManager.isEnabled()) {
            VoiceManager.speak(
                questionNumberText.text.toString() + ". " + question.questionText,
                force = true
            )
        }
    }

    private fun onOptionSelected(selectedIndex: Int) {
        if (answered) return
        answered = true

        val question = questions[currentQuestionIndex]
        val correctIndex = question.correctAnswerIndex
        val hasAnswerKey = correctIndex != null && correctIndex in question.options.indices
        val isCorrect = hasAnswerKey && selectedIndex == correctIndex

        if (isCorrect) {
            questionScore += question.scoreValue
        }

        selectedAnswers.add(selectedIndex)

        optionButtons.forEachIndexed { index, button ->
            button.isEnabled = false
            when {
                hasAnswerKey && index == correctIndex -> {
                    setOptionStyle(button, ContextCompat.getColor(this, R.color.success), ContextCompat.getColor(this, R.color.success))
                }
                hasAnswerKey && index == selectedIndex && !isCorrect -> {
                    setOptionStyle(button, ContextCompat.getColor(this, R.color.danger), ContextCompat.getColor(this, R.color.danger))
                }
                index == selectedIndex -> {
                    setOptionStyle(button, ContextCompat.getColor(this, R.color.primary), ContextCompat.getColor(this, R.color.primary))
                }
                else -> {
                    setOptionStyle(button, ContextCompat.getColor(this, R.color.border), ContextCompat.getColor(this, R.color.text_muted))
                }
            }
        }

        nextButton.isEnabled = true
        if (VoiceManager.isEnabled()) {
            VoiceManager.speak(
                if (isCorrect) LanguageManager.get("correct_answer")
                else LanguageManager.get("wrong_answer"),
                force = true
            )
        }
    }

    private fun resetOptionStyle(button: MaterialButton) {
        button.setBackgroundColor(Color.TRANSPARENT)
        button.setTextColor(ContextCompat.getColor(this, R.color.white))
        button.strokeColor = ContextCompat.getColorStateList(this, R.color.border)
    }

    private fun setOptionStyle(button: MaterialButton, strokeColor: Int, textColor: Int) {
        button.strokeColor = ContextCompat.getColorStateList(this, R.color.border)
        button.setBackgroundColor(strokeColor)
        button.alpha = 0.2f
        button.setTextColor(textColor)
    }

    private fun onNextClicked() {
        currentQuestionIndex++
        if (currentQuestionIndex >= questions.size) {
            showResults()
        } else {
            displayQuestion()
        }
    }

    private fun computeLocalResult(): Pair<Int, Boolean> {
        // Align with server: weighted question percent (truncated), then 50/50 average.
        val questionPercent = questionPercentScore()
        val actionClamped = actionScore.coerceIn(0, 100)
        val totalScore = (actionClamped + questionPercent) / 2
        val threshold = passThreshold()
        return totalScore to (totalScore >= threshold)
    }

    private fun passThreshold(): Int {
        val fromModule = TrainingDataStore.getModule(moduleId)?.passThreshold
        return if (fromModule != null && fromModule > 0f) fromModule.toInt() else 70
    }

    private fun questionPercentScore(): Int {
        // Same formula as server AssessmentController: earned/max * 100, truncated.
        val maxQuestionPoints = questions.sumOf { it.scoreValue.coerceAtLeast(1) }.coerceAtLeast(1)
        return ((questionScore * 100) / maxQuestionPoints).coerceIn(0, 100)
    }

    private fun showResults() {
        questionPanel.visibility = View.GONE
        resultPanel.visibility = View.VISIBLE

        val (localTotal, localPassed) = computeLocalResult()
        lastTotalScore = localTotal
        lastPassed = localPassed
        renderResult(localTotal, localPassed, localQuestionScore = questionPercentScore())

        submitAssessment()
    }

    private fun renderResult(totalScore: Int, passed: Boolean, localQuestionScore: Int) {
        lastTotalScore = totalScore
        lastPassed = passed

        val threshold = passThreshold()
        resultTitleText.text = LanguageManager.get(if (passed) "passed" else "failed")
        resultTitleText.setTextColor(
            ContextCompat.getColor(this, if (passed) R.color.success else R.color.danger)
        )

        try {
            actionScoreText.text = LanguageManager.get("assess_actions") + " $actionScore"
            questionScoreText.text = LanguageManager.get("assess_questions") + " $localQuestionScore"
            totalScoreText.text = LanguageManager.get("assess_total") + " $totalScore"
        } catch (e: Exception) {
            actionScoreText.text = "Actions: $actionScore"
            questionScoreText.text = "Questions: $localQuestionScore"
            totalScoreText.text = "Total: $totalScore"
        }

        resultMessageText.text = when {
            totalScore >= 90 -> LanguageManager.get("assess_outstanding")
            totalScore >= threshold -> LanguageManager.get("assess_good")
            totalScore >= 50 -> LanguageManager.get("assess_improve")
            else -> LanguageManager.get("assess_failed")
        }

        viewCertificateButton.visibility = if (passed) View.VISIBLE else View.GONE

        if (VoiceManager.isEnabled()) {
            VoiceManager.speak(
                LanguageManager.get(if (passed) "passed" else "failed") + ". " + resultMessageText.text,
                force = true
            )
        }
    }

    private fun submitAssessment() {
        if (!SessionManager.isAuthenticated) return

        val localQuestionScore = questionPercentScore()
        val (localTotal, localPassed) = computeLocalResult()

        val request = AssessmentSubmitRequest(
            employeeId = SessionManager.userId,
            moduleId = moduleId,
            attemptId = attemptId,
            actionScore = actionScore,
            questionScore = localQuestionScore,
            totalScore = localTotal,
            passed = localPassed,
            answers = selectedAnswers
        )

        lifecycleScope.launch {
            try {
                val response = ApiClient.api.submitAssessment(
                    SessionManager.authHeader,
                    request
                )
                val body = response.body()
                if (response.isSuccessful && body != null) {
                    renderResult(body.totalScore, body.passed, localQuestionScore)
                } else {
                    com.surakshaar.data.offline.OfflineQueue.enqueueAssessment(request)
                    Toast.makeText(
                        this@AssessmentActivity,
                        LanguageManager.get("offline_queued"),
                        Toast.LENGTH_SHORT
                    ).show()
                }
            } catch (e: Exception) {
                com.surakshaar.data.offline.OfflineQueue.enqueueAssessment(request)
                Toast.makeText(
                    this@AssessmentActivity,
                    LanguageManager.get("offline_queued"),
                    Toast.LENGTH_SHORT
                ).show()
            }
        }
    }

    private fun onViewCertificateClicked() {
        val intent = Intent(this, CertificateActivity::class.java).apply {
            putExtra("moduleId", moduleId)
            putExtra("actionScore", actionScore)
            putExtra("totalScore", lastTotalScore)
            putExtra("passed", lastPassed)
        }
        startActivity(intent)
    }

    private fun onRetryClicked() {
        currentQuestionIndex = 0
        questionScore = 0
        selectedAnswers.clear()
        lastTotalScore = 0
        lastPassed = false
        resultPanel.visibility = View.GONE
        questionPanel.visibility = View.VISIBLE
        loadQuestions()
    }

    private fun onHomeClicked() {
        val intent = Intent(this, DashboardActivity::class.java).apply {
            flags = Intent.FLAG_ACTIVITY_CLEAR_TOP or Intent.FLAG_ACTIVITY_NEW_TASK
        }
        startActivity(intent)
        finish()
    }

    @Suppress("DEPRECATION")
    override fun onBackPressed() {
        super.onBackPressed()
        finish()
    }
}
