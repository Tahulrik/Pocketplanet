using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens.Actions;
using Buildings;

namespace Citizens{

	public class Citizen_Criminal_Robber : Citizen_Criminal {

		public float RobbingSpeed = 1f;

		public float viewRange = 2.5f;


		void OnEnable()
		{
			//Building.OnDestroyed += CheckIfHomeWasDestroyed;
		}

		void Disable()
		{
			//Building.OnDestroyed -= CheckIfHomeWasDestroyed;

		}
			
		protected override void Start ()
		{
			base.Start ();
			 
			var rand = Random.Range (1, 3);
			//rand = 2;
			if (rand == 1) {
				CurrentState = ActionState.RobStore;
                CurrentAction = new RobStoreAction(this, WorldObjects.GetBuildingTarget(this));

			} else if (rand == 2) {
				CurrentState = ActionState.RobPerson;
                //print(WorldObjects.GetPersonTarget(this).gameObject.name);
                CurrentAction = new RobPersonAction(this, WorldObjects.GetPersonTarget(this));
			}
		}

		void CheckIfHomeWasDestroyed(GameObject GO)
		{
			print(GO.name);
			try
			{
				print("Inside try");
				try
				{
					if (GO == null)
					{
						print("No GO");
						return;
					}
					if (GO != HomeBuilding.gameObject)
					{
						print("GO is not homebuilding");
						return;
					}
				}
				catch (System.NullReferenceException e)
				{
					Destroy(gameObject);
					return;
				}
				//print ("building destroyed was " + GO.name);
				print("Coroutine started");
				StartCoroutine(GetNewHome ());

			}
			catch(System.NullReferenceException e) {
				print("Catch: " + e);
				Destroy (gameObject);
			}
		}

		IEnumerator GetNewHome()
		{
			yield return new WaitForEndOfFrame ();
			var newHome = FindObjectOfType<Building_PublicService_FireDepartment> ();
			//print ("getting new home");
			if (newHome != null) {
				//print ("new home found");
				HomeBuilding = newHome;
			} 
		}

		protected override void Update ()
		{
			if (HomeBuilding == null) {
				Destroy (gameObject);
			}
			base.Update ();
		}


		void OnDestroy()
		{
		}
	}
}
