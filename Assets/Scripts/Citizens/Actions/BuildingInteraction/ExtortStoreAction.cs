using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

namespace Citizens.Actions
{
	public class ExtortStoreAction : MafiaAction {

		public static event MafiaCrime ExtortingStore;

		Building_Normal_Service targetScript;


		bool startedExtorsion = false;
		bool storeExtorted = false;

		float extorsionTimer = 0f;


		public override void ExecuteAction ()
		{
			base.ExecuteAction ();

			if (targetPosition == null) {
				MonoBehaviour.Destroy (Self.gameObject);
			}

			if (actionPaused) {
				return;
			}

			//Debug.Log ("Starting Store Extorsion Execute");
			if (targetScript.IsOnFire) {
				FinishedExtorting ();
			}

			if (CurrentActionTarget == null) {
				GetNewTarget (WorldObjects.ValidTargets);
			}

			//If a police officer is in near, start running in the opposite direction
			if (ReachedTarget(Self.gameObject, targetPosition.position)) {
               // Debug.Log("Reached target");

                if (storeExtorted) {
               //     Debug.Log("Entering home building");
                    MonoBehaviour.Destroy(Self.gameObject);
                    //EnterBuilding(Self.HomeBuilding);
				} else {

					ExtortTarget ();
				}
			}
			else
			{
               // Debug.Log("Moving to target");

                MoveToTarget(Self.CurrentMoveSpeed);
			}
		}

		public override void StopAction()
		{
			ActionCompleted = true;
			Self.CurrentAction = null;
		}

		void ExtortTarget()
		{
			//this is no good....
			var mafia = Self.GetComponent<Citizen_Criminal_Mafia> ();
			if (!startedExtorsion) {
				Self.committedCrime = true;

				Self.CurrentSubState = SubState.ExecutingAction;


				EnterBuilding (targetScript);

				if (ExtortingStore != null) {
					ExtortingStore (Self.gameObject);
				}

				extorsionTimer = mafia.ExtortionSpeed / 2;
				startedExtorsion = true;
              //  Debug.Log("Initialising Extorsion");
			}
			extorsionTimer += Time.deltaTime;
			if (extorsionTimer >= mafia.ExtortionSpeed) {

               // Debug.Log("Rolling extorsion result");

                //Change this such that it is dependant on a persons mood
                var rand = Random.Range (1, 3);
				if (rand == 1) {
					TakeMoney (mafia.StealAmount);
					FinishedExtorting ();
				} else if (rand == 2) {
					AskAgain ();
				}
			}

		}

		void FinishedExtorting()
		{
			if (startedExtorsion) {
               // Debug.Log("Ending Extorsion");

                ExitBuilding(targetScript);

				startedExtorsion = false;
				storeExtorted = true;

				if (CurrentActionTarget != Self.HomeBuilding.gameObject) {
					CurrentActionTarget = Self.HomeBuilding.gameObject;
                    targetPosition = CurrentActionTarget.transform;
                }
			}
		}

		void TakeMoney(float percentage)
		{
			var moneyPercent = (targetScript.moneyAmount / 100) * percentage;

			targetScript.moneyAmount -= moneyPercent;
			RobbedMoney += moneyPercent;
		}

		void AskAgain()
		{
			//this is no good....
			var mafia = Self.GetComponent<Citizen_Criminal_Mafia> ();

			//Damage Citizen
			//Buildings need to take less damage from gunfire
			targetScript.TakeDamage(mafia.GunDamage/10);

			extorsionTimer = 0f;
		}


		public override bool GetNewTarget(List<GameObject> targets)
		{

			if (targets == null) {
				targets = new List<GameObject> ();
			}
			if (targets.Count == 0)
			{
				return false;
			}

			List<GameObject> buildingTargets = new List<GameObject>();

			for (int i = 0; i < targets.Count; i++) {
                
				if (targets [i].GetComponent<Building_Normal_Service> ()) {
                    if (targets[i].GetComponent<Building_Normal_Service_Restaurant_ItalianRestaurant>())
                    {
                        //Debug.Log("Target is mafia restaurant, continue");

                        continue;
                    }

                    buildingTargets.Add(targets [i]);
				}

			}

			CurrentActionTarget = buildingTargets [Random.Range (0, buildingTargets.Count)];
            targetPosition = CurrentActionTarget.transform;
			targetScript = CurrentActionTarget.GetComponent<Building_Normal_Service> ();

			return true;
		}


		public ExtortStoreAction(CitizenBehaviour self, Building target)
		{
			Self = self;
			ActionCompleted = false;
			try{
				try{
					CurrentActionTarget = target.gameObject;

				}
				catch(System.Exception) {
					MonoBehaviour.Destroy (self.gameObject);
				}

				Gunparticle = Self.transform.GetChild (0).GetChild(0). GetChild (1).GetComponent<ParticleSystem> ();

				animator = Self.anim;
				ObjectiveRange = 0.15f;
				Self.SetPersonActive (true);
                if (target.GetType() == typeof(Building_Normal_Service_Restaurant_ItalianRestaurant))
                {
                    //Debug.Log("Target is mafia restaurant");
                    GetNewTarget(WorldObjects.ValidTargets);

                    return;
                }


                CurrentActionTarget = target.gameObject;
				targetScript = target as Building_Normal_Service;
				targetPosition = target.transform;



            }
			catch(MissingReferenceException)
			{
				MonoBehaviour.Destroy (Self.gameObject);
			}
		}
	}
}
