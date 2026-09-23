using UnityEngine;

public class AudioGenerator : MonoBehaviour
{
    public static AudioGenerator Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public AudioClip GenerateAlarm(float duration = 2f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(800f, 1200f, Mathf.PingPong(t * 4f, 1f));
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f;
            data[i] *= Mathf.PingPong(t * 8f, 1f);
            data[i] *= Mathf.Lerp(1f, 0f, t / duration);
        }

        var clip = AudioClip.Create("Alarm", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public AudioClip GenerateFireCrackle(float duration = 3f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float noise = Random.Range(-1f, 1f);
            float crackle = Mathf.Sin(2f * Mathf.PI * 200f * t + noise * 10f) * 0.3f;
            float rumble = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.2f;
            float pop = (Random.Range(0f, 1f) > 0.997f) ? Random.Range(0.5f, 1f) : 0f;
            data[i] = (noise * 0.15f + crackle + rumble + pop) * Mathf.Lerp(1f, 0f, t / duration);
        }

        var clip = AudioClip.Create("FireCrackle", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public AudioClip GenerateGasHiss(float duration = 4f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float noise = Random.Range(-1f, 1f);
            float hiss = noise * Mathf.Sin(2f * Mathf.PI * 3000f * t) * 0.2f;
            float flow = Mathf.Sin(2f * Mathf.PI * 100f * t + noise * 5f) * 0.15f;
            float intensity = Mathf.PingPong(t * 2f, 1f);
            data[i] = (hiss + flow) * intensity * Mathf.Lerp(1f, 0.3f, t / duration);
        }

        var clip = AudioClip.Create("GasHiss", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public AudioClip GenerateExtinguisher(float duration = 2f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float noise = Random.Range(-1f, 1f);
            float spray = noise * 0.4f;
            float rush = Mathf.Sin(2f * Mathf.PI * 150f * t + noise * 20f) * 0.25f;
            float attack = Mathf.Clamp01(t * 10f);
            float release = Mathf.Clamp01(1f - (t - duration + 0.3f) * 5f);
            data[i] = (spray + rush) * attack * release;
        }

        var clip = AudioClip.Create("Extinguisher", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public AudioClip GenerateFootstep(float duration = 0.3f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float impact = Mathf.Exp(-t * 30f);
            float noise = Random.Range(-1f, 1f) * 0.3f;
            float thud = Mathf.Sin(2f * Mathf.PI * 80f * t) * 0.5f;
            data[i] = (impact * thud + noise * impact) * 0.6f;
        }

        var clip = AudioClip.Create("Footstep", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public AudioClip GenerateUIClick(float duration = 0.05f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            data[i] = Mathf.Sin(2f * Mathf.PI * 1000f * t) * Mathf.Exp(-t * 100f) * 0.3f;
        }

        var clip = AudioClip.Create("UIClick", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public AudioClip GenerateBell(float duration = 1.5f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float bell = Mathf.Sin(2f * Mathf.PI * 2000f * t) * 0.3f;
            float harmonics = Mathf.Sin(2f * Mathf.PI * 4000f * t) * 0.15f;
            float decay = Mathf.Exp(-t * 3f);
            data[i] = (bell + harmonics) * decay;
        }

        var clip = AudioClip.Create("Bell", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public AudioClip GenerateExplosion(float duration = 1f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float noise = Random.Range(-1f, 1f);
            float boom = Mathf.Sin(2f * Mathf.PI * 40f * t) * Mathf.Exp(-t * 8f);
            float crack = noise * Mathf.Exp(-t * 20f) * 0.6f;
            float rumble = Mathf.Sin(2f * Mathf.PI * 30f * t) * Mathf.Exp(-t * 4f) * 0.4f;
            data[i] = boom + crack + rumble;
        }

        var clip = AudioClip.Create("Explosion", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public AudioClip GenerateCollapse(float duration = 2f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float noise = Random.Range(-1f, 1f);
            float rumble = Mathf.Sin(2f * Mathf.PI * 50f * t) * Mathf.Exp(-t * 3f) * 0.5f;
            float crack = noise * Mathf.Exp(-t * 5f) * 0.4f;
            float debris = (Random.Range(0f, 1f) > 0.99f ? Random.Range(-0.5f, 0.5f) : 0f);
            data[i] = rumble + crack + debris;
        }

        var clip = AudioClip.Create("Collapse", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}