using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Lean.Touch;
using PocketPlanet.Planet;
using UnityEngine;
using Object = UnityEngine.Object;

// Replays two-finger gestures through LeanTouch's own update steps, frame by frame.
public static class PinchDriver
{
    const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
    const int Frames = 40;

    public static Task<string> Pinch() => Replay((c, t) => (c + new Vector2(Mathf.Lerp(60f, 400f, t), 0f), c - new Vector2(Mathf.Lerp(60f, 400f, t), 0f)));

    public static Task<string> SwipeUp() => Replay((c, t) => (c + new Vector2(0f, 600f * t), c + new Vector2(0f, 600f * t)));

    public static Task<string> SwipeDown() => Replay((c, t) => (c - new Vector2(0f, 600f * t), c - new Vector2(0f, 600f * t)));

    static async Task<string> Replay(Func<Vector2, float, (Vector2, Vector2)> path)
    {
        if (!Application.isPlaying || LeanTouch.Instances.Count == 0)
            return "skipped: not playing";
        var touch = LeanTouch.Instances[0];
        var cam = Object.FindAnyObjectByType<CameraController>();
        var rows = Object.FindAnyObjectByType<BuildRowSelection>();
        touch.enabled = false;
        LeanTouch.Instances.Add(touch);
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint w, out uint h);
        var center = new Vector2(w, h) * 0.5f;
        var log = new StringBuilder($"before: {Describe(cam, rows)}\n");
        try
        {
            for (int f = 0; f <= Frames + 1; f++)
            {
                var (a, b) = path(center, Mathf.Min(f, Frames) / (float)Frames);
                Step(touch, a, b, f <= Frames, true);
                await NextFrame();
            }
            for (int i = 0; i < 90; i++) { Step(touch, center, center, false, false); await NextFrame(); }
            log.AppendLine($"after: {Describe(cam, rows)}");
        }
        finally
        {
            LeanTouch.Instances.Remove(touch);
            touch.enabled = true;
        }
        return log.ToString();
    }

    static string Describe(CameraController cam, BuildRowSelection rows) =>
        $"state={cam.CurrentCameraState} zoom={cam.CurrentZoomAmount:0.000} focusHeight={cam.CameraHolder.transform.localPosition.y:0.00} radius={cam.PlanetRadius:0.00} row={BuildRowSelection.ActiveRow}";

    static void Step(LeanTouch touch, Vector2 a, Vector2 b, bool set, bool addFingers)
    {
        Invoke(touch, "BeginFingers");
        if (addFingers)
        {
            AddFinger(touch, -1, a, set);
            AddFinger(touch, -2, b, set);
        }
        Invoke(touch, "EndFingers");
        Invoke(touch, "UpdateEvents");
    }

    static void Invoke(LeanTouch touch, string method) => typeof(LeanTouch).GetMethod(method, Flags).Invoke(touch, null);

    static void AddFinger(LeanTouch touch, int index, Vector2 pos, bool set) =>
        typeof(LeanTouch).GetMethod("AddFinger", Flags).Invoke(touch, new object[] { index, pos, 1f, set });

    static async Task NextFrame()
    {
        var frame = Time.frameCount;
        while (Time.frameCount == frame)
            await Task.Yield();
    }
}

public static class SwipeCheck
{
    public static async Task<string> Run()
    {
        var log = new StringBuilder();
        void OnChange(SurfaceRow row) => log.Append($"[{UnityEngine.Time.frameCount}:{row}] ");
        BuildRowSelection.ActiveRowChanged += OnChange;
        try
        {
            foreach (var gesture in new[] { "SwipeUp", "SwipeUp", "SwipeDown", "SwipeDown", "Pinch" })
            {
                log.Append($"\n{gesture} start@{UnityEngine.Time.frameCount} row={BuildRowSelection.ActiveRow}: ");
                await (gesture == "SwipeUp" ? PinchDriver.SwipeUp() : gesture == "SwipeDown" ? PinchDriver.SwipeDown() : PinchDriver.Pinch());
            }
            log.Append($"\nend row={BuildRowSelection.ActiveRow} swiping={BuildRowSwipeInput.IsSwiping}");
        }
        finally
        {
            BuildRowSelection.ActiveRowChanged -= OnChange;
        }
        return log.ToString();
    }
}
