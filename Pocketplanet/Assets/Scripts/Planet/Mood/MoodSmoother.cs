using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class MoodSmoother
    {
        public static float Step(float current, float target, float deltaTime, float unitsPerSecond) =>
            Mathf.Clamp(Mathf.MoveTowards(current, target, deltaTime * unitsPerSecond), -1f, 1f);
    }
}
