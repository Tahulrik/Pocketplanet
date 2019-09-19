using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using Lean.Touch;
using UnityEngine.UI;

public class ObjectSpawner : MonoBehaviour {

	public delegate void ReleasedObject(EventCategory type, Vector2 PlacementPos, GameObject prefab, bool fromPlayer);

	public static event ReleasedObject Released;

	// The distance from the finger this object is placed in world space
	[HideInInspector]
	public float Distance;

	// The finger used to spawn this
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
		// Hook events
		LeanTouch.OnFingerUp += OnFingerUp;

	}

	protected virtual void OnDisable()
	{
		// Unhook events
		LeanTouch.OnFingerUp -= OnFingerUp;
	}

	void Start()
	{
		CursorSprite = GetComponent<SpriteRenderer> ();
		SelectedSprite = prefab.GetComponentInChildren<SpriteRenderer>().sprite;
		if (SelectedSprite.bounds.extents.magnitude < 0.5f && currentType != EventCategory.Building)
			gameObject.transform.localScale = Vector3.one * Mathf.Clamp(10 * GameHandler.Instance.CameraControls.zoomAmount, 1, Mathf.Infinity);
		CursorSprite.sprite = SelectedSprite;
	}

	protected virtual void Update()
	{
		// Does the spawn finger exist?
		if (SpawnFinger != null)
		{
//			print (currentType);
			transform.position = SpawnFinger.GetWorldPosition(Distance);
			Planet.RotateObjectToSurface (gameObject);
			var surfacePosition = Planet.FindPointOnSurface (Planet.Radius, transform.position);
			//Debug.DrawRay (Planet.PlanetObject.transform.localPosition, surfacePosition, Color.cyan);
			//Debug.DrawRay (transform.position, -surfacePosition);

			if (currentType == EventCategory.Building) {
				//print ("building");
				ValidPlacement = ValidBuildPosition (surfacePosition, prefab);

				if (ValidPlacement) {
					if (CursorSprite.color != Color.green) {
						CursorSprite.color = GameHandler.Instance.selectionAvailable;
					}
				} 
				else 
				{
					if (CursorSprite.color != Color.grey) {
						CursorSprite.color = GameHandler.Instance.selectionUnavailable;
					}
				}
			}
			else if(currentType != EventCategory.Building)
			{
				//print ("Eventing");
				ValidPlacement = ValidEventPosition (transform.position);

				if (ValidPlacement) {
					if (CursorSprite.color != Color.green) {
						CursorSprite.color = GameHandler.Instance.selectionAvailable;
					}
				}
				else 
				{
					if (CursorSprite.color != Color.gray) {
						CursorSprite.color = GameHandler.Instance.selectionUnavailable;
					}
				}
			}
		}
	}

	public void OnFingerUp(LeanFinger finger)
	{
		//If finger is on valid placement
		if (finger.IsOverGui) {
			CancelPlacement ();
		}

		// Was the spawning finger released?
		if (finger == SpawnFinger)
		{
			if (Released != null) {
				if (ValidPlacement) {
					var fingerWorldPos = Camera.main.ScreenToWorldPoint (finger.ScreenPosition);
					Released (currentType, fingerWorldPos, prefab, true);



					GameHandler.Instance.SpendGamePoint (costScript.Cost);
				}
			}

			// Reset the spawn finger
			SpawnFinger = null;
			Destroy (gameObject);
		}
	}

	public void CancelPlacement()
	{
		selection.GetComponent<DragObjectToWorld> ().CheckForPointAvailability (0);
		Destroy (gameObject);
		SpawnFinger = null;
	}
		
	bool ValidEventPosition(Vector2 position)
	{
		//print (position.magnitude);
		//print (Planet.Radius);
		if (AppropriateDistanceToPlanet()) {
			return true;
		}
			
		return false;
	}

	bool ValidBuildPosition(Vector2 position, GameObject building)
	{

		bool hitBuilding = false;
		bool hitWater = false;
		bool hitGround = false;

		if (!AppropriateDistanceToPlanet()) {
			return false;
		}

		var buildingBounds = building.GetComponentInChildren<SpriteRenderer> ().bounds;
		var size = new Vector2 (buildingBounds.extents.x, 0.2f);

		Collider2D[] hits = Physics2D.OverlapCircleAll (position, size.x);

		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].gameObject.transform.root.tag == "Building") {
				hitBuilding = true;
			}
			if (hits [i].gameObject.tag == "Water") {
				hitWater = true;
			}
			if (hits [i].gameObject.tag == "Ground") {
				hitGround = true;
			}
		}
		if (hitBuilding) {
			return false;
		} 
		if (hitGround && hitWater) {
			return true;
		}

		return false;
	}

	bool AppropriateDistanceToPlanet()
	{
		if (transform.position.magnitude >= Planet.Radius - 2f && transform.position.magnitude <= Planet.Radius + 10f) {
			return true;
		}
		return false;
	}

}
