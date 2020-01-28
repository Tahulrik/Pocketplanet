using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Lean.Touch;
using UnityEngine.UI;
/*
public class DragObjectToWorld : MonoBehaviour {

	[Tooltip("The prefab that will spawn when dragging this UI element")]
	public GameObject SpawnType;
  //  [HideInInspector]
	public EventCategory EventType;

	public ObjectSpawner spawner;

	float Distance = 10.0f;

	ScrollRect scroller;
	Image selection;

	public bool canPlace = false;
	public bool selected = false;


	EventCost costScript;



	protected virtual void OnEnable()
	{
		// Hook into the events we need
		LeanTouch.OnFingerHeldDown += OnFingerHeldDown;

		LeanTouch.OnFingerHeldUp += OnFingerUp;

		GameHandler.PointsRecieved += CheckForPointAvailability;
		GameHandler.PointsSpent += CheckForPointAvailability;

	}

	protected virtual void OnDisable()
	{
		// Unhook events
		LeanTouch.OnFingerHeldDown -= OnFingerHeldDown;

		LeanTouch.OnFingerHeldUp -= OnFingerUp;

		GameHandler.PointsRecieved -= CheckForPointAvailability;
		GameHandler.PointsSpent -= CheckForPointAvailability;

	}

	void Start()
	{
		selection = gameObject.GetComponent<Image> ();
		scroller = gameObject.GetComponentInParent<ScrollRect> ();

        if (SpawnType != null)
        {
			if (SpawnType.GetComponent<EventType> () != null)
				
                EventType = SpawnType.GetComponent<EventType>()._EventType;
            else
                EventType = EventCategory.Building;
        }

		costScript = SpawnType.GetComponent<EventCost> ();

		CheckForPointAvailability (GameHandler.Instance.gamePoints);
    }
		
	public void OnFingerHeldDown(LeanFinger finger)
	{
		if (!canPlace) {
			return;
		}

		// Does the prefab exist?
		if (SpawnType != null)
		{
			if (finger.IsOverGui) {
				//print ("Grabbing Object " + SpawnType.name);
				// Get the RaycastResult under this finger's current position
				var result = LeanTouch.RaycastGui(finger.ScreenPosition);
				
				if (result.isValid == true)
				{
					// Is this finger over this UI element?
					var hit = result.gameObject.transform.parent.gameObject;
					if (hit == gameObject)
					{
						selected = true;

						selection.color = Color.blue;


						scroller.enabled = false;

						var instance = Instantiate (spawner);

                        instance.currentType = EventType;
                        instance.SpawnFinger = finger;
						instance.selection = selection;
						instance.prefab = SpawnType;
						instance.costScript = costScript;

						instance.Distance = Distance;
					}
				}
			}
		}
	}

	public void OnFingerUp(LeanFinger finger)
	{
		if (scroller != null) {
			selected = false;
			scroller.enabled = true;
		}
	}

	public void CheckForPointAvailability(int points)
	{
		if (selected) {
			//return;
		}

		if (GameHandler.Instance.gamePoints >= costScript.Cost) {
			
			selection.color = GameHandler.Instance.selectionAvailable;

			canPlace = true;
		} else {
			selection.color = GameHandler.Instance.selectionUnavailable;

			canPlace = false;
		}

	}
}
*/