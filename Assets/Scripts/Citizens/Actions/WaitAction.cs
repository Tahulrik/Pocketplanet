using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Citizens.Actions
{
	public class WaitAction : CitizenAction {



		public override void ExecuteAction ()
		{
			if (actionPaused) {
				return;
			}

            base.ExecuteAction();


			if (actionTimer > 0)
            {
                timer += Time.deltaTime;
				if (timer > actionTimer) {
                    //Wait();
				    StopAction ();
			    } else {
				    return;
			    }
            }
            else
            {
				if (ReachedTarget(Self.gameObject, CurrentActionTarget.transform.position))
                {
                    StopAction();
                }
                else
                {
                    return;
                }
            }


		}
			
		public WaitAction(CitizenBehaviour self, GameObject target, float time)
		{
			ActionCompleted = false;
			CurrentActionTarget = target;
			actionTimer = time;

			Self = self;

			animator = Self.anim;
		}
    }
}

