using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;
using System.Linq;

namespace Citizens.Actions
{
	public class HealPerson : PublicServantAction {

		//Temporary
		Citizen_PublicServant_Paramedic paramedicScript;
		CitizenBehaviour targetScript;

		bool healedTarget = false;
		bool healing = false;
		bool deliveringPatient = false;
		bool goingHome = false;

		float healTimer = 0;
		float deliveringTime = 2f;
		float deliveringTimer = 0f;

		public override void ExecuteAction ()
		{
			if (actionPaused) {
				return;
			}

			base.ExecuteAction();

			if (healing) {
				HealTarget ();
			}
			if (deliveringPatient) {
				DeliverCitizen ();
			}
				
			//check if you have reached target location yet
			if (ReachedTarget(Self.gameObject, targetPosition.transform.position))
			{
				//Debug.Log ("Reached Target");
				if (!healedTarget) {
					//Debug.Log ("Starting Heal");
					if (targetScript.stats.CurrentStatus != Status.Dead) {
						//Debug.Log ("Attempting Heal");
						AttemptHeal ();
					} else if (targetScript.stats.CurrentStatus == Status.Dead) {
						//Debug.Log ("Bringing person to hospital");
						BringToHospital ();
					}
				}
				else 
				{
					if (goingHome) {
						//Debug.Log ("Entering Home");
						Building_PublicService_Hospital hospital = Self.HomeBuilding as Building_PublicService_Hospital;
						EnterBuilding (Self.HomeBuilding);
						hospital.InactiveParamedics.Add (Self.gameObject);
						StopAction ();
					} else {
						//Debug.Log ("Starting Delivery");
						StartDelivery ();
					}
				}

			}
			else
			{
				//Debug.Log ("moving to target");
				MoveToTarget (Self.CurrentMoveSpeed);
			}
		}



		void BringToHospital()
		{
			targetPosition = Self.HomeBuilding.transform;
			CurrentActionTarget.transform.parent = Self.transform;
			isBusy = true;
			healedTarget = true;
			goingHome = false;
		}

		void StartDelivery()
		{
			Building_PublicService_Hospital hospital = Self.HomeBuilding as Building_PublicService_Hospital;
			EnterBuilding (Self.HomeBuilding);
			CurrentActionTarget.GetComponent<CitizenBehaviour> ().SetPersonActive (false);
			deliveringPatient = true;
		}

		void DeliverCitizen()
		{
			deliveringTimer += Time.deltaTime;
			if (deliveringTimer >= deliveringTime) {
				MonoBehaviour.Destroy (CurrentActionTarget);
				 
				Building_PublicService_Hospital hospital = Self.HomeBuilding as Building_PublicService_Hospital;

				ExitBuilding (Self.HomeBuilding);
				deliveringTime = 0;
				deliveringPatient = false;
				isBusy = false;
				GetNewTarget ();
			}
		}

		void AttemptHeal()
		{
			healing = true;
			paramedicScript.CurrentSubState = SubState.ExecutingAction;
			isBusy = true;
		}

		void HealTarget()
		{
			healTimer += Time.deltaTime;
			if (healTimer >= paramedicScript.Healtime) {
				var Rand = Random.value;

				if(Rand >= 0.5f)
				{
					var totalHealAmount = 0f;
					healTimer = 0f;
					if (targetScript.stats.Health < 0f) {
						totalHealAmount = Mathf.Abs (targetScript.stats.Health);
					}

					totalHealAmount += paramedicScript.HealAmount;

					targetScript.stats.Health = totalHealAmount;

					targetScript.stats.CheckHealthStatus();

					if (targetScript.anim.isInitialized) {
						targetScript.anim.SetBool ("Stunned", false);
					}

					CurrentActionTarget = Self.HomeBuilding.gameObject;
					healedTarget = true;
					healing = false;

					GetNewTarget ();
				}
			}
		}

		public override void StopAction()
		{
			healedTarget = false;
			ActionCompleted = true;
			Self.CurrentAction = null;
		}

		public override bool GetNewTarget()
		{
			Status status = Status.Incapacitated;
			Vector3 currentPos = Self.transform.position;
			while ((int)status <= 2) {
				ILookup<Status, CitizenBehaviour> AllCitizens = WorldObjects.People.ToLookup(o => o.stats.CurrentStatus);
				IEnumerable<CitizenBehaviour> priorities = AllCitizens[status];

				GameObject tMin = null;
				float minDist = Mathf.Infinity;

				foreach (var priority in priorities)
				{
					float dist = Vector3.Distance(priority.transform.position, currentPos);
					if (dist < minDist)
					{
						tMin = priority.gameObject;
						minDist = dist;
					}
				}
				if (tMin != null) {
					CurrentActionTarget = tMin;
					targetScript = tMin.GetComponent<CitizenBehaviour> ();
					targetPosition = tMin.transform.GetChild (0).GetChild (0);
					healedTarget = false;
					goingHome = false;
					return true;
				}

				status++;
			}	

			targetPosition = Self.HomeBuilding.transform;
			goingHome = true;

			return false;
		}

		public HealPerson(CitizenBehaviour self, GameObject target)
		{
			Self = self;
			paramedicScript = Self as Citizen_PublicServant_Paramedic;
			ActionCompleted = false;
			try{
				if(target == null)
				{
					GetNewTarget();
				}

				CurrentActionTarget = target;
				ObjectiveRange = 0.15f;

				targetScript = CurrentActionTarget.GetComponent<CitizenBehaviour> ();
				targetPosition = CurrentActionTarget.transform.GetChild (0).GetChild(0);


				Self.SetPersonActive (true);

				animator = Self.anim;
			}
			catch(MissingReferenceException)
			{
				MonoBehaviour.Destroy (Self.gameObject);
			}
		}

	}

}

