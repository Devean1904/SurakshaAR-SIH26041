using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneLoader] Loading: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public void LoadARScene()
    {
        LoadScene("ARScene");
    }

    public void LoadMainScene()
    {
        LoadScene("MainScene");
    }

    public void LoadLoginScene()
    {
        LoadScene("LoginScene");
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadSceneAsync(string sceneName)
    {
        Debug.Log($"[SceneLoader] Async loading: {sceneName}");
        SceneManager.LoadSceneAsync(sceneName);
    }
}