using UnityEngine;
using System.Collections;

public class FireSimulation : MonoBehaviour
{
    public static FireSimulation Instance { get; private set; }

    [Header("Fire Particles")]
    public ParticleSystem flameParticles;
    public ParticleSystem smokeParticles;
    public ParticleSystem sparkParticles;

    [Header("Fire Visual")]
    public Light fireLight;
    public Renderer fireRenderer;
    public float minIntensity = 1f;
    public float maxIntensity = 5f;

    [Header("Growth")]
    public float growthMultiplier = 1f;
    public float maxScale = 4f;
    private Vector3 _originalScale;
    private float _currentGrowth = 1f;

    [Header("Audio")]
    public AudioSource fireAudioSource;
    public AudioClip fireLoop;
    public AudioClip fireExtinguish;

    private bool _isActive = false;
    private float _emissionScale = 1f;

    public void SetEmissionScale(float scale)
    {
        _emissionScale = scale;
        if (!_isActive) return;

        if (flameParticles != null)
        {
            var emission = flameParticles.emission;
            emission.rateOverTime = 20f * _currentGrowth * scale;
            var main = flameParticles.main;
            main.maxParticles = Mathf.RoundToInt(60 * scale);
        }
        if (smokeParticles != null)
        {
            var emission = smokeParticles.emission;
            emission.rateOverTime = 10f * _currentGrowth * scale;
            var main = smokeParticles.main;
            main.maxParticles = Mathf.RoundToInt(30 * scale);
        }
        if (sparkParticles != null)
        {
            var main = sparkParticles.main;
            main.maxParticles = Mathf.RoundToInt(20 * scale);
        }
    }
    private bool _isExtinguishing = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _originalScale = transform.localScale;
    }

    void Start()
    {
        if (fireLight == null)
        {
            var lightObj = new GameObject("FireLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.up * 0.5f;
            fireLight = lightObj.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.5f, 0f);
            fireLight.range = 5f;
            fireLight.intensity = 2f;
        }

        if (fireRenderer == null)
            fireRenderer = GetComponentInChildren<Renderer>();

        if (fireAudioSource == null)
        {
            fireAudioSource = gameObject.AddComponent<AudioSource>();
            fireAudioSource.loop = true;
            fireAudioSource.volume = 0.5f;
            fireAudioSource.clip = fireLoop;
        }
    }

    public void StartFire()
    {
        _isActive = true;
        _currentGrowth = 1f;
        _isExtinguishing = false;

        if (flameParticles != null)
        {
            var main = flameParticles.main;
            main.startSize = 0.5f;
            flameParticles.Play();
        }

        if (smokeParticles != null) smokeParticles.Play();
        if (sparkParticles != null) sparkParticles.Play();
        if (fireAudioSource != null && fireLoop != null) fireAudioSource.Play();

        Debug.Log("[Fire] Fire started");
    }

    public void GrowFire(float multiplier)
    {
        if (!_isActive || _isExtinguishing) return;

        _currentGrowth = Mathf.Min(multiplier, maxScale);
        growthMultiplier = multiplier;

        transform.localScale = _originalScale * _currentGrowth;

        if (fireLight != null)
            fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, _currentGrowth / maxScale);

        if (flameParticles != null)
        {
            var main = flameParticles.main;
            main.startSize = 0.5f * _currentGrowth;
            var emission = flameParticles.emission;
            emission.rateOverTime = 20f * _currentGrowth;
        }

        if (smokeParticles != null)
        {
            var emission = smokeParticles.emission;
            emission.rateOverTime = 10f * _currentGrowth;
        }

        if (fireRenderer != null)
        {
            float r = Mathf.Clamp01(1f);
            float g = Mathf.Clamp01(0.5f / _currentGrowth);
            fireRenderer.material.color = new Color(r, g, 0f);
            fireRenderer.material.EnableKeyword("_EMISSION");
            fireRenderer.material.SetColor("_EmissionColor", new Color(1f, g, 0f) * _currentGrowth);
        }

        Debug.Log($"[Fire] Growth: x{_currentGrowth}");
    }

    public void ExtinguishFire()
    {
        if (!_isActive || _isExtinguishing) return;
        _isExtinguishing = true;

        if (fireAudioSource != null && fireExtinguish != null)
        {
            fireAudioSource.Stop();
            fireAudioSource.PlayOneShot(fireExtinguish);
        }

        StartCoroutine(ExtinguishCoroutine());
    }

    IEnumerator ExtinguishCoroutine()
    {
        float duration = 2f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            if (fireLight != null)
                fireLight.intensity = Mathf.Lerp(fireLight.intensity, 0f, t);

            if (flameParticles != null)
            {
                var emission = flameParticles.emission;
                emission.rateOverTime = Mathf.Lerp(emission.rateOverTime.constant, 0f, t);
            }

            yield return null;
        }

        if (flameParticles != null) flameParticles.Stop();
        if (smokeParticles != null) smokeParticles.Stop();
        if (sparkParticles != null) sparkParticles.Stop();
        if (fireAudioSource != null) fireAudioSource.Stop();
        if (fireLight != null) fireLight.intensity = 0f;

        _isActive = false;
        _currentGrowth = 0f;

        Debug.Log("[Fire] Fire extinguished");
    }

    public void StopFire()
    {
        _isActive = false;
        _isExtinguishing = false;

        if (flameParticles != null) flameParticles.Stop();
        if (smokeParticles != null) smokeParticles.Stop();
        if (sparkParticles != null) sparkParticles.Stop();
        if (fireAudioSource != null) fireAudioSource.Stop();
        if (fireLight != null) fireLight.intensity = 0f;

        transform.localScale = Vector3.zero;

        Debug.Log("[Fire] Fire stopped");
    }

    public float GetGrowth() => _currentGrowth;
    public bool IsActive => _isActive;
    public bool IsExtinguishing => _isExtinguishing;
}