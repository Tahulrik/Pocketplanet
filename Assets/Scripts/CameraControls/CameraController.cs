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
    MovingTo, //Used When Camera must move towards double click position - event view might be sufficient
    EventView //Used When Camera must move to look at an event happening
}

public class CameraController : MonoBehaviour
{
    public LeanFingerFilter Use = new LeanFingerFilter(true);

    public float planetRadius;

    public float FarthestZoom;
    public float NearestZoom;
    public AnimationCurve CameraZoomMoveCurve;
    public CinemachineVirtualCamera vCam;


    public GameObject CameraHolder;
    public GameObject CameraTarget;
    public Camera mainCam;
    [Range(0,1)]
    public float currentZoomAmount = 0;

    public CameraState currentCameraState;

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
        mainCam = Camera.main;
        //vCam = GetComponentInChildren<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        
    }
    Vector3 targetPos;
    // Update is called once per frame
    void LateUpdate()
    {
        switch (currentCameraState)
        {
            case CameraState.CityView:
                //Hold and movein x-y directions
                CommandMoveCamera();
                break;
            case CameraState.PlanetView:

                var fingers = Use.GetFingers();
                /*if (CheckForDoubleTap())
                {
                    targetPos = fingers[0].GetWorldPosition(15);

                    //use event here
                    
                    currentCameraState = CameraState.MovingTo;
                }*/

                if (fingers.Count > 0)
                {
                    Vector3 fingerScreenPos = fingers[0].ScreenPosition;

                    int leftBoundary = Screen.width / 100 * 20;
                    int rightBoundary = Screen.width / 100 * 80;


                    if (fingerScreenPos.x < leftBoundary)
                    {
                        print("left");
                        transform.Rotate(Vector3.forward);
                        CameraTarget.transform.localPosition = new Vector3(targetPos.x, 0);
                        //rotate towards the left

                    }
                    else if (fingerScreenPos.x > rightBoundary)
                    {
                        print("right");
                        transform.Rotate(-Vector3.forward);
                        CameraTarget.transform.localPosition = new Vector3(targetPos.x, 0);
                        //rotate towards the right
                    }
                }
                

                //Rotate Camera?
                //Swipe Rotate
                //Double tap to zoom to point
                break;
            case CameraState.Zooming:
                ZoomCamera();
                break;
            case CameraState.MovingTo:

                transform.Rotate(Vector3.forward / 5);

                if (targetPos.x != CameraHolder.transform.position.x)
                {
                    Vector3 holderPos = CameraHolder.transform.position;
                    currentZoomAmount = Mathf.Lerp(0, 1, Vector3.Distance(targetPos, transform.position));
                    float lerpAmount = Mathf.Clamp01(CameraZoomMoveCurve.Evaluate(currentZoomAmount));
                    holderPos.y = Mathf.Lerp(0, planetRadius, lerpAmount);
                    vCam.m_Lens.OrthographicSize = Mathf.Lerp(NearestZoom, FarthestZoom, 1 - currentZoomAmount);

                    CameraHolder.transform.position = holderPos;
                }
                else 
                {
                    ResetCamera();
                }
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
        if (!IsCurrentActionManual())
        {
            return;
        }

        if (LeanTouch.Fingers.Count > 1)
        { 
            currentCameraState = CameraState.Zooming;
        }
    }

    //Find out what the resulting 
    void CommandRemoveFinger(LeanFinger finger)
    {
        if (!IsCurrentActionManual())
        {
            return;
        }


        ResetCamera();
    }

    void ZoomCamera()
    {
        var fingers = Use.GetFingers(true);
        var zoomAmount = LeanGesture.GetPinchScale(fingers);

       
        if (zoomAmount != 1.0f) //Fingers changed position
        {
            var screenPoint = default(Vector2);

            if (LeanGesture.TryGetScreenCenter(fingers, ref screenPoint) == true)
            {
                currentZoomAmount *= zoomAmount;

                currentZoomAmount = Mathf.Clamp(currentZoomAmount, 0.1f, 1f);
                Vector3 newCamPos = Vector3.zero;
                float lerpAmount = Mathf.Clamp01(CameraZoomMoveCurve.Evaluate(currentZoomAmount));
                newCamPos.y = Mathf.Lerp(0, planetRadius, lerpAmount);
                vCam.m_Lens.OrthographicSize = Mathf.Lerp(NearestZoom, FarthestZoom, 1- currentZoomAmount);

                CameraHolder.transform.localPosition = newCamPos;
            }
        }
    }

    void CommandMoveCamera()
    {
        var fingers = Use.GetFingers(true);
        
        if (fingers.Count > 0)
        {
            //Move Cam in field
            Vector3 targetPos = fingers[0].GetWorldPosition(15);

            targetPos = CameraTarget.transform.InverseTransformPoint(targetPos);
            targetPos.x = Mathf.Clamp(targetPos.x, -0.5f, 0.5f);
            targetPos.y = Mathf.Clamp(targetPos.y, 0, 0.5f);
            CameraTarget.transform.localPosition = targetPos;

            //Rotate planet if outside boundaries
            Vector3 fingerScreenPos = fingers[0].ScreenPosition;

            int leftBoundary = Screen.width / 100 * 20;
            int rightBoundary = Screen.width / 100 * 80;

            CameraTarget.transform.localRotation = Quaternion.identity;
            if (fingerScreenPos.x < leftBoundary)
            {
                print("left");
                transform.Rotate(Vector3.forward);
                //CameraTarget.transform.localPosition = new Vector3(targetPos.x, 0);
                //rotate towards the left

            }
            else if (fingerScreenPos.x > rightBoundary)
            {
                print("right");
                transform.Rotate(-Vector3.forward);
                //CameraTarget.transform.localPosition = new Vector3(targetPos.x, 0);
                //rotate towards the right
            }
        }

    }

    void ResetCamera()
    {
        if (currentZoomAmount > 0.5f)
        {
            currentCameraState = CameraState.CityView;
        }
        else
        {
            currentCameraState = CameraState.PlanetView;
        }


        //CameraTarget.transform.localPosition = Vector3.zero;
        //Remember to calculate the position of the camera target.
        /*if (currentCameraState == CameraState.PlanetView)
        {
            currentZoomAmount = 1;
        }
        else if (currentCameraState == CameraState.CityView)
        {
            currentZoomAmount = 0;
        }*/

    }

    bool IsCurrentActionManual()
    {
        if (currentCameraState == CameraState.EventView || currentCameraState == CameraState.MovingTo)
        {
            return false;
        }
        return true;
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

        // Reset
        MultiTap = false;

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
                    print("doubletap");
                }
            }
        }
        // Reset
        else
        {
            MultiTapCount = 0;
            HighestFingerCount = 0;
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
