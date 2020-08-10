using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public LeanFingerFilter TapFilter = new LeanFingerFilter(true);
    public LeanFingerFilter PinchFilter = new LeanFingerFilter(true);
    public LeanFingerFilter SwipeFilter = new LeanFingerFilter(true);


    [Tooltip("This is set to true the frame a multi-tap occurs.")]
    public bool MultiTap;

    /// <summary>This is set to the current multi-tap count.</summary>
    [Tooltip("This is set to the current multi-tap count.")]
    public int MultiTapCount;

    /// <summary>Highest number of fingers held down during this multi-tap.</summary>
    [Tooltip("Highest number of fingers held down during this multi-tap.")]
    public int HighestFingerCount;

    private int lastFingerCount;
    private Vector2 lastFingerWorldPos;

    // Seconds at least one finger has been held down
    private float age;

    public static InputController instance;

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
    }

    // Update is called once per frame
    void Update()
    {
        CheckForDoubleTap();
    }

    void CommandSetFinger(LeanFinger finger)
    {
        var activePinchFingers = PinchFilter.GetFingers();
        if (activePinchFingers.Count == 2)
        {
            if (activePinchFingers[0] == finger)
            {

            }
            LeanGesture.GetPinchScale(PinchFilter.GetFingers());
        }
    }

    void CommandRemoveFinger(LeanFinger finger)
    {

    }

    void CommandFingerSwipe(LeanFinger finger)
    {
       //print(SwipeFilter.GetFingers()[0].ToString());
    }

    bool CheckForDoubleTap()
    {
        bool result = false;
        // Get fingers and calculate how many are still touching the screen
        var fingers = TapFilter.GetFingers();
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
}
