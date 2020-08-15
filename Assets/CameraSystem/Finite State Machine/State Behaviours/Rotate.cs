using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem.CameraSystem.States
{
    public class Rotate : SceneLinkedSMB<CameraController>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            controller.CurrentCameraState = CameraState.Moving;
        }

        public override void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            RotateCameraView();
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        void RotateCameraView()
        {
                Vector3 fingerScreenPos = InputController.instance.ActiveFingers[0].ScreenPosition;

                int leftBoundary = Screen.width / 100 * 20;
                int rightBoundary = Screen.width / 100 * 80;

                float rotationDirection = 0;
                if (fingerScreenPos.x < leftBoundary)
                    rotationDirection = 1;
                else if (fingerScreenPos.x > rightBoundary)
                    rotationDirection = -1;

                controller.RotateCameraByAmount(controller.CameraRotationSpeed * rotationDirection);
        }
    }
}
