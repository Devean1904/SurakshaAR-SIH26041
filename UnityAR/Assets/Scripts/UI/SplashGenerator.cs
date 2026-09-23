using UnityEngine;

public class SplashGenerator : MonoBehaviour
{
    public static SplashGenerator Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Texture2D GenerateSplashScreen(int width = 1920, int height = 1080)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color[width * height];

        Color bgTop = new Color(0.04f, 0.05f, 0.08f);
        Color bgBottom = new Color(0.08f, 0.1f, 0.15f);
        Color primary = new Color(0.23f, 0.51f, 0.96f);
        Color accent = new Color(0.13f, 0.85f, 0.53f);
        Color white = Color.white;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / width;
                float ny = (float)y / height;
                int idx = y * width + x;

                Color bg = Color.Lerp(bgBottom, bgTop, ny);
                pixels[idx] = bg;

                float cx = nx - 0.5f;
                float cy = ny - 0.5f;
                float dist = Mathf.Sqrt(cx * cx + cy * cy);

                if (dist < 0.15f)
                {
                    float glow = Mathf.InverseLerp(0.15f, 0.05f, dist);
                    pixels[idx] = Color.Lerp(bg, primary, glow * 0.3f);
                }

                float gridX = Mathf.Repeat(nx * 30f, 1f);
                float gridY = Mathf.Repeat(ny * 20f, 1f);
                if ((gridX > 0.48f && gridX < 0.52f) || (gridY > 0.48f && gridY < 0.52f))
                {
                    float gridFade = Mathf.Abs(ny - 0.5f) * 2f;
                    pixels[idx] = Color.Lerp(pixels[idx], primary, 0.05f * (1f - gridFade));
                }

                float lineY = Mathf.Abs(cy - 0.05f);
                if (lineY < 0.002f && Mathf.Abs(cx) < 0.2f)
                {
                    float lineGlow = Mathf.InverseLerp(0.002f, 0f, lineY);
                    pixels[idx] = Color.Lerp(pixels[idx], accent, lineGlow * 0.5f);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    public Texture2D GenerateLoadingBar(float progress, int width = 400, int height = 20)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color[width * height];

        Color bg = new Color(0.15f, 0.16f, 0.2f);
        Color fill = new Color(0.23f, 0.51f, 0.96f);
        Color track = new Color(0.1f, 0.11f, 0.15f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / width;
                int idx = y * width + x;

                bool isBorder = y == 0 || y == height - 1 || x == 0 || x == width - 1;

                if (isBorder)
                {
                    pixels[idx] = track;
                }
                else if (nx < progress)
                {
                    float edge = Mathf.InverseLerp(progress - 0.02f, progress, nx);
                    pixels[idx] = Color.Lerp(fill, track, edge);
                }
                else
                {
                    pixels[idx] = bg;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}