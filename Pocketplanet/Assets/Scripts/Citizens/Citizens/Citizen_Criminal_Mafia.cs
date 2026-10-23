using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens.Actions;
using Buildings;

namespace Citizens{

	public class Citizen_Criminal_Mafia : Citizen_Criminal {

		public float ExtortionSpeed = 10f;
		public float StealAmount = 10;
		public float GunDamage = 10;

		void OnEnable()
		{
			Building.OnDestroyed += CheckIfHomeWasDestroyed;
		}

		void Disable()
		{
			Building.OnDestroyed -= CheckIfHomeWasDestroyed;

		}
			
		void TargetDestroyed(GameObject GO)
		{

			if (CurrentAction != null) {
				if (GO == CurrentAction.CurrentActionTarget) {
					CurrentAction.CurrentActionTarget = HomeBuilding.gameObject;
				}
			}
		}
		
		void CheckIfHomeWasDestroyed(GameObject GO)
		{
			if (GO != HomeBuilding) {
				return;
			}
			
			GetNewHome ();
			
		}

		protected override void Start ()
		{

			//BuildingsOnFire = new List<GameObject> ();

			base.Start ();

			var rand = Random.Range (1, 3);

			//rand = 1;
			if (rand == 1) {
				CurrentState = ActionState.ExtortPerson;
                CurrentAction = new ExtortPersonAction(this, WorldObjects.GetPersonTarget(this));

            }
            else if (rand == 2) {
				CurrentState = ActionState.ExtortStore;
                CurrentAction = new ExtortStoreAction(this, WorldObjects.GetBuildingTarget(this));

            }
        }

		void GetNewHome()
		{
			var newHome = GameObject.FindObjectOfType<Building_Normal_Residence> ();
			if (newHome != null) {
				HomeBuilding = newHome;
			}

			//what if there is no more buildings??
		}

		protected override void Update ()
		{

			base.Update ();
		}

		void OnDestroy()
		{
			var itaRest = HomeBuilding as Building_Normal_Service_Restaurant_ItalianRestaurant;

			itaRest.currentMobsters--;
		}
	}
}
