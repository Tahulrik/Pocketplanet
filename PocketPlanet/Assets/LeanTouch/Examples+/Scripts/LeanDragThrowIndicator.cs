#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
	#define UNITY_OLD_LINE_RENDERER
#endif
using UnityEngine;

namespace Lean.Touch
{
	// This script will draw a line between the start and current finger points and change the thickness based on the distance
	public class LeanDragThrowIndicator : LeanDragLine
	{
		[Tooltip("The thickness scale per unit")]
		public float ThicknessScale = 0.1f;

		[Tooltip("Limit the length (0 = none)")]
		public float LengthLimit;
		
		protected override void WritePositions(LineRenderer line, LeanFinger finger)
		{
			// Get start and current world position of finger
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

			// Write positions
			var thickness = distance * ThicknessScale;

#if UNITY_OLD_LINE_RENDERER
			line.SetVertexCount(2);

			line.SetWidth(thickness, thickness);
#else
			line.positionCount = 2;

			line.startWidth = thickness;
			line.endWidth   = thickness;
#endif

			line.SetPosition(0, start);
			line.SetPosition(1, end);
		}
	}
}