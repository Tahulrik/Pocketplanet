using System.Collections.Generic;
using System.Linq;
using Lean.Touch;
using UnityEngine;

[RequireComponent(typeof(RadialContextMenu))]
public class WorldTapSelector : MonoBehaviour
{
	[SerializeField, Range(0f, 0.1f), Tooltip("Extra tap tolerance around sprites, as a fraction of the camera's view height.")]
	float tapTolerance = 0.015f;

	readonly List<IContextActionProvider> providers = new List<IContextActionProvider>
	{
		new BuildingActionProvider(),
		new CitizenActionProvider()
	};

	RadialContextMenu menu;

	void Awake() => menu = GetComponent<RadialContextMenu>();

	void OnEnable() => LeanTouch.OnFingerTap += OnTap;

	void OnDisable() => LeanTouch.OnFingerTap -= OnTap;

	void OnTap(LeanFinger finger)
	{
		if (finger.StartedOverGui || finger.IsOverGui || finger.TapCount > 1 || IsPlacingSomething())
			return;

		var margin = Camera.main.orthographicSize * 2f * tapTolerance;
		var target = WorldPicker.Pick(PlanetPlane.FromScreen(finger.ScreenPosition), margin);
		var actions = target != null ? ActionsFor(target) : new List<ContextAction>();
		if (actions.Count == 0 || target == menu.Target)
		{
			menu.Close();
			return;
		}
		menu.Open(target, actions);
	}

	List<ContextAction> ActionsFor(GameObject target) =>
		providers.Where(p => p.Handles(target)).SelectMany(p => p.ActionsFor(target)).ToList();

	static bool IsPlacingSomething() => FindAnyObjectByType<ObjectSpawner>() != null;
}
