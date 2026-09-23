using UnityEngine;
using System.Collections;

public class GasLeakSimulation : MonoBehaviour
{
    public static GasLeakSimulation Instance { get; private set; }

    [Header("Gas Cloud")]
    public ParticleSystem gasCloudParticles;
    public Renderer cloudRenderer;
    public float cloudExpansionRate = 0.1f;
    public float maxCloudScale = 5f;

    [Header("Perimeter Ring")]
    public GameObject perimeterRing;
    public float ringExpansionRate = 0.05f;
    public float maxRingRadius = 8f;
    public Color ringColor = Color.red;

    [Header("Gas Level")]
    public float currentGasLevel = 0f;
    public float maxGasLevel = 10f;
    public float gasIncreaseRate = 0.1f;

    [Header("Visual")]
    public Color gasColor = new Color(0f, 0.8f, 0f, 0.4f);
    private Vector3 _originalCloudScale;
    private float _currentExpansion = 1f;
    private float _emissionScale = 1f;

    public void SetEmissionScale(float scale)
    {
        _emissionScale = scale;
        if (gasCloudParticles != null)
        {
            var main = gasCloudParticles.main;
            main.maxParticles = Mathf.RoundToInt(40 * scale);
            var emission = gasCloudParticles.emission;
            emission.rateOverTime = 15f * _currentExpansion * scale;
        }
    }

    [Header("Audio")]
    public AudioSource gasAudioSource;
    public AudioClip gasLeakLoop;
    public AudioClip gasWarning;

    private bool _isActive = false;
    private bool _isDetected = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _originalCloudScale = transform.localScale;
    }

    void Start()
    {
        if (cloudRenderer == null)
            cloudRenderer = GetComponentInChildren<Renderer>();

        if (cloudRenderer != null)
        {
            cloudRenderer.material.color = gasColor;
            cloudRenderer.material.SetFloat("_Mode", 3);
            cloudRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            cloudRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            cloudRenderer.material.SetInt("_ZWrite", 0);
            cloudRenderer.material.DisableKeyword("_ALPHATEST_ON");
            cloudRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            cloudRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            cloudRenderer.material.renderQueue = 3000;
        }

        if (gasAudioSource == null)
        {
            gasAudioSource = gameObject.AddComponent<AudioSource>();
            gasAudioSource.loop = true;
            gasAudioSource.volume = 0.3f;
            gasAudioSource.clip = gasLeakLoop;
        }

        if (perimeterRing != null)
            perimeterRing.SetActive(false);
    }

    public void StartGasLeak()
    {
        _isActive = true;
        _isDetected = false;
        currentGasLevel = 1.5f;
        _currentExpansion = 1f;

        if (gasCloudParticles != null)
        {
            var main = gasCloudParticles.main;
            main.startColor = gasColor;
            main.startSize = 0.5f;
            gasCloudParticles.Play();
        }

        if (gasAudioSource != null && gasLeakLoop != null)
            gasAudioSource.Play();

        if (perimeterRing != null)
            perimeterRing.SetActive(true);

        Debug.Log("[GasLeak] Gas leak started - Level: 1.5%");
    }

    void Update()
    {
        if (!_isActive) return;

        currentGasLevel += gasIncreaseRate * Time.deltaTime;
        currentGasLevel = Mathf.Min(currentGasLevel, maxGasLevel);

        _currentExpansion = 1f + (currentGasLevel / maxGasLevel) * (maxCloudScale - 1f);
        transform.localScale = _originalCloudScale * _currentExpansion;

        if (cloudRenderer != null)
        {
            float alpha = Mathf.Lerp(0.2f, 0.7f, currentGasLevel / maxGasLevel);
            Color c = gasColor;
            c.a = alpha;
            cloudRenderer.material.color = c;
        }

        if (gasCloudParticles != null)
        {
            var main = gasCloudParticles.main;
            main.startSize = 0.5f * _currentExpansion;
            var emission = gasCloudParticles.emission;
            emission.rateOverTime = 15f * _currentExpansion;
        }

        if (perimeterRing != null)
        {
            float ringRadius = Mathf.Lerp(1f, maxRingRadius, currentGasLevel / maxGasLevel);
            perimeterRing.transform.localScale = new Vector3(ringRadius * 2f, 0.05f, ringRadius * 2f);
            var ringRenderer = perimeterRing.GetComponent<Renderer>();
            if (ringRenderer != null)
            {
                float danger = currentGasLevel / maxGasLevel;
                ringRenderer.material.color = danger > 0.5f ? Color.red : Color.yellow;
                ringRenderer.material.EnableKeyword("_EMISSION");
                ringRenderer.material.SetColor("_EmissionColor", (danger > 0.5f ? Color.red : Color.yellow) * 1.5f);
            }
        }
    }

    public void DetectGas()
    {
        _isDetected = true;

        if (gasAudioSource != null && gasWarning != null)
            gasAudioSource.PlayOneShot(gasWarning);

        Debug.Log($"[GasLeak] Gas detected! Level: {currentGasLevel:F1}%");
    }

    public void ActivateVentilation()
    {
        gasIncreaseRate *= 0.5f;
        StartCoroutine(VentilationEffect());
        Debug.Log("[GasLeak] Ventilation activated - gas increase slowed");
    }

    IEnumerator VentilationEffect()
    {
        float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration && _isActive)
        {
            currentGasLevel -= Time.deltaTime * 0.3f;
            currentGasLevel = Mathf.Max(currentGasLevel, 0f);
            yield return null;
        }
    }

    public void StopGasLeak()
    {
        _isActive = false;
        _isDetected = false;

        if (gasCloudParticles != null) gasCloudParticles.Stop();
        if (gasAudioSource != null) gasAudioSource.Stop();
        if (perimeterRing != null) perimeterRing.SetActive(false);

        transform.localScale = Vector3.zero;
        currentGasLevel = 0f;

        Debug.Log("[GasLeak] Gas leak stopped");
    }

    public float GetGasLevel() => currentGasLevel;
    public float GetGasLevelNormalized() => currentGasLevel / maxGasLevel;
    public bool IsActive => _isActive;
    public bool IsDetected => _isDetected;
    public string GetGasLevelDisplay() => $"Level: {currentGasLevel:F1}%";
}