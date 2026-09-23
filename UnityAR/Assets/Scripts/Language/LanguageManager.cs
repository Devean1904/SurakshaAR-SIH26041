using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    public string CurrentLanguage { get; private set; } = "en";

    public delegate void LanguageChanged(string newLang);
    public event LanguageChanged OnLanguageChanged;

    private Dictionary<string, Dictionary<string, string>> _translations = new();
    private Dictionary<string, string> _currentTranslations = new();

    private static readonly string[] SupportedLanguages = { "en", "hi", "sat", "mr", "ta", "te", "kn" };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAllTranslations();
        string saved = PlayerPrefs.GetString("Language", "en");
        SetLanguage(saved);
    }

    void LoadAllTranslations()
    {
        foreach (string lang in SupportedLanguages)
        {
            string json = LoadJsonFile(lang);
            if (!string.IsNullOrEmpty(json))
            {
                var dict = ParseJsonToDict(json);
                if (dict.Count > 0)
                    _translations[lang] = dict;
            }
        }

        if (!_translations.ContainsKey("en"))
            _translations["en"] = GetFallbackEnglish();
    }

    string LoadJsonFile(string langCode)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Lang", $"{langCode}.json");
        if (File.Exists(path))
            return File.ReadAllText(path);

        Debug.LogWarning($"[Language] JSON not found: {path}, trying Resources");
        var textAsset = Resources.Load<TextAsset>($"Lang/{langCode}");
        return textAsset?.text;
    }

    Dictionary<string, string> ParseJsonToDict(string json)
    {
        var dict = new Dictionary<string, string>();
        try
        {
            var wrapper = JsonUtility.FromJson<JsonWrapper>(json);
            if (wrapper != null && wrapper.entries != null)
            {
                foreach (var entry in wrapper.entries)
                    dict[entry.key] = entry.value;
            }
        }
        catch
        {
            dict = ParseJsonManually(json);
        }
        return dict;
    }

    Dictionary<string, string> ParseJsonManually(string json)
    {
        var dict = new Dictionary<string, string>();
        json = json.Trim();
        if (json.StartsWith("{")) json = json.Substring(1);
        if (json.EndsWith("}")) json = json.Substring(0, json.Length - 1);

        bool inKey = false, inValue = false;
        string currentKey = "", currentValue = "";
        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            if (c == '"' && !inValue) { inKey = !inKey; continue; }
            if (c == ':' && inKey) { inKey = false; inValue = true; continue; }
            if (c == ',' && inValue) { dict[currentKey.Trim()] = currentValue.Trim(); currentKey = ""; currentValue = ""; inValue = false; continue; }
            if (inKey) currentKey += c;
            if (inValue) currentValue += c;
        }
        if (!string.IsNullOrEmpty(currentKey))
            dict[currentKey.Trim()] = currentValue.Trim();
        return dict;
    }

    public void SetLanguage(string langCode)
    {
        CurrentLanguage = langCode;
        PlayerPrefs.SetString("Language", langCode);
        PlayerPrefs.Save();

        if (_translations.ContainsKey(langCode))
            _currentTranslations = _translations[langCode];
        else if (_translations.ContainsKey("en"))
            _currentTranslations = _translations["en"];
        else
            _currentTranslations = new Dictionary<string, string>();

        Debug.Log($"[Language] Set to: {langCode} ({_currentTranslations.Count} keys)");
        OnLanguageChanged?.Invoke(langCode);
    }

    public string Get(string key)
    {
        if (_currentTranslations.TryGetValue(key, out var value))
            return value;
        if (_translations.TryGetValue("en", out var enDict) && enDict.TryGetValue(key, out var en))
            return en;
        return key;
    }

    public bool HasLanguage(string langCode) => _translations.ContainsKey(langCode);
    public string[] GetSupportedLanguages() => SupportedLanguages;
    public int GetKeyCount(string langCode) => _translations.TryGetValue(langCode, out var d) ? d.Count : 0;

    public void DownloadVoicePack(string langCode)
    {
        Debug.Log($"[Language] Voice pack download requested for: {langCode}");
    }

    public void AnnounceTextToSpeech(string text)
    {
        Debug.Log($"[TTS] {text}");
    }

    Dictionary<string, string> GetFallbackEnglish()
    {
        return new Dictionary<string, string>
        {
            ["login_title"] = "SurakshaAR",
            ["login_button"] = "Login",
            ["home_title"] = "Home",
            ["training_title"] = "Training",
            ["training_passed"] = "PASSED",
            ["training_failed"] = "FAILED",
            ["assessment_title"] = "Assessment",
            ["cert_generated"] = "Certificate issued!",
            ["nav_home"] = "Home",
            ["nav_training"] = "Training",
            ["nav_certificate"] = "Certificate",
            ["nav_profile"] = "Profile"
        };
    }

    [System.Serializable]
    private class JsonWrapper { public List<JsonEntry> entries; }

    [System.Serializable]
    private class JsonEntry { public string key; public string value; }
}
