using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

namespace Citizens.Actions
{
	public class ExtortPersonAction : MafiaAction {

		public static event MafiaCrime ExtortingPerson;

		Citizen_Civilian targetScript;
		CitizenStats targetStatsScript;


		bool startedExtorsion = false;
		bool personExtorted = false;

		float extorsionTimer = 0f;


		public override void ExecuteAction ()
		{
			if (actionPaused) {
				return;
			}

			base.ExecuteAction ();



            if (CurrentActionTarget == null) {
				GetNewTarget (WorldObjects.ValidTargets);
			}

            if (!personExtorted)
            {
                if (targetScript.stats.Health <= 0)
                {
                    //Debug.Log(Self.name + " Target died, going home");
                    FinishedExtorting();
                    return;
                }

                if (targetScript.inBuilding)
                {
                    GetNewTarget(WorldObjects.ValidTargets);
                }
            }  
               

          
            //If a police officer is in near, start running in the opposite direction
            if (ReachedTarget(Self.gameObject, targetPosition.transform.position)) {
               // Debug.Log("Reached Target");
        

                if (personExtorted) {
                   // Debug.Log("Entering Home");

                    MonoBehaviour.Destroy(Self.gameObject);
                    //EnterBuilding(Self.HomeBuilding);
					//StopAction ();
				} else {
                   // Debug.Log("Starting Extorsion");

                    ExtortTarget();
				}
			}
			else
			{
                //Debug.Log("Moving to target");

                MoveToTarget(Self.CurrentMoveSpeed);
			}
		}

		void ExtortTarget()
		{
			//this is no good....
			var mafia = Self.GetComponent<Citizen_Criminal_Mafia> ();
            if (!startedExtorsion) {
               // Debug.Log("Initialising extorsion");


                Self.committedCrime = true;
                Self.CurrentSubState = SubState.ExecutingAction;
                if (ExtortingPerson != null) {
                    ExtortingPerson(Self.gameObject);
                }


                extorsionTimer = mafia.ExtortionSpeed / 2;
                Self.CurrentState = ActionState.ExtortPerson;
                startedExtorsion = true;

                if (targetScript.CurrentAction != null)
                {
                    targetScript.CurrentAction.PauseAction(true);
                }
			}

			extorsionTimer += Time.deltaTime;

            //make sure target is alive

			if (extorsionTimer >= mafia.ExtortionSpeed) {
                if (targetScript.CurrentAction != null)
                {
                    targetScript.CurrentAction.PauseAction(true);
                }
               //Debug.Log("Rolling extorsion result");

                //Change this such that it is dependant on a persons mood
                var rand = Random.Range (1, 3);
				if (rand == 1) {
                    //Debug.Log("Reached Target");

                    TakeMoney(mafia.StealAmount);
					FinishedExtorting ();
				} else if (rand == 2) {

					AskAgain ();
				}
			}

		}

		void FinishedExtorting()
		{
			startedExtorsion = false;
            personExtorted = true;

            if (targetScript.CurrentAction != null)
            {
                targetScript.CurrentAction.PauseAction(false);

            }

			CurrentActionTarget = Self.HomeBuilding.gameObject;
            targetPosition = CurrentActionTarget.transform;

               // Debug.Log("Going Home");

            //targetScript = null;
		}

		void TakeMoney(float percentage)
		{
			var moneyPercent = (targetStatsScript.MoneyAmount / 100) * percentage;

			targetStatsScript.MoneyAmount -= moneyPercent;
			RobbedMoney += moneyPercent;

			personExtorted = true;
		}

		void AskAgain()
		{
			//this is no good....
			var mafia = Self.GetComponent<Citizen_Criminal_Mafia> ();
			Gunparticle.Play ();
            //Gunparticle.GetComponent<AudioSource>().Play();
            targetScript.stats.TakeDamage(mafia.GunDamage);
			

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

			List<GameObject> personTargets = new List<GameObject>();

			for (int i = 0; i < targets.Count; i++) {
				if (targets [i].GetComponent<Citizen_Civilian> ()) {
					personTargets.Add(targets [i]);
				}

			}
           // Debug.Log("Getting new target");
			CurrentActionTarget = personTargets [Random.Range (0, personTargets.Count)];

            targetPosition = CurrentActionTarget.transform.GetChild(0).GetChild(0); ;
			targetScript = CurrentActionTarget.GetComponent<Citizen_Civilian> ();

			return true;
		}


		public ExtortPersonAction(CitizenBehaviour self, CitizenBehaviour target)
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

			targetStatsScript = CurrentActionTarget.GetComponent<CitizenStats> ();
			targetScript = target as Citizen_Civilian;
			targetPosition = CurrentActionTarget.transform.GetChild (0).GetChild(0);

			animator = Self.anim;

			//Change this to a "chasing" system. Also in the talk action
			//ForceTargetToHalt ();

			Gunparticle = Self.transform.GetChild (0).GetChild(0).GetChild (1).GetComponentInChildren<ParticleSystem> ();

			ExecuteAction ();
		}
	}
}
