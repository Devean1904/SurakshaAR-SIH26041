using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SurfaceDetector : MonoBehaviour
{
    public static SurfaceDetector Instance { get; private set; }

    [Header("AR References")]
    [SerializeField] private ARRaycastManager _arRaycastManager;
    [SerializeField] private ARPlaneManager _arPlaneManager;

    [Header("Settings")]
    [SerializeField] private float _maxRaycastDistance = 30f;
    [SerializeField] private GameObject _placementIndicatorPrefab;

    private GameObject _placementIndicator;
    private Pose _lastHitPose;
    private bool _hasDetectedSurface;
    private List<ARRaycastHit> _hits = new();

    public delegate void SurfaceDetected(Pose pose, ARPlane plane);
    public event SurfaceDetected OnSurfaceDetected;

    public delegate void SurfaceLost();
    public event SurfaceLost OnSurfaceLost;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (_arRaycastManager == null)
            _arRaycastManager = FindObjectOfType<ARRaycastManager>();
        if (_arPlaneManager == null)
            _arPlaneManager = FindObjectOfType<ARPlaneManager>();

        if (_placementIndicatorPrefab != null)
        {
            _placementIndicator = Instantiate(_placementIndicatorPrefab);
            _placementIndicator.SetActive(false);
        }

        if (_arPlaneManager != null)
        {
            _arPlaneManager.planesChanged += OnPlanesChanged;
        }

        Debug.Log("[SurfaceDetector] Initialized");
    }

    void OnDestroy()
    {
        if (_arPlaneManager != null)
            _arPlaneManager.planesChanged -= OnPlanesChanged;
    }

    void Update()
    {
        if (_arRaycastManager == null) return;

        UpdateScreenCenterRaycast();
    }

    void UpdateScreenCenterRaycast()
    {
        Vector2 screenCenter = new(Screen.width / 2f, Screen.height / 2f);

        _hits.Clear();
        if (_arRaycastManager.Raycast(screenCenter, _hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = _hits[0].pose;
            var hitPlane = GetPlaneFromHit(_hits[0]);

            _lastHitPose = hitPose;
            _hasDetectedSurface = true;

            if (_placementIndicator != null)
            {
                _placementIndicator.SetActive(true);
                _placementIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }
        }
        else
        {
            _hasDetectedSurface = false;
            if (_placementIndicator != null)
                _placementIndicator.SetActive(false);
        }
    }

    public bool TryRaycast(Vector2 screenPosition, out Pose pose, out ARPlane plane)
    {
        pose = Pose.identity;
        plane = null;

        if (_arRaycastManager == null) return false;

        _hits.Clear();
        if (_arRaycastManager.Raycast(screenPosition, _hits, TrackableType.PlaneWithinPolygon))
        {
            pose = _hits[0].pose;
            plane = GetPlaneFromHit(_hits[0]);
            return true;
        }
        return false;
    }

    public bool TryRaycastVertical(Vector2 screenPosition, out Pose pose, out ARPlane plane)
    {
        pose = Pose.identity;
        plane = null;

        if (_arRaycastManager == null) return false;

        _hits.Clear();
        if (_arRaycastManager.Raycast(screenPosition, _hits, TrackableType.PlaneWithinPolygon |
            TrackableType.PlaneWithinBounds))
        {
            var hitPlane = GetPlaneFromHit(_hits[0]);
            if (hitPlane != null && hitPlane.alignment == PlaneAlignment.Vertical)
            {
                pose = _hits[0].pose;
                plane = hitPlane;
                return true;
            }

            pose = _hits[0].pose;
            plane = hitPlane;
            return true;
        }
        return false;
    }

    ARPlane GetPlaneFromHit(ARRaycastHit hit)
    {
        foreach (var plane in _arPlaneManager.trackables)
        {
            if (plane.trackableId == hit.trackableId)
                return plane;
        }
        return null;
    }

    public List<ARPlane> GetAllPlanes()
    {
        var planes = new List<ARPlane>();
        if (_arPlaneManager == null) return planes;

        foreach (var plane in _arPlaneManager.trackables)
        {
            planes.Add(plane);
        }
        return planes;
    }

    public List<ARPlane> GetPlanesByType(int alignment)
    {
        var planes = new List<ARPlane>();
        if (_arPlaneManager == null) return planes;

        foreach (var plane in _arPlaneManager.trackables)
        {
            if ((int)plane.alignment == alignment)
                planes.Add(plane);
        }
        return planes;
    }

    public List<ARPlane> GetHorizontalPlanes() => GetPlanesByType(10);
    public List<ARPlane> GetVerticalPlanes() => GetPlanesByType(20);

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach (var plane in args.added)
        {
            Debug.Log($"[SurfaceDetector] Plane detected: {plane.alignment} size={plane.size.x:F2}x{plane.size.y:F2}");
            OnSurfaceDetected?.Invoke(
                new Pose(plane.center, plane.transform.rotation),
                plane);
        }

        if (args.removed.Count > 0)
        {
            OnSurfaceLost?.Invoke();
        }
    }

    public Pose GetLastHitPose() => _lastHitPose;
    public bool HasDetectedSurface() => _hasDetectedSurface;
    public ARPlaneManager GetPlaneManager() => _arPlaneManager;
}