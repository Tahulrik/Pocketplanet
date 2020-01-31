using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraSystem.StateMachine.States
{
    public class Swipe : Camera_BaseState
    {
        public float SwipeForce = 0;
        public float SwipeTargetAngle = 0;
        public float SwipeDistance = 0;
        public float SwipeCurrentDampening = 2.5f;
        float SwipeNormalDampening = 2.5f, SwipeBreakDampening = 8f;
        float MaxForce = 350;

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            base.OnStateMachineEnter(animator, stateMachinePathHash);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            SwipeCurrentDampening = SwipeNormalDampening;

            animator.SetBool("IsSwiping", true);
            SwipeDistance = animator.GetFloat("SwipeDistance");

            SetSwipeTarget();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            MoveTowardsSwipe();

            if (Mathf.Abs(SwipeTargetAngle) < 2f)
            {
                SwipeTargetAngle = 0;
                animator.SetBool("IsSwiping", false);
                
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetFloat("SwipeDistance", 0);
        }



        public void SetMaxSwipeForce(float newMaxForce)
        {
            MaxForce = newMaxForce;
        }
    }
}
