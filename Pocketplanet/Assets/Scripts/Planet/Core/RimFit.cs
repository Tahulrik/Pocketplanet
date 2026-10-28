using UnityEngine;

namespace PocketPlanet.Planet
{
    // A flat-bottomed object resting on the round rim touches it only at its centre; its corners hang
    // above the ground by the sag. Sinking an object by the sag (the planet hides what's below the rim)
    // or filling under it with a foundation closes that gap.
    public static class RimFit
    {
        const float SeamMargin = 0.02f;

        public static float Sag(float width, float radius)
        {
            var halfWidth = Mathf.Min(width / 2f, radius);
            return radius - Mathf.Sqrt(radius * radius - halfWidth * halfWidth);
        }

        public static float SinkDepth(float width, float radius) => Sag(width, radius) + SeamMargin;
    }
}
