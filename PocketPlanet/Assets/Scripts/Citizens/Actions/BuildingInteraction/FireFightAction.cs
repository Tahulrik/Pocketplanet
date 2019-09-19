using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

namespace Citizens.Actions
{
	public class FireFightAction : PublicServantAction {

		Building targetBuildingScript;

		bool startedFighting = false;

		ParticleSystem waterParticle;

		public override void ExecuteAction ()
		{
			base.ExecuteAction();
			if (startedFighting) {
				FightFire ();
				return;
			}

            //if target has been destroy return home!
            if (targetPosition == null)
            {
                StopAction();
            }


			//check if you have reached target location yet
			if (ReachedTarget(Self.gameObject, targetPosition.position))
			{
				//Debug.Log ("Reached target " + CurrentActionTarget.name);
				if (targetBuildingScript.IsOnFire) {
					//Debug.Log ("Fighting Fire on target " + CurrentActionTarget.name);
					StartFireFight ();
				} 
				else if(CurrentActionTarget == Self.HomeBuilding.gameObject)
				{
				    //Debug.Log ("Entering Building" + CurrentActionTarget.name);
					//is there really not a better method for this?


					var Home = Self.HomeBuilding as Building_PublicService_FireDepartment;
					Home.InactiveFireFighters.Add (Self.gameObject);
					EnterBuilding (Home);

					StopAction ();
				}
			}
			else
			{
				MoveToTarget (Self.CurrentMoveSpeed);
			}
		}

		public override bool GetNewTarget(List<GameObject> targets)
		{
			//Debug.Log ("Buildings On Fire " + WorldObjects.BuildingsOnFire.Count);
			if (targets.Count == 0)
			{
				//add standard range and "firing range"

				ObjectiveRange = 0.15f;
				CurrentActionTarget = Self.HomeBuilding.gameObject;
				targetPosition = CurrentActionTarget.transform;
				return false;
			}

			CurrentActionTarget = FindClosestTarget (targets);
            targetPosition = CurrentActionTarget.transform;
			targetBuildingScript = CurrentActionTarget.GetComponent<Building> ();

			return true;
		}

		void StartFireFight()
		{
			ObjectiveRange = 1.5f;
			waterParticle.Play ();

			Self.CurrentSubState = SubState.ExecutingAction;
			startedFighting = true;

			FindDirectionToTarget (CurrentActionTarget.transform.position);
		}

		void FightFire()
		{
			//add damage to fire variable
			targetBuildingScript.RemoveFireHealth(10);

			if (!targetBuildingScript.IsOnFire) {
				StopFighting ();
			}
		}

		void StopFighting()
		{
			//Debug.Log ("Stopped Fighting");

			startedFighting = false;

			waterParticle.Stop ();

			GetNewTarget (WorldObjects.BuildingsOnFire);
		}

		public FireFightAction(CitizenBehaviour self, GameObject target)
		{
			try{
				if(target == null)
				{
					GetNewTarget(WorldObjects.BuildingsOnFire);
				}

				Self = self;
				ActionCompleted = false;
				CurrentActionTarget = target;
				ObjectiveRange = 1.5f;

				targetPosition = target.transform;

				//make this better
				waterParticle = Self.GetComponent<Citizen_PublicServant_FireFighter> ().waterParticle;

				targetBuildingScript = CurrentActionTarget.GetComponent<Building> ();

				animator = Self.anim;


				ExitBuilding (Self.HomeBuilding);
		}
		catch(MissingReferenceException)
		{
			MonoBehaviour.Destroy (Self.gameObject);
		}
		}
			
	}
}
