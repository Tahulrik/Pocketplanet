using UnityEngine;

namespace Lean.Touch
{
	// This script shows you how you can check tos ee which part of the screen a finger is on, and work accordingly
	public class LeanDragThrow : MonoBehaviour
	{
		[Tooltip("The prefab you want to throw")]
		public GameObject Prefab;

		[Tooltip("The strength of the throw relative to the drag length")]
		public float ForceMultiplier = 1.0f;

		[Tooltip("The distance from the camera the prefab will be spawned from")]
		public float Distance = 1.0f;

		[Tooltip("Limit the length (0 = none)")]
		public float LengthLimit;

		protected virtual void OnEnable()
		{
			// Hook events
			LeanTouch.OnFingerUp += OnFingerUp;
		}
		
		protected virtual void OnDisable()
		{
			// Unhook events
			LeanTouch.OnFingerUp -= OnFingerUp;
		}
		
		public void OnFingerUp(LeanFinger finger)
		{
			if (finger.StartedOverGui == false)
			{
				// Start and end points of the drag
				var start    = finger.GetStartWorldPosition(Distance);
				var end      = finger.GetWorldPosition(Distance);
				var distance = Vector3.Distance(start, end);

				// Limit the length?
				if (LengthLimit > 0.0f && distance > LengthLimit)
				{
					var direction = Vector3.Normalize(end - start);

					distance = LengthLimit;
					end      = start + direction * distance;
				}

				// Vector between points
				var vector = end - start;

				// Angle between points
				var angle = Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;

				// Instance the prefab, position it at the start point, and rotate it to the vector
				var instance = Instantiate(Prefab);

				instance.transform.position = start;
				instance.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -angle);

				// Apply 3D force?
				var rigidbody3D = instance.GetComponent<Rigidbody>();
				
				if (rigidbody3D != null)
				{
					rigidbody3D.velocity = vector * ForceMultiplier;
				}

				// Apply 2D force?
				var rigidbody2D = instance.GetComponent<Rigidbody2D>();
				
				if (rigidbody2D != null)
				{
					rigidbody2D.velocity = vector * ForceMultiplier;
				}
			}
		}
	}
}