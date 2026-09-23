using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;

public class ARSessionSetup : MonoBehaviour
{
    public ARSession arSession;
    public XROrigin arSessionOrigin;
    public ARRaycastManager arRaycastManager;
    public ARPlaneManager arPlaneManager;
    public ARAnchorManager arAnchorManager;

    private ARSessionState _previousState;

    void Awake()
    {
        if (arSession != null)
            arSession.enabled = true;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void OnEnable()
    {
        ARSession.stateChanged += OnARSessionStateChanged;
    }

    void OnDisable()
    {
        ARSession.stateChanged -= OnARSessionStateChanged;
    }

    void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
    {
        switch (args.state)
        {
            case ARSessionState.SessionTracking:
                Debug.Log("[AR] Session tracking active");
                break;
            case ARSessionState.Unsupported:
                Debug.Log("[AR] ARCore is unsupported on this device");
                break;
            case ARSessionState.NeedsInstall:
                Debug.Log("[AR] ARCore needs installation");
                break;
        }

        if (_previousState == ARSessionState.SessionTracking && args.state != ARSessionState.SessionTracking)
        {
            Debug.Log($"[AR] Tracking lost (transitioned from Tracking to {args.state})");
        }

        _previousState = args.state;
    }
}
