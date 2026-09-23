using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UINavigator : MonoBehaviour
{
    public static UINavigator Instance { get; private set; }

    [Header("Panels")]
    public GameObject homePanel;
    public GameObject settingsPanel;
    public GameObject adminPanel;
    public GameObject managerPanel;
    public GameObject workerPanel;
    public GameObject escalationPanel;
    public GameObject scenarioPanel;
    public GameObject citificationPanel;

    [Header("Navigation")]
    public Button settingsButton;
    public Button homeButton;
    public Button logoutButton;

    [Header("Role Panels")]
    public GameObject adminOnlyUI;
    public GameObject managerOnlyUI;
    public GameObject workerOnlyUI;

    private Stack<GameObject> _panelHistory = new();
    private string _currentRole = "Worker";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _currentRole = PlayerPrefs.GetString("Role", "Worker");

        if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettings);
        if (homeButton != null) homeButton.onClick.AddListener(ShowHome);
        if (logoutButton != null) logoutButton.onClick.AddListener(Logout);

        ApplyRoleVisibility();
        ShowHome();
    }

    void ApplyRoleVisibility()
    {
        if (adminOnlyUI != null) adminOnlyUI.SetActive(_currentRole == "Admin");
        if (managerOnlyUI != null) managerOnlyUI.SetActive(_currentRole == "Manager");
        if (workerOnlyUI != null) workerOnlyUI.SetActive(_currentRole == "Worker");
    }

    void ShowPanel(GameObject panel)
    {
        if (_panelHistory.Count > 0 && _panelHistory.Peek() != null)
            _panelHistory.Peek().SetActive(false);

        _panelHistory.Push(panel);
        panel.SetActive(true);
    }

    public void ShowHome()
    {
        _panelHistory.Clear();
        HideAll();

        string role = PlayerPrefs.GetString("Role", "Worker");
        switch (role)
        {
            case "Admin":
                ShowPanel(adminPanel != null ? adminPanel : homePanel);
                break;
            case "Manager":
                ShowPanel(managerPanel != null ? managerPanel : homePanel);
                break;
            default:
                ShowPanel(workerPanel != null ? workerPanel : homePanel);
                break;
        }
    }

    public void ShowSettings()
    {
        if (settingsPanel != null) ShowPanel(settingsPanel);
    }

    public void ShowEscalation()
    {
        if (escalationPanel != null) ShowPanel(escalationPanel);
    }

    public void ShowScenario()
    {
        if (scenarioPanel != null) ShowPanel(scenarioPanel);
    }

    public void ShowCitification()
    {
        if (citificationPanel != null) ShowPanel(citificationPanel);
    }

    public void GoBack()
    {
        if (_panelHistory.Count > 1)
        {
            _panelHistory.Pop().SetActive(false);
            _panelHistory.Peek().SetActive(true);
        }
        else
        {
            ShowHome();
        }
    }

    void HideAll()
    {
        var panels = new[] { homePanel, settingsPanel, adminPanel, managerPanel, workerPanel, escalationPanel, scenarioPanel, citificationPanel };
        foreach (var p in panels)
        {
            if (p != null) p.SetActive(false);
        }
    }

    void Logout()
    {
        PlayerPrefs.DeleteKey("Token");
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.DeleteKey("Role");
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}
