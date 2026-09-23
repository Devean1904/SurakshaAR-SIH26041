using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class QualityManager : MonoBehaviour
{
    public static QualityManager Instance { get; private set; }

    public QualityConfig Config { get; private set; } = new();
    public QualityLevel CurrentLevel { get; private set; }
    public int CurrentIndex { get; private set; } = 1;

    private const string PrefKey = "QualityLevel";

    public delegate void QualityChanged(int levelIndex, string levelName);
    public event QualityChanged OnQualityChanged;

    private List<ParticleSystem> _trackedParticles = new();
    private List<FireSimulation> _trackedFires = new();
    private List<GasLeakSimulation> _trackedGas = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        int saved = PlayerPrefs.GetInt(PrefKey, -1);
        if (saved == -1)
            saved = Config.GetDefaultLevel();

        ApplyLevel(saved);
    }

    void OnLevelWasLoaded(int level)
    {
        RefreshTrackedObjects();
        ApplyToTrackedObjects();
    }

    void RefreshTrackedObjects()
    {
        _trackedParticles.Clear();
        _trackedFires.Clear();
        _trackedGas.Clear();

        _trackedParticles.AddRange(FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        _trackedFires.AddRange(FindObjectsByType<FireSimulation>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        _trackedGas.AddRange(FindObjectsByType<GasLeakSimulation>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
    }

    public void ApplyLevel(int index)
    {
        index = Mathf.Clamp(index, 0, 3);
        CurrentIndex = index;
        CurrentLevel = Config.GetLevel(index);

        ApplyUnityQualitySettings();
        ApplyRenderPipelineSettings();
        ApplyToTrackedObjects();

        PlayerPrefs.SetInt(PrefKey, index);
        PlayerPrefs.Save();

        Debug.Log($"[QualityManager] Applied: {CurrentLevel.Name}");
        OnQualityChanged?.Invoke(index, CurrentLevel.Name);
    }

    void ApplyUnityQualitySettings()
    {
        QualitySettings.SetQualityLevel(CurrentIndex + 1, true);

        QualitySettings.antiAliasing = CurrentLevel.AntiAliasing;
        QualitySettings.vSyncCount = CurrentLevel.VSync ? 1 : 0;
        QualitySettings.lodBias = CurrentLevel.LodBias;
        QualitySettings.anisotropicFiltering = (AnisotropicFiltering)CurrentLevel.Anisotropic;
        QualitySettings.pixelLightCount = CurrentLevel.ShadowsEnabled ? 1 : 0;

        if (CurrentLevel.ShadowsEnabled)
        {
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowResolution = (ShadowResolution)CurrentLevel.ShadowResolution;
            QualitySettings.shadowDistance = CurrentLevel.ShadowDistance;
            QualitySettings.shadowCascades = CurrentIndex >= 2 ? 2 : 1;
        }
        else
        {
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0;
        }

        QualitySettings.softParticles = CurrentLevel.SoftParticles;
        Application.targetFrameRate = CurrentLevel.TargetFrameRate;

        if (QualitySettings.vSyncCount == 0)
            Application.targetFrameRate = CurrentLevel.TargetFrameRate;
    }

    void ApplyRenderPipelineSettings()
    {
        var pipelineAsset = GraphicsSettings.currentRenderPipeline;
        if (pipelineAsset == null) return;

        var rpType = pipelineAsset.GetType();
        var renderScaleProp = rpType.GetProperty("renderScale");
        if (renderScaleProp != null)
        {
            renderScaleProp.SetValue(pipelineAsset, CurrentLevel.RenderScale);
        }

        var msaaProp = rpType.GetProperty("msaaSampleCount");
        if (msaaProp != null)
        {
            msaaProp.SetValue(pipelineAsset, CurrentLevel.AntiAliasing);
        }
    }

    void ApplyToTrackedObjects()
    {
        foreach (var ps in _trackedParticles)
        {
            if (ps == null) continue;
            ApplyToParticleSystem(ps);
        }

        foreach (var fire in _trackedFires)
        {
            if (fire == null) continue;
            fire.SetEmissionScale(CurrentLevel.EmissionScale);
        }

        foreach (var gas in _trackedGas)
        {
            if (gas == null) continue;
            gas.SetEmissionScale(CurrentLevel.EmissionScale);
        }
    }

    void ApplyToParticleSystem(ParticleSystem ps)
    {
        var main = ps.main;
        int maxParticles = Mathf.RoundToInt(main.maxParticles * CurrentLevel.EmissionScale);
        maxParticles = Mathf.Min(maxParticles, CurrentLevel.ParticleCap);
        var mainModule = ps.main;
        mainModule.maxParticles = maxParticles;

        var emission = ps.emission;
        float rate = emission.rateOverTime.constant * CurrentLevel.EmissionScale;
        emission.rateOverTime = rate;
    }

    public void RegisterParticleSystem(ParticleSystem ps)
    {
        if (ps == null || _trackedParticles.Contains(ps)) return;
        _trackedParticles.Add(ps);
        ApplyToParticleSystem(ps);
    }

    public void UnregisterParticleSystem(ParticleSystem ps)
    {
        _trackedParticles.Remove(ps);
    }

    public void RegisterFire(FireSimulation fire)
    {
        if (fire == null || _trackedFires.Contains(fire)) return;
        _trackedFires.Add(fire);
    }

    public void RegisterGas(GasLeakSimulation gas)
    {
        if (gas == null || _trackedGas.Contains(gas)) return;
        _trackedGas.Add(gas);
    }

    public void SetLow() => ApplyLevel(0);
    public void SetMedium() => ApplyLevel(1);
    public void SetHigh() => ApplyLevel(2);
    public void SetUltra() => ApplyLevel(3);

    public string[] GetLevelNames() => new[] { "Low", "Medium", "High", "Ultra" };

    public string GetCurrentDeviceRecommendation()
    {
        var gpu = SystemInfo.graphicsDeviceName;
        var ram = SystemInfo.systemMemorySize;
        var cpu = SystemInfo.processorType;

        return $"GPU: {gpu}\nCPU: {cpu}\nRAM: {ram}MB\nRecommended: {Config.GetLevel(Config.GetDefaultLevel()).Name}";
    }
}