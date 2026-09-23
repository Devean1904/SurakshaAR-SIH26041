using UnityEngine;

[System.Serializable]
public class QualityLevel
{
    public string Name;
    public int ParticleCap;
    public float EmissionScale;
    public float RenderScale;
    public int AntiAliasing;
    public bool ShadowsEnabled;
    public int ShadowResolution;
    public int ShadowDistance;
    public bool SoftParticles;
    public int Anisotropic;
    public float LodBias;
    public int TextureQuality;
    public bool VSync;
    public int TargetFrameRate;
}

[System.Serializable]
public class QualityConfig
{
    public QualityLevel Low = new()
    {
        Name = "Low",
        ParticleCap = 20,
        EmissionScale = 0.4f,
        RenderScale = 0.75f,
        AntiAliasing = 0,
        ShadowsEnabled = false,
        ShadowResolution = 0,
        ShadowDistance = 0,
        SoftParticles = false,
        Anisotropic = 0,
        LodBias = 0.5f,
        TextureQuality = 1,
        VSync = false,
        TargetFrameRate = 30
    };

    public QualityLevel Medium = new()
    {
        Name = "Medium",
        ParticleCap = 50,
        EmissionScale = 0.7f,
        RenderScale = 0.9f,
        AntiAliasing = 0,
        ShadowsEnabled = true,
        ShadowResolution = 0,
        ShadowDistance = 15,
        SoftParticles = false,
        Anisotropic = 1,
        LodBias = 1f,
        TextureQuality = 0,
        VSync = false,
        TargetFrameRate = 30
    };

    public QualityLevel High = new()
    {
        Name = "High",
        ParticleCap = 80,
        EmissionScale = 1.0f,
        RenderScale = 1.0f,
        AntiAliasing = 2,
        ShadowsEnabled = true,
        ShadowResolution = 1,
        ShadowDistance = 25,
        SoftParticles = false,
        Anisotropic = 2,
        LodBias = 1.5f,
        TextureQuality = 0,
        VSync = true,
        TargetFrameRate = 60
    };

    public QualityLevel Ultra = new()
    {
        Name = "Ultra",
        ParticleCap = 120,
        EmissionScale = 1.2f,
        RenderScale = 1.0f,
        AntiAliasing = 4,
        ShadowsEnabled = true,
        ShadowResolution = 2,
        ShadowDistance = 40,
        SoftParticles = true,
        Anisotropic = 2,
        LodBias = 2f,
        TextureQuality = 0,
        VSync = true,
        TargetFrameRate = 60
    };

    public QualityLevel GetLevel(int index)
    {
        return index switch
        {
            0 => Low,
            1 => Medium,
            2 => High,
            3 => Ultra,
            _ => Medium
        };
    }

    public int GetDefaultLevel()
    {
        var gpu = SystemInfo.graphicsDeviceName.ToLower();

        if (gpu.Contains("adreno 3") || gpu.Contains("adreno 4") || gpu.Contains("mali-t") ||
            gpu.Contains("powervr") || SystemInfo.systemMemorySize <= 2000)
            return 0;

        if (gpu.Contains("adreno 5") || gpu.Contains("adreno 61") || gpu.Contains("mali-g51") ||
            SystemInfo.systemMemorySize <= 3000)
            return 1;

        if (gpu.Contains("adreno 6") || gpu.Contains("adreno 7") || gpu.Contains("mali-g7") ||
            gpu.Contains("mali-g9") || gpu.Contains("apple gpu"))
            return 2;

        return 3;
    }
}