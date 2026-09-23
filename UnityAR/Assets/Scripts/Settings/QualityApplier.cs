using UnityEngine;

public class QualityApplier : MonoBehaviour
{
    public static QualityApplier Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (QualityManager.Instance != null)
        {
            QualityManager.Instance.OnQualityChanged += OnQualityChanged;
            ApplyToScene();
        }
    }

    void OnDestroy()
    {
        if (QualityManager.Instance != null)
            QualityManager.Instance.OnQualityChanged -= OnQualityChanged;
    }

    void OnQualityChanged(int levelIndex, string levelName)
    {
        ApplyToScene();
    }

    void ApplyToScene()
    {
        if (QualityManager.Instance == null) return;

        var level = QualityManager.Instance.CurrentLevel;

        ApplyToParticles();
        ApplyToLights();
        ApplyToRenderers();
        ApplyToShadows();

        Debug.Log($"[QualityApplier] Applied {level.Name} settings to scene");
    }

    void ApplyToParticles()
    {
        var particles = FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var level = QualityManager.Instance.CurrentLevel;

        foreach (var ps in particles)
        {
            if (ps == null) continue;

            var main = ps.main;
            int maxP = Mathf.RoundToInt(main.maxParticles * level.EmissionScale);
            maxP = Mathf.Min(maxP, level.ParticleCap);
            main.maxParticles = maxP;

            var emission = ps.emission;
            emission.rateOverTime = emission.rateOverTime.constant * level.EmissionScale;

            if (QualityManager.Instance != null)
                QualityManager.Instance.RegisterParticleSystem(ps);
        }
    }

    void ApplyToLights()
    {
        var lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var level = QualityManager.Instance.CurrentLevel;

        foreach (var light in lights)
        {
            if (light == null) continue;

            if (light.type == LightType.Point || light.type == LightType.Spot)
            {
                light.shadowStrength = level.ShadowsEnabled ? 0.8f : 0f;
            }
        }
    }

    void ApplyToRenderers()
    {
        if (QualityManager.Instance == null) return;

        var level = QualityManager.Instance.CurrentLevel;

        if (level.TextureQuality == 1)
        {
            QualitySettings.globalTextureMipmapLimit = 1;
        }
        else
        {
            QualitySettings.globalTextureMipmapLimit = 0;
        }
    }

    void ApplyToShadows()
    {
        if (QualityManager.Instance == null) return;

        var level = QualityManager.Instance.CurrentLevel;

        if (level.ShadowsEnabled)
        {
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowDistance = level.ShadowDistance;
            QualitySettings.shadowResolution = (ShadowResolution)level.ShadowResolution;
        }
        else
        {
            QualitySettings.shadows = ShadowQuality.Disable;
        }
    }

    public void ApplyToObject(GameObject obj)
    {
        if (QualityManager.Instance == null || obj == null) return;

        var level = QualityManager.Instance.CurrentLevel;

        var particles = obj.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            var main = ps.main;
            main.maxParticles = Mathf.Min(main.maxParticles, level.ParticleCap);

            var emission = ps.emission;
            emission.rateOverTime = emission.rateOverTime.constant * level.EmissionScale;

            QualityManager.Instance.RegisterParticleSystem(ps);
        }

        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (renderer.material.HasProperty("_Smoothness"))
            {
                float smoothness = renderer.material.GetFloat("_Smoothness");
                smoothness *= level.EmissionScale;
                renderer.material.SetFloat("_Smoothness", smoothness);
            }
        }
    }
}