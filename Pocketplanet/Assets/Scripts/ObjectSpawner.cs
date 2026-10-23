using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSpawner : MonoBehaviour {

	public delegate void ReleasedObject(EventCategory type, Vector2 PlacementPos, GameObject prefab, bool fromPlayer);

	public static event ReleasedObject Released;

	[HideInInspector]
	public LeanFinger SpawnFinger;
	[HideInInspector]
	public EventCategory currentType;
	[HideInInspector]
	public GameObject prefab;
	[HideInInspector]
	public Image selection;
	[HideInInspector]
	public EventCost costScript;

	protected SpriteRenderer CursorSprite;
	protected Sprite SelectedSprite;

	bool ValidPlacement = false;

	protected virtual void OnEnable()
	{
		LeanTouch.OnFingerUp += OnFingerUp;
	}

	protected virtual void OnDisable()
	{
		LeanTouch.OnFingerUp -= OnFingerUp;
	}

	void Start()
	{
		CursorSprite = GetComponent<SpriteRenderer> ();
		SelectedSprite = prefab.GetComponentInChildren<SpriteRenderer>().sprite;
		if (SelectedSprite.bounds.extents.magnitude < 0.5f && currentType != EventCategory.Building)
			gameObject.transform.localScale = Vector3.one * Mathf.Clamp(10 * GameHandler.Instance.CameraController.ZoomedOutAmount, 1, Mathf.Infinity);
		CursorSprite.sprite = SelectedSprite;
	}

	protected virtual void Update()
	{
		if (SpawnFinger == null)
			return;

		transform.position = ScreenToPlanetPlane (SpawnFinger.ScreenPosition);
		Planet.RotateObjectToSurface (gameObject);

		ValidPlacement = currentType == EventCategory.Building
			? ValidBuildPosition (Planet.FindPointOnSurface (Planet.Radius, transform.position), prefab)
			: ValidEventPosition (transform.position);

		CursorSprite.color = ValidPlacement ? GameHandler.Instance.selectionAvailable : GameHandler.Instance.selectionUnavailable;
	}

	static Vector3 ScreenToPlanetPlane(Vector2 screenPosition)
	{
		var ray = Camera.main.ScreenPointToRay (screenPosition);
		var plane = new Plane (Vector3.forward, Planet.PlanetObject.transform.position);
		return plane.Raycast (ray, out var distance) ? ray.GetPoint (distance) : ray.origin;
	}

	void OnFingerUp(LeanFinger finger)
	{
		if (finger != SpawnFinger)
			return;

		if (ValidPlacement && !finger.IsOverGui)
			Place ();

		CancelPlacement ();
	}

	void Place()
	{
		Released?.Invoke (currentType, transform.position, prefab, true);
		GameHandler.Instance.SpendGamePoint (costScript.Cost);
	}

	public void CancelPlacement()
	{
		SpawnFinger = null;
		var dragHandler = selection != null ? selection.GetComponent<DragObjectToWorld> () : null;
		if (dragHandler != null)
			dragHandler.CheckForPointAvailability (GameHandler.Instance.gamePoints);
		Destroy (gameObject);
	}

	bool ValidEventPosition(Vector2 position)
	{
		return AppropriateDistanceToPlanet();
	}

	bool ValidBuildPosition(Vector2 position, GameObject building)
	{
		var planet = PocketPlanet.Planet.PlanetHost.Active.Planet;
		var arc = planet.Frame.ToCoord(position).Arc;
		return planet.Placement.Check(PocketPlanet.Planet.PlanetFootprint.For(building), arc).IsValid;
	}

	bool AppropriateDistanceToPlanet()
	{
		var magnitude = transform.position.magnitude;
		return magnitude >= Planet.Radius - 2f && magnitude <= Planet.Radius + 10f;
	}
}
