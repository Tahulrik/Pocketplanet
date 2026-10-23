using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Citizens.Actions
{

	//This should not be an action. Make it part of behaviour instead?
	//perhaps the same with movement?
	public class StunAction : CitizenAction {
		private Vector2 stunOrigin;

		public override void ExecuteAction ()
		{

			if (Self.isStunned) {
				timer += Time.deltaTime;
				//Debug.Log ("Stunned for" + timer + "/" + actionTimer);
				if (timer > actionTimer) {
					Revive ();
					StopAction ();
					return;
				} 		
			} 


			if (!Self.isStunned) {
				//Debug.Log ("Stunned " + Self.gameObject.name);
				Stun (stunOrigin);
				Self.stats.CheckHealthStatus ();
			}
		}

		public void Stun(Vector2 direction)
		{					
			Self.isStunned = true;
			FindDirectionToTarget (direction);
			//if (animator.isInitialized) {
				animator.SetBool ("Stunned", true);
			//}
		}

		void Revive()
		{
			Self.isStunned = false;
			//Debug.Log ("Revived");
			//if (animator.isInitialized) {
				animator.SetTrigger ("Revived");
				animator.SetBool ("Stunned", false);
			//}
			Self.stats.CheckHealthStatus ();
		}

		public override void StopAction()
		{
			timer = 0;
            
			if (Self.stats.CurrentStatus != Status.Alive && Self.stats.CurrentStatus != Status.Injured) {
				Self.CurrentAction = null;
				return;
			}
			Self.CurrentAction = previousAction;
		}

		public StunAction(CitizenBehaviour self, GameObject stunTarget, float time, Vector2 stunOrigin, CitizenAction previousAction)
        {
            ActionCompleted = false;
            CurrentActionTarget = stunTarget;
			Self = self;
			actionTimer = time;
			this.previousAction = previousAction;

            this.stunOrigin = stunOrigin;
			animator = Self.anim;

            ExecuteAction();
        }
	}
}