using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buildings{
	public class Building_ConstructionSite : MonoBehaviour {

		const float BuildingCompletionAmount = 100f;
		public float BuildingProgress = 0f;

		public float buildingSpeed = 10f;

		GameObject currentBuilding;

		public GameObject buildingPrefab;
		Building building;
		bool buildingConstructed = false;
		Animator anim;
		// Use this for initialization
		void Start () {
			anim = GetComponent<Animator> ();

		}

		// Update is called once per frame
		void Update () {
			BuildingProgress += Time.deltaTime * buildingSpeed;

			//anim.SetFloat ("BuildingProgress", BuildingProgress);
			if (BuildingProgress >= BuildingCompletionAmount) {
				if (!buildingConstructed) {
					ConstructBuilding ();
				}
			}
		}

		void ConstructBuilding()
		{
			var newBuilding = Instantiate (buildingPrefab);
			newBuilding.transform.position = transform.position;
			building = newBuilding.GetComponent<Building> ();
			buildingConstructed = true;
			building.fromConstruction = true;
			Planet.RotateObjectToSurface (newBuilding);
			WorldObjects.Targets.Add (newBuilding);
			RemoveScaffold ();
		}

		void RemoveScaffold()
		{
			anim.SetTrigger ("Completed");
		}

		public void Completed()
		{
			building.BuildingConstructed ();
			Destroy (gameObject);
		}
	}
}

