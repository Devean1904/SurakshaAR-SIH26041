using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SupportController : MonoBehaviour
{
    public static SupportController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject _supportPanel;
    [SerializeField] private TMP_InputField _feedbackInput;
    [SerializeField] private Button _submitFeedbackButton;
    [SerializeField] private Button _openSupportButton;
    [SerializeField] private TMP_Dropdown _categoryDropdown;
    [SerializeField] private TextMeshProUGUI _statusText;

    private string _currentUserId;
    private string _currentLanguage = "en";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (_submitFeedbackButton != null)
            _submitFeedbackButton.onClick.AddListener(OnSubmitFeedback);

        if (_openSupportButton != null)
            _openSupportButton.onClick.AddListener(OpenSupport);

        if (_supportPanel != null)
            _supportPanel.SetActive(false);

        if (_categoryDropdown != null)
        {
            _categoryDropdown.ClearOptions();
            _categoryDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Training Issue", "Bug Report", "Feature Request",
                "Account Problem", "Certificate Issue", "Other"
            });
        }

        if (LanguageManager.Instance != null)
            _currentLanguage = LanguageManager.Instance.CurrentLanguage;
    }

    public void OpenSupport()
    {
        if (_supportPanel != null)
            _supportPanel.SetActive(true);
        if (_statusText != null) _statusText.text = "";
    }

    public void CloseSupport()
    {
        if (_supportPanel != null) _supportPanel.SetActive(false);
        if (_feedbackInput != null) _feedbackInput.text = "";
    }

    void OnSubmitFeedback()
    {
        string feedback = _feedbackInput?.text?.Trim();
        if (string.IsNullOrEmpty(feedback))
        {
            ShowStatus("Please enter feedback");
            return;
        }

        StartCoroutine(SubmitFeedbackCoroutine(feedback));
    }

    System.Collections.IEnumerator SubmitFeedbackCoroutine(string feedback)
    {
        string category = _categoryDropdown?.captionText?.text ?? "Other";

        var feedbackData = new
        {
            UserId = _currentUserId,
            Category = category,
            Message = feedback,
            Language = _currentLanguage,
            Timestamp = System.DateTime.UtcNow.ToString("O"),
            Platform = Application.platform.ToString(),
            AppVersion = Application.version
        };

        string json = JsonUtility.ToJson(feedbackData);

        using var request = new UnityEngine.Networking.UnityWebRequest(
            $"{ServerConfig.Instance.ApiBase}/worker/support/feedback",
            "POST");
        request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(
            System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            ShowStatus("Feedback submitted successfully!");
            Debug.Log("[SupportController] Feedback submitted");
        }
        else
        {
            ShowStatus("Failed to submit. Please try again.");
            Debug.LogWarning($"[SupportController] Submit failed: {request.error}");
        }
    }

    void ShowStatus(string message)
    {
        if (_statusText != null) _statusText.text = message;
        Debug.Log($"[SupportController] {message}");
    }

    public void SetUserId(string userId)
    {
        _currentUserId = userId;
    }
}