using UnityEngine;

namespace Lean.Touch
{
	// This script will orbit the current GameObject around its parent
	[ExecuteInEditMode]
	public class LeanOrbit : MonoBehaviour
	{
		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreGuiFingers = true;

		[Tooltip("Allows you to force input with a specific amount of fingers (0 = any)")]
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
		
		[Tooltip("The current orbit distance")]
		public float Distance = 10.0f;
		
		[Tooltip("The minimum orbit distance")]
		public float DistanceMin = 1.0f;

		[Tooltip("The maximum orbit distance")]
		public float DistanceMax = 100.0f;

		[Tooltip("If you want the mouse wheel to simulate pinching then set the strength of it here")]
		[Range(-1.0f, 1.0f)]
		public float WheelSensitivity;

		protected virtual void LateUpdate()
		{
			// Get the fingers we want to use to rotate
			var fingers = LeanTouch.GetFingers(IgnoreGuiFingers, RequiredFingerCount);
			
			// Get the scaled average movement vector of these fingers
			var drag = LeanGesture.GetScaledDelta(fingers);

			// Modify the pitch and yaw values based on the dragDelta
			Pitch -= drag.y * Sensitivity;
			Yaw   += drag.x * Sensitivity;
			
			// Limit zoom to min/max values
			Pitch = Mathf.Clamp(Pitch, PitchMin, PitchMax);

			// Change the zoom based on pinch of all fingers
			Distance *= LeanGesture.GetPinchRatio(WheelSensitivity);

			// Limit zoom to min/max values
			Distance = Mathf.Clamp(Distance, DistanceMin, DistanceMax);

			// Rotate camera to pitch and yaw values
			transform.localRotation = Quaternion.Euler(Pitch, Yaw, 0.0f);

			// Reset position and move back by Distance
			transform.localPosition = Vector3.zero;

			transform.Translate(0.0f, 0.0f, -Distance);
		}
	}
}