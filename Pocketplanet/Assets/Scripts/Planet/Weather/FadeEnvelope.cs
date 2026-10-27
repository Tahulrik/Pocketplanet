using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class FadeEnvelope
    {
        public static float Alpha(float age, float lifetime, float fadeSeconds)
        {
            if (age <= 0f || age >= lifetime)
                return 0f;
            var fade = Mathf.Min(fadeSeconds, lifetime / 2f);
            if (fade <= 0f)
                return 1f;
            return Mathf.Clamp01(Mathf.Min(age, lifetime - age) / fade);
        }
    }
}
