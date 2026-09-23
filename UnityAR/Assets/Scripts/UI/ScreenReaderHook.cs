using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ScreenReaderHook : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [Header("Accessibility")]
    public string accessibilityLabel;
    public bool useDoubleTap = false;
    public UnityEngine.Events.UnityEvent onDoubleClickAction;

    private float _lastTapTime = 0f;
    private const float DOUBLE_TAP_THRESHOLD = 0.4f;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!useDoubleTap || string.IsNullOrEmpty(accessibilityLabel)) return;

        float currentTime = Time.time;
        float timeSinceLastTap = currentTime - _lastTapTime;

        if (timeSinceLastTap <= DOUBLE_TAP_THRESHOLD)
        {
            VoiceModule.Instance.StopSpeaking();
            onDoubleClickAction?.Invoke();
            Debug.Log($"[TalkBack] Double-tap activated: {accessibilityLabel}");
        }
        else
        {
            string lang = LanguageManager.Instance.CurrentLanguage;
            VoiceModule.Instance.Speak(accessibilityLabel, lang);
        }

        _lastTapTime = currentTime;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(accessibilityLabel))
        {
            string lang = LanguageManager.Instance.CurrentLanguage;
            VoiceModule.Instance.Speak(accessibilityLabel, lang);
        }
    }
}
