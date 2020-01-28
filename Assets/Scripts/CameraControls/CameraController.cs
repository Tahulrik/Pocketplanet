using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean;
using Lean.Touch;
using Cinemachine;
using System.Linq;

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
    public LeanFingerFilter Use = new LeanFingerFilter(true);

    public float PlanetRadius = 8;

    public float FarthestZoom;
    public float NearestZoom;
    public AnimationCurve CameraZoomMoveCurve;
    public CinemachineVirtualCamera VCam;


    public GameObject CameraHolder;
    public GameObject CameraTarget;
    public Camera MainCam;
    [Range(0.1f, 1)]
    public float CurrentZoomAmount = 0.1f;
    [Range(0.15f, 0.95f)]
    public float ZoomLevelSnapThreshold = 0.5f;
    public float CameraMinMoveHeight = 0f, CameraMaxMoveHeight = 1.2f, CameraMaxMoveWidth = 0.5f;

    [Range(10f, 50f)]
    public float CameraRotationSpeed = 25f;

    public CameraState CurrentCameraState;
    bool IsCameraReset
    {
        get {
            bool result = true;
            if (CameraTarget.transform.localPosition != Vector3.zero || CurrentZoomAmount != 0.1f || CurrentZoomAmount == 1f)
            {
                result = false;
            }

            return result;
        }
    }

    bool IsCurrentActionManual
    {
        get {
            if (CurrentCameraState == CameraState.EventView || CurrentCameraState == CameraState.MovingTo)
            {
                return false;
            }
            return true;
        }
    }


    [Tooltip("This is set to true the frame a multi-tap occurs.")]
    public bool MultiTap;

    /// <summary>This is set to the current multi-tap count.</summary>
    [Tooltip("This is set to the current multi-tap count.")]
    public int MultiTapCount;

    /// <summary>Highest number of fingers held down during this multi-tap.</summary>
    [Tooltip("Highest number of fingers held down during this multi-tap.")]
    public int HighestFingerCount;

    // Seconds at least one finger has been held down
    private float age;

    // Previous fingerCount
    private int lastFingerCount;

    float cameraDistance;

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

    // Start is called before the first frame update
    void Start()
    {
        MainCam = Camera.main;
        VCam = GetComponentInChildren<CinemachineVirtualCamera>();
        float cameraDistance = VCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance;
    }

    private void Update()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        switch (CurrentCameraState)
        {
            case CameraState.CityView:
                //Hold and movein x-y directions
                CommandMoveCamera();
                break;
            case CameraState.PlanetView:

                var fingers = Use.GetFingers(true);
                if (CheckForDoubleTap())
                {
                    var targetPos = fingers[0].GetWorldPosition(cameraDistance);
                    targetPos.z = 0;
                    StartCoroutine(MoveCameraToSelectedPosition(targetPos));
                }

                RotateCameraView(fingers);
                //Swipe Rotate
                //Double tap to zoom to point
                break;
            case CameraState.Zooming:
                ZoomCamera();
                break;
            case CameraState.MovingTo:
                break;
            case CameraState.EventView:
                //From Event

                //Keep the camera looking at an event, follow the event if necessary
                break;
        }
    }

    //Find out what State the camera should go to
    void CommandSetFinger(LeanFinger finger)
    {
        if (!IsCurrentActionManual)
        {
            return;
        }

        if (LeanTouch.Fingers.Count > 1)
        { 
            CurrentCameraState = CameraState.Zooming;
        }
    }

    //Find out what the resulting 
    void CommandRemoveFinger(LeanFinger finger)
    {
        if (!IsCurrentActionManual)
        {
            return;
        }

        ResetCamera();
    }

    #region ZoomFunctions
    void ZoomCamera()
    {
        var fingers = Use.GetFingers(true);
        var zoomAmount = LeanGesture.GetPinchScale(fingers);

        if (zoomAmount != 1.0f) //Fingers changed position
        {
            var screenPoint = default(Vector2);

            if (LeanGesture.TryGetScreenCenter(fingers, ref screenPoint) == true)
            {
                var newZoomLevel = CurrentZoomAmount * zoomAmount;

                SetZoomLevel(newZoomLevel);
            }
        }
    }

    void SetZoomLevel(float newZoomLevel)
    {
        var editedZoomLevel = Mathf.Clamp(newZoomLevel, 0.1f, 1f);
        Vector3 newCamPos = Vector3.zero;
        float lerpAmount = Mathf.Clamp01(CameraZoomMoveCurve.Evaluate(editedZoomLevel));
        newCamPos.y = Mathf.Lerp(0, PlanetRadius, lerpAmount);
        VCam.m_Lens.OrthographicSize = Mathf.Lerp(NearestZoom, FarthestZoom, 1 - editedZoomLevel);

        CameraHolder.transform.localPosition = newCamPos;
        CurrentZoomAmount = editedZoomLevel;

        if (CurrentZoomAmount < ZoomLevelSnapThreshold)
        {
            CentreCameraOnOrigin();
        }
    }
    #endregion

    #region MoveFunctions
    void CommandMoveCamera()
    {
        var fingers = Use.GetFingers(true);
        
        if (fingers.Count > 0)
        {
            //Move Cam in field
            MoveCameraInViewField(fingers);

            //Rotate planet if outside boundaries
            RotateCameraView(fingers);
        }
    }

    void MoveCameraInViewField(List<LeanFinger> fingers)
    {
        Vector3 targetPos = fingers[0].GetWorldPosition(cameraDistance);

        targetPos = CameraTarget.transform.InverseTransformPoint(targetPos);
        targetPos.x = Mathf.Clamp(targetPos.x, -CameraMaxMoveWidth, CameraMaxMoveWidth);
        targetPos.y = Mathf.Clamp(targetPos.y, CameraMinMoveHeight, CameraMaxMoveHeight);
        CameraTarget.transform.localPosition = targetPos;
    }

    void RotateCameraView(List<LeanFinger> fingers)
    {
        if (fingers.Count > 0) // refactor to have one reference for current fingers on screen
        {
            Vector3 fingerScreenPos = fingers[0].ScreenPosition;

            int leftBoundary = Screen.width / 100 * 20;
            int rightBoundary = Screen.width / 100 * 80;

            float rotationDirection = 0;
            if (fingerScreenPos.x < leftBoundary)
                rotationDirection = 1;
            else if (fingerScreenPos.x > rightBoundary)
                rotationDirection = -1;

            RotateCameraByAmount(CameraRotationSpeed * rotationDirection);
        }
    }

    void RotateCameraByAmount(float amount)
    {
        transform.Rotate(Vector3.forward * amount * Time.deltaTime);
    }
    #endregion

    #region SwipeCameraFunctions
    #endregion

    #region AutomaticMoveFunctions
    IEnumerator SnapCameraToZoomPosition()
    {
        CurrentCameraState = CameraState.MovingTo;
        float snapZoomCurrentLevel = CurrentZoomAmount;
        float zoomDir = (snapZoomCurrentLevel > ZoomLevelSnapThreshold) ? 1f : -1f;

        yield return new WaitUntil(() =>
        {
            snapZoomCurrentLevel += (Time.deltaTime * zoomDir)/1.5f; // this will always take 1.5 seconds - how do i make it take 1.5 seconds for the entire distance
            SetZoomLevel(snapZoomCurrentLevel);

            if (CurrentZoomAmount == 0.1f || CurrentZoomAmount == 1)
                return true;
            else
                return false;
        });

        SetCameraZoomState();
    }

    IEnumerator MoveCameraToSelectedPosition(Vector2 targetPosition)
    {
        CurrentCameraState = CameraState.MovingTo;
        float angleBetweenCameraAndPoint = 0;
        float CurrentRotation = transform.rotation.eulerAngles.z;
        float TargetRotation = 0;
        float RemainingLerp = 0;
        float moveCurveVal = 0f;
        var RemainingZoom = CurrentZoomAmount;
        Vector3 rotationEuler = Vector3.zero;

        angleBetweenCameraAndPoint = Vector2.SignedAngle(targetPosition, CameraHolder.transform.position);
        TargetRotation = -angleBetweenCameraAndPoint;
        RemainingLerp = 0;
        yield return new WaitUntil(() =>
        {
            RemainingLerp = Mathf.Clamp01(RemainingLerp + (Time.deltaTime / 2f));
            RemainingZoom = Mathf.Clamp01(RemainingZoom + (Time.deltaTime / 2f));

            moveCurveVal = CameraZoomMoveCurve.Evaluate(RemainingLerp) * TargetRotation;
            CurrentRotation += moveCurveVal * Time.deltaTime;
            rotationEuler = new Vector3(0, 0, CurrentRotation);
            Quaternion rot = transform.rotation;
            rot.eulerAngles = rotationEuler;
            transform.rotation = rot;

            SetZoomLevel(RemainingZoom);

            if (RemainingLerp < 1)
                return false;
            else
                return true;
        });

        SetCameraZoomState();
    }
    #endregion

    void ResetCamera()
    {
        SetCameraZoomState();

        if (IsCameraReset)
            return;

        StartCoroutine(SnapCameraToZoomPosition());
    }

    void SetCameraZoomState()
    {
        if (CurrentZoomAmount > ZoomLevelSnapThreshold)
        {
            CurrentCameraState = CameraState.CityView;
        }
        else
        {
            CurrentCameraState = CameraState.PlanetView;
            CentreCameraOnOrigin();
        }
    }

    void CentreCameraOnOrigin()
    {
        CameraTarget.transform.localPosition = Vector3.zero;
    }

    
    bool CheckForDoubleTap()
    {
        bool result = false;
        // Get fingers and calculate how many are still touching the screen
        var fingers = Use.GetFingers();
        var fingerCount = GetFingerCount(fingers);

        // At least one finger set?
        if (fingerCount > 0)
        {
            // Did this just begin?
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

        // Is a multi-tap still eligible?
        if (age <= LeanTouch.CurrentTapThreshold)
        {
            // All fingers released?
            if (fingerCount == 0 && lastFingerCount > 0)
            {
                MultiTapCount += 1;

                if (MultiTapCount == 2)
                {
                    result = true;
                }
            }
        }
        // Reset
        else
        {
            MultiTapCount = 0;
            HighestFingerCount = 0;
            MultiTap = false;
        }

        lastFingerCount = fingerCount;
        return result;
    }

    private int GetFingerCount(List<LeanFinger> fingers)
    {
        var count = 0;

        for (var i = fingers.Count - 1; i >= 0; i--)
        {
            if (fingers[i].Up == false)
            {
                count += 1;
            }
        }

        return count;
    }

}
