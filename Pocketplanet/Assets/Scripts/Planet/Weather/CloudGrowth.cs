using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class CloudGrowth
    {
        const float PuffShare = 0.55f;
        const float Overshoot = 1.70158f;

        // Puffs grow in one after another, then shrink away in reverse order.
        // Each puff animates over PuffShare of the grow time; the rest is spread out as stagger.
        public static float PuffProgress(float age, float lifetime, float growSeconds, int index, int count)
        {
            if (age <= 0f || age >= lifetime)
                return 0f;
            var grow = Mathf.Min(growSeconds, lifetime / 2f);
            if (grow <= 0f)
                return 1f;
            var growingIn = Staggered(age, grow, index, count);
            var shrinkingOut = Staggered(lifetime - age, grow, index, count);
            return Mathf.Min(growingIn, shrinkingOut);
        }

        public static float PopScale(float progress)
        {
            var t = Mathf.Clamp01(progress) - 1f;
            return 1f + (Overshoot + 1f) * t * t * t + Overshoot * t * t;
        }

        static float Staggered(float elapsed, float duration, int slot, int count)
        {
            if (count <= 1)
                return Mathf.Clamp01(elapsed / duration);
            var puffDuration = duration * PuffShare;
            var delay = (duration - puffDuration) * slot / (count - 1);
            return Mathf.Clamp01((elapsed - delay) / puffDuration);
        }
    }
}
