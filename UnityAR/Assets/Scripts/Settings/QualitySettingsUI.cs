using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QualitySettingsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private Button _openSettingsButton;
    [SerializeField] private Button _closeSettingsButton;
    [SerializeField] private TMP_Dropdown _qualityDropdown;
    [SerializeField] private TextMeshProUGUI _currentQualityText;
    [SerializeField] private TextMeshProUGUI _deviceInfoText;
    [SerializeField] private TextMeshProUGUI _recommendedText;
    [SerializeField] private Toggle _autoDetectToggle;
    [SerializeField] private Button _applyButton;

    void Start()
    {
        if (_openSettingsButton != null)
            _openSettingsButton.onClick.AddListener(OpenSettings);
        if (_closeSettingsButton != null)
            _closeSettingsButton.onClick.AddListener(CloseSettings);
        if (_applyButton != null)
            _applyButton.onClick.AddListener(ApplySelection);

        if (_qualityDropdown != null)
        {
            _qualityDropdown.ClearOptions();
            _qualityDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Low", "Medium", "High", "Ultra"
            });
            _qualityDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        if (_autoDetectToggle != null)
        {
            _autoDetectToggle.onValueChanged.AddListener(OnAutoDetectToggled);
        }

        if (_settingsPanel != null)
            _settingsPanel.SetActive(false);

        RefreshDisplay();
    }

    public void OpenSettings()
    {
        if (_settingsPanel != null) _settingsPanel.SetActive(true);
        RefreshDisplay();
    }

    public void CloseSettings()
    {
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
    }

    void RefreshDisplay()
    {
        if (QualityManager.Instance == null) return;

        int current = QualityManager.Instance.CurrentIndex;
        string name = QualityManager.Instance.CurrentLevel.Name;

        if (_qualityDropdown != null)
            _qualityDropdown.SetValueWithoutNotify(current);

        if (_currentQualityText != null)
            _currentQualityText.text = $"Current: {name}";

        if (_deviceInfoText != null)
            _deviceInfoText.text = QualityManager.Instance.GetCurrentDeviceRecommendation();

        if (_recommendedText != null)
        {
            int rec = QualityManager.Instance.Config.GetDefaultLevel();
            string recName = QualityManager.Instance.Config.GetLevel(rec).Name;
            _recommendedText.text = $"Recommended: {recName}";
        }

        if (_autoDetectToggle != null)
            _autoDetectToggle.isOn = PlayerPrefs.GetInt("AutoQuality", 1) == 1;
    }

    void OnDropdownChanged(int index)
    {
        Debug.Log($"[QualityUI] Selected: {index}");
    }

    void OnAutoDetectToggled(bool isOn)
    {
        PlayerPrefs.SetInt("AutoQuality", isOn ? 1 : 0);
        if (isOn)
        {
            int rec = QualityManager.Instance.Config.GetDefaultLevel();
            QualityManager.Instance.ApplyLevel(rec);
            RefreshDisplay();
        }
    }

    void ApplySelection()
    {
        if (_qualityDropdown == null || QualityManager.Instance == null) return;

        int selected = _qualityDropdown.value;
        QualityManager.Instance.ApplyLevel(selected);
        RefreshDisplay();

        Debug.Log($"[QualityUI] Applied: {QualityManager.Instance.CurrentLevel.Name}");
    }

    public void SetQualityFromDashboard(int index)
    {
        if (QualityManager.Instance != null)
        {
            QualityManager.Instance.ApplyLevel(index);
            RefreshDisplay();
        }
    }
}