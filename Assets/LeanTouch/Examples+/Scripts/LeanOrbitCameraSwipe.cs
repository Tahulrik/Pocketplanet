using UnityEngine;

namespace Lean.Touch
{
	// This script will orbit the current GameObject
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class LeanOrbitCameraSwipe : MonoBehaviour
	{
		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreGuiFingers = true;

		[Tooltip("The amount the pitch/yaw changes with swipes in degrees")]
		public float SwipeAngle = 45.0f;

		[Tooltip("Pitch of the orbit in degrees")]
		public float Pitch;

		[Tooltip("The minimum pitch angle in degrees")]
		public float PitchMin = -90.0f;

		[Tooltip("The maximum pitch angle in degrees")]
		public float PitchMax = 90.0f;
		
		[Tooltip("Yaw of the orbit in degrees")]
		public float Yaw;

		[Tooltip("Distance of the orbit")]
		public float Distance = 10.0f;
		
		[Tooltip("The current zoom fieldOfView/orthographicSize")]
		public float Zoom = 10.0f;
		
		[Tooltip("The minimum camera fieldOfView/orthographicSize")]
		public float ZoomMin = 1.0f;

		[Tooltip("The maximum camera fieldOfView/orthographicSize")]
		public float ZoomMax = 100.0f;

		[Tooltip("If you want the mouse wheel to simulate pinching then set the strength of it here")]
		[Range(-1.0f, 1.0f)]
		public float WheelSensitivity;

		protected Camera thisCamera;

		protected virtual void OnEnable()
		{
			LeanTouch.OnFingerSwipe += OnFingerSwipe;
		}

		protected virtual void OnDisable()
		{
			LeanTouch.OnFingerSwipe -= OnFingerSwipe;
		}

		protected virtual void LateUpdate()
		{
			// Limit zoom to min/max values
			Pitch = Mathf.Clamp(Pitch, PitchMin, PitchMax);

			// Change the zoom based on pinch
			Zoom *= LeanGesture.GetPinchRatio(WheelSensitivity);

			// Limit zoom to min/max values
			Zoom = Mathf.Clamp(Zoom, ZoomMin, ZoomMax);

			// Rotate camera to pitch and yaw values
			transform.localRotation = Quaternion.Euler(Pitch, Yaw, 0.0f);

			// Reset position and move back by Distance
			transform.localPosition = Vector3.zero;

			transform.Translate(0.0f, 0.0f, -Distance);

			// Zoom camera
			if (thisCamera == null)
			{
				thisCamera = GetComponent<Camera>();
			}

			if (thisCamera.orthographic == true)
			{
				thisCamera.orthographicSize = Zoom;
			}
			else
			{
				thisCamera.fieldOfView = Zoom;
			}
		}

		private void OnFingerSwipe(LeanFinger finger)
		{
			// Ignore this swipe?
			if (IgnoreGuiFingers == true && finger.StartedOverGui == true)
			{
				return;
			}

			var swipe = finger.SwipeScreenDelta;

			if (swipe.x < -Mathf.Abs(swipe.y))
			{
				Yaw -= SwipeAngle;
			}
			
			if (swipe.x > Mathf.Abs(swipe.y))
			{
				Yaw += SwipeAngle;
			}
			
			if (swipe.y < -Mathf.Abs(swipe.x))
			{
				Pitch += SwipeAngle;
			}
			
			if (swipe.y > Mathf.Abs(swipe.x))
			{
				Pitch -= SwipeAngle;
			}
		}
	}
}