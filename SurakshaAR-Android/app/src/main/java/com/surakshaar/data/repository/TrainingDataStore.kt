package com.surakshaar.data.repository

import com.surakshaar.data.model.*

object TrainingDataStore {

    val modules: List<TrainingModule> = listOf(
        // ────────────────────────────────────────────────
        // MODULE 1 – Fire & Explosion Safety
        // ────────────────────────────────────────────────
        TrainingModule(
            moduleId = "fire-safety-101",
            title = "Fire & Explosion Safety",
            description = "Comprehensive training on identifying fire hazards, selecting extinguishers, and executing evacuation protocols in mining workshops.",
            safetyDomain = "Fire Safety",
            difficultyLevel = 1,
            timeLimitSeconds = 300f,
            passThreshold = 70f,
            requiredEquipment = listOf(
                "ABC Dry Chemical Extinguisher",
                "Fire Blanket",
                "Safety Goggles",
                "Heat-Resistant Gloves"
            ),
            scenarios = listOf(
                ScenarioConfig(
                    scenarioId = "fire-1",
                    moduleId = "fire-safety-101",
                    hazardType = "Workshop Fire",
                    scenarioName = "Workshop Fire Response",
                    initialDangerLevel = 0.1f,
                    escalationIntervalSeconds = 15f,
                    maxEscalationLevel = 4,
                    hazardZones = listOf(
                        HazardZone(
                            zoneId = "fire-zone-center",
                            zoneName = "Ignition Point",
                            riskColor = "#FF4500",
                            position = floatArrayOf(0f, 0f, 0f),
                            radius = 2.5f,
                            dangerLevel = 0.9f
                        ),
                        HazardZone(
                            zoneId = "fire-zone-perimeter",
                            zoneName = "Heat Perimeter",
                            riskColor = "#FF8C00",
                            position = floatArrayOf(0f, 0f, 0f),
                            radius = 5f,
                            dangerLevel = 0.5f
                        ),
                        HazardZone(
                            zoneId = "fire-zone-exit",
                            zoneName = "Exit Corridor",
                            riskColor = "#FFD700",
                            position = floatArrayOf(8f, 0f, 0f),
                            radius = 3f,
                            dangerLevel = 0.3f
                        )
                    ),
                    requiredActions = listOf(
                        RequiredAction(
                            actionId = "activate-alarm",
                            actionName = "Activate Alarm",
                            description = "Pull the nearest fire alarm lever to alert all personnel in the workshop.",
                            completionFeedback = "Fire alarm activated. All personnel are being notified.",
                            interactionRadius = 2f,
                            timeLimitSeconds = 15f,
                            scoreValue = 20,
                            isCritical = true
                        ),
                        RequiredAction(
                            actionId = "use-extinguisher",
                            actionName = "Use Extinguisher",
                            description = "Select the correct ABC extinguisher and discharge it at the base of the fire.",
                            completionFeedback = "Extinguisher discharged. Fire size reduced. Watch for reignition.",
                            interactionRadius = 3f,
                            timeLimitSeconds = 30f,
                            scoreValue = 30,
                            isCritical = true
                        ),
                        RequiredAction(
                            actionId = "evacuate",
                            actionName = "Evacuate",
                            description = "Lead all personnel through the nearest safe exit to the assembly point.",
                            completionFeedback = "All personnel evacuated safely to the muster point.",
                            interactionRadius = 4f,
                            timeLimitSeconds = 45f,
                            scoreValue = 50,
                            isCritical = true
                        )
                    ),
                    escalationEvents = listOf(
                        EscalationEvent(
                            level = 0,
                            levelName = "Normal",
                            timeThreshold = 0f,
                            scorePenalty = 0f,
                            visualEffect = "none",
                            audioEffect = "ambient-machinery",
                            warningText = "All clear. Proceed with scenario.",
                            showEvacuationArrows = false,
                            triggerAutoFail = false,
                            hazardGrowthMultiplier = 1f
                        ),
                        EscalationEvent(
                            level = 1,
                            levelName = "Minor",
                            timeThreshold = 15f,
                            scorePenalty = 0.10f,
                            visualEffect = "light-smoke",
                            audioEffect = "distant-alarm",
                            warningText = "Smoke detected. Investigate the source immediately.",
                            showEvacuationArrows = false,
                            triggerAutoFail = false,
                            hazardGrowthMultiplier = 1.2f
                        ),
                        EscalationEvent(
                            level = 2,
                            levelName = "Moderate",
                            timeThreshold = 30f,
                            scorePenalty = 0.25f,
                            visualEffect = "heavy-smoke-sparks",
                            audioEffect = "crackling-fire",
                            warningText = "Fire is spreading. Use an extinguisher and prepare to evacuate.",
                            showEvacuationArrows = true,
                            triggerAutoFail = false,
                            hazardGrowthMultiplier = 1.5f
                        ),
                        EscalationEvent(
                            level = 3,
                            levelName = "Severe",
                            timeThreshold = 45f,
                            scorePenalty = 0.50f,
                            visualEffect = "full-flames-heat-distortion",
                            audioEffect = "roaring-fire-beams-groaning",
                            warningText = "Danger! Fire has spread to fuel storage. Evacuate now!",
                            showEvacuationArrows = true,
                            triggerAutoFail = false,
                            hazardGrowthMultiplier = 2f
                        ),
                        EscalationEvent(
                            level = 4,
                            levelName = "Critical",
                            timeThreshold = 60f,
                            scorePenalty = 1f,
                            visualEffect = "explosion-screen-shake-blackout",
                            audioEffect = "explosion-siren",
                            warningText = "CRITICAL: Explosion risk. Immediate evacuation failed. Scenario failed.",
                            showEvacuationArrows = true,
                            triggerAutoFail = true,
                            hazardGrowthMultiplier = 3f
                        )
                    )
                )
            ),
            questions = listOf(
                AssessmentQuestion(
                    questionId = "fire-q1",
                    moduleId = "fire-safety-101",
                    questionText = "What is the first action you should take when a fire is detected in a mining workshop?",
                    options = listOf(
                        "Attempt to extinguish the fire immediately",
                        "Activate the nearest fire alarm",
                        "Call the mine control room",
                        "Open all windows for ventilation"
                    ),
                    correctAnswerIndex = 1,
                    scoreValue = 20,
                    explanation = "The first priority is to alert all personnel by activating the fire alarm. This ensures everyone is aware of the hazard before any suppression attempt.",
                    difficulty = "easy"
                ),
                AssessmentQuestion(
                    questionId = "fire-q2",
                    moduleId = "fire-safety-101",
                    questionText = "Which extinguisher type is most appropriate for a fire involving electrical equipment in a workshop?",
                    options = listOf(
                        "Water-type extinguisher",
                        "Foam-type extinguisher",
                        "CO₂ or dry chemical (ABC) extinguisher",
                        "Sand bucket only"
                    ),
                    correctAnswerIndex = 2,
                    scoreValue = 20,
                    explanation = "CO₂ or ABC dry chemical extinguishers are non-conductive and safe for electrical fires. Water and foam can conduct electricity and cause electrocution.",
                    difficulty = "medium"
                ),
                AssessmentQuestion(
                    questionId = "fire-q3",
                    moduleId = "fire-safety-101",
                    questionText = "What does the 'PASS' acronym stand for when using a fire extinguisher?",
                    options = listOf(
                        "Pull, Aim, Squeeze, Sweep",
                        "Position, Activate, Secure, Suppress",
                        "Prepare, Aim, Spray, Stop",
                        "Pull, Alert, Spray, Safety"
                    ),
                    correctAnswerIndex = 0,
                    scoreValue = 20,
                    explanation = "PASS: Pull the pin, Aim at the base of the fire, Squeeze the handle, and Sweep side to side. This is the standard technique for extinguisher use.",
                    difficulty = "easy"
                ),
                AssessmentQuestion(
                    questionId = "fire-q4",
                    moduleId = "fire-safety-101",
                    questionText = "At what escalation level should evacuation become the absolute priority over fire suppression?",
                    options = listOf(
                        "Normal (level 0)",
                        "Minor (level 1)",
                        "Moderate (level 2)",
                        "Severe (level 3) or when instructed"
                    ),
                    correctAnswerIndex = 3,
                    scoreValue = 20,
                    explanation = "At severe escalation or when ordered, evacuation becomes the top priority. No score is worth risking lives when the fire is out of control.",
                    difficulty = "medium"
                ),
                AssessmentQuestion(
                    questionId = "fire-q5",
                    moduleId = "fire-safety-101",
                    questionText = "After evacuating from a fire incident, where should all personnel assemble?",
                    options = listOf(
                        "Inside the nearest vehicle",
                        "At the designated muster point outside the hazard zone",
                        "At the workshop entrance",
                        "In the locker room"
                    ),
                    correctAnswerIndex = 1,
                    scoreValue = 20,
                    explanation = "The muster point is a pre-designated safe location outside all hazard zones where headcounts are taken to confirm everyone is accounted for.",
                    difficulty = "easy"
                )
            )
        ),

        // ────────────────────────────────────────────────
        // MODULE 2 – Gas Leak & Confined Space
        // ────────────────────────────────────────────────
        TrainingModule(
            moduleId = "gas-leak-101",
            title = "Gas Leak & Confined Space",
            description = "Training on detecting methane and other hazardous gases, selecting appropriate PPE, activating ventilation, and safe evacuation from confined spaces.",
            safetyDomain = "Gas Safety",
            difficultyLevel = 2,
            timeLimitSeconds = 300f,
            passThreshold = 75f,
            requiredEquipment = listOf(
                "4-Gas Detector (CH₄, CO, H₂S, O₂)",
                "Self-Contained Breathing Apparatus (SCBA)",
                "Chemical-Resistant PPE Suit",
                "Ventilation Fan"
            ),
            scenarios = listOf(
                ScenarioConfig(
                    scenarioId = "gas-1",
                    moduleId = "gas-leak-101",
                    hazardType = "Methane Leak",
                    scenarioName = "Methane Detection Response",
                    initialDangerLevel = 0.15f,
                    escalationIntervalSeconds = 15f,
                    maxEscalationLevel = 4,
                    hazardZones = listOf(
                        HazardZone(
                            zoneId = "gas-zone-source",
                            zoneName = "Leak Source",
                            riskColor = "#FF0000",
                            position = floatArrayOf(0f, 0f, 0f),
                            radius = 2f,
                            dangerLevel = 0.95f
                        ),
                        HazardZone(
                            zoneId = "gas-zone-spread",
                            zoneName = "Gas Dispersion Area",
                            riskColor = "#FF6347",
                            position = floatArrayOf(0f, 1f, 0f),
                            radius = 4f,
                            dangerLevel = 0.6f
                        ),
                        HazardZone(
                            zoneId = "gas-zone-ventilation",
                            zoneName = "Ventilation Intake",
                            riskColor = "#32CD32",
                            position = floatArrayOf(6f, 0f, 0f),
                            radius = 2.5f,
                            dangerLevel = 0.2f
                        )
                    ),
                    requiredActions = listOf(
                        RequiredAction(
                            actionId = "detect-gas",
                            actionName = "Detect Gas",
                            description = "Use the 4-gas detector to identify methane concentration and confirm the gas type.",
                            completionFeedback = "Methane detected at 2.1% LEL. Gas type confirmed. Proceed with response.",
                            interactionRadius = 3f,
                            timeLimitSeconds = 10f,
                            scoreValue = 15,
                            isCritical = true
                        ),
                        RequiredAction(
                            actionId = "select-ppe",
                            actionName = "Select PPE",
                            description = "Don the SCBA and chemical-resistant suit before entering the contaminated zone.",
                            completionFeedback = "PPE equipped. You are now protected against methane exposure.",
                            interactionRadius = 2f,
                            timeLimitSeconds = 20f,
                            scoreValue = 20,
                            isCritical = true
                        ),
                        RequiredAction(
                            actionId = "activate-ventilation",
                            actionName = "Activate Ventilation",
                            description = "Switch on the ventilation fan to disperse the gas cloud and reduce concentration below dangerous levels.",
                            completionFeedback = "Ventilation active. Methane concentration is dropping.",
                            interactionRadius = 3f,
                            timeLimitSeconds = 30f,
                            scoreValue = 25,
                            isCritical = true
                        ),
                        RequiredAction(
                            actionId = "evacuate",
                            actionName = "Evacuate",
                            description = "Guide all workers out of the confined space to the fresh air assembly point.",
                            completionFeedback = "All personnel evacuated. Confined space cleared.",
                            interactionRadius = 4f,
                            timeLimitSeconds = 45f,
                            scoreValue = 40,
                            isCritical = true
                        )
                    ),
                    escalationEvents = listOf(
                        EscalationEvent(
                            level = 0,
                            levelName = "Normal",
                            timeThreshold = 0f,
                            scorePenalty = 0f,
                            visualEffect = "none",
                            audioEffect = "ambient-underground",
                            warningText = "Baseline readings normal. Begin scenario.",
                            showEvacuationArrows = false,
                            triggerAutoFail = false,
                            hazardGrowthMultiplier = 1f
                        ),
                        EscalationEvent(
                            level = 1,
                            levelName = "Minor",
                            timeThreshold = 15f,
                            scorePenalty = 0.10f,
                            visualEffect = "slight-vapour-haze",
                            audioEffect = "gas-hiss-alarm-beep",
                            warningText = "Gas detector shows rising methane. Locate the source.",
                            showEvacuationArrows = false,
                            triggerAutoFail = false,
                            hazardGrowthMultiplier = 1.3f
                        ),
                        EscalationEvent(
                            level = 2,
                            levelName = "Moderate",
                            timeThreshold = 30f,
                            scorePenalty = 0.25f,
                            visualEffect = "dense-vapour-limited-visibility",
                            audioEffect = "continuous-alarm-gas-hiss-loud",
                            warningText = "Methane above 5% LEL. Don PPE and activate ventilation immediately.",
                            showEvacuationArrows = true,
                            triggerAutoFail = false,
                            hazardGrowthMultiplier = 1.6f
                        ),
                        EscalationEvent(
                            level = 3,
                            levelName = "Severe",
                            timeThreshold = 45f,
                            scorePenalty = 0.50f,
                            visualEffect = "thick-gas-screen-edges-red-tint",
                            audioEffect = "alarms-siren-rumbling",
                            warningText = "WARNING: Methane approaching explosive limit. Evacuate immediately!",
                            showEvacuationArrows = true,
                            triggerAutoFail = false,
                            hazardGrowthMultiplier = 2.2f
                        ),
                        EscalationEvent(
                            level = 4,
                            levelName = "Critical",
                            timeThreshold = 60f,
                            scorePenalty = 1f,
                            visualEffect = "explosion-flash-screen-shake-blackout",
                            audioEffect = "explosion-siren-echo",
                            warningText = "CRITICAL: Methane reached explosive limit. Detonation occurred. Scenario failed.",
                            showEvacuationArrows = true,
                            triggerAutoFail = true,
                            hazardGrowthMultiplier = 3f
                        )
                    )
                )
            ),
            questions = listOf(
                AssessmentQuestion(
                    questionId = "gas-q1",
                    moduleId = "gas-leak-101",
                    questionText = "What is the Lower Explosive Limit (LEL) of methane, and why is it critical for safety assessments?",
                    options = listOf(
                        "5% – methane is explosive above this concentration in air",
                        "15% – methane becomes flammable above this level",
                        "25% – this is the minimum for detection",
                        "50% – methane is only dangerous above half saturation"
                    ),
                    correctAnswerIndex = 0,
                    scoreValue = 20,
                    explanation = "Methane has an LEL of approximately 5% in air. Below this, the gas concentration is too lean to ignite. Above the LEL, any ignition source can cause an explosion.",
                    difficulty = "medium"
                ),
                AssessmentQuestion(
                    questionId = "gas-q2",
                    moduleId = "gas-leak-101",
                    questionText = "Which PPE is essential before entering a confined space with a confirmed methane leak?",
                    options = listOf(
                        "Standard hard hat and safety boots only",
                        "Chemical-resistant suit with a self-contained breathing apparatus (SCBA)",
                        "Dust mask and safety goggles",
                        "Hi-vis vest and ear protection"
                    ),
                    correctAnswerIndex = 1,
                    scoreValue = 20,
                    explanation = "SCBA provides breathable air independent of the environment, and a chemical-resistant suit prevents skin contact with hazardous substances. Other PPE does not protect against gas inhalation.",
                    difficulty = "easy"
                ),
                AssessmentQuestion(
                    questionId = "gas-q3",
                    moduleId = "gas-leak-101",
                    questionText = "What is the primary purpose of activating forced ventilation during a gas leak response?",
                    options = listOf(
                        "To cool the area and reduce fire risk",
                        "To dilute and disperse the gas concentration below hazardous levels",
                        "To improve lighting for rescue teams",
                        "To create positive pressure for structural stability"
                    ),
                    correctAnswerIndex = 1,
                    scoreValue = 20,
                    explanation = "Ventilation fans force fresh air into the confined space, diluting the methane concentration and pushing contaminated air out, reducing the risk of reaching explosive limits.",
                    difficulty = "medium"
                ),
                AssessmentQuestion(
                    questionId = "gas-q4",
                    moduleId = "gas-leak-101",
                    questionText = "Why should you NEVER use a spark-producing device (phone, flashlight switch) in a methane-rich confined space?",
                    options = listOf(
                        "It may damage the equipment",
                        "Methane can ignite at concentrations above the LEL with any ignition source",
                        "It will interfere with the gas detector readings",
                        "It is only a concern for hydrogen gas, not methane"
                    ),
                    correctAnswerIndex = 1,
                    scoreValue = 20,
                    explanation = "Methane becomes explosive between 5–15% LEL. Any spark, static discharge, or open flame can trigger a detonation. Intrinsically safe equipment must be used in gas-hazardous zones.",
                    difficulty = "easy"
                ),
                AssessmentQuestion(
                    questionId = "gas-q5",
                    moduleId = "gas-leak-101",
                    questionText = "In a confined space methane emergency, what is the correct sequence of actions?",
                    options = listOf(
                        "Evacuate → Detect → PPE → Ventilate",
                        "Detect → Select PPE → Activate Ventilation → Evacuate",
                        "Activate Ventilation → Detect → Evacuate → PPE",
                        "Select PPE → Evacuate → Detect → Activate Ventilation"
                    ),
                    correctAnswerIndex = 1,
                    scoreValue = 20,
                    explanation = "The correct sequence is: first detect and confirm the gas, then don PPE for personal protection, activate ventilation to reduce concentration, and finally evacuate all personnel.",
                    difficulty = "medium"
                )
            )
        ),

        // ────────────────────────────────────────────────
        // MODULE 3 – Machinery LOTO
        // ────────────────────────────────────────────────
        TrainingModule(
            moduleId = "machinery-101",
            title = "Machinery Lockout/Tagout (LOTO)",
            description = "Training on lockout/tagout procedures for de-energizing and isolating machinery before maintenance.",
            safetyDomain = "Machinery Safety",
            difficultyLevel = 2,
            timeLimitSeconds = 300f,
            passThreshold = 70f,
            requiredEquipment = listOf(
                "Lockout Hasp",
                "Padlock",
                "LOTO Tag",
                "Safety Glasses"
            ),
            scenarios = listOf(
                ScenarioConfig(
                    scenarioId = "machinery-1",
                    moduleId = "machinery-101",
                    hazardType = "Running Machinery",
                    scenarioName = "Conveyor Belt Maintenance",
                    initialDangerLevel = 0.2f,
                    escalationIntervalSeconds = 30f,
                    maxEscalationLevel = 3,
                    hazardZones = listOf(
                        HazardZone(zoneId = "mach-zone-belt", zoneName = "Conveyor Belt", riskColor = "#FF4500", position = floatArrayOf(0f, 0f, 0f), radius = 3f, dangerLevel = 0.8f),
                        HazardZone(zoneId = "mach-zone-panel", zoneName = "Control Panel", riskColor = "#FF8C00", position = floatArrayOf(5f, 0f, 0f), radius = 2f, dangerLevel = 0.6f)
                    ),
                    requiredActions = listOf(
                        RequiredAction(actionId = "notify-supervisor", actionName = "Notify Supervisor", description = "Inform your supervisor before starting LOTO procedures.", completionFeedback = "Supervisor notified.", interactionRadius = 3f, timeLimitSeconds = 15f, scoreValue = 15, isCritical = true),
                        RequiredAction(actionId = "shut-down", actionName = "Shut Down Machinery", description = "Press the emergency stop button to shut down the conveyor.", completionFeedback = "Conveyor stopped.", interactionRadius = 2f, timeLimitSeconds = 20f, scoreValue = 20, isCritical = true),
                        RequiredAction(actionId = "apply-lockout", actionName = "Apply Lockout Device", description = "Attach lockout hasp and padlock to the energy isolation device.", completionFeedback = "Lockout device applied.", interactionRadius = 2f, timeLimitSeconds = 25f, scoreValue = 25, isCritical = true),
                        RequiredAction(actionId = "tag-out", actionName = "Tag Out Energy Source", description = "Place LOTO tag on the isolation device with your name and date.", completionFeedback = "Energy source tagged out.", interactionRadius = 2f, timeLimitSeconds = 15f, scoreValue = 20, isCritical = true),
                        RequiredAction(actionId = "verify-zero", actionName = "Verify Zero Energy", description = "Attempt to restart the machine to verify zero energy state.", completionFeedback = "Zero energy state verified.", interactionRadius = 2f, timeLimitSeconds = 20f, scoreValue = 20, isCritical = true)
                    ),
                    escalationEvents = listOf(
                        EscalationEvent(level = 0, levelName = "Normal", timeThreshold = 0f, scorePenalty = 0f, visualEffect = "none", audioEffect = "ambient-machinery", warningText = "All clear. Proceed with LOTO.", showEvacuationArrows = false, triggerAutoFail = false, hazardGrowthMultiplier = 1f),
                        EscalationEvent(level = 1, levelName = "Warning", timeThreshold = 30f, scorePenalty = 0.15f, visualEffect = "warning-flash", audioEffect = "warning-beep", warningText = "Equipment is still energized! Shut down immediately.", showEvacuationArrows = false, triggerAutoFail = false, hazardGrowthMultiplier = 1.3f),
                        EscalationEvent(level = 2, levelName = "Violation", timeThreshold = 60f, scorePenalty = 0.3f, visualEffect = "red-overlay", audioEffect = "alarm-siren", warningText = "LOTO violation detected! Production must stop.", showEvacuationArrows = true, triggerAutoFail = false, hazardGrowthMultiplier = 1.8f),
                        EscalationEvent(level = 3, levelName = "Critical", timeThreshold = 90f, scorePenalty = 1f, visualEffect = "critical-shake", audioEffect = "critical-siren", warningText = "CRITICAL: Severe safety violation! All work must cease.", showEvacuationArrows = true, triggerAutoFail = true, hazardGrowthMultiplier = 3f)
                    )
                )
            ),
            questions = listOf(
                AssessmentQuestion(questionId = "machinery-q1", moduleId = "machinery-101", questionText = "What is the FIRST step before performing maintenance on machinery?", options = listOf("Start maintenance immediately", "Notify supervisor and shut down", "Apply lockout device", "Check tools"), correctAnswerIndex = 1, scoreValue = 20, explanation = "Always notify your supervisor and shut down the equipment before applying lockout/tagout devices.", difficulty = "easy"),
                AssessmentQuestion(questionId = "machinery-q2", moduleId = "machinery-101", questionText = "What must you verify after applying a lockout device?", options = listOf("Check the time", "Verify zero energy state", "Start the machine", "Nothing needed"), correctAnswerIndex = 1, scoreValue = 20, explanation = "After applying the lockout device, always verify that the equipment is in a zero energy state before proceeding.", difficulty = "medium"),
                AssessmentQuestion(questionId = "machinery-q3", moduleId = "machinery-101", questionText = "Who is authorized to remove a lockout device?", options = listOf("Any worker", "The person who applied it", "Only the supervisor", "Security team"), correctAnswerIndex = 1, scoreValue = 20, explanation = "Only the person who applied the lockout device is authorized to remove it, ensuring the worker's safety.", difficulty = "medium")
            )
        ),

        // ────────────────────────────────────────────────
        // MODULE 4 – Electrical Hazards
        // ────────────────────────────────────────────────
        TrainingModule(
            moduleId = "electrical-101",
            title = "Electrical Hazards Safety",
            description = "Training on identifying electrical hazards, proper PPE usage, and safe work practices around electrical equipment.",
            safetyDomain = "Electrical Safety",
            difficultyLevel = 2,
            timeLimitSeconds = 300f,
            passThreshold = 75f,
            requiredEquipment = listOf(
                "Insulated Gloves",
                "Safety Helmet",
                "Voltage Tester",
                "Insulated Tools"
            ),
            scenarios = listOf(
                ScenarioConfig(
                    scenarioId = "electrical-1",
                    moduleId = "electrical-101",
                    hazardType = "Live Electrical Panel",
                    scenarioName = "Electrical Panel Inspection",
                    initialDangerLevel = 0.3f,
                    escalationIntervalSeconds = 25f,
                    maxEscalationLevel = 3,
                    hazardZones = listOf(
                        HazardZone(zoneId = "elec-zone-panel", zoneName = "Electrical Panel", riskColor = "#FFD700", position = floatArrayOf(0f, 0f, 0f), radius = 2f, dangerLevel = 0.9f),
                        HazardZone(zoneId = "elec-zone-floor", zoneName = "Wet Floor Area", riskColor = "#00BFFF", position = floatArrayOf(3f, 0f, 0f), radius = 4f, dangerLevel = 0.7f)
                    ),
                    requiredActions = listOf(
                        RequiredAction(actionId = "identify-hazard", actionName = "Identify Electrical Hazard", description = "Look for exposed wires, damaged panels, and wet conditions near electrical equipment.", completionFeedback = "Electrical hazard identified.", interactionRadius = 3f, timeLimitSeconds = 15f, scoreValue = 15, isCritical = true),
                        RequiredAction(actionId = "don-ppe", actionName = "Don PPE", description = "Put on insulated gloves and safety helmet before approaching the panel.", completionFeedback = "PPE equipped.", interactionRadius = 2f, timeLimitSeconds = 20f, scoreValue = 20, isCritical = true),
                        RequiredAction(actionId = "use-tester", actionName = "Use Voltage Tester", description = "Use a non-contact voltage tester to verify the panel is live.", completionFeedback = "Voltage confirmed.", interactionRadius = 2f, timeLimitSeconds = 20f, scoreValue = 25, isCritical = true),
                        RequiredAction(actionId = "lock-panel", actionName = "Lock Out Panel", description = "Apply lockout device to the electrical panel breaker.", completionFeedback = "Panel locked out.", interactionRadius = 2f, timeLimitSeconds = 20f, scoreValue = 20, isCritical = true),
                        RequiredAction(actionId = "inspect", actionName = "Perform Inspection", description = "Conduct thorough inspection of the panel and wiring.", completionFeedback = "Inspection complete.", interactionRadius = 2f, timeLimitSeconds = 30f, scoreValue = 20, isCritical = false)
                    ),
                    escalationEvents = listOf(
                        EscalationEvent(level = 0, levelName = "Normal", timeThreshold = 0f, scorePenalty = 0f, visualEffect = "none", audioEffect = "ambient-hum", warningText = "All clear. Proceed with inspection.", showEvacuationArrows = false, triggerAutoFail = false, hazardGrowthMultiplier = 1f),
                        EscalationEvent(level = 1, levelName = "Warning", timeThreshold = 25f, scorePenalty = 0.2f, visualEffect = "spark-flash", audioEffect = "electrical-crackle", warningText = "Warning: Risk of electrocution! Put on insulated gloves.", showEvacuationArrows = false, triggerAutoFail = false, hazardGrowthMultiplier = 1.4f),
                        EscalationEvent(level = 2, levelName = "Danger", timeThreshold = 50f, scorePenalty = 0.4f, visualEffect = "electric-arc", audioEffect = "loud-crackle", warningText = "DANGER: Working near live panel without PPE!", showEvacuationArrows = true, triggerAutoFail = false, hazardGrowthMultiplier = 2f),
                        EscalationEvent(level = 3, levelName = "Critical", timeThreshold = 80f, scorePenalty = 1f, visualEffect = "explosion-blackout", audioEffect = "explosion-siren", warningText = "CRITICAL: Potential electrocution hazard! Evacuate area.", showEvacuationArrows = true, triggerAutoFail = true, hazardGrowthMultiplier = 3f)
                    )
                )
            ),
            questions = listOf(
                AssessmentQuestion(questionId = "electrical-q1", moduleId = "electrical-101", questionText = "What PPE is mandatory when working near electrical panels?", options = listOf("Regular cotton gloves", "Insulated rubber gloves and safety helmet", "Safety shoes only", "Hard hat only"), correctAnswerIndex = 1, scoreValue = 20, explanation = "Insulated rubber gloves and a safety helmet are mandatory PPE when working near electrical panels to prevent electrocution.", difficulty = "easy"),
                AssessmentQuestion(questionId = "electrical-q2", moduleId = "electrical-101", questionText = "What tool should you use to check if a panel is live?", options = listOf("Screwdriver", "Non-contact voltage tester", "Wire cutter", "Pliers"), correctAnswerIndex = 1, scoreValue = 20, explanation = "A non-contact voltage tester is the proper tool for checking if a panel or circuit is live. Never use improvised tools.", difficulty = "easy"),
                AssessmentQuestion(questionId = "electrical-q3", moduleId = "electrical-101", questionText = "What should you do if you see water near an electrical panel?", options = listOf("Ignore it", "Report immediately and avoid the area", "Mop it up while working", "Use a water hose to clean"), correctAnswerIndex = 1, scoreValue = 20, explanation = "Water near electrical equipment is extremely dangerous. Report it immediately and stay away from the area.", difficulty = "easy")
            )
        ),

        // ────────────────────────────────────────────────
        // MODULE 5 – Heights & Fall Protection
        // ────────────────────────────────────────────────
        TrainingModule(
            moduleId = "heights-101",
            title = "Heights & Fall Protection",
            description = "Training on working at heights safely, proper use of fall protection equipment, and rescue procedures.",
            safetyDomain = "Fall Protection",
            difficultyLevel = 2,
            timeLimitSeconds = 300f,
            passThreshold = 75f,
            requiredEquipment = listOf(
                "Full Body Harness",
                "Lanyard with Shock Absorber",
                "Anchor Point",
                "Safety Helmet"
            ),
            scenarios = listOf(
                ScenarioConfig(
                    scenarioId = "heights-1",
                    moduleId = "heights-101",
                    hazardType = "Working at Height",
                    scenarioName = "Scaffold Inspection",
                    initialDangerLevel = 0.2f,
                    escalationIntervalSeconds = 30f,
                    maxEscalationLevel = 3,
                    hazardZones = listOf(
                        HazardZone(zoneId = "heights-zone-scaffold", zoneName = "Scaffold Platform", riskColor = "#FF4500", position = floatArrayOf(0f, 5f, 0f), radius = 3f, dangerLevel = 0.8f),
                        HazardZone(zoneId = "heights-zone-ground", zoneName = "Ground Level", riskColor = "#FFD700", position = floatArrayOf(0f, 0f, 0f), radius = 5f, dangerLevel = 0.3f)
                    ),
                    requiredActions = listOf(
                        RequiredAction(actionId = "inspect-harness", actionName = "Inspect Harness", description = "Check all webbing, stitching, and hardware for damage before use.", completionFeedback = "Harness inspected - all clear.", interactionRadius = 2f, timeLimitSeconds = 20f, scoreValue = 15, isCritical = true),
                        RequiredAction(actionId = "find-anchor", actionName = "Identify Anchor Point", description = "Locate a certified anchor point rated for fall arrest.", completionFeedback = "Anchor point identified.", interactionRadius = 3f, timeLimitSeconds = 15f, scoreValue = 20, isCritical = true),
                        RequiredAction(actionId = "don-harness", actionName = "Don Full Body Harness", description = "Put on the harness and adjust all straps for a snug fit.", completionFeedback = "Harness fitted correctly.", interactionRadius = 2f, timeLimitSeconds = 25f, scoreValue = 20, isCritical = true),
                        RequiredAction(actionId = "connect-lanyard", actionName = "Connect Lanyard", description = "Attach the lanyard snap hook to the harness D-ring and anchor point.", completionFeedback = "Lanyard connected to anchor.", interactionRadius = 2f, timeLimitSeconds = 15f, scoreValue = 25, isCritical = true),
                        RequiredAction(actionId = "proceed", actionName = "Proceed to Work Area", description = "Move to the work area while maintaining 100% tie-off.", completionFeedback = "Arrived at work area safely.", interactionRadius = 3f, timeLimitSeconds = 30f, scoreValue = 20, isCritical = false)
                    ),
                    escalationEvents = listOf(
                        EscalationEvent(level = 0, levelName = "Normal", timeThreshold = 0f, scorePenalty = 0f, visualEffect = "none", audioEffect = "wind-ambient", warningText = "All clear. Proceed with fall protection setup.", showEvacuationArrows = false, triggerAutoFail = false, hazardGrowthMultiplier = 1f),
                        EscalationEvent(level = 1, levelName = "Warning", timeThreshold = 30f, scorePenalty = 0.15f, visualEffect = "wind-gust", audioEffect = "wind-strong", warningText = "Fall hazard! Attach your lanyard to an anchor point immediately.", showEvacuationArrows = false, triggerAutoFail = false, hazardGrowthMultiplier = 1.3f),
                        EscalationEvent(level = 2, levelName = "Danger", timeThreshold = 60f, scorePenalty = 0.35f, visualEffect = "shake-warning", audioEffect = "alarm-beep", warningText = "DANGER: Working at height without fall protection!", showEvacuationArrows = true, triggerAutoFail = false, hazardGrowthMultiplier = 1.8f),
                        EscalationEvent(level = 3, levelName = "Critical", timeThreshold = 100f, scorePenalty = 1f, visualEffect = "fall-critical", audioEffect = "critical-siren", warningText = "CRITICAL: Unprotected work at height! Stop work immediately.", showEvacuationArrows = true, triggerAutoFail = true, hazardGrowthMultiplier = 3f)
                    )
                )
            ),
            questions = listOf(
                AssessmentQuestion(questionId = "heights-q1", moduleId = "heights-101", questionText = "At what height must fall protection be used?", options = listOf("10 feet or more", "6 feet or more", "15 feet or more", "20 feet or more"), correctAnswerIndex = 1, scoreValue = 20, explanation = "Fall protection is required when working at heights of 6 feet or more in most safety regulations.", difficulty = "easy"),
                AssessmentQuestion(questionId = "heights-q2", moduleId = "heights-101", questionText = "What should you inspect BEFORE using a harness?", options = listOf("Nothing - trust the equipment", "All webbing, stitching, and hardware", "Only the buckle", "Only the color tag"), correctAnswerIndex = 1, scoreValue = 20, explanation = "Always inspect all webbing, stitching, and hardware (buckles, D-rings, hooks) for damage before each use.", difficulty = "medium"),
                AssessmentQuestion(questionId = "heights-q3", moduleId = "heights-101", questionText = "What does '100% tie-off' mean?", options = listOf("Tie off once at the start", "Always connected to an anchor point", "Tie off at lunch break", "Use two harnesses"), correctAnswerIndex = 1, scoreValue = 20, explanation = "100% tie-off means you are always connected to an anchor point, even when moving between positions.", difficulty = "medium")
            )
        )
    )

    fun getModule(moduleId: String): TrainingModule? {
        return modules.firstOrNull { it.moduleId == moduleId }
    }

    fun getScenario(moduleId: String, scenarioId: String): ScenarioConfig? {
        return getModule(moduleId)?.scenarios?.firstOrNull { it.scenarioId == scenarioId }
    }

    fun getRandomQuestions(moduleId: String, count: Int): List<AssessmentQuestion> {
        val module = getModule(moduleId) ?: return emptyList()
        return module.questions.shuffled().take(count.coerceAtMost(module.questions.size))
    }
}
