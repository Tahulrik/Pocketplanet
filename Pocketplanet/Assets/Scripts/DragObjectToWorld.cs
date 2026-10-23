using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

public class DragObjectToWorld : MonoBehaviour {

	[Tooltip("The prefab that will spawn when dragging this UI element")]
	public GameObject SpawnType;
	public EventCategory EventType;
	public ObjectSpawner spawner;

	public bool canPlace = false;
	public bool selected = false;

	ScrollRect scroller;
	Image selection;
	EventCost costScript;

	void OnEnable()
	{
		LeanTouch.OnFingerOld += OnFingerHeld;
		LeanTouch.OnFingerUp += OnFingerUp;
		GameHandler.PointsRecieved += CheckForPointAvailability;
		GameHandler.PointsSpent += CheckForPointAvailability;
	}

	void OnDisable()
	{
		LeanTouch.OnFingerOld -= OnFingerHeld;
		LeanTouch.OnFingerUp -= OnFingerUp;
		GameHandler.PointsRecieved -= CheckForPointAvailability;
		GameHandler.PointsSpent -= CheckForPointAvailability;
	}

	void Start()
	{
		selection = GetComponent<Image>();
		scroller = GetComponentInParent<ScrollRect>();

		if (SpawnType == null)
			return;

		var tag = SpawnType.GetComponent<EventTypeTag>();
		EventType = tag != null ? tag._EventType : EventCategory.Building;
		costScript = SpawnType.GetComponentInChildren<EventCost>();

		CheckForPointAvailability(GameHandler.Instance.gamePoints);
	}

	void OnFingerHeld(LeanFinger finger)
	{
		if (!canPlace || SpawnType == null || !IsFingerOverThisButton(finger))
			return;

		selected = true;
		selection.color = Color.blue;
		SetScrollingEnabled(false);
		SpawnPlacementCursor(finger);
	}

	void OnFingerUp(LeanFinger finger)
	{
		if (!selected)
			return;

		selected = false;
		SetScrollingEnabled(true);
	}

	bool IsFingerOverThisButton(LeanFinger finger)
	{
		if (!finger.IsOverGui)
			return false;

		var hits = LeanTouch.RaycastGui(finger.ScreenPosition);
		if (hits.Count == 0)
			return false;

		var hit = hits[0].gameObject.transform;
		return hit == transform || hit.IsChildOf(transform);
	}

	void SpawnPlacementCursor(LeanFinger finger)
	{
		var instance = Instantiate(spawner);
		instance.currentType = EventType;
		instance.SpawnFinger = finger;
		instance.selection = selection;
		instance.prefab = SpawnType;
		instance.costScript = costScript;
	}

	void SetScrollingEnabled(bool value)
	{
		if (scroller != null)
			scroller.enabled = value;
	}

	public void CheckForPointAvailability(int points)
	{
		if (costScript == null)
			return;

		canPlace = GameHandler.Instance.gamePoints >= costScript.Cost;
		selection.color = canPlace ? GameHandler.Instance.selectionAvailable : GameHandler.Instance.selectionUnavailable;
	}
}
