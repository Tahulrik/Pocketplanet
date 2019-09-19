using UnityEngine;

namespace Lean.Touch
{
	// This script modifies LeanOrbit to be smooth
	[ExecuteInEditMode]
	public class LeanOrbitSmooth : LeanOrbit
	{
		[Tooltip("How sharp the orbit value changes update")]
		public float Dampening = 3.0f;

		private float currentPitch;

		private float currentYaw;

		private float currentDistance;

		protected virtual void OnEnable()
		{
			currentPitch    = Pitch;
			currentYaw      = Yaw;
			currentDistance = Distance;
		}

		protected override void LateUpdate()
		{
			// Use the LateUpdate code from LeanOrbitCamera
			base.LateUpdate();
			
			// Lerp the current values to the target ones
			currentPitch    = Dampen(currentPitch   , Pitch   );
			currentYaw      = Dampen(currentYaw     , Yaw     );
			currentDistance = Dampen(currentDistance, Distance);

			// Rotate camera to pitch and yaw values
			transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0.0f);

			// Reset position and move back by Distance
			transform.localPosition = Vector3.zero;

			transform.Translate(0.0f, 0.0f, -currentDistance);
		}

		// Time independent dampened lerp
		private float Dampen(float from, float to)
		{
			var speed  = Dampening * Time.unscaledDeltaTime;
			var factor = Mathf.Exp(-speed);

			return Mathf.Lerp(to, from, factor);
		}
	}
}