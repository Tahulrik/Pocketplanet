using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Citizens.Actions
{
	public class RobPersonAction : RobberAction {

		public static event RobberCrime RobbingPerson;
		CitizenBehaviour targetScript;

		bool startedRobbing = false;
		bool robbedPerson = false;

		public override void ExecuteAction ()
		{
            if (actionPaused)
            {
                return;
            }
            //If a police officer is in near, start running in the opposite direction

            //Only check on "officer layermask"
            //var Hits = Physics2D.OverlapCircleAll(Self.transform.position, Self.viewRange);


			if (ReachedTarget(Self.gameObject, targetPosition.transform.position)) {
               // Debug.Log("Moving to target");

                if (!robbedPerson) {
                 //   Debug.Log("Have not robbed yet");

                    if (RobbedMoney <= RobAmount) {
                  //      Debug.Log("Have not reached rob money target yet");

                        if (targetScript.stats.MoneyAmount > 0) {
                    //        Debug.Log("Starting robbing");

                            RobTarget();
						} 
						else
						{
                  //          Debug.Log("Finished robbing, target has no money");

                            FinishedRobbing();
						}
					} 
					else 
					{
                    //    Debug.Log("Finished robbing, target money amount reached");

                        FinishedRobbing();
					}
				}
				else
				{
                    MonoBehaviour.Destroy(Self.gameObject);
                    //EnterBuilding(Self.HomeBuilding);
					//StopAction ();
				}
			}
			else
			{
            //    Debug.Log("moving to target");

                MoveToTarget(Self.CurrentMoveSpeed);
			}
		}

		public override void StopAction()
		{
			ActionCompleted = true;
			Self.CurrentAction = null;
		}

		void RobTarget()
		{
			var robber = Self as Citizen_Criminal_Robber;

			if (!startedRobbing) {

				if (RobbingPerson != null) {
					RobbingPerson (Self.gameObject);
				}
				Self.committedCrime = true;
				Self.CurrentState = ActionState.RobPerson;
				startedRobbing = true;
			}

			//Debug.Log ("Robbing target");
			StealMoney (robber.RobbingSpeed);
		}

		void FinishedRobbing()
		{
			if (startedRobbing) {
				
				startedRobbing = false;
				robbedPerson = true;

				if (CurrentActionTarget != Self.HomeBuilding) {
					//Debug.Log ("Going Home");

					CurrentActionTarget = Self.HomeBuilding.gameObject;
                    targetPosition = CurrentActionTarget.transform;
				}
			}
		}

		void StealMoney(float speed)
		{
			Self.stats.MoneyAmount -= speed*Time.deltaTime;
            Self.stats.MoneyAmount += speed * Time.deltaTime;
            RobbedMoney += speed * Time.deltaTime;
            //Debug.Log ("stealing money");

            //targetBuildingScript.Hit ((speed * 0.25f) * Time.deltaTime);
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

			List<GameObject> personTargets = new List<GameObject>();

			for (int i = 0; i < targets.Count; i++) {
				if (targets [i].GetComponent<Citizen_Civilian> ()) {
					personTargets.Add(targets [i]);
				}

			}

			CurrentActionTarget = personTargets [Random.Range (0, personTargets.Count)];
			targetScript = CurrentActionTarget.GetComponent<Citizen_Civilian> ();

			return true;
		}


		public RobPersonAction(Citizen_Criminal_Robber self, CitizenBehaviour target)
		{

			Self = self;
			ActionCompleted = false;
			try{
				CurrentActionTarget = target.gameObject;
				
			}
			catch(System.Exception) {
				MonoBehaviour.Destroy (self.gameObject);
			}
			ObjectiveRange = 0.15f;

			//Make sure all citizens spawn inside houses, and then moves out
			Self.SetPersonActive (true);

			targetScript = target as Citizen_Civilian;
			targetPosition = CurrentActionTarget.transform.GetChild (0).GetChild(0);

			animator = Self.anim;

		}
	}
}
