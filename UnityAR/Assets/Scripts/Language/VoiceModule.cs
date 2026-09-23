using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoiceModule : MonoBehaviour
{
    public static VoiceModule Instance { get; private set; }

    private Dictionary<string, AudioClip> _voiceClips = new();
    private AudioSource _audioSource;
    private bool _isSpeaking = false;

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject _tts;
    private bool _ttsInitialized = false;
#endif

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _audioSource = gameObject.AddComponent<AudioSource>();
        InitializePlatformTTS();
    }

    void InitializePlatformTTS()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            _tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new TtsInitListener());
            Debug.Log("[Voice] Android TTS initialized");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Voice] Android TTS init failed: {e.Message}");
            _ttsInitialized = false;
        }
#endif
    }

    public void Speak(string text, string languageCode = "en")
    {
        if (_isSpeaking) return;
        StartCoroutine(SpeakCoroutine(text, languageCode));
    }

    IEnumerator SpeakCoroutine(string text, string langCode)
    {
        _isSpeaking = true;

        string clipKey = $"{langCode}_{text.GetHashCode()}";
        if (_voiceClips.ContainsKey(clipKey))
        {
            _audioSource.clip = _voiceClips[clipKey];
            _audioSource.Play();
            yield return new WaitWhile(() => _audioSource.isPlaying);
        }
        else
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            yield return SpeakWithAndroidTTS(text, langCode);
#else
            Debug.Log($"[Voice] TTS: [{langCode}] {text}");
            yield return new WaitForSeconds(Mathf.Min(text.Length * 0.05f, 3f));
#endif
        }

        _isSpeaking = false;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    IEnumerator SpeakWithAndroidTTS(string text, string langCode)
    {
        if (_tts == null)
        {
            Debug.LogWarning("[Voice] Android TTS not available, using fallback");
            yield return new WaitForSeconds(Mathf.Min(text.Length * 0.05f, 3f));
            yield break;
        }

        var langCodeMap = new Dictionary<string, string>
        {
            ["en"] = "eng",
            ["hi"] = "hin",
            ["sat"] = "sat",
            ["mr"] = "mar",
            ["ta"] = "tam",
            ["te"] = "tel",
            ["kn"] = "kan"
        };

        string androidLang = langCodeMap.ContainsKey(langCode) ? langCodeMap[langCode] : "eng";

        try
        {
            var locale = new AndroidJavaClass("java.util.Locale");
            var localeObj = locale.CallStatic<AndroidJavaObject>("forLanguageTag", androidLang);
            _tts.Call<bool>("setLanguage", localeObj);

            _tts.Call<int>("speak", text, 0, null, "utter_" + System.Guid.NewGuid().ToString());
            yield return new WaitUntil(() => !_tts.Call<bool>("isSpeaking"));
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Voice] Android TTS speak failed: {e.Message}");
            yield return new WaitForSeconds(Mathf.Min(text.Length * 0.05f, 3f));
        }
    }
#endif

    public void StopSpeaking()
    {
        if (_audioSource.isPlaying) _audioSource.Stop();
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_tts != null)
        {
            try { _tts.Call("stop"); } catch { }
        }
#endif
        _isSpeaking = false;
    }

    public void SetSpeechRate(float rate)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_tts != null)
        {
            try { _tts.Call<bool>("setSpeechRate", rate); } catch { }
        }
#endif
    }

    public void SetPitch(float pitch)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_tts != null)
        {
            try { _tts.Call<bool>("setPitch", pitch); } catch { }
        }
#endif
    }

    public void DownloadVoicePack(string languageCode)
    {
        Debug.Log($"[Voice] Downloading voice pack for: {languageCode}");
        StartCoroutine(DownloadVoiceCoroutine(languageCode));
    }

    IEnumerator DownloadVoiceCoroutine(string langCode)
    {
        string[] urls = new[]
        {
            $"https://translate.google.com/translate_tts?ie=UTF-8&q=hello&tl={langCode}&client=tw-ob"
        };

        foreach (var url in urls)
        {
            using var req = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(req);
                if (clip != null)
                {
                    _voiceClips[$"{langCode}_test"] = clip;
                    Debug.Log($"[Voice] Voice pack downloaded for: {langCode}");
                }
            }
            else
            {
                Debug.Log($"[Voice] Download failed for {langCode}: {req.error}");
            }
        }
    }

    public bool IsSpeaking => _isSpeaking;

    public void AnnounceForLanguage(string text)
    {
        string lang = LanguageManager.Instance.CurrentLanguage;
        Speak(text, lang);
    }

    void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_tts != null)
        {
            try
            {
                _tts.Call("stop");
                _tts.Call("shutdown");
                _tts.Dispose();
            }
            catch { }
        }
#endif
    }
}

#if UNITY_ANDROID && !UNITY_EDITOR
public class TtsInitListener : AndroidJavaProxy
{
    public TtsInitListener() : base("android.speech.tts.TextToSpeech$OnInitListener") { }

    void onInit(int status)
    {
        if (status == 0)
            Debug.Log("[Voice] Android TTS engine initialized successfully");
        else
            Debug.LogWarning($"[Voice] Android TTS engine failed to init: {status}");
    }
}
#endif