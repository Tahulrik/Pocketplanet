using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//separate these into specific types of actions
public enum InteractionCategory
{
	Buildings,
	Events,
}
public class MenuContentFiller : MonoBehaviour {

	public InteractionCategory Content;
	string ContentSelection;

	public GameObject ContentButton;

	// Use this for initialization
	void Start () {
		ContentSelection = Content.ToString ();

		var Objects = Resources.LoadAll<GameObject> (ContentSelection);

		foreach (var Object in Objects) {
			var objectImage = Object.GetComponentInChildren<SpriteRenderer> ();
			var eventCost = Object.GetComponentInChildren<EventCost> ();

			var newButton = Instantiate (ContentButton, gameObject.transform);

			var buttonSprite = newButton.transform.GetChild (0).GetChild(0).GetComponent<Image> ();

			var costText = newButton.transform.GetChild (1).GetComponentInChildren<Text> ();
            
			buttonSprite.sprite = objectImage.sprite;

			//var dragHandler = newButton.GetComponentInChildren<DragObjectToWorld> ();

			//dragHandler.SpawnType = Object;

			costText.text = "Cost: " + eventCost.Cost.ToString ();
			newButton.name = Object.name + " Button";



		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
