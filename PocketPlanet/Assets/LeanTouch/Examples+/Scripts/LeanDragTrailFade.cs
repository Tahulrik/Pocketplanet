#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
	#define UNITY_OLD_LINE_RENDERER
#endif
using UnityEngine;
using System.Collections.Generic;

namespace Lean.Touch
{
	// This script will draw the path each finger has taken since it started being pressed
	public class LeanDragTrailFade : MonoBehaviour
	{
		// This class will store an association between a finger and a LineRenderer instance
		[System.Serializable]
		public class Link
		{
			// The finger associated with this link
			public LeanFinger Finger;

			// The LineRenderer instance associated with this link
			public LineRenderer Line;

			// The amount of seconds until this link disappears
			public float Life;
		}

		[Tooltip("The line prefab")]
		public LineRenderer Prefab;

		[Tooltip("The distance from the camera the line points will be spawned in world space")]
		public float Distance = 1.0f;

		[Tooltip("How many seconds it takes for each line to disappear after a finger is released")]
		public float FadeTime = 1.0f;

		public Color StartColor = Color.white;

		public Color EndColor = Color.white;

		// This stores all the links between fingers and LineRenderer instances
		private List<Link> links = new List<Link>();
	
		protected virtual void OnEnable()
		{
			// Hook events
			LeanTouch.OnFingerSet += OnFingerSet;
			LeanTouch.OnFingerUp  += OnFingerUp;
		}

		protected virtual void OnDisable()
		{
			// Unhook events
			LeanTouch.OnFingerSet -= OnFingerSet;
			LeanTouch.OnFingerUp  -= OnFingerUp;
		}

		protected virtual void Update()
		{
			// Loop through all links
			for (var i = 0; i < links.Count; i++)
			{
				var link = links[i];

				// Has this link's finger been unlinked? (via OnFingerUp)
				if (link.Finger == null)
				{
					// Remove life from the link
					link.Life -= Time.deltaTime;

					// Is the link still alive?
					if (link.Life > 0.0f)
					{
						// Make sure FadeTime is set to prevent divide by 0
						if (FadeTime > 0.0f)
						{
							// Find the life to FadeTime 0..1 ratio
							var ratio = link.Life / FadeTime;

							// Copy the start & end colors and fade them by the ratio
							var color0 = StartColor;
							var color1 =   EndColor;
							
							color0.a *= ratio;
							color1.a *= ratio;

							// Write the new colors
#if UNITY_OLD_LINE_RENDERER
							link.Line.SetColors(color0, color1);
#else
							link.Line.startColor = color0;
							link.Line.endColor   = color1;
#endif
						}
					}
					// Kill the link?
					else
					{
						// Remove link from list
						links.Remove(link);

						// Destroy line GameObject
						Destroy(link.Line.gameObject);
					}
				}
			}
		}

		// Override the WritePositions method from LeanDragLine
		protected virtual void WritePositions(LineRenderer line, LeanFinger finger)
		{
			// Reserve one vertex for each snapshot
#if UNITY_OLD_LINE_RENDERER
			line.SetVertexCount(finger.Snapshots.Count);
#else
			line.positionCount = finger.Snapshots.Count;
#endif
			
			// Loop through all snapshots
			for (var i = 0; i < finger.Snapshots.Count; i++)
			{
				var snapshot = finger.Snapshots[i];
				
				// Get the world postion of this snapshot
				var position = snapshot.GetWorldPosition(Distance);

				// Write position
				line.SetPosition(i, position);
			}
		}

		private void OnFingerSet(LeanFinger finger)
		{
			// Make sure the prefab exists
			if (Prefab != null)
			{
				// Try and find the link for this finger
				var link = links.Find(l => l.Finger == finger);

				// Link doesn't exist?
				if (link == null)
				{
					// Make new link
					link = new Link();

					// Assign this finger to this link
					link.Finger = finger;

					// Create LineRenderer instance for this link
					link.Line = Instantiate(Prefab);

					// Write the start and end colors (because you can't read them??)
#if UNITY_OLD_LINE_RENDERER
					link.Line.SetColors(StartColor, EndColor);
#else
					link.Line.startColor = StartColor;
					link.Line.endColor   = EndColor;
#endif
					// Add new link to list
					links.Add(link);
				}

				WritePositions(link.Line, link.Finger);
			}
		}

		private void OnFingerUp(LeanFinger finger)
		{
			// Try and find the link for this finger
			var link = links.Find(l => l.Finger == finger);

			// Link exists?
			if (link != null)
			{
				// Unlink finger
				link.Finger = null;

				// Assign life
				link.Life = FadeTime;
			}
		}
	}
}