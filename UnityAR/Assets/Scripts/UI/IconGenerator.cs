using UnityEngine;

public class IconGenerator : MonoBehaviour
{
    public static IconGenerator Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Texture2D GenerateAppIcon(int size = 512)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];

        Color bgColor = new Color(0.06f, 0.07f, 0.09f);
        Color shieldColor = new Color(0.23f, 0.51f, 0.96f);
        Color accentColor = new Color(0.13f, 0.85f, 0.53f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (float)x / size;
                float ny = (float)y / size;
                int idx = y * size + x;

                float cx = nx - 0.5f;
                float cy = ny - 0.5f;
                float dist = Mathf.Sqrt(cx * cx + cy * cy);

                if (dist < 0.35f)
                {
                    float shieldShape = Mathf.Abs(cx) * 2f + Mathf.Max(0, cy - 0.1f) * 1.5f;
                    if (shieldShape < 0.3f)
                    {
                        float gradient = Mathf.InverseLerp(0.3f, 0f, shieldShape);
                        pixels[idx] = Color.Lerp(bgColor, shieldColor, gradient);

                        float innerDist = Mathf.Sqrt(cx * cx + (cy + 0.05f) * (cy + 0.05f));
                        if (innerDist < 0.12f)
                        {
                            float crossH = Mathf.Abs(cy + 0.05f) < 0.02f && Mathf.Abs(cx) < 0.08f ? 1f : 0f;
                            float crossV = Mathf.Abs(cx) < 0.02f && Mathf.Abs(cy + 0.05f) < 0.1f ? 1f : 0f;
                            float cross = Mathf.Max(crossH, crossV);
                            pixels[idx] = Color.Lerp(pixels[idx], accentColor, cross * 0.8f);
                        }
                    }
                    else
                    {
                        pixels[idx] = bgColor;
                    }
                }
                else if (dist < 0.38f)
                {
                    pixels[idx] = Color.Lerp(shieldColor, bgColor, Mathf.InverseLerp(0.35f, 0.38f, dist));
                }
                else
                {
                    pixels[idx] = bgColor;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    public Texture2D GenerateIcon(int size, Color primary, string symbol)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];

        Color bg = new Color(0.1f, 0.11f, 0.15f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (float)x / size;
                float ny = (float)y / size;
                int idx = y * size + x;

                float cornerRadius = 0.1f;
                float dx = Mathf.Max(0, cornerRadius - nx, nx - (1f - cornerRadius));
                float dy = Mathf.Max(0, cornerRadius - ny, ny - (1f - cornerRadius));
                float cornerDist = Mathf.Sqrt(dx * dx + dy * dy);

                if (cornerDist < cornerRadius)
                {
                    float cx = nx - 0.5f;
                    float cy = ny - 0.5f;
                    float circleDist = Mathf.Sqrt(cx * cx + cy * cy);

                    if (circleDist < 0.3f)
                    {
                        float glow = Mathf.InverseLerp(0.3f, 0.1f, circleDist);
                        pixels[idx] = Color.Lerp(bg, primary, glow * 0.8f);
                    }
                    else
                    {
                        pixels[idx] = bg;
                    }
                }
                else
                {
                    pixels[idx] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    public Texture2D GenerateFireIcon(int size = 64)
    {
        return GenerateIcon(size, new Color(1f, 0.4f, 0f), "fire");
    }

    public Texture2D GenerateGasIcon(int size = 64)
    {
        return GenerateIcon(size, new Color(0.2f, 1f, 0.2f), "gas");
    }

    public Texture2D GenerateStructuralIcon(int size = 64)
    {
        return GenerateIcon(size, new Color(1f, 0.8f, 0f), "struct");
    }

    public Texture2D GenerateElectricalIcon(int size = 64)
    {
        return GenerateIcon(size, new Color(1f, 1f, 0f), "elec");
    }

    public Texture2D GenerateHeightsIcon(int size = 64)
    {
        return GenerateIcon(size, new Color(1f, 0f, 0f), "high");
    }

    public Texture2D GenerateWarningIcon(int size = 64)
    {
        return GenerateIcon(size, new Color(1f, 0.8f, 0f), "warn");
    }

    public Texture2D GenerateSuccessIcon(int size = 64)
    {
        return GenerateIcon(size, new Color(0.13f, 0.85f, 0.53f), "ok");
    }

    public Texture2D GenerateDangerIcon(int size = 64)
    {
        return GenerateIcon(size, new Color(0.94f, 0.27f, 0.27f), "err");
    }

    public Sprite TextureToSprite(Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
    }

    public Sprite GetAppIconSprite()
    {
        return TextureToSprite(GenerateAppIcon(512));
    }
}