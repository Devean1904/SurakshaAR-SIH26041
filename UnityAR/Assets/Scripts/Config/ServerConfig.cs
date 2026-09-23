using UnityEngine;

[CreateAssetMenu(fileName = "ServerConfig", menuName = "SurakshaAR/Server Config")]
public class ServerConfig : ScriptableObject
{
    public string serverUrl = "http://localhost:5000";
    
    private static ServerConfig _instance;
    
    public static ServerConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ServerConfig>("ServerConfig");
                if (_instance == null)
                {
                    _instance = CreateInstance<ServerConfig>();
                    Debug.LogWarning("[ServerConfig] No ServerConfig found in Resources. Using defaults.");
                }
            }
            return _instance;
        }
    }
    
    public string ApiBase => $"{serverUrl}/api";
}
