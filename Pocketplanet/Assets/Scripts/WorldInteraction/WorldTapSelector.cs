using Lean.Touch;
using PocketPlanet.Planet;
using UnityEngine;

// Turns taps in city view into selections: a tap on something selects it, a tap on empty space clears.
public class WorldTapSelector : MonoBehaviour
{
	[SerializeField, Range(0f, 0.1f), Tooltip("Extra tap tolerance around sprites, as a fraction of the camera's view height.")]
	float tapTolerance = 0.015f;

	public Selection Selection { get; private set; }
	public ContextActionCatalog Actions { get; } = new ContextActionCatalog();

	void Awake() => Selection = new Selection(WorldPicker.IsPickable);

	void OnEnable() => LeanTouch.OnFingerTap += OnTap;

	void OnDisable() => LeanTouch.OnFingerTap -= OnTap;

	void Update() => Selection.Refresh();

	void OnTap(LeanFinger finger)
	{
		if (finger.StartedOverGui || finger.IsOverGui || finger.TapCount > 1 || IsPlacingSomething() || !CameraRig.IsInCityView() || Camera.main == null)
			return;

		var margin = Camera.main.orthographicSize * 2f * tapTolerance;
		var target = WorldPicker.Pick(PlanetPlane.FromScreen(finger.ScreenPosition), margin);
		if (target == null || !Actions.HasActions(target))
			Selection.Clear();
		else
			Selection.Select(target);
	}

	static bool IsPlacingSomething() => FindAnyObjectByType<ObjectSpawner>() != null;
}
