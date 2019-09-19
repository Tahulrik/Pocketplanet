using UnityEngine;

namespace Lean.Touch
{
	// This script will orbit the current GameObject and allow you to zoom the attached camera's fieldOfView/orthographicSize
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class LeanOrbitCamera : MonoBehaviour
	{
		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreGuiFingers = true;

		[Tooltip("Allows you to force rotation with a specific amount of fingers (0 = any)")]
		public int RequiredFingerCount;
		
		[Tooltip("Sensitivity of the rotation")]
		public float Sensitivity = 0.25f;

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
		
		protected virtual void LateUpdate()
		{
			// Get the fingers we want to use
			var fingers = LeanTouch.GetFingers(IgnoreGuiFingers, RequiredFingerCount);
			
			// Get the scaled average movement vector of these fingers
			var drag = LeanGesture.GetScaledDelta(fingers);

			// Modify the pitch and yaw values based on the dragDelta
			Pitch -= drag.y * Sensitivity;
			Yaw   += drag.x * Sensitivity;

			// Limit pitch so the orbit doesn't go upside down
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
	}
}