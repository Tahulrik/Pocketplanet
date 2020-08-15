using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean;
using Lean.Touch;
using Cinemachine;
using System.Linq;
using InteractionSystem.CameraSystem.States;

namespace InteractionSystem.CameraSystem
{ 
    public enum ViewState
    {
        PlanetView, //Used when the camera is zoomed out viewing the planet
        CityView, //Used when the camera is near the city

        EventView //Used When Camera must move to look at an event happening
    }

    public enum CameraState
    {
        Idle,
        Moving, 
        Zooming, 
        Swiping,
        Following
    }

    public class CameraController : BaseGameController
    {
        public float PlanetRadius = 8;

        public float FarthestZoom;
        public float NearestZoom;
        public AnimationCurve CameraZoomMoveCurve;
        public AnimationCurve CameraMoveToCurve;
        public AnimationCurve CameraFocusOnEventCurve;
        CinemachineVirtualCamera VCam;

        internal GameObject CameraHolder;
        private GameObject CameraTarget;


        private Camera MainCam;
        private Animator animator;

        [Range(0.1f, 1)]
        public float CurrentZoomAmount = 0.1f;
        [Range(0.15f, 0.95f)]
        public float ZoomLevelSnapThreshold = 0.5f;
        public float CameraMinMoveHeight = 0f, CameraMaxMoveHeight = 1.2f, CameraMaxMoveWidth = 0.5f;

        internal float CityViewMaxZoom = 0f;
        
        [Range(0.0f, 1f)]
        public float CityViewMinZoom = 0.90f;

        [Range(10f, 50f)]
        public float CameraRotationSpeed = 25f;
        internal float SwipeTargetAngle = 0;
        
        public float CurrentSwipeForce = 0f;
        public float SwipeCurrentDampening = 0;
        public float SwipeMinimumDampening = 2.5f, SwipeMaximumDampening = 5f;
        [Range(0, 2f)]
        public float SwipeForceModifier = 1;

        private ViewState LastViewState;
        public ViewState CurrentViewState;
        public CameraState CurrentCameraState;
        bool IsCameraReset
        {
            get
            {
                bool result = true;
                if (CameraTarget.transform.localPosition != Vector3.zero || CurrentZoomAmount != 0.1f || CurrentZoomAmount == 1f)
                {
                    result = false;
                }

                return result;
            }
        }

        bool IsCurrentActionManual = true;


        internal Vector2? targetFocusPosition;

        float cameraDistance;

        public static CameraController instance;

        void OnEnable()
        {
            InputController.Swiped += CommandStartSwipe;
            InputController.Hold += CommandEndSwipe;

            InputController.PinchStarted += CommandStartPinch;
            InputController.PinchEnded += CommandEndPinch;

            InteractionController.MapClicked += CommandMoveToTargetPosition;
            InteractionController.EntitySelected += CommandMoveToEntity;
            InteractionController.EntityDeselected += EntityDeselected;
        }

        void OnDisable()
        {
            InputController.Swiped -= CommandStartSwipe;
            InputController.Hold -= CommandEndSwipe;
            InputController.PinchStarted -= CommandStartPinch;
            InputController.PinchEnded -= CommandEndPinch;

            InteractionController.MapClicked -= CommandMoveToTargetPosition;
            InteractionController.EntitySelected -= CommandMoveToEntity;
            InteractionController.EntityDeselected -= EntityDeselected;
        }

        // Start is called before the first frame update
        void Awake()
        {
            MainCam = Camera.main;
            animator = GetComponent<Animator>();
            VCam = GetComponentInChildren<CinemachineVirtualCamera>();
            cameraDistance = VCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance;
            CameraTarget = VCam.Follow.gameObject;
            CameraHolder = CameraTarget.transform.parent.gameObject;

            SceneLinkedSMB<CameraController>.Initialise(animator, this);
            CurrentViewState = ViewState.PlanetView;
            LastViewState = CurrentViewState;
            SetManualAction(true);

            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            var activeFingers = InputController.instance.fingerCount;
            
            animator.SetInteger("ActiveFingers", activeFingers);

            SetCameraViewState();
            
        }

        #region Commands
        private void CommandMoveToPosition(Vector2 position)
        {
            animator.SetBool("IsMovingToPosition", true);
            targetFocusPosition = position;
        }

        private void CommandMoveToEntity(Vector2 clickPosition, WorldEntity entity)
        {
            if (IsCurrentActionManual)
            { 
                animator.SetBool("FocusingOnTarget", true); //Keep camera focused on target after click
                CommandMoveToPosition(entity.gameObject.transform.position);
            }
        }

        private void CommandMoveToTargetPosition(Vector2 clickPosition, WorldEntity entity)
        {
            if (IsCurrentActionManual)
            {
                animator.SetBool("FocusingOnTarget", false); //Just move to position
                CommandMoveToPosition(clickPosition);
            }
        }

        private void CommandStartSwipe(float swipeForce)
        {
            SetSwipeForce(swipeForce);
            animator.SetBool("IsSwiping", true);
        }

        private void CommandEndSwipe(float swipeForce)
        {
            if (SwipeTargetAngle != 0)
            {
                Log("Finger held, stopping swipe");
                CurrentSwipeForce = 0;
                SwipeCurrentDampening = SwipeMaximumDampening;
            }
        }

        private void CommandStartPinch()
        {
            animator.SetBool("IsPinching", true);
        }

        private void CommandEndPinch()
        {
            animator.SetBool("IsPinching", false);
        }
        #endregion


        #region ZoomFunctions
        public void SetZoomLevel(float newZoomLevel)
        {
            var clampedZoomLevel = Mathf.Clamp(newZoomLevel, 0.1f, 1f);
            float curveAdjustedZoomLevel = Mathf.Clamp01(CameraZoomMoveCurve.Evaluate(clampedZoomLevel));

            float zoomedLevel = 0f;
            zoomedLevel = Mathf.Lerp(NearestZoom, FarthestZoom, 1 - clampedZoomLevel);
            Vector3 newCamPos = new Vector3(0, curveAdjustedZoomLevel * PlanetRadius ,0);

            VCam.m_Lens.OrthographicSize = zoomedLevel;

            CameraHolder.transform.localPosition = newCamPos;
            CurrentZoomAmount = clampedZoomLevel;

            if (CurrentZoomAmount < ZoomLevelSnapThreshold)
            {
                CentreCameraOnOrigin();
            }

            animator.SetFloat("ZoomLevel", CurrentZoomAmount);
        }
        #endregion

        #region MoveFunctions
        internal void MoveCameraInViewField()
        {
            var firstFinger = InputController.instance.GetTouchingFinger();
            if (firstFinger != null)
            {
                
                var deltaPos = firstFinger.ScaledDelta.normalized;

                Vector3 targetPos = CameraTarget.transform.position * deltaPos;
                targetPos = CameraTarget.transform.InverseTransformPoint(targetPos);
                targetPos.x = Mathf.Clamp(targetPos.x, -CameraMaxMoveWidth, CameraMaxMoveWidth);
                targetPos.y = Mathf.Clamp(targetPos.y, CameraMinMoveHeight, CameraMaxMoveHeight);
                CameraTarget.transform.localPosition = targetPos;
            }
            else
            {
                return;
            }
            
        }
        internal void RotateCameraByAmount(float amount)
        {
            transform.Rotate(Vector3.forward * amount * Time.deltaTime);
        }
        #endregion

        #region SwipeCameraFunctions
        void SetSwipeForce(float swipeForce)
        {
            float maxForce = 350;

            CurrentSwipeForce += swipeForce;
            float clampedForce = Mathf.Clamp(CurrentSwipeForce, -maxForce, maxForce);
            var swipeAmountInAngles = (clampedForce / maxForce) * (1000 * SwipeForceModifier);

            SwipeTargetAngle += swipeAmountInAngles;
        }

        internal void SetSwipeTarget(float swipeForce)
        {
            float maxForce = 350;

            float clampedForce = Mathf.Clamp(swipeForce, -maxForce, maxForce);
            var swipeAmountInAngles = (clampedForce / maxForce) * (1000 * SwipeForceModifier);

            SwipeTargetAngle += swipeAmountInAngles;

            animator.SetBool("IsSwiping", true);
        }
        #endregion

        #region AutomaticMoveFunctions


        IEnumerator MoveCameraToSelectedAngle(float targetAngle)
        {
            float currentRotation = transform.rotation.eulerAngles.z;
            float targetRotation = targetAngle;
            float remainingLerp = 0;
            float moveCurveVal = 0f;
            float rotateDir = 1;
            float angleBetweenCameraAndPoint = currentRotation - targetRotation;
            Vector3 rotationEuler = Vector3.zero;

            if (angleBetweenCameraAndPoint < currentRotation)
                rotateDir = -1;
            else
                rotateDir = 1;

            remainingLerp = 0;
            var remainingRotate = 0f;
            yield return new WaitUntil(() =>
            {
                if (currentRotation == targetRotation)
                    return true;

                moveCurveVal = CameraMoveToCurve.Evaluate(remainingLerp);
                currentRotation = moveCurveVal * targetRotation;
                rotationEuler = new Vector3(0, 0, currentRotation * rotateDir);

                Quaternion rot = transform.rotation;

                rot.eulerAngles = rotationEuler;
                transform.rotation = rot;

                SetZoomLevel(remainingLerp);

                if (remainingLerp >= 1)
                    return true;
                else
                    return false;

            });
            animator.SetBool("IsMovingToPosition", false);
            SetCameraViewState();
        }

        IEnumerator MoveCameraToEvent(Vector2 targetPosition)
        {
            float angleBetweenCameraAndPoint = 0;
            float CurrentRotation = transform.rotation.eulerAngles.z;
            float TargetRotation = 0;
            float RemainingMove = 0;
            float moveCurveVal = 0f;
            var RemainingZoom = CurrentZoomAmount;
            Vector3 rotationEuler = Vector3.zero;
            Debug.DrawLine(Vector3.zero, targetPosition, Color.blue, 10);
            angleBetweenCameraAndPoint = Vector2.SignedAngle(targetPosition, CameraHolder.transform.position);
            TargetRotation = -angleBetweenCameraAndPoint;
            float maxAngle = 180;
            float maxZoomAmount = 0f;
            float timeToReachPosition = 2f;

            if(CurrentZoomAmount > ZoomLevelSnapThreshold)
            { 
                maxZoomAmount = Mathf.Clamp01(1 - (Mathf.Abs(TargetRotation/2f) / maxAngle)*0.25f);
            }

            timeToReachPosition = 2;

            Keyframe start = new Keyframe(0.1f, CurrentZoomAmount);
            Keyframe midLeft = new Keyframe(0.45f, maxZoomAmount);
            midLeft.weightedMode = WeightedMode.Both;

            Keyframe midRight = new Keyframe(0.55f, maxZoomAmount);


            Keyframe end = new Keyframe(1f, 1f);
            Keyframe[] keys = new Keyframe[4] {start, midLeft, midRight, end};
            CameraFocusOnEventCurve = new AnimationCurve(keys);

            
            RemainingMove = 0f;
            var lerpAmount = 0f;
            yield return new WaitUntil(() =>
            {
                lerpAmount += Time.deltaTime / timeToReachPosition;
                RemainingMove = Mathf.Clamp01(lerpAmount);
                RemainingZoom = Mathf.Clamp01(CameraFocusOnEventCurve.Evaluate(lerpAmount));

                if (RemainingMove <= 1)
                {
                    moveCurveVal = CameraMoveToCurve.Evaluate(RemainingMove) * TargetRotation;
                    CurrentRotation += moveCurveVal * Time.deltaTime;
                    rotationEuler = new Vector3(0, 0, CurrentRotation);
                    Quaternion rot = transform.rotation;
                    rot.eulerAngles = rotationEuler;
                    transform.rotation = rot;
                }
                if (lerpAmount <= 1)
                { 
                    SetZoomLevel(RemainingZoom);
                }

                if (RemainingMove == 1 && RemainingZoom == 1)
                    return true;
                else
                    return false;
            });
            animator.SetBool("IsMovingToPosition", false);
            SetCameraViewState();
        }
        #endregion

        internal void ResetCamera()
        {
            SetCameraViewState();
            if (IsCameraReset)
                return;
        }

        internal void SetCameraViewState()
        {
            var newViewState = LastViewState;

            if (CurrentZoomAmount > ZoomLevelSnapThreshold)
            {
                newViewState = ViewState.CityView;
            }
            else
            {
                newViewState = ViewState.PlanetView;
            }

            if (newViewState != LastViewState)
            {
                CentreCameraOnOrigin();
                LastViewState = newViewState;
                CurrentViewState = newViewState;
            }
        }

        internal void CentreCameraOnOrigin()
        {
            CameraTarget.transform.localPosition = Vector3.zero;
        }

        internal void CentreCameraOnTargetPosition(Vector3 focusPosition)
        {
            var targetPos = CameraTarget.transform.InverseTransformPoint(focusPosition);
            CameraTarget.transform.localPosition = targetPos;
        }

        internal void SetManualAction(bool newValue)
        {
            IsCurrentActionManual = newValue;
            animator.SetBool("IsCurrentActionManual", newValue);
        }

        private void EntityDeselected(Vector2 clickedPosition, WorldEntity worldEntity)
        {
            CentreCameraOnOrigin();
            targetFocusPosition = null;
            animator.SetBool("FocusingOnTarget", false);
        }
        private void OnValidate()
        {
            if (CityViewMinZoom < ZoomLevelSnapThreshold)
            {
                CityViewMinZoom = ZoomLevelSnapThreshold;
            }
        }
    }
}