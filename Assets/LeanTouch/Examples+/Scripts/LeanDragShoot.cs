using UnityEngine;

namespace Lean.Touch
{
	// This script shows you how you can check tos ee which part of the screen a finger is on, and work accordingly
	public class LeanDragShoot : MonoBehaviour
	{
		[Tooltip("The prefab you want to throw")]
		public GameObject Prefab;

		[Tooltip("The strength of the throw")]
		public float Force = 1.0f;

		[Tooltip("The distance from the camera the prefab will be spawned from")]
		public float Distance = 1.0f;

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
				
				if (start != end)
				{
					// Vector between points
					var direction = Vector3.Normalize(end - start);

					// Angle between points
					var angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

					// Instance the prefab, position it at the start point, and rotate it to the vector
					var instance = Instantiate(Prefab);

					instance.transform.position = start;
					instance.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -angle);

					// Apply 3D force?
					var rigidbody3D = instance.GetComponent<Rigidbody>();
				
					if (rigidbody3D != null)
					{
						rigidbody3D.velocity = direction * Force;
					}

					// Apply 2D force?
					var rigidbody2D = instance.GetComponent<Rigidbody2D>();
				
					if (rigidbody2D != null)
					{
						rigidbody2D.velocity = direction * Force;
					}
				}
			}
		}
	}
}