using InteractionSystem.CameraSystem;
using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InteractionSystem
{
    public class InputController : BaseGameController
    {
        public delegate void SwipeEvent(float swipeForce);
        public static event SwipeEvent Swiped;
        public static event SwipeEvent Hold;

        public delegate void PinchEvent();
        public static event PinchEvent PinchStarted;
        public static event PinchEvent PinchEnded;

        public LeanFingerFilter TapFilter = new LeanFingerFilter(true);
        public LeanFingerFilter PinchFilter = new LeanFingerFilter(true);
        public LeanFingerFilter SwipeFilter = new LeanFingerFilter(true);

        [HideInInspector]
        public List<LeanFinger> ActiveFingers = new List<LeanFinger>();
        public int fingerCount = 0;

        [Tooltip("This is set to true the frame a multi-tap occurs.")]
        public bool MultiTap;

        /// <summary>This is set to the current multi-tap count.</summary>
        [Tooltip("This is set to the current multi-tap count.")]
        public int MultiTapCount;

        /// <summary>Highest number of fingers held down during this multi-tap.</summary>
        [Tooltip("Highest number of fingers held down during this multi-tap.")]
        public int HighestFingerCount;

        internal float pinchLevel;
        private int lastFingerCount;


        // Seconds since a tap was last made
        public float timeSinceLastTap;
        // Seconds since the finger started on the screen
        public float timeFingerHeld;

        public static InputController instance;

        public bool CanSwipe
        {
            get {
                if (CameraController.instance.CurrentViewState == ViewState.PlanetView)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CanPinch
        {
            get
            {
                if (CameraController.instance.CurrentCameraState != CameraState.Swiping && CameraController.instance.CurrentViewState != ViewState.EventView)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CanTap
        {
            get
            {
                if (CameraController.instance.CurrentCameraState != CameraState.Swiping && CameraController.instance.CurrentViewState != ViewState.EventView)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CanHold
        {
            get
            {
                if (CameraController.instance.CurrentCameraState == CameraState.Swiping && CameraController.instance.CurrentViewState == ViewState.PlanetView)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        void OnEnable()
        {
            LeanTouch.OnFingerDown += CommandSetFinger;
            LeanTouch.OnFingerUp += CommandRemoveFinger;

            LeanTouch.OnFingerSwipe += CommandFingerSwipe;
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
            instance = this;

            TapFilter.RequiredFingerCount = 1;
            SwipeFilter.RequiredFingerCount = 1;
            PinchFilter.RequiredFingerCount = 2;
        }

        // Update is called once per frame
        void Update()
        {
            ActiveFingers = LeanTouch.GetFingers(true, false);
            fingerCount = ActiveFingers.Count;

            timeSinceLastTap += Time.unscaledDeltaTime;

            // Must always check if we should reset multitap
            if (timeSinceLastTap >= LeanTouch.CurrentTapThreshold)
            {
                ResetMultiTap();
            }

            if(CanTap)
                CheckForDoubleTap();
            
            if(CanPinch)
                CheckForPinchInput();

            if(CanHold)
                CheckForHoldInput();
        }

        void CommandSetFinger(LeanFinger finger)
        {

        }

        void CommandRemoveFinger(LeanFinger finger)
        {

        }

        void CommandFingerSwipe(LeanFinger finger)
        {
            if (CanSwipe)
            {
                var swipeFingers = SwipeFilter.GetFingers();
                if (swipeFingers.Count > 0)
                { 
                    float swipeDelta = SwipeFilter.GetFingers().First().SwipeScaledDelta.x;
                    Log($"Swiped with {swipeDelta} amount of force");
                    Swiped?.Invoke(swipeDelta);
                    MultiTapCount = 0; //So swipe doesnt count towards multitap
                }
            }
        }

        public LeanFinger GetTouchingFinger()
        {
            if (ActiveFingers.Count > 0)
            {
                return ActiveFingers.First();
            }

            return null;
        }
        private bool CheckForPinchInput()
        {
            var fingers = PinchFilter.GetFingers();
            if (fingers.Count < 2)
            {
                PinchEnded?.Invoke();

                return false;
            }
            if (fingers.Count == 2 && fingers[0].Down)
            {
                PinchStarted?.Invoke();
            }


            pinchLevel = LeanGesture.GetPinchScale(fingers);
            return true;
        }

        private void CheckForDoubleTap()
        {
            // Get fingers and calculate how many are still touching the screen
            var fingers = TapFilter.GetFingers();
            var fingerCount = GetFingerCount(fingers);

            // At least one finger set?
            if (fingerCount > 0)
            {
                // Did this just begin?
                if (lastFingerCount == 0)
                {
                    timeSinceLastTap = 0.0f;
                    HighestFingerCount = fingerCount;
                }
                else if (fingerCount > HighestFingerCount)
                {
                    HighestFingerCount = fingerCount;
                }
            }

            // Is a multi-tap still eligible?
            if (timeSinceLastTap <= LeanTouch.CurrentTapThreshold)
            {
                // All fingers released?
                if (fingerCount == 0 && lastFingerCount > 0)
                {
                    MultiTapCount += 1;

                    if (MultiTapCount == 2)
                    {
                        MultiTap = true;
                    }

                }
            }

            lastFingerCount = fingerCount;

        }

        private void CheckForHoldInput()
        {
            var tappedFingers = TapFilter.GetFingers(true);
            var holdThreshold = 0.4f;
            if (tappedFingers.Count > 0 && timeFingerHeld < holdThreshold)
            {
                timeFingerHeld += Time.deltaTime;
                if (timeFingerHeld > 0.4f)
                {
                    Hold?.Invoke(0); //No input necessary
                }
            }
            else
            {
                timeFingerHeld = 0;
            }
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

        private void ResetMultiTap()
        {
            MultiTapCount = 0;
            HighestFingerCount = 0;
            MultiTap = false;
        }
    }

}
