using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens.Actions;
using Buildings;

namespace Citizens
{
	[RequireComponent (typeof (HostileCitizenTarget))]

	public class Citizen_Civilian : CitizenBehaviour {
		
		HostileCitizenTarget targetScript;
		public GameObject tempTarget;

		bool waiting = false;
		protected override void Start()
		{
			targetScript = GetComponent<HostileCitizenTarget> ();

			base.Start ();

			targetScript.SetValidTarget (true);


            GetNewTask();
		}

		protected override void Update ()
		{
			base.Update ();

            
            switch (CurrentState)
            {
                case ActionState.Idle:
				if (!waiting)
				{
					StartCoroutine(WaitFor(Random.Range(5f, 10f)));
					waiting = true;
				}        
				break;

                case ActionState.Test:

                    break;
                default:
                    break;
            }


		}

        IEnumerator WaitFor(float time)
        {
			anim.SetInteger ("Movement", 0);
			CurrentSubState = SubState.Waiting;
			CurrentState = ActionState.Idle;
            yield return new WaitForSeconds(time);
            GetNewTask();
			waiting = false;
        }

        public override void GetNewTask()
        {
            if (stats.MoneyAmount <= 5)
            {
                var bank = FindObjectOfType<Building_Normal_Service_Bank>();
                if (bank == null)
                {
                    return;
                }

                CurrentAction = new BuyFromStore(this, bank);
                return;
            }


            var rand = Random.Range(1, 4);

            if (rand == 1)
            {
				CurrentAction = new TalkAction (this, WorldObjects.GetPersonTarget (this), Random.Range (5f, 15f));
            }
            if (rand == 2)
            {
                CurrentAction = new IdleMovement(this, Random.Range(5f, 15f));
            }
            if (rand == 3)
            {
                var store = FindObjectOfType<Building_Normal_Service_Store>();
                if (store == null)
                {
                    return;
                }
                CurrentAction = new BuyFromStore(this, store);
            }

        }
    }	
}
