using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem.CameraSystem.States
{
    public class Swipe : SceneLinkedSMB<CameraController>
    {
        
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            SetCurrentSwipeDampening(controller.SwipeMinimumDampening);
            controller.CurrentCameraState = CameraState.Swiping;
        }

        public override void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
        {
            FindNextSwipePosition(animator);
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ForceStopSwipe();
        }

        void FindNextSwipePosition(Animator animator)
        {
            if (Mathf.Abs(controller.SwipeTargetAngle) < 5f)
            {
                controller.SwipeTargetAngle = 0;
                animator.SetBool("IsSwiping", false);
            }

            var moveAmountPerFrame = controller.SwipeTargetAngle * Time.deltaTime;

            var newRotationEuler = new Vector3(0, 0, moveAmountPerFrame);

            controller.transform.Rotate(newRotationEuler);

            controller.SwipeTargetAngle -= controller.SwipeTargetAngle * (Time.deltaTime * controller.SwipeCurrentDampening);
        }

        void SetCurrentSwipeDampening(float newDampening)
        {
            controller.SwipeCurrentDampening = newDampening;
        }

        void ForceStopSwipe()
        {
            controller.CurrentSwipeForce = 0;
            controller.SwipeTargetAngle = 0;
            SetCurrentSwipeDampening(controller.SwipeMinimumDampening);
            controller.ResetCamera();
        }
    }
}
