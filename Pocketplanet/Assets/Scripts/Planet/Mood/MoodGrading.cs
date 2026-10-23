using UnityEngine;

namespace PocketPlanet.Planet
{
    public readonly struct MoodGrading
    {
        const float DoomSaturation = -40f;
        const float HallowedSaturation = 35f;
        static readonly Color DoomFilter = new Color(1f, 0.72f, 0.6f);
        static readonly Color HallowedFilter = new Color(1f, 0.97f, 0.88f);

        public MoodGrading(float saturation, Color filter)
        {
            Saturation = saturation;
            Filter = filter;
        }

        public float Saturation { get; }
        public Color Filter { get; }

        public static MoodGrading Evaluate(float mood)
        {
            mood = Mathf.Clamp(mood, -1f, 1f);
            return mood < 0f
                ? new MoodGrading(DoomSaturation * -mood, Color.Lerp(Color.white, DoomFilter, -mood))
                : new MoodGrading(HallowedSaturation * mood, Color.Lerp(Color.white, HallowedFilter, mood));
        }
    }
}
