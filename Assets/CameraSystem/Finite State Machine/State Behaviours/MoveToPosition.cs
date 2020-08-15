using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem.CameraSystem.States
{
    public class MoveToPosition : SceneLinkedSMB<CameraController>
    {
        float RemainingLerp;
        float CurrentRotation;
        float TargetRotation;

        float moveCurveVal;
        Vector3 rotationEuler;
        float angleBetweenCameraAndPoint;
        float destinationZoomLevel;
        Vector3? focusPosition;

        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
        {
            RemainingLerp = 0;
            TargetRotation = 0;

            moveCurveVal = 0f;
            rotationEuler = Vector3.zero;
            CurrentRotation = controller.transform.rotation.eulerAngles.z;


            focusPosition = controller.targetFocusPosition;

            angleBetweenCameraAndPoint = Vector2.SignedAngle(focusPosition.Value, controller.CameraHolder.transform.position);
            TargetRotation = CurrentRotation - angleBetweenCameraAndPoint;
            controller.CurrentCameraState = CameraState.Moving;

            bool hasEntitySelection = InteractionController.instance.HasSelectedEntity();
            if (hasEntitySelection)
            {
                var rendererSize = InteractionController.instance.GetSizeOfSelectedWorldEntity();
                var percentageSize = (1280 / rendererSize) * 100f;
                var cityViewDifferenceFromMaxZoom = 1f - CameraController.instance.CityViewMinZoom;
                var adjustmentToCityView = (cityViewDifferenceFromMaxZoom * percentageSize / 100f);
                var finalViewZoom = Mathf.Clamp(CameraController.instance.CityViewMinZoom + adjustmentToCityView, CameraController.instance.CityViewMinZoom, 1f);
                destinationZoomLevel = finalViewZoom; //find the correct lerp amount pased on object size
            }
            else
            {
                destinationZoomLevel = 1f;
            }
        }

        public override void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(RemainingLerp < destinationZoomLevel)
            {
                MoveCameraToSelectedPosition();
            }
            else
            {
                 // this is a bad solution
                animator.SetBool("IsMovingToPosition", false);
                controller.SetCameraViewState();

                if (InteractionController.instance.HasSelectedEntity())
                {
                    controller.CentreCameraOnTargetPosition(InteractionController.instance.GetCentreOfSelectedWorldEntity());
                }
            }
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            controller.SetManualAction(true);
        }

        void MoveCameraToSelectedPosition()
        {
            RemainingLerp = Mathf.Clamp01(RemainingLerp + (Time.deltaTime/2f));

            moveCurveVal = CurrentRotation + controller.CameraMoveToCurve.Evaluate(RemainingLerp) * (TargetRotation - CurrentRotation);
            var nextRotation = moveCurveVal;
            rotationEuler = new Vector3(0, 0, nextRotation);
            Quaternion rot = controller.transform.rotation;
            rot.eulerAngles = rotationEuler;
            controller.transform.rotation = rot;

            var RemainingZoomLerp = RemainingLerp;
            if (InteractionController.instance.HasSelectedEntity())
            {
                if (controller.CurrentZoomAmount > destinationZoomLevel)
                {
                    RemainingZoomLerp = Mathf.Clamp(RemainingLerp, destinationZoomLevel, controller.CurrentZoomAmount);
                }
                else if (controller.CurrentZoomAmount <= destinationZoomLevel)
                {
                    RemainingZoomLerp = Mathf.Clamp(RemainingLerp, controller.CurrentZoomAmount, destinationZoomLevel);
                }

                
            }
            
            
            controller.SetZoomLevel(RemainingZoomLerp);
        }

    }
}
