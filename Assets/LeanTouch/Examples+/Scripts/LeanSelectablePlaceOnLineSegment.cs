using UnityEngine;

namespace Lean.Touch
{
	// This script will place this GameObject along a line segment when selected
	public class LeanSelectablePlaceOnLineSegment : LeanSelectableBehaviour
	{
		[Tooltip("The camera we will be used")]
		public Camera Camera;

		[Tooltip("A start point on the line in world space")]
		public Vector3 LineStart;

		[Tooltip("The end point of the line in world space")]
		public Vector3 LineEnd = Vector3.right;

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
					// Offset finger screen position
					var screenPosition = finger.ScreenPosition + screenOffset;

					// Find offset ray for this finger
					var ray = Camera.ScreenPointToRay(screenPosition);
					
					var direction = LineEnd - LineStart;

					// Make sure they're not parallel
					if (ray.direction != direction.normalized)
					{
						// Find distance along LinePoint & LineDirection that is closes to the ray
						var a = Vector3.Cross(ray.direction, direction);
						var b = Vector3.Cross(LineStart - ray.origin, ray.direction);
						var d = Vector3.Dot(b, a) / Vector3.Dot(a, a);

						// Place this GameObject there
						transform.position = LineStart + direction * Mathf.Clamp01(d);
					}
				}
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.DrawLine(LineStart, LineEnd);
		}
#endif
	}
}