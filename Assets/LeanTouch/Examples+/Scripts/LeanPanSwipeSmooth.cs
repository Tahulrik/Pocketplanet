using UnityEngine;

namespace Lean.Touch
{
	// This script allows you to pan the current GameObject when you swipe
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class LeanPanSwipeSmooth : MonoBehaviour
	{
		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreGuiFingers = true;
		
		[Tooltip("Does translation require an object to be selected?")]
		public LeanSelectable RequiredSelectable;

		[Tooltip("The camera used to calculate the pan direction (default = Main Camera)")]
		public Camera Camera;

		[Tooltip("The movement distance when swiping horizontally")]
		public float HorizontalSensitivity = 1.0f;

		[Tooltip("The movement distance when swiping vertically")]
		public float VerticalSensitivity = 1.0f;

		[Tooltip("How quickly this object moves to its target position")]
		public float Dampening = 10.0f;
		
		// The position we still need to add
		protected Vector3 remainingDelta;

		protected virtual void OnEnable()
		{
			LeanTouch.OnFingerSwipe += OnFingerSwipe;
		}

		protected virtual void Update()
		{
			// The framerate independent damping factor
			var factor = Mathf.Exp(-Dampening * Time.deltaTime);

			// Dampen remainingDelta
			var newDelta = remainingDelta * factor;

			// Shift this transform by the change in delta
			transform.position += remainingDelta - newDelta;

			// Update remainingDelta with the dampened value
			remainingDelta = newDelta;
		}

		protected virtual void OnDisable()
		{
			LeanTouch.OnFingerSwipe -= OnFingerSwipe;
		}
		
		protected virtual void OnFingerSwipe(LeanFinger finger)
		{
			// Ignore this swipe?
			if (IgnoreGuiFingers == true && finger.StartedOverGui == true)
			{
				return;
			}

			// If we require a selectable and it isn't selected, cancel swipe
			if (RequiredSelectable != null && RequiredSelectable.IsSelected == false)
			{
				return;
			}
			
			// Does a camera exist?
			if (LeanTouch.GetCamera(ref Camera) == true)
			{
				var swipe = finger.SwipeScreenDelta;

				if (swipe.x < -Mathf.Abs(swipe.y))
				{
					remainingDelta += Camera.transform.right * HorizontalSensitivity;
				}
				
				if (swipe.x > Mathf.Abs(swipe.y))
				{
					remainingDelta -= Camera.transform.right * HorizontalSensitivity;
				}
				
				if (swipe.y < -Mathf.Abs(swipe.x))
				{
					remainingDelta += Camera.transform.up * HorizontalSensitivity;
				}
				
				if (swipe.y > Mathf.Abs(swipe.x))
				{
					remainingDelta -= Camera.transform.up * HorizontalSensitivity;
				}
			}
		}
	}
}