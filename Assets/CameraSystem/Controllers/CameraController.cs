using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean;
using Lean.Touch;
using Cinemachine;
using System.Linq;


namespace CameraSystem.StateMachine.States
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
        public GameObject TestEventObject;


        LeanFingerFilter TouchFilter = new LeanFingerFilter(true);
        LeanFingerFilter TapFilter = new LeanFingerFilter(true);
        LeanFingerFilter SwipeFilter = new LeanFingerFilter(true);
        LeanFingerFilter PinchFilter = new LeanFingerFilter(true);
        List<LeanFinger> ActiveFingers;

        public float PlanetRadius = 8;

        public float FarthestZoom;
        public float NearestZoom;
        public AnimationCurve CameraZoomMoveCurve;
        public AnimationCurve CameraMoveToCurve;
        public AnimationCurve CameraFocusOnEventCurve;
        CinemachineVirtualCamera VCam;

        GameObject CameraHolder;
        GameObject CameraTarget;
        Camera MainCam;
        Animator animator;

        public float PinchLevel = 1f;
        [Range(0.1f, 1)]
        public float CurrentZoomAmount = 0.1f;
        [Range(0.15f, 0.95f)]
        public float ZoomLevelSnapThreshold = 0.5f;
        public float CameraMinMoveHeight = 0f, CameraMaxMoveHeight = 1.2f, CameraMaxMoveWidth = 0.5f;

        [Range(10f, 50f)]
        public float CameraRotationSpeed = 25f;
        static float SwipeTargetAngle = 0;
        
        float SwipeCurrentDampening = 2.5f;
        public float SwipeMinimumDampening = 2.5f, SwipeMaximumDampening = 5f;
        [Range(0, 2f)]
        public float SwipeForceModifier = 1;

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

        [Tooltip("This is set to true the frame a multi-tap occurs.")]
        public bool MultiTap;

        /// <summary>This is set to the current multi-tap count.</summary>
        [Tooltip("This is set to the current multi-tap count.")]
        public int MultiTapCount;

        /// <summary>Highest number of fingers held down during this multi-tap.</summary>
        [Tooltip("Highest number of fingers held down during this multi-tap.")]
        public int HighestFingerCount;

        // Seconds at least one finger has been held down
        private float age;

        // Previous fingerCount
        private int lastFingerCount;
        public Vector2 lastFingerWorldPos;

        float cameraDistance;

        void OnEnable()
        {
            LeanTouch.OnFingerDown += CommandSetFinger;
            LeanTouch.OnFingerUp += CommandRemoveFinger;

            LeanTouch.OnFingerSwipe += CommandFingerSwipe;
            LeanTouch.OnFingerOld += CommandFingerHold;
        }

        void OnDisable()
        {
            LeanTouch.OnFingerDown -= CommandSetFinger;
            LeanTouch.OnFingerUp -= CommandRemoveFinger;

            LeanTouch.OnFingerSwipe -= CommandFingerSwipe;
            LeanTouch.OnFingerOld -= CommandFingerHold;
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

            TapFilter.RequiredFingerCount = 1;
            SwipeFilter.RequiredFingerCount = 1;
            PinchFilter.RequiredFingerCount = 2;

            SwipeCurrentDampening = SwipeMinimumDampening;
            SetManualAction(true);
        }

        // Update is called once per frame
        void Update()
        {
            ActiveFingers = TouchFilter.GetFingers();
            
            animator.SetInteger("ActiveFingers", ActiveFingers.Count);


            if (CheckForDoubleTap())
            {
                lastFingerWorldPos = TouchFilter.GetFingers()[0].GetWorldPosition(cameraDistance); //Should be part of checking for double tap
                animator.SetBool("IsMovingToPosition", true);
            }

            CheckForFingerPinch();
        }

        #region Commands
        //Find out what State the camera should go to
        void CommandSetFinger(LeanFinger finger)
        {
            if (!IsCurrentActionManual)
            {
                return;
            }
        }

        //Find out what the resulting 
        void CommandRemoveFinger(LeanFinger finger)
        {
            if (!IsCurrentActionManual)
            {
                return;
            }
        }

        void CommandFingerSwipe(LeanFinger finger)
        {
            if (CurrentZoomAmount < ZoomLevelSnapThreshold)
            {
                if (SwipeFilter.GetFingers().Count == 1)
                {
                    SetSwipeTarget(finger);
                }
            }
        }

        void CommandFingerHold(LeanFinger finger)
        {
            if (SwipeTargetAngle != 0)
                SwipeCurrentDampening = SwipeMaximumDampening; 
        }
    
        public void CommandZoomCamera()
        {
            var screenPoint = default(Vector2);

            if (LeanGesture.TryGetScreenCenter(ActiveFingers, ref screenPoint) == true)
            {
                var newZoomLevel = CurrentZoomAmount * PinchLevel;

                SetZoomLevel(newZoomLevel);

                if (CurrentZoomAmount < ZoomLevelSnapThreshold)
                {
                    CentreCameraOnOrigin();
                }

            }

        }

        public void CommandMoveCamera()
        {
            var fingers = TouchFilter.GetFingers(true);

            if (fingers.Count > 0)
            {
                //Move Cam in field
                MoveCameraInViewField(fingers);
            }
        }

        public void CommandRotateCamera()
        {
            var fingers = TouchFilter.GetFingers(true);

            if (fingers.Count > 0)
            {
                //Rotate planet if outside boundaries
                RotateCameraView(fingers);
            }
        }
        public void CommandSwipeCamera()
        {
            if (Mathf.Abs(SwipeTargetAngle) < 5f)
            {
                SwipeTargetAngle = 0;
                animator.SetBool("IsSwiping", false);
            }

            FindNextSwipePosition();
        }

        public void CommandMoveTo()
        {
            var target = lastFingerWorldPos;
            StartCoroutine(MoveCameraToSelectedPosition(target));
        }

        public void CommandMoveToEvent()
        {
            StartCoroutine(MoveCameraToEvent(EventPos));
        }
        #endregion


        #region ZoomFunctions
        void SetZoomLevel(float newZoomLevel)
        {
            var editedZoomLevel = Mathf.Clamp(newZoomLevel, 0.1f, 1f);
            Vector3 newCamPos = Vector3.zero;
            float lerpAmount = Mathf.Clamp01(CameraZoomMoveCurve.Evaluate(editedZoomLevel));
            newCamPos.y = Mathf.Lerp(0, PlanetRadius, lerpAmount);
            VCam.m_Lens.OrthographicSize = Mathf.Lerp(NearestZoom, FarthestZoom, 1 - editedZoomLevel);

            CameraHolder.transform.localPosition = newCamPos;
            CurrentZoomAmount = editedZoomLevel;

            if (CurrentZoomAmount < ZoomLevelSnapThreshold)
            {
                CentreCameraOnOrigin();
            }

            animator.SetFloat("ZoomLevel", CurrentZoomAmount);
        }
        #endregion

        #region MoveFunctions

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

        #region SwipeCameraFunctions
        public void SetCurrentSwipeDampening(float newDampening)
        {
            SwipeCurrentDampening = newDampening;
        }
        void SetSwipeTarget(LeanFinger finger)
        {
            float maxForce = 350;

            float swipeAmountInAngles = 0;

            float swipeForce = Mathf.Clamp(finger.SwipeScreenDelta.x, -maxForce, maxForce);
            swipeAmountInAngles = (swipeForce / maxForce) * (1000 * SwipeForceModifier);

            SwipeTargetAngle += swipeAmountInAngles;

            animator.SetBool("IsSwiping", true);
        }

        void FindNextSwipePosition()
        {
            var moveAmountPerFrame = SwipeTargetAngle * Time.deltaTime;

            var newRotationEuler = new Vector3(0, 0, moveAmountPerFrame);

            transform.Rotate(newRotationEuler);

            SwipeTargetAngle -= SwipeTargetAngle * (Time.deltaTime * SwipeCurrentDampening);
        }
        #endregion

        #region AutomaticMoveFunctions
        IEnumerator SnapCameraToZoomPosition()
        {
            CurrentCameraState = CameraState.MovingTo;
            float snapZoomCurrentLevel = CurrentZoomAmount;
            float zoomDir = (snapZoomCurrentLevel > ZoomLevelSnapThreshold) ? 1f : -1f;

            yield return new WaitUntil(() =>
            {
                snapZoomCurrentLevel += (Time.deltaTime * zoomDir) / 1.5f; // this will always take 1.5 seconds - how do i make it take 1.5 seconds for the entire distance
                SetZoomLevel(snapZoomCurrentLevel);
                
                if (CurrentZoomAmount == 0.1f || CurrentZoomAmount == 0.9f)
                    return true;
                else
                    return false;
            });

            SetCameraZoomState();
        }

        IEnumerator MoveCameraToSelectedPosition(Vector2 targetPosition)
        {
            CurrentCameraState = CameraState.MovingTo;
            float angleBetweenCameraAndPoint = 0;
            float CurrentRotation = transform.rotation.eulerAngles.z;
            float TargetRotation = 0;
            float RemainingLerp = 0;
            float moveCurveVal = 0f;
            var RemainingZoom = CurrentZoomAmount;
            Vector3 rotationEuler = Vector3.zero;
            
            angleBetweenCameraAndPoint = Vector2.SignedAngle(targetPosition, CameraHolder.transform.position);
            TargetRotation = -angleBetweenCameraAndPoint;
            RemainingLerp = 0;

            //Debug.DrawLine(Vector3.zero, targetPosition, Color.magenta, 10);
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
                SetZoomLevel(RemainingZoom);

                if (RemainingLerp < 1)
                    return false;
                else
                    return true;
            });
            animator.SetBool("IsMovingToPosition", false);
            SetCameraZoomState();
        }

        IEnumerator MoveCameraToEvent(Vector2 targetPosition)
        {
            CurrentCameraState = CameraState.MovingTo;
            float angleBetweenCameraAndPoint = 0;
            float CurrentRotation = transform.rotation.eulerAngles.z;
            float TargetRotation = 0;
            float RemainingMove = 0;
            float moveCurveVal = 0f;
            var RemainingZoom = CurrentZoomAmount;
            Vector3 rotationEuler = Vector3.zero;
            var CurrentPos = new Vector2(CameraTarget.transform.position.x, 1);
            Debug.DrawLine(Vector3.zero, CurrentPos, Color.blue, 10);
            angleBetweenCameraAndPoint = Vector2.SignedAngle(targetPosition, CurrentPos);
            TargetRotation = (-angleBetweenCameraAndPoint);
            float maxAngle = 180;
            float maxZoomAmount = 0f;
            float timeToReachPosition = 2f;

            if(CurrentZoomAmount > ZoomLevelSnapThreshold)
            { 
                maxZoomAmount = Mathf.Clamp01(1 - (Mathf.Abs(TargetRotation/2f) / maxAngle)*0.25f);
            }

            timeToReachPosition = 4;

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
                RemainingZoom = CameraFocusOnEventCurve.Evaluate(lerpAmount);

                if (RemainingMove < 1)
                {
                    moveCurveVal = CameraMoveToCurve.Evaluate(RemainingMove) * TargetRotation;
                    CurrentRotation += moveCurveVal * Time.deltaTime;
                    rotationEuler = new Vector3(0, 0, CurrentRotation);
                    Quaternion rot = transform.rotation;
                    rot.eulerAngles = rotationEuler;
                    transform.rotation = rot;
                }
                if (lerpAmount < 1)
                { 
                    SetZoomLevel(RemainingZoom);
                }

                if (RemainingMove == 1 && RemainingZoom == 1)
                    return true;
                else
                    return false;
            });
            animator.SetBool("IsMovingToPosition", false);
            SetCameraZoomState();
        }
        //Go To Event
        //FocusOnEvent
        #endregion

        public void ResetCamera()
        {
            SetCameraZoomState();
            if (IsCameraReset)
                return;
            
            StartCoroutine(SnapCameraToZoomPosition());
        }

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

        bool CheckForDoubleTap()
        {
            bool result = false;
            // Get fingers and calculate how many are still touching the screen
            var fingers = TouchFilter.GetFingers();
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
        bool CheckForFingerPinch()
        {
            var fingers = PinchFilter.GetFingers();
            if (fingers.Count < 2)
            {
                animator.SetBool("IsPinching", false);
                return false;
            }

            animator.SetBool("IsPinching", true);
            PinchLevel = LeanGesture.GetPinchScale(ActiveFingers);
            return true;
        }
        public void SetManualAction(bool newValue)
        {
            IsCurrentActionManual = newValue;
            animator.SetBool("IsCurrentActionManual", newValue);
        }
        Vector2 EventPos;
        public void TestMethod_SpawnRandomEvent()
        {
            EventPos = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
           /* var obj = Instantiate(TestEventObject);
            obj.transform.position = EventPos.normalized * PlanetRadius;
            Quaternion quat = Quaternion.identity;
            quat.eulerAngles = EventPos.normalized * PlanetRadius;
            obj.transform.rotation = quat;
            Destroy(obj, 10);
            Debug.DrawLine(Vector3.zero, EventPos.normalized*PlanetRadius, Color.red, 10);*/
            //EventPos = new Vector2(2, 2);
            var randomVal = Random.Range(1, 10);
            if (randomVal > 5)
            { 
               // animator.SetTrigger("GoToPreviousPosition");
            }
            animator.SetBool("IsMovingToPosition", true);
            animator.SetTrigger("EventHappened");
        }




    }
}