using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

namespace Citizens.Actions
{
	public class RobStoreAction : RobberAction {

		public static event RobberCrime RobbingStore;


		Building_Normal_Service targetScript;
		HostileCitizenTarget targetingScript;
		bool robbedStore = false;
		bool startedRobbing = false;


        public override void ExecuteAction()
        {
            if (actionPaused)
            {
                return;
            }

			if (targetPosition == null) {
				MonoBehaviour.Destroy (Self.gameObject);
			}

			if (robbedStore) {
				//Make this better;
				if (CurrentActionTarget != Self.HomeBuilding.gameObject) {
					//Debug.Log ("going home");
					CurrentActionTarget = Self.HomeBuilding.gameObject;
				}
			}

			//Debug.Log (robbedStore);
			if (ReachedTarget(Self.gameObject, targetPosition.position)) {
				if (!robbedStore) {
					//Debug.Log ("reached store");
					if (RobbedMoney <= RobAmount) {
						//Debug.Log ("st ill robbing");
						if (targetScript.moneyAmount > 0) {
							//Debug.Log ("target still has money");
							RobTarget ();
						} else {
							//Debug.Log ("target has no more money");
							FinishedRobbing ();
						}
					} else {
						//Debug.Log ("stole all the neccessary money");
						FinishedRobbing ();
					}
				} else
				{
					//Debug.Log ("entering home");
                    MonoBehaviour.Destroy(Self.gameObject);
                    //EnterBuilding (targetScript);
					//StopAction ();
				}
			}
			else
			{
				MoveToTarget(Self.CurrentMoveSpeed);
			}
		}
			

		void RobTarget()
		{
			var robber = Self as Citizen_Criminal_Robber;
			if (!startedRobbing) {
				Self.CurrentSubState = SubState.ExecutingAction;
				EnterBuilding (targetScript);

				if (RobbingStore != null) {
					RobbingStore (Self.gameObject);
				}
				Self.committedCrime = true;
				targetScript = CurrentActionTarget.GetComponent<Building_Normal_Service> ();

				startedRobbing = true;
			}

			StealMoney (robber.RobbingSpeed);
		}

		void StealMoney(float speed)
		{
			targetScript.moneyAmount -= speed*Time.deltaTime;
			Self.stats.MoneyAmount += speed * Time.deltaTime;
            RobbedMoney += speed * Time.deltaTime;

            //targetScript.TakeDamage ((speed * 0.25f) * Time.deltaTime);
        }

		void FinishedRobbing()
		{
			robbedStore = true;
			Self.CurrentMoveSpeed = Self.WalkingSpeed;
			ObjectiveRange = 0.15f;
			CurrentActionTarget = Self.HomeBuilding.gameObject;
			targetPosition = CurrentActionTarget.transform;

			ExitBuilding (targetScript);
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
					buildingTargets.Add(targets [i]);
				}
			
			}

			CurrentActionTarget = buildingTargets [Random.Range (0, buildingTargets.Count)];
			targetScript = CurrentActionTarget.GetComponent<Building_Normal_Service> ();
			targetPosition = CurrentActionTarget.transform;

			ObjectiveRange = 1.5f;
			return true;
		}



		public RobStoreAction(Citizen_Criminal_Robber self, Building target)
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
				CurrentActionTarget = target.gameObject;
				ObjectiveRange = 0.15f;

				Self.SetPersonActive (true);

				targetScript = target as Building_Normal_Service;

				targetPosition = target.transform;

				animator = Self.anim;

			}
			catch(MissingReferenceException)
			{
				MonoBehaviour.Destroy (Self.gameObject);
			}
			
			
		}


	}
}
