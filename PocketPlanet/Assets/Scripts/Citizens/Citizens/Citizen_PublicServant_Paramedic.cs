using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

namespace Citizens
{

	public class Citizen_PublicServant_Paramedic : Citizen_PublicServant{
		[HideInInspector]
		public GameObject HomeDepartment;

		//make sure than endurance is refilling when 
		public float Stamina;
		public float MaxStamina = 25f;

		public float Healtime = 3f;
		public float HealAmount = 50f;



		void OnEnable()
		{
			Building.OnDestroyed += CheckIfHomeWasDestroyed;
			CitizenStats.Dead += TargetDied;
			//check for closer whenever an event is recieved?
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
			if (GO != HomeDepartment) {
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
			switch (CurrentState) {
			case ActionState.HealPerson:

				//ideally should find the closest building on fire
				if (CurrentAction == null || CurrentAction.GetType() != typeof(Actions.HealPerson))
				{
					//print (true);
					//CurrentAction = new CatchCriminal(gameObject, );
				}

				break;
			}

			base.Update ();
		}

	}
}

