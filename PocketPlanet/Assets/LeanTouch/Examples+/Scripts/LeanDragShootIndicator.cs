#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
	#define UNITY_OLD_LINE_RENDERER
#endif
using UnityEngine;

namespace Lean.Touch
{
	// This script will draw a line between the start and current finger points and change the thickness based on the distance
	public class LeanDragShootIndicator : LeanDragLine
	{
		[Tooltip("The thickness of the indicator")]
		public float Thickness = 0.1f;

		[Tooltip("The length of the indicator")]
		public float Length = 1.0f;
		
		protected override void WritePositions(LineRenderer line, LeanFinger finger)
		{
			// Get start and current world position of finger
			var start = finger.GetStartWorldPosition(Distance);
			var end   = finger.GetWorldPosition(Distance);

			if (start != end)
			{
				// Change the distance of the end point
				var direction = Vector3.Normalize(end - start);
				
				end = start + direction * Length;

				// Write positions
#if UNITY_OLD_LINE_RENDERER
			line.SetVertexCount(2);

			line.SetWidth(Thickness, Thickness);
#else
			line.positionCount = 2;

			line.startWidth = Thickness;
			line.endWidth   = Thickness;
#endif

				line.SetPosition(0, start);
				line.SetPosition(1, end);
			}
		}
	}
}