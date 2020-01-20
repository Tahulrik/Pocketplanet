using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean;
using Lean.Touch;
using Cinemachine;

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
    CinemachineVirtualCamera vCam;


    GameObject CameraHolder;
    Camera mainCam;
    [Range(0,1)]
    public float currentZoomAmount = 0;

    public CameraState currentCameraState;
    /// <summary>Pitch of the rotation in degrees.</summary>
    [Tooltip("Pitch of the rotation in degrees.")]
    public float Pitch;

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
        CameraHolder = mainCam.transform.parent.gameObject;
        
        vCam = GetComponentInChildren<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var fingers = LeanTouch.Fingers;
        if (fingers.Count > 1)
        { 
        
        }
        CommandMoveCamera();
        switch (currentCameraState)
        {
            case CameraState.CityView:
                //Hold and movein x-y directions
                break;
            case CameraState.PlanetView:
                //Rotate Camera?
                //Swipe Rotate
                //Double tap to zoom to point
                break;
            case CameraState.Zooming:
                ZoomCamera();
                break;
            case CameraState.MovingTo:
                //From doubleclick
                break;
            case CameraState.EventView:
                //From Event

                //Moveto the same as Movingto - keep state change though
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

        if (currentZoomAmount > 0.5f)
        {
            currentCameraState = CameraState.CityView;
        }
        else
        {
            currentCameraState = CameraState.PlanetView;
        }
    }

    void ZoomCamera()
    {
        var fingers = Use.GetFingers();
        var zoomAmount = LeanGesture.GetPinchScale(fingers);

       
        if (zoomAmount != 1.0f) //Fingers changed position
        {
            var screenPoint = default(Vector2);

            if (LeanGesture.TryGetScreenCenter(fingers, ref screenPoint) == true)
            {
                currentZoomAmount *= zoomAmount;

                currentZoomAmount = Mathf.Clamp(currentZoomAmount, 0.1f, 1f);
                Vector3 newCamPos = CameraHolder.transform.position;
                float lerpAmount = Mathf.Clamp01(CameraZoomMoveCurve.Evaluate(currentZoomAmount));
                newCamPos.y = Mathf.Lerp(0, planetRadius, lerpAmount);
                vCam.m_Lens.OrthographicSize = Mathf.Lerp(NearestZoom, FarthestZoom, 1 - lerpAmount);

                CameraHolder.transform.position = newCamPos;
            }
        }
    }

    void CommandMoveCamera()
    {
        if (LeanTouch.Fingers.Count > 1)
        {
            return;
        }
        var fingers = Use.GetFingers();
        var distance = LeanGesture.GetScreenDelta(fingers).magnitude;
        print(distance);
        
    }

    void ResetCamera()
    { 
    
    }

    bool IsCurrentActionManual()
    {
        if (currentCameraState == CameraState.EventView || currentCameraState == CameraState.MovingTo)
        {
            return false;
        }
        return true;
    }

}
