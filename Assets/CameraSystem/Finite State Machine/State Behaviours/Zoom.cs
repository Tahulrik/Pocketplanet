using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem.CameraSystem.States
{
    public class Zoom : SceneLinkedSMB<CameraController>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            controller.CurrentCameraState = CameraState.Zooming;
        }

        public override void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ZoomCamera();
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (controller.CurrentZoomAmount < controller.ZoomLevelSnapThreshold)
            {
                controller.ResetCamera();
            }
        }

        void ZoomCamera()
        {
            var screenPoint = default(Vector2);

            if (LeanGesture.TryGetScreenCenter(InputController.instance.PinchFilter.GetFingers(), ref screenPoint) == true)
            {
                var newZoomLevel = controller.CurrentZoomAmount * InputController.instance.pinchLevel;

                controller.SetZoomLevel(newZoomLevel);

                if (controller.CurrentZoomAmount < controller.ZoomLevelSnapThreshold)
                {
                    controller.CentreCameraOnOrigin();
                }
            }
        }
    }
}

