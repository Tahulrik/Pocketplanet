using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionTypes;
using UnityEngine.UI;

//Split this into multiple scripts depending on what buttons/abilities is needed
public class GameplayCanvasUpdater : MonoBehaviour {

	GameObject InputHandler;

	Button BuildingButton;
	Image BuildingImage;


	Button MeteorButton;
	Image MeteorImage;
	float TimeBeforeNextMeteor = 5f;

	void OnEnable()
	{
//		ConstructionHandler.AllowBuilding += UpdateBuildingGUI;
	//	Meteor.MeteorFired += ResetMeteorCooldown;
	}

	/*void Awake()
	{

		if (gameObject.activeInHierarchy) {


			BuildingButton = transform.GetChild (0).GetComponent<Button> ();
			BuildingImage = BuildingButton.transform.GetChild(0).GetComponent<Image> ();
			BuildingButton.onClick.AddListener (() => InteractionType.ChangeModeToBuilding ());
			BuildingButton.onClick.AddListener (() => SetSelectionColor(BuildingImage));

			UpdateBuildingGUI (true);


			MeteorButton = transform.GetChild (1).GetComponent<Button>();
			MeteorImage = MeteorButton.transform.GetChild(0).GetComponent<Image> ();
			MeteorButton.onClick.AddListener (() => InteractionType.ChangeModeToMeteor ());
			MeteorButton.onClick.AddListener (() => SetSelectionColor(MeteorImage));

		}
	}*/

	/*void Update()
	{
		if (!MeteorButton.interactable) {
			UpdateMeteorCooldown ();
		}
	}

	void UpdateBuildingGUI(bool value)
	{
		if (InteractionType.currentInteraction == InteractionSetting.Building) {
			return;
		}

		BuildingButton.interactable = value;
		if (value) {
			BuildingImage.color = Color.green;
		}
		else{
			BuildingImage.color = Color.red;
		}
	}

	void UpdateMeteorCooldown()
	{
		if (TimeBeforeNextMeteor <= GameHandler.Instance.StunScript.cooldown) {
			TimeBeforeNextMeteor += Time.deltaTime;
		} 
			 
		var currentCD = TimeBeforeNextMeteor / GameHandler.Instance.StunScript.cooldown;
		MeteorImage.fillAmount = currentCD;

		if (TimeBeforeNextMeteor >= GameHandler.Instance.StunScript.cooldown) {
			MeteorImage.color = Color.green;
			MeteorButton.interactable = true;
			return;
		}
	}

	void ResetMeteorCooldown()
	{
		TimeBeforeNextMeteor = 0.0f;
		MeteorImage.color = Color.red;

		MeteorButton.interactable = false;
	}

	void SetSelectionColor(Image selection)
	{
		selection.color = Color.blue;
	}*/
}
