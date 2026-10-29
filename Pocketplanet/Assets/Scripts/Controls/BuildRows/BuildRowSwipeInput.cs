using System.Collections.Generic;
using Lean.Touch;
using PocketPlanet.Planet;
using UnityEngine;

// Two fingers moving up together push the layer belt back, down pushes it forward.
// A pinch changes the finger spread a lot, a swipe barely does, which is how the two are told apart.
[RequireComponent(typeof(BuildRowSelection))]
public class BuildRowSwipeInput : MonoBehaviour
{
	[SerializeField, Range(0.02f, 0.3f), Tooltip("Vertical travel needed, as a fraction of screen height.")]
	float swipeDistance = 0.08f;
	[SerializeField, Range(0.1f, 1f), Tooltip("Largest spread change allowed, relative to the vertical travel.")]
	float maxSpreadRatio = 0.5f;

	BuildRowSelection selection;
	Vector2 travel;
	float startSpread;
	bool tracking;
	bool usedThisGesture;

	public static bool IsSwiping { get; private set; }

	void Awake() => selection = GetComponent<BuildRowSelection>();

	void OnEnable() => LeanTouch.OnGesture += OnGesture;

	void OnDisable()
	{
		LeanTouch.OnGesture -= OnGesture;
		IsSwiping = false;
	}

	void OnGesture(List<LeanFinger> fingers)
	{
		// Lifted fingers still appear for one frame; dropping them here is what ends the gesture,
		// since Lean sends no gesture event once every finger is gone.
		var touching = LeanTouch.GetFingers(true, false).FindAll(finger => !finger.Up);
		if (touching.Count != 2)
		{
			ResetGesture();
			return;
		}
		if (!tracking)
			BeginGesture(touching);
		else
			Track(touching);
	}

	void BeginGesture(List<LeanFinger> fingers)
	{
		tracking = true;
		usedThisGesture = false;
		travel = Vector2.zero;
		startSpread = LeanGesture.GetScreenDistance(fingers);
	}

	void Track(List<LeanFinger> fingers)
	{
		if (usedThisGesture)
			return;
		travel += LeanGesture.GetScreenDelta(fingers);
		var needed = swipeDistance * Screen.height;
		var spreadChange = Mathf.Abs(LeanGesture.GetScreenDistance(fingers) - startSpread);
		IsSwiping = Mathf.Abs(travel.y) > needed * 0.3f && spreadChange < Mathf.Abs(travel.y) * maxSpreadRatio;
		if (Mathf.Abs(travel.y) < needed || !IsSwiping)
			return;

		if (travel.y > 0f)
			selection.PushBack();
		else
			selection.PushForward();
		usedThisGesture = true;
	}

	void ResetGesture()
	{
		tracking = false;
		IsSwiping = false;
	}
}
