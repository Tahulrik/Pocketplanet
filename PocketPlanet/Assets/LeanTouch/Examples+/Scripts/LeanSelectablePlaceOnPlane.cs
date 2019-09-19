using UnityEngine;

namespace Lean.Touch
{
	// This script will place this GameObject on the surface of a plane when selected
	public class LeanSelectablePlaceOnPlane : LeanSelectableBehaviour
	{
		[Tooltip("The camera we will be used")]
		public Camera Camera;

		[Tooltip("A point on the plane in world space")]
		public Vector3 PlanePoint;

		[Tooltip("The normal of the plane in world space")]
		public Vector3 PlaneNormal = Vector3.up;

		private Vector2 screenOffset;

		protected override void OnSelect(LeanFinger finger)
		{
			base.OnSelect(finger);
			
			// Do we have a target camera?
			if (LeanTouch.GetCamera(ref Camera) == true)
			{
				// Calculate finger offset
				var screenPosition = (Vector2)Camera.WorldToScreenPoint(transform.position);

				screenOffset = screenPosition - finger.ScreenPosition;
			}
		}

		protected virtual void Update()
		{
			// Is this GameObject selected?
			if (Selectable.IsSelected == true)
			{
				// Does it have a selected finger?
				var finger = Selectable.SelectingFinger;

				if (finger != null && LeanTouch.GetCamera(ref Camera) == true)
				{
					// Make the plane we will place this GameObject on
					var plane = new Plane(PlaneNormal, PlanePoint);

					// Offset finger screen position
					var screenPosition = finger.ScreenPosition + screenOffset;

					// Find offset ray for this finger
					var ray = Camera.ScreenPointToRay(screenPosition);

					// Try and raycast onto the plane
					var distance = default(float);

					if (plane.Raycast(ray, out distance) == true)
					{
						// Place GameObject at hit point
						transform.position = ray.GetPoint(distance);
					}
				}
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			// Transform gizmos into plane space
			var rotation = Quaternion.FromToRotation(Vector3.up, PlaneNormal);

			Gizmos.matrix = Matrix4x4.TRS(PlanePoint, rotation, Vector3.one);

			// Draw plane with cross
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.0f, 0.0f, 1.0f));
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.0f, 0.0f, 0.0f));
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.0f, 0.0f, 1.0f));

			// Draw normal
			Gizmos.DrawRay(Vector3.zero, Vector3.up);
		}
#endif
	}
}