using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CitificationManager : MonoBehaviour
{
    public static CitificationManager Instance { get; private set; }

    [Header("UI")]
    public GameObject citificationPanel;
    public TextMeshProUGUI zoneNameText;
    public TextMeshProUGUI riskLevelText;
    public TextMeshProUGUI occupancyText;
    public TextMeshProUGUI equipmentText;
    public TextMeshProUGUI evacuationText;
    public Image riskIndicator;
    public Slider occupancyBar;

    [Header("Colors")]
    public Color lowRisk = Color.green;
    public Color mediumRisk = Color.yellow;
    public Color highRisk = new Color(1f, 0.5f, 0f);
    public Color criticalRisk = Color.red;

    private CitificationZoneData _currentZone;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void LoadZone(CitificationZoneData zone)
    {
        _currentZone = zone;

        if (citificationPanel != null) citificationPanel.SetActive(true);

        if (zoneNameText != null) zoneNameText.text = zone.ZoneName;
        if (riskLevelText != null) riskLevelText.text = $"Risk: {zone.RiskLevel}";

        if (riskIndicator != null)
        {
            riskIndicator.color = zone.RiskLevel.ToLower() switch
            {
                "low" => lowRisk,
                "medium" => mediumRisk,
                "high" => highRisk,
                "critical" => criticalRisk,
                _ => mediumRisk
            };
        }

        if (occupancyBar != null)
        {
            occupancyBar.maxValue = zone.MaxOccupancy;
            occupancyBar.value = zone.CurrentOccupancy;
        }

        if (occupancyText != null)
            occupancyText.text = $"Occupancy: {zone.CurrentOccupancy}/{zone.MaxOccupancy}";

        if (equipmentText != null)
            equipmentText.text = $"Safety Equipment: {string.Join(", ", zone.SafetyEquipment)}";

        if (evacuationText != null)
            evacuationText.text = $"Evacuation Routes: {string.Join(", ", zone.EvacuationRoutes)}";

        LocalBlockchain.Instance.AddBlock("citification_viewed", PlayerPrefs.GetString("UserId"), zone.ZoneName);
        Debug.Log($"[Citification] Zone loaded: {zone.ZoneName}");
    }

    public void UpdateOccupancy(float current)
    {
        if (_currentZone == null) return;
        _currentZone.CurrentOccupancy = current;
        if (occupancyBar != null) occupancyBar.value = current;
        if (occupancyText != null) occupancyText.text = $"Occupancy: {current}/{_currentZone.MaxOccupancy}";
    }

    public void AnnounceSafety()
    {
        var lang = LanguageManager.Instance.CurrentLanguage;
        var zone = _currentZone?.ZoneName ?? "current zone";
        var risk = _currentZone?.RiskLevel ?? "unknown";
        LanguageManager.Instance.AnnounceTextToSpeech($"Caution. {risk} risk level in {zone}. Follow evacuation routes.");
    }
}

[System.Serializable]
public class CitificationZoneData
{
    public string ZoneId;
    public string ZoneName;
    public string RiskLevel;
    public float MaxOccupancy;
    public float CurrentOccupancy;
    public List<string> SafetyEquipment;
    public List<string> EvacuationRoutes;
}
