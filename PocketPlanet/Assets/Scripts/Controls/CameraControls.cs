using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionTypes;
using Lean.Touch;
using System.Linq;
using Citizens;
using UnityEngine.Audio;

public class CameraControls : MonoBehaviour {

    public delegate void ZoomEvent();

    public static event ZoomEvent ZoomedIn;
    public static event ZoomEvent ZoomedOut;

	//do the audiostuff through events in its own script




	[Tooltip("Ignore fingers with StartedOverGui?")]
	public bool IgnoreGuiFingers = true;

	[Tooltip("Allows you to force rotation with a specific amount of fingers (0 = any)")]
	public int RequiredFingerCount;

	[Tooltip("If you want the mouse wheel to simulate pinching then set the strength of it here")]
	[Range(-1.0f, 1.0f)]
	public float WheelSensitivity;

	Camera MainCamera;

	[Tooltip("The minimum FOV/Size we want to zoom to")]
	public float Minimum = 10.0f;

	[Tooltip("The maximum FOV/Size we want to zoom to")]
	public float Maximum = 60.0f;

	public float Dampening = 1.0f;
	public float HeldDampening = 10f;
	float originalDampening;

	public float CurrentOrbitDistance;
	public float MaxOrbitDistance = 10f, MinOrbitDistance = 0.1f;
	Vector3 startPosition;
	public Vector3 desiredPosition;
	float currentFov;
	public float zoomAmount;
	public float zoomSpeed = 1;
	float swipeSpeed;
	float cameraSpeed = 1;
    float[] audioWeights = new float[2] { 0, 1 };


	bool nearCity = false;


	void OnEnable()
	{
		LeanTouch.OnFingerHeldDown += HoldCamera;
		LeanTouch.OnFingerHeldSet += DragCamera;
		//LeanTouch.OnFingerSwipe += SwipeCamera;
	}

	void Awake()
	{

	}

	void Start ()
	{
		originalDampening = Dampening;
		InteractionType.ChangeModeToCamera ();

		if (LeanTouch.GetCamera (ref MainCamera) == true) {
			currentFov = GetCurrent ();
		}
		//set current zoom to a specific value?	

		//Not a nice solution so far but it should work
		Camera.main.cullingMask = ~(1<<10);


		CurrentOrbitDistance = 7.5f;
		startPosition = new Vector3 (Planet.PlanetObject.transform.position.x, Planet.PlanetObject.transform.position.y, MainCamera.transform.position.z);
		zoomAmount = currentFov / Maximum;
		CurrentOrbitDistance = Mathf.Lerp (MaxOrbitDistance, MinOrbitDistance, zoomAmount);
		MainCamera.transform.position = (MainCamera.transform.position - startPosition).normalized * CurrentOrbitDistance + startPosition;


	}

	protected virtual void LateUpdate()
	{
		if(LeanTouch.Fingers.Count < RequiredFingerCount) {
			if (swipeSpeed != 0) {
				swipeSpeed = Dampen(swipeSpeed, 0);

				if (InteractionType.currentInteraction == InteractionSetting.Camera) {
					OrbitCamera (swipeSpeed*cameraSpeed);
				}
			}

		}

	}

	void Update()
	{
		FindCameraViewHeight ();

		switch (InteractionType.currentInteraction) {
		case InteractionSetting.Building:
			
			break;
		case InteractionSetting.Camera:
			CurrentOrbitDistance = Mathf.Clamp (CurrentOrbitDistance, MinOrbitDistance, MaxOrbitDistance);
			if (LeanTouch.Fingers.Count == RequiredFingerCount) {
				ZoomCamera ();
				OrbitCamera (0);
			} 
			break;
		case InteractionSetting.Meteor:
			
			break;

		default:
			break;

		}
	}

	void DragCamera(LeanFinger finger)
	{
		if (LeanTouch.Fingers.Count >= RequiredFingerCount) {
			return;
		}

		// Ignore this swipe?
		if (IgnoreGuiFingers == true && finger.StartedOverGui == true)
		{
			return;
		}
			
		var speed = finger.StartScreenPosition.x- finger.ScreenPosition.x  < 0 ? -(finger.StartScreenPosition - finger.ScreenPosition).magnitude
			: (finger.StartScreenPosition - finger.ScreenPosition).magnitude;

		OrbitCamera (speed * Time.deltaTime*cameraSpeed);

	}

	void HoldCamera(LeanFinger finger)
	{
		Dampening = HeldDampening;
	}

	void SwipeCamera(LeanFinger finger)
	{
		if (LeanTouch.Fingers.Count >= RequiredFingerCount) {
			return;
		}

		// Ignore this swipe?
		if (IgnoreGuiFingers == true && finger.StartedOverGui == true)
		{
			return;
		}
		Dampening = originalDampening;
		var swipe = finger.SwipeScreenDelta;
		swipeSpeed = swipe.x > 0 ? swipe.magnitude : -swipe.magnitude;
	}

	void OrbitCamera(float angularSpeed)
	{
		MainCamera.transform.RotateAround (Planet.PlanetObject.transform.position, MainCamera.transform.forward, angularSpeed * Time.deltaTime);
	}

	void FindCameraViewHeight()
	{
		desiredPosition = (MainCamera.transform.position - startPosition).normalized * CurrentOrbitDistance + startPosition;
		MainCamera.transform.position = Vector3.MoveTowards(MainCamera.transform.position, desiredPosition,  zoomSpeed);
	}

	void ZoomCamera()
	{
		// If camera is null, try and get the main camera, return true if a camera was found
		if (LeanTouch.GetCamera(ref MainCamera) == true)
		{

			// Get the fingers we want to use
			var fingers = LeanTouch.GetFingers(IgnoreGuiFingers, RequiredFingerCount);

			// Store the current size/fov in a temp variable
			currentFov = GetCurrent();

			// Scale the current value based on the pinch ratio
			float pinchChange = LeanGesture.GetPinchRatio(fingers, WheelSensitivity);

			var newFov = currentFov*pinchChange;
			var FovChange = newFov - currentFov;

			currentFov += FovChange*1.5f;
			zoomSpeed = Mathf.Clamp((1000f*FovChange)*Time.deltaTime, 10, 100);
			// Clamp the current value to min/max values
			currentFov = Mathf.Clamp(currentFov, Minimum, Maximum);


			var threshold = 0.5f;
			var deductedMax = Maximum * threshold;

			zoomAmount = currentFov / Maximum;

            audioWeights[1] = zoomAmount;
            audioWeights[0] = 1 - zoomAmount;
            GameHandler.Instance.AudioManager.SetZoomLevelWeights(audioWeights);

			if (currentFov > deductedMax) {
				CurrentOrbitDistance = Mathf.Lerp (MaxOrbitDistance, MinOrbitDistance, zoomAmount);
				cameraSpeed = 5f;

				if (nearCity) {
					nearCity = false;

                    if (ZoomedOut != null)
                    {
                        //print("near city " + nearCity);
                        //audioWeights[0] = zoomAmount;
                        //audioWeights[1] = 1 - zoomAmount;
                        //GameHandler.Instance.AudioManager.SetZoomLevelWeights(audioWeights);
                        ZoomedOut();
                    }
                }

				//Show Farlod. Improve this to not run every frame
				Camera.main.cullingMask = ~(1<<10);
			} else {
				CurrentOrbitDistance = MaxOrbitDistance;
				cameraSpeed = 1f;

				if (!nearCity) {
					nearCity = true;


                    if (ZoomedIn != null)
                    {
                        //print("near city " + nearCity);
                        //audioWeights[0] = zoomAmount;
                        //audioWeights[1] = 1 - zoomAmount;
                        //GameHandler.Instance.AudioManager.SetZoomLevelWeights(audioWeights);
                        ZoomedIn();
                    }

				}
				//show closeLod
				Camera.main.cullingMask |= (1<<10);
			}
				
			SetCurrent(currentFov);
		}
	}

	private float GetCurrent()
	{
		if (MainCamera.orthographic == true)
		{
			return MainCamera.orthographicSize;
		}
		else
		{
			return MainCamera.fieldOfView;
		}
	}

	private void SetCurrent(float current)
	{
		if (MainCamera.orthographic == true)
		{
			MainCamera.orthographicSize = current;
		}
		else
		{
			MainCamera.fieldOfView = current;
		}
	}

	private float Dampen(float from, float to)
	{
		var speed  = Dampening * Time.unscaledDeltaTime;
		var factor = Mathf.Exp(-speed);

		return Mathf.Lerp(to, from, factor);
	}


}


