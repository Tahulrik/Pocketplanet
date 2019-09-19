using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;
using System.Linq;

namespace Citizens.Actions
{
	public class CatchCriminal : PublicServantAction {

		//Temporary
		CitizenBehaviour targetScript;

		Citizen_PublicServant_PoliceOfficer personScript;

		bool startedFighting = false;

		ParticleSystem gunParticle;

		bool startedChase = false;
		bool chasingCriminal = false;
		bool targetArrested = false;
		bool attemptingArrest = false;
        bool jailingTarget = false;
		float timeToShoot = 5f;
        float timeToJail = 2f;


        float catchTimer = 0f;
        float jailTimer = 0f;

		Building enteredBuilding;

		public override void ExecuteAction ()
		{
			base.ExecuteAction();

			if (actionPaused) {
				return;
			}


			if (!targetArrested && !Self.inBuilding) {
			   // Debug.Log ("Attempting chase");
				var dist = Vector2.Distance (CurrentActionTarget.transform.root.position, Self.transform.root.position);
				
				if (dist < personScript.ViewRange) {
					if (!startedChase) {
						StartChase ();
					}
				} else {
					//Should they stop running if getting out of view range?
					
					if (startedChase) {
						startedChase = false;
						StopRunning ();
					}
				}
			}




			if (attemptingArrest) {
                //Debug.Log("Attempting Arrest");

                ArrestTarget(targetScript);
                return;
			}

            if (jailingTarget)
            {
                JailTarget();
                return;
            }

            if (chasingCriminal)
            {
                // Debug.Log("Chasing");

                ChaseTarget();
            }

            //check if you have reached target location yet
            if (ReachedTarget(Self.gameObject, targetPosition.transform.position))
            {
                //Debug.Log("Reached target");

                if (!targetArrested) {
                    //Debug.Log("At target position");

                    if (targetScript.inBuilding) {
                        //Debug.Log("Target inside building");

                        enteredBuilding = targetScript.CurrentAction.CurrentActionTarget.GetComponent<Building> ();
						EnterBuilding (enteredBuilding);
					}
					StartArrestAttempt (targetScript);
				}
				else if(returningHome)
				{
                   // Debug.Log("Going home");
                    EnterBuilding(Self.HomeBuilding);
					//if no more criminals
					Building_PublicService_PoliceStation policeStation = Self.HomeBuilding as Building_PublicService_PoliceStation;
					policeStation.InactiveOfficers.Add (Self.gameObject);
                    //JailTarget ();
                    jailingTarget = true;
					//else set target as new criminal
				}
			}
			else
			{
                //Debug.Log("Moving to target");

                MoveToTarget(personScript.CurrentMoveSpeed);
			}
		}
			
		public override void StopAction()
		{
			ActionCompleted = true;
			personScript.CurrentAction = null;
		}


		void StartChase()
		{
			//Remember to clamp stamina, to avoid wierd float values
			if (personScript.Stamina == personScript.MaxStamina) {

				timeToShoot = Random.Range (2.5f, 10f);

				startedChase = true;
				chasingCriminal = true;
				StartRunning ();
			}
		}

 


		void ChaseTarget()
		{

            //clamp stamina?
            if (personScript.Stamina > 0)
            {
                personScript.Stamina -= Time.deltaTime;
            }
            else if (personScript.Stamina <= 0)
            {
                StopRunning();
            }
            //Depending on severity of crime
            //Depending if criminal is armed or not
            //Depending on Officer mood?

            //Do this at a random timer
            timer += Time.deltaTime;
			if (timer >= timeToShoot) {
				if (targetScript.stats.CurrentStatus == Status.Dead) {
                    GetNewTarget();
					return;
				}
				timer = 0;
				timeToShoot = Random.Range (2.5f, 10f);
				ShootAtTarget (targetScript);
			}
		}
		
		void ShootAtTarget(CitizenBehaviour target)
		{

			//Raycast to check for hits
			//Select a target to hit based on multiple factors?
			gunParticle.Play ();


			target.stats.TakeDamage(personScript.GunDamage);
		}

		void StartArrestAttempt(CitizenBehaviour target)
		{
            //Debug.Log("Starting Arrest");
			targetScript.CurrentAction.PauseAction(true);

			attemptingArrest = true;
		}

		void ArrestTarget(CitizenBehaviour target)
		{
			//Make sure target is no less than incapacitated
			if (target.stats.CurrentStatus != Status.Incapacitated && target.stats.CurrentStatus != Status.Dead) {

				catchTimer += Time.deltaTime;

			//	if (Self.inBuilding && !targetScript.inBuilding) {
				//	ExitBuilding (enteredBuilding);
				//}

				if (catchTimer >= personScript.TimeToCatch) {
					target.stats.CurrentStatus = Status.Arrested;
					
					target.transform.parent = Self.transform;
                   // target.transform.position = -Self.transform.right;
					
					targetArrested = true;
					attemptingArrest = false;
					returningHome = true;

                    CurrentActionTarget = Self.HomeBuilding.gameObject;
                    targetPosition = Self.HomeBuilding.transform;

					GiveUpChase ();

                    catchTimer = 0.0f;

					if (personScript.inBuilding) {
						ExitBuilding (enteredBuilding);
						targetScript.CurrentAction.ExitBuilding (enteredBuilding);
					}
				}
			}
		}

		void GiveUpChase()
		{
			if (startedChase) {

				//Should the police officer give up completely, or still continue but without running.
				returningHome = true;
				startedChase = false;
				chasingCriminal = false;

				targetArrested = true;

				targetPosition = Self.HomeBuilding.transform;
				//Set movespeed to original
				StopRunning();
			}
		}

		void JailTarget()
		{
			try{
				
			
		        jailTimer += Time.deltaTime;
		       // Debug.Log("jailing");
		        MonoBehaviour.Destroy (targetScript.gameObject);
		        if (jailTimer > timeToJail)
		        {
		            //Debug.Log("jailed target");
		    		if (!GetNewTarget ()) {
			    		StopAction ();
				    }
		            jailTimer = 0f;
		        }
		        //Debug.Log("Jailed target" + targetScript.name);
			}
			catch(System.Exception) {
				MonoBehaviour.Destroy (Self.gameObject);
			}
		}
			
		public override bool GetNewTarget()
		{
            ResetOfficerTargetValues();

			Status status = Status.Incapacitated;
			Vector3 currentPos = Self.transform.position;
			while ((int)status <= 4) {
				if ((int)status == 3) {
					status++;
					continue;
				}
				ILookup<Status, CitizenBehaviour> AllCitizens = WorldObjects.People.ToLookup(o  => o.stats.CurrentStatus);
				IEnumerable<CitizenBehaviour> priorities = AllCitizens[status];

				GameObject tMin = null;
				float minDist = Mathf.Infinity;

				foreach (var priority in priorities)
				{
					if(priority.committedCrime)
					{
						float dist = Vector3.Distance(priority.transform.position, currentPos);
						if (dist < minDist)
						{
							tMin = priority.gameObject;
							minDist = dist;
						}
					}
				}
				if (tMin != null) {
					CurrentActionTarget = tMin;
					targetScript = tMin.GetComponent<CitizenBehaviour> ();
					targetPosition = tMin.transform.GetChild (0).GetChild (0);
			
                    if (Self.inBuilding) {
						Building_PublicService_PoliceStation policeStation = Self.HomeBuilding as Building_PublicService_PoliceStation;
						ExitBuilding (policeStation);
					}
                    //Debug.Log("more criminal targets ");
					return true;
				}
				status++;
			}	

			targetPosition = Self.HomeBuilding.transform;
			returningHome = true;
           // Debug.Log("no more criminal targets ");
            return false;
		}

        void ResetOfficerTargetValues()
        {

            ActionCompleted = false;

            startedChase = false;
            chasingCriminal = false;
            targetArrested = false;
            attemptingArrest = false;
            startedFighting = false;
            returningHome = false;
            jailingTarget = false;


            catchTimer = 0f;
            ObjectiveRange = 0.15f;

            targetScript = null;
            targetPosition = null;
        }

        public CatchCriminal(CitizenBehaviour self, CitizenBehaviour target)
		{
			Self = self;
			ActionCompleted = false;
			try{
				if(target == null)
				{
					GetNewTarget();
				}

				CurrentActionTarget = target.gameObject;
				ObjectiveRange = 0.15f;

				personScript = self.GetComponent<Citizen_PublicServant_PoliceOfficer> ();

				targetScript = target;
				targetPosition = CurrentActionTarget.transform.GetChild (0).GetChild(0);

				personScript.SetPersonActive (true);

				gunParticle = Self.GetComponentInChildren<ParticleSystem> ();

				animator = personScript.anim;
			}
			catch(MissingReferenceException)
			{
				MonoBehaviour.Destroy (Self.gameObject);
			}
		}

    }

}

