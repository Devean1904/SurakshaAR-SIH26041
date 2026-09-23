using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using System.Collections.Generic;

public class ARSiteMapper : MonoBehaviour
{
    public static ARSiteMapper Instance { get; private set; }

    [Header("AR Components")]
    public XROrigin arSessionOrigin;
    public ARRaycastManager arRaycastManager;
    public ARAnchorManager arAnchorManager;

    [Header("Mapping")]
    public GameObject markerPrefab;
    public LineRenderer connectionLine;

    private List<ARAnchor> _placedAnchors = new();
    private List<SitePoint> _sitePoints = new();
    private bool _isMapping = false;
    private string _currentSiteName = "";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartMapping(string siteName)
    {
        _isMapping = true;
        _currentSiteName = siteName;
        _placedAnchors.Clear();
        _sitePoints.Clear();
        Debug.Log($"[SiteMapper] Started mapping: {siteName}");
    }

    public async void OnTapToPlace(Pose hitPose)
    {
        if (!_isMapping) return;

        var result = await arAnchorManager.TryAddAnchorAsync(hitPose);
        if (!result.status.IsSuccess()) return;

        var anchor = result.value;
        _placedAnchors.Add(anchor);

        var point = new SitePoint
        {
            Id = System.Guid.NewGuid().ToString(),
            Position = new float[] { hitPose.position.x, hitPose.position.y, hitPose.position.z },
            Rotation = new float[] { hitPose.rotation.x, hitPose.rotation.y, hitPose.rotation.z, hitPose.rotation.w }
        };
        _sitePoints.Add(point);

        if (markerPrefab != null)
        {
            Instantiate(markerPrefab, anchor.transform);
        }

        UpdateConnections();
        Debug.Log($"[SiteMapper] Point placed: {_sitePoints.Count} total");
    }

    void UpdateConnections()
    {
        if (connectionLine == null || _placedAnchors.Count < 2) return;
        connectionLine.positionCount = _placedAnchors.Count;
        for (int i = 0; i < _placedAnchors.Count; i++)
        {
            connectionLine.SetPosition(i, _placedAnchors[i].transform.position);
        }
    }

    public SiteMappingData StopMapping()
    {
        _isMapping = false;
        var firstAnchor = _placedAnchors.Count > 0 ? _placedAnchors[0] : null;

        var mapping = new SiteMappingData
        {
            Id = System.Guid.NewGuid().ToString(),
            SiteName = _currentSiteName,
            AdminId = PlayerPrefs.GetString("UserId", ""),
            Points = _sitePoints,
            AnchorPosition = firstAnchor != null
                ? new float[] { firstAnchor.transform.position.x, firstAnchor.transform.position.y, firstAnchor.transform.position.z }
                : new float[] { 0, 0, 0 },
            AnchorRotation = firstAnchor != null
                ? new float[] { firstAnchor.transform.rotation.x, firstAnchor.transform.rotation.y, firstAnchor.transform.rotation.z, firstAnchor.transform.rotation.w }
                : new float[] { 0, 0, 0, 1 },
            RecordedAt = System.DateTime.UtcNow.ToString("O")
        };

        LocalBlockchain.Instance.AddBlock("site_mapped", mapping.AdminId, $"Site: {_currentSiteName} with {_sitePoints.Count} points");
        Debug.Log($"[SiteMapper] Mapping complete: {_sitePoints.Count} points recorded");
        return mapping;
    }

    public void ClearMapping()
    {
        foreach (var anchor in _placedAnchors)
        {
            if (anchor != null) Destroy(anchor.gameObject);
        }
        _placedAnchors.Clear();
        _sitePoints.Clear();
        if (connectionLine != null) connectionLine.positionCount = 0;
    }
}

[System.Serializable]
public class SitePoint
{
    public string Id;
    public float[] Position;
    public float[] Rotation;
    public string ScenarioType;
    public string Description;
}

[System.Serializable]
public class SiteMappingData
{
    public string Id;
    public string AdminId;
    public string SiteName;
    public float[] AnchorPosition;
    public float[] AnchorRotation;
    public List<SitePoint> Points;
    public string RecordedAt;
}
