using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens.Actions;
using Buildings;

namespace Citizens{

	public class Citizen_PublicServant_PoliceOfficer : Citizen_PublicServant {


		//make sure than endurance is refilling when 
		public float Stamina;
		public float MaxStamina = 25f;

		public float TimeToCatch = 5f;

		public float GunDamage = 10f;

		void OnEnable()
		{
			Building.OnDestroyed += CheckIfHomeWasDestroyed;
			CitizenStats.Dead += TargetDied;

		}

		void Disable()
		{
			Building.OnDestroyed -= CheckIfHomeWasDestroyed;
			CitizenStats.Dead -= TargetDied;

		}

		protected override void Start ()
		{
			base.Start ();

			Stamina = MaxStamina;
		}

		void CheckIfHomeWasDestroyed(GameObject GO)
		{
			if (GO != HomeBuilding) {
				return;
			}

 			StartCoroutine(GetNewHome ());

		}

		void TargetDied(GameObject GO)
		{
			if (CurrentAction == null) {
				return;
			}

			if (CurrentAction.isBusy) {
				print (name + " is busy");
				return;
			}


			if (GO == CurrentAction.CurrentActionTarget) {
				print (name + " is finding new target");
				CurrentAction.GetNewTarget ();
			}
		}

		protected override void Update ()
		{
			base.Update ();
		}

	}

}
