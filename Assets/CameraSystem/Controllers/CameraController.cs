using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean;
using Lean.Touch;
using Cinemachine;
using System.Linq;
using CameraSystem.StateMachine.States;

namespace CameraSystem
{
    public enum CameraState
    {
        PlanetView, //Used when the camera is zoomed out viewing the planet
        CityView, //Used when the camera is near the city
        Zooming, //Used when the camera is currently zooming
        MovingTo, //Used When Camera must move towards a position - either from snapping or from double click zoom
        EventView //Used When Camera must move to look at an event happening
    }

    public class CameraController : MonoBehaviour
    {
        public LeanFingerFilter Fingers = new LeanFingerFilter(true);

        public float PlanetRadius = 8;

        public float FarthestZoom;
        public float NearestZoom;
        public AnimationCurve CameraZoomMoveCurve;
        public AnimationCurve CameraMoveToCurve;
        public Animator animator;

        private CinemachineVirtualCamera VCam;
        private GameObject CameraHolder;
        private GameObject CameraTarget;
        private Camera MainCam;

        [Range(0.1f, 1)]
        public float CurrentZoomAmount = 0.1f;
        [Range(0.15f, 0.95f)]
        public float ZoomLevelSnapThreshold = 0.5f;
        public float CameraMinMoveHeight = 0f, CameraMaxMoveHeight = 1.2f, CameraMaxMoveWidth = 0.5f;

        [Range(10f, 50f)]
        public float CameraRotationSpeed = 25f;

        public float SwipeTargetAngle = 0;
        public float swipeForce = 0;
        public float SwipeCurrentDampening = 2.5f;
        public float SwipeMinimumDampening = 2.5f, SwipeMaximumDampening = 5f;

        public CameraState CurrentCameraState;

        bool isPinching;
        bool IsPinching
        {
            get {
                return isPinching;
            }
            set
            {
                isPinching = value;
                if (animator == null)
                {
                    animator = GetComponent<Animator>();
                }
                animator.SetBool("IsPinching", IsPinching);
            }
        }

        bool isTouching;
        bool IsTouching
        {
            get {
                return isTouching;
            }
            set
            {
                isTouching = value;
                if (animator == null)
                {
                    animator = GetComponent<Animator>();
                }
                animator.SetBool("IsTouching", isTouching);
            }
        }

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

        bool IsCurrentActionManual
        {
            get
            {
                if (CurrentCameraState == CameraState.EventView || CurrentCameraState == CameraState.MovingTo)
                {
                    return false;
                }
                return true;
            }
        }

        [Tooltip("This is set to true the frame a multi-tap occurs.")]
        public bool MultiTap;

        /// <summary>This is set to the current multi-tap count.</summary>
        [Tooltip("This is set to the current multi-tap count.")]
        public int MultiTapCount;

        /// <summary>Highest number of fingers held down during this multi-tap.</summary>
        [Tooltip("Highest number of fingers held down during this multi-tap.")]
        public int HighestFingerCount;

        // Seconds at least one finger has been held down
        public float age;

        // Previous fingerCount
        private int lastFingerCount;

        public float cameraDistance;

        void OnEnable()
        {
            LeanTouch.OnFingerDown += CommandSetFinger;
            LeanTouch.OnFingerUp += CommandRemoveFinger;

            LeanTouch.OnFingerSwipe += CommandFingerSwipe;

            SceneLinkedSMB<CameraController>.Initialise(animator, this);
        }

        void OnDisable()
        {
            LeanTouch.OnFingerDown -= CommandSetFinger;
            LeanTouch.OnFingerUp -= CommandRemoveFinger;

            LeanTouch.OnFingerSwipe -= CommandFingerSwipe;
        }

        // Start is called before the first frame update
        void Awake()
        {
            MainCam = Camera.main;
            VCam = GetComponentInChildren<CinemachineVirtualCamera>();
            cameraDistance = VCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance;
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (Fingers.GetFingers(true).Count >= 2)
            {
                CommandPinch(Fingers.GetFingers(true));
            }
        }

        void CommandPinch(List<LeanFinger> fingers)
        {
            var zoomAmount = LeanGesture.GetPinchScale(fingers, 1);

            if (zoomAmount != 1.0f) //Fingers changed position
            {
                var screenPoint = default(Vector2);

                if (LeanGesture.TryGetScreenCenter(fingers, ref screenPoint) == true)
                {
                    var newZoomLevel = CurrentZoomAmount * zoomAmount;
                    CurrentZoomAmount = newZoomLevel;
                    animator.SetFloat("ZoomLevel", CurrentZoomAmount);
                }
            }
        }

        //Find out what State the camera should go to
        void CommandSetFinger(LeanFinger finger)
        {
            animator.SetInteger("ActiveFingers", Fingers.GetFingers(true).Count);
        }

        //Find out what the resulting 
        void CommandRemoveFinger(LeanFinger finger)
        {
            animator.SetInteger("ActiveFingers", Fingers.GetFingers(true).Count);
        }

        void CommandFingerSwipe(LeanFinger finger)
        {
            var animatorState = animator.GetCurrentAnimatorStateInfo(0);

            if (animatorState.IsName("Idle") || animatorState.IsName("Swipe"))
            { 
                animator.SetTrigger("Swipe");
                animator.SetFloat("SwipeDistance", finger.SwipeScreenDelta.x);
            }
        }

        void ResetCamera()
        {
            SetCameraZoomState();

            if (IsCameraReset)
                return;

            StartCoroutine(SnapCameraToZoomPosition());
        }

        #region MoveFunctions
        public void MoveCamera()
        {
            //var fingers = Use.GetFingers(true);

           /* if (fingers.Count > 0)
            {
                //Move Cam in field
                MoveCameraInViewField(fingers);
            }*/
        }
        public void RotateCamera()
        {
            //var fingers = Use.GetFingers(true);

           /* if (fingers.Count > 0)
            {
                //Rotate planet if outside boundaries
                RotateCameraView(fingers);
            }*/
        }


        void MoveCameraInViewField(List<LeanFinger> fingers)
        {
            Vector3 targetPos = fingers[0].GetWorldPosition(cameraDistance);

            targetPos = CameraTarget.transform.InverseTransformPoint(targetPos);
            targetPos.x = Mathf.Clamp(targetPos.x, -CameraMaxMoveWidth, CameraMaxMoveWidth);
            targetPos.y = Mathf.Clamp(targetPos.y, CameraMinMoveHeight, CameraMaxMoveHeight);
            CameraTarget.transform.localPosition = targetPos;
        }

        void RotateCameraView(List<LeanFinger> fingers)
        {
            if (fingers.Count > 0) // refactor to have one reference for current fingers on screen
            {
                Vector3 fingerScreenPos = fingers[0].ScreenPosition;

                int leftBoundary = Screen.width / 100 * 20;
                int rightBoundary = Screen.width / 100 * 80;

                float rotationDirection = 0;
                if (fingerScreenPos.x < leftBoundary)
                    rotationDirection = 1;
                else if (fingerScreenPos.x > rightBoundary)
                    rotationDirection = -1;

                RotateCameraByAmount(CameraRotationSpeed * rotationDirection);
            }
        }

        void RotateCameraByAmount(float amount)
        {
            transform.Rotate(Vector3.forward * amount * Time.deltaTime);
        }
        #endregion

        #region ZoomFunctions
        /*

        void SetZoomLevel(float newZoomLevel)
        {
            var editedZoomLevel = Mathf.Clamp(newZoomLevel, 0.1f, 1f);
            Vector3 newCamPos = Vector3.zero;
            float lerpAmount = Mathf.Clamp01(CameraZoomMoveCurve.Evaluate(editedZoomLevel));
            newCamPos.y = Mathf.Lerp(0, PlanetRadius, lerpAmount);
            VCam.m_Lens.OrthographicSize = Mathf.Lerp(NearestZoom, FarthestZoom, 1 - editedZoomLevel);

            CameraHolder.transform.localPosition = newCamPos;
            CurrentZoomAmount = editedZoomLevel;
            animator.SetFloat("ZoomLevel", CurrentZoomAmount);

            if (CurrentZoomAmount < ZoomLevelSnapThreshold)
            {
                CentreCameraOnOrigin();
            }
        }*/
        #endregion

        #region SwipeCameraFunctions

        void SetSwipeTarget()
        {
            float swipeAmountInAngles = 0;

            SwipeForce = Mathf.Clamp(SwipeDistance, -MaxForce, MaxForce);
            swipeAmountInAngles = (SwipeForce / MaxForce) * 500;

            SwipeTargetAngle += swipeAmountInAngles;
        }

        void MoveTowardsSwipe()
        {
            var moveAmountPerFrame = Mathf.Lerp(0, SwipeTargetAngle, Time.deltaTime * SwipeCurrentDampening);

            var newRotationEuler = new Vector3(0, 0, moveAmountPerFrame);

            cameraBody.Rotate(newRotationEuler);

            SwipeTargetAngle -= moveAmountPerFrame;
        }


        #endregion

        #region AutomaticMoveFunctions
        public void StartCameraSnapZoomPosition()
        {
            StartCoroutine(SnapCameraToZoomPosition());
        }

        public void StartCameraMoveToPosition()
        {
            StartCoroutine(MoveCameraToSelectedPosition());
        }

        IEnumerator SnapCameraToZoomPosition()
        {
            CurrentCameraState = CameraState.MovingTo;
            float snapZoomCurrentLevel = CurrentZoomAmount;
            float zoomDir = (snapZoomCurrentLevel > ZoomLevelSnapThreshold) ? 1f : -1f;

            yield return new WaitUntil(() =>
            {
                snapZoomCurrentLevel += (Time.deltaTime * zoomDir) / 1.5f; // this will always take 1.5 seconds - how do i make it take 1.5 seconds for the entire distance
                //SetZoomLevel(snapZoomCurrentLevel);

                if (CurrentZoomAmount == 0.1f || CurrentZoomAmount == 1)
                    return true;
                else
                    return false;
            });

            SetCameraZoomState();
        }

        IEnumerator MoveCameraToSelectedPosition()
        {
            animator.SetBool("MoveTowards", true);

            CurrentCameraState = CameraState.MovingTo;
            float angleBetweenCameraAndPoint = 0;
            float CurrentRotation = transform.rotation.eulerAngles.z;
            float TargetRotation = 0;
            float RemainingLerp = 0;
            float moveCurveVal = 0f;
            var RemainingZoom = CurrentZoomAmount;
            Vector3 rotationEuler = Vector3.zero;

            angleBetweenCameraAndPoint = Vector2.SignedAngle(CameraTarget.transform.position, CameraHolder.transform.position);
            TargetRotation = -angleBetweenCameraAndPoint;
            RemainingLerp = 0;
            yield return new WaitUntil(() =>
            {
                RemainingLerp = Mathf.Clamp01(RemainingLerp + (Time.deltaTime / 2f));
                RemainingZoom = Mathf.Clamp01(RemainingZoom + (Time.deltaTime / 2f));

                moveCurveVal = CameraMoveToCurve.Evaluate(RemainingLerp) * TargetRotation;
                CurrentRotation += moveCurveVal * Time.deltaTime;
                rotationEuler = new Vector3(0, 0, CurrentRotation);
                Quaternion rot = transform.rotation;
                rot.eulerAngles = rotationEuler;
                transform.rotation = rot;

                //SetZoomLevel(RemainingZoom);

                if (RemainingLerp < 1)
                    return false;
                else
                    return true;
            });

            SetCameraZoomState();

            animator.SetBool("MoveTowards", false);
        }

        //Go To Event
        //FocusOnEvent
        #endregion

        void SetCameraZoomState()
        {
            if (CurrentZoomAmount > ZoomLevelSnapThreshold)
            {
                CurrentCameraState = CameraState.CityView;
            }
            else
            {
                CurrentCameraState = CameraState.PlanetView;
                CentreCameraOnOrigin();
            }
        }

        void CentreCameraOnOrigin()
        {
            CameraTarget.transform.localPosition = Vector3.zero;
        }


        public bool CheckForDoubleTap()
        {
            bool result = false;
            // Get fingers and calculate how many are still touching the screen
            var fingers = Fingers.GetFingers();
            var fingerCount = GetFingerCount(fingers);

            // At least one finger set?
            if (fingerCount > 0)
            {
                // Did this just begin?
                if (lastFingerCount == 0)
                {
                    age = 0.0f;
                    HighestFingerCount = fingerCount;
                }
                else if (fingerCount > HighestFingerCount)
                {
                    HighestFingerCount = fingerCount;
                }
            }

            age += Time.unscaledDeltaTime;

            // Is a multi-tap still eligible?
            if (age <= LeanTouch.CurrentTapThreshold)
            {
                // All fingers released?
                if (fingerCount == 0 && lastFingerCount > 0)
                {
                    MultiTapCount += 1;

                    if (MultiTapCount == 2)
                    {
                        result = true;
                        animator.SetTrigger("DoubleTap");
                    }
                }
            }
            // Reset
            else
            {
                MultiTapCount = 0;
                HighestFingerCount = 0;
                MultiTap = false;
            }

            lastFingerCount = fingerCount;
            return result;
        }

        private int GetFingerCount(List<LeanFinger> fingers)
        {
            var count = 0;

            for (var i = fingers.Count - 1; i >= 0; i--)
            {
                if (fingers[i].Up == false)
                {
                    count += 1;
                }
            }

            return count;
        }

    }

}
