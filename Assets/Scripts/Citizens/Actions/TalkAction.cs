using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Citizens.Actions{

	public class TalkAction : CitizenAction {
        
		CitizenBehaviour targetScript;

		bool isTalking = false;

		//AnimHash
        public override void ExecuteAction ()
		{
			if (actionPaused) {
				return;
			}

            base.ExecuteAction();
			try{
				if (targetScript.CurrentAction != null) {
					if (targetScript.CurrentAction.actionPaused && targetScript.CurrentState == ActionState.Talking) {
                        PauseAction(true);
					} else {
                        PauseAction(false);
					}
				}

				
			if (actionPaused) {
				return;
			}

			if (CurrentActionTarget == null) {
				StopAction ();
			}


			//perhaps add "halt distance"
			if (GetDistanceToTarget(Self.gameObject, targetScript.gameObject.transform.position) < 4) {
				ForceTargetToHalt (targetScript);
			}
				
			if (ReachedTarget(Self.gameObject, targetPosition.position))
            {
				if (Self.CurrentState != ActionState.Talking)
                {
					if (targetScript.CurrentAction != null) {
						targetScript.CurrentAction.PauseAction (true);
					}
					Self.CurrentState = ActionState.Talking;
					targetScript.CurrentState = ActionState.Talking;
					targetScript.CurrentSubState = SubState.Waiting;

                }
				if (animator.isInitialized) {
					targetScript.anim.SetBool ("IsTalking", true);
					animator.SetBool ("IsTalking", true);
				}

                timer += Time.deltaTime;
				if (timer > actionTimer)
				{
                    StopAction();
                }
            }
            else
            {
				MoveToTarget(Self.CurrentMoveSpeed);
            }

			}
			catch(System.NullReferenceException) {
				StopAction ();
			}
		}

		public CitizenBehaviour GetCitizenScript (GameObject target)
		{
			return target.GetComponent<CitizenBehaviour> ();
		}
        
		public override void StopAction()
		{
			base.StopAction ();
			try{
				
				if (targetScript.CurrentAction != null) {
					targetScript.CurrentAction = targetScript.CurrentAction.previousAction;
					targetScript.CurrentAction.PauseAction(false);

				}
				if (targetScript.anim.isInitialized) {
					targetScript.anim.SetBool ("IsTalking", false);
				}
				if (animator.isInitialized) {
					animator.SetBool ("IsTalking", false);
				}

			}
			catch(System.NullReferenceException) {
				
			}
		
		}

		public TalkAction(CitizenBehaviour self, CitizenBehaviour target, float time)
		{
			Self = self;
			ActionCompleted = false;
			if (target == null) {
				StopAction ();
				return;
			}
            CurrentActionTarget = target.gameObject;

            targetPosition = target.transform;
				
			targetScript = target;

			actionTimer = time;

			animator = Self.anim;

		}
	}
}
