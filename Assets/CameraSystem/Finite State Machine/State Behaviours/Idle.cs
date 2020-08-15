using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem.CameraSystem.States
{
    public class Idle : SceneLinkedSMB<CameraController>
    {
        float snapZoomCurrentLevel = 0f;
        float zoomDir = 0f;
        float targetSnapZoom = 0f;
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            snapZoomCurrentLevel = controller.CurrentZoomAmount;
            zoomDir = (snapZoomCurrentLevel > controller.ZoomLevelSnapThreshold) ? 1f : -1f;

            targetSnapZoom = Mathf.Abs(controller.CityViewMinZoom - snapZoomCurrentLevel);
        }

        public override void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (snapZoomCurrentLevel > controller.CityViewMinZoom) //dont snap
            {
                controller.CurrentCameraState = CameraState.Idle;
            }
            else
            { 
                SnapCameraToLevel();
            }
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        void SnapCameraToLevel()
        {
            controller.CurrentCameraState = CameraState.Moving;
            snapZoomCurrentLevel += (Time.deltaTime * zoomDir) / (targetSnapZoom * 0.75f); // this will always take 1.5 seconds - how do i make it take 1.5 seconds for the entire distance
            snapZoomCurrentLevel = Mathf.Clamp(snapZoomCurrentLevel, 0.0f, controller.CityViewMinZoom);
            controller.SetZoomLevel(snapZoomCurrentLevel);

            controller.SetCameraViewState();
        }
    }
}
