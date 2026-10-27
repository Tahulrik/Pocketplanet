using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using PocketPlanet.Planet;
using UnityEngine;

public enum CameraState
{
    PlanetView, //Used when the camera is zoomed out viewing the planet
    CityView, //Used when the camera is near the city
    Zooming, //Used when the camera is currently zooming
    MovingTo, //Used When Camera must move towards a position - either from snapping or from double click zoom
    EventView //Used When Camera must move to look at an event happening
}

public class CameraController : MonoBehaviour
{
    const float MinZoomLevel = 0.1f;

    public LeanFingerFilter Use = new LeanFingerFilter(true);

    public float PlanetRadius = 8;

    public float FarthestZoom;
    public float NearestZoom;
    public AnimationCurve CameraZoomMoveCurve;
    public Unity.Cinemachine.CinemachineVirtualCamera VCam;

    public GameObject CameraHolder;
    public GameObject CameraTarget;
    public Camera MainCam;
    [Range(0.1f, 1)]
    public float CurrentZoomAmount = 0.1f;
    [Range(0.15f, 0.95f), Tooltip("Below this zoom level the camera snaps back out to the whole planet when released.")]
    public float ZoomLevelSnapThreshold = 0.5f;
    [Tooltip("How far below and above the surface the camera can pan, in world units.")]
    public float CameraMinMoveHeight = 0f, CameraMaxMoveHeight = 6f;

    [SerializeField, Range(0.5f, 1f), Tooltip("Zoom level used when double-tapping into the city or following someone.")]
    float comfortableZoom = 0.72f;
    [SerializeField, Range(0.02f, 0.5f), Tooltip("View size change per mouse wheel notch.")]
    float wheelZoomStep = 0.18f;
    [SerializeField] float zoomSnapSeconds = 0.8f;
    [SerializeField, Tooltip("Pause after the last wheel notch before a zoom below the snap threshold settles in or out.")]
    float wheelSettleSeconds = 0.4f;

    public CameraState CurrentCameraState;

    [Tooltip("This is set to true the frame a multi-tap occurs.")]
    public bool MultiTap;
    [Tooltip("This is set to the current multi-tap count.")]
    public int MultiTapCount;
    [Tooltip("Highest number of fingers held down during this multi-tap.")]
    public int HighestFingerCount;

    float age;
    int lastFingerCount;
    float lastWheelTime = float.NegativeInfinity;
    float lastWheelDirection;
    Transform followTarget;
    ZoomScale zoomScale;

    public float ZoomedOutAmount => 1f - CurrentZoomAmount;

    bool IsCurrentActionManual => CurrentCameraState != CameraState.EventView && CurrentCameraState != CameraState.MovingTo;

    bool IsZoomedIn => CurrentZoomAmount >= ZoomLevelSnapThreshold;

    void OnEnable()
    {
        LeanTouch.OnFingerDown += CommandSetFinger;
        LeanTouch.OnFingerUp += CommandRemoveFinger;
    }

    void OnDisable()
    {
        LeanTouch.OnFingerDown -= CommandSetFinger;
        LeanTouch.OnFingerUp -= CommandRemoveFinger;
    }

    void Start()
    {
        InteractionTypes.InteractionType.ChangeModeToCamera();
        FitToPlanet();
        MainCam = Camera.main;
        VCam = GetComponentInChildren<Unity.Cinemachine.CinemachineVirtualCamera>();
        zoomScale = new ZoomScale(NearestZoom, FarthestZoom, MinZoomLevel);
        SetZoomLevel(CurrentZoomAmount);
    }

    void FitToPlanet()
    {
        var planetPosition = Planet.PlanetObject.transform.position;
        transform.position = new Vector3(planetPosition.x, planetPosition.y, transform.position.z);
        FarthestZoom = PlanetScale.FromReference(FarthestZoom, PlanetRadius, Planet.Radius);
        PlanetRadius = Planet.Radius;
    }

    public void Follow(Transform target)
    {
        followTarget = target;
        StopAllCoroutines();
        if (!IsZoomedIn)
            SetZoomLevel(comfortableZoom);
        SetCameraZoomState();
    }

    public void StopFollowing() => followTarget = null;

    void LateUpdate()
    {
        HandleWheelZoom();
        switch (CurrentCameraState)
        {
            case CameraState.CityView:
                DragAroundPlanet(allowHeight: true);
                FollowTarget();
                break;
            case CameraState.PlanetView:
                var fingers = Use.GetFingers(true);
                if (CheckForDoubleTap() && fingers.Count > 0)
                {
                    var targetPos = fingers[0].GetWorldPosition(0f);
                    targetPos.z = 0;
                    StartCoroutine(MoveCameraToSelectedPosition(targetPos));
                    break;
                }
                DragAroundPlanet(allowHeight: false);
                break;
            case CameraState.Zooming:
                ZoomCamera();
                break;
        }
    }

    void CommandSetFinger(LeanFinger finger)
    {
        if (!IsCurrentActionManual)
            return;
        if (LeanTouch.Fingers.Count > 1)
            CurrentCameraState = CameraState.Zooming;
    }

    void CommandRemoveFinger(LeanFinger finger)
    {
        if (!IsCurrentActionManual)
            return;
        ReleaseZoom();
    }

    void ZoomCamera()
    {
        if (BuildRowSwipeInput.IsSwiping)
            return;
        var pinch = LeanGesture.GetPinchScale(PinchFingers());
        if (pinch != 1f && pinch > 0f)
            ScaleView(1f / pinch);
    }

    // A pinch counts as long as one finger started on the world; a second finger landing on
    // on-screen UI (common with the editor's mirrored pinch simulation) must not cancel it.
    static List<LeanFinger> PinchFingers()
    {
        var fingers = LeanTouch.GetFingers(false, false);
        foreach (var finger in fingers)
            if (!finger.StartedOverGui)
                return fingers;
        return new List<LeanFinger>();
    }

    void HandleWheelZoom()
    {
        if (!IsCurrentActionManual || BuildRowDesktopInput.IsLayerWheelHeld || LeanTouch.Fingers.Count > 0
            || LeanTouch.PointOverGui(Input.mousePosition))
            return;
        var wheel = Input.mouseScrollDelta.y;
        if (wheel != 0f)
        {
            ScaleView(Mathf.Pow(1f - wheelZoomStep, wheel));
            lastWheelTime = Time.unscaledTime;
            lastWheelDirection = Mathf.Sign(wheel);
            SetCameraZoomState();
        }
        else if (!IsZoomedIn && CurrentZoomAmount > MinZoomLevel
            && Time.unscaledTime - lastWheelTime > wheelSettleSeconds)
        {
            StartCoroutine(AnimateZoomTo(lastWheelDirection > 0f ? comfortableZoom : MinZoomLevel));
        }
    }

    void ScaleView(float orthoFactor) => SetZoomLevel(zoomScale.LevelAfterScaling(CurrentZoomAmount, orthoFactor));

    void SetZoomLevel(float newZoomLevel)
    {
        CurrentZoomAmount = zoomScale.ClampLevel(newZoomLevel);
        var lerpAmount = Mathf.Clamp01(CameraZoomMoveCurve.Evaluate(CurrentZoomAmount));
        CameraHolder.transform.localPosition = new Vector3(0f, Mathf.Lerp(0f, PlanetRadius, lerpAmount), 0f);
        VCam.m_Lens.OrthographicSize = zoomScale.OrthoAt(CurrentZoomAmount);

        if (!IsZoomedIn)
        {
            CentreCameraOnOrigin();
            StopFollowing();
        }
    }

    // One finger grabs the planet: moving around the centre spins the view, moving away from it pans up into the sky.
    void DragAroundPlanet(bool allowHeight)
    {
        var fingers = Use.GetFingers(true);
        if (fingers.Count != 1 || fingers[0].ScreenDelta == Vector2.zero)
            return;

        StopFollowing();
        var finger = fingers[0];
        Vector2 center = transform.position;
        Vector2 from = PlanetPlane.FromScreen(finger.LastScreenPosition);
        Vector2 to = PlanetPlane.FromScreen(finger.ScreenPosition);

        transform.Rotate(0f, 0f, -Vector2.SignedAngle(from - center, to - center));
        if (allowHeight)
            PanHeight(-((to - center).magnitude - (from - center).magnitude));
    }

    void PanHeight(float delta)
    {
        var local = CameraTarget.transform.localPosition;
        local.x = 0f;
        local.y = Mathf.Clamp(local.y + delta, CameraMinMoveHeight, CameraMaxMoveHeight);
        CameraTarget.transform.localPosition = local;
    }

    void FollowTarget()
    {
        if (followTarget == null)
            return;
        Vector2 offset = followTarget.position - transform.position;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, offset);
        var heightAboveCameraBase = offset.magnitude - CameraHolder.transform.localPosition.y;
        var local = CameraTarget.transform.localPosition;
        local.y = Mathf.Clamp(heightAboveCameraBase, CameraMinMoveHeight, CameraMaxMoveHeight);
        CameraTarget.transform.localPosition = local;
    }

    void ReleaseZoom()
    {
        if (IsZoomedIn || CurrentZoomAmount <= MinZoomLevel)
        {
            SetCameraZoomState();
            return;
        }
        StartCoroutine(AnimateZoomTo(MinZoomLevel));
    }

    IEnumerator AnimateZoomTo(float level)
    {
        CurrentCameraState = CameraState.MovingTo;
        var start = CurrentZoomAmount;
        for (float t = 0f; t < 1f; t += Time.deltaTime / zoomSnapSeconds)
        {
            SetZoomLevel(Mathf.Lerp(start, level, Mathf.SmoothStep(0f, 1f, t)));
            yield return null;
        }
        SetZoomLevel(level);
        SetCameraZoomState();
    }

    IEnumerator MoveCameraToSelectedPosition(Vector2 targetPosition)
    {
        CurrentCameraState = CameraState.MovingTo;
        var startRotation = transform.rotation;
        var endRotation = Quaternion.LookRotation(Vector3.forward, targetPosition - (Vector2)transform.position);
        var startZoom = CurrentZoomAmount;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 1.5f)
        {
            var eased = Mathf.SmoothStep(0f, 1f, t);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, eased);
            SetZoomLevel(Mathf.Lerp(startZoom, comfortableZoom, eased));
            yield return null;
        }
        transform.rotation = endRotation;
        SetZoomLevel(comfortableZoom);
        SetCameraZoomState();
    }

    void SetCameraZoomState()
    {
        if (IsZoomedIn)
        {
            CurrentCameraState = CameraState.CityView;
        }
        else
        {
            CurrentCameraState = CameraState.PlanetView;
            CentreCameraOnOrigin();
        }
    }

    void CentreCameraOnOrigin() => CameraTarget.transform.localPosition = Vector3.zero;

    bool CheckForDoubleTap()
    {
        var result = false;
        var fingerCount = GetFingerCount(Use.GetFingers());

        if (fingerCount > 0)
        {
            if (lastFingerCount == 0)
            {
                age = 0.0f;
                HighestFingerCount = fingerCount;
            }
            else if (fingerCount > HighestFingerCount)
            {
                HighestFingerCount = fingerCount;
            }
        }

        age += Time.unscaledDeltaTime;

        if (age <= LeanTouch.CurrentTapThreshold)
        {
            if (fingerCount == 0 && lastFingerCount > 0)
            {
                MultiTapCount += 1;
                result = MultiTapCount == 2;
            }
        }
        else
        {
            MultiTapCount = 0;
            HighestFingerCount = 0;
            MultiTap = false;
        }

        lastFingerCount = fingerCount;
        return result;
    }

    static int GetFingerCount(List<LeanFinger> fingers)
    {
        var count = 0;
        foreach (var finger in fingers)
            if (!finger.Up)
                count++;
        return count;
    }
}
