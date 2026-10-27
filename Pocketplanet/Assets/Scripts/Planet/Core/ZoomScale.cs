using UnityEngine;

namespace PocketPlanet.Planet
{
    // Maps a zoom level (MinLevel = whole planet, 1 = closest) to an orthographic size.
    // The mapping is exponential so each pinch or wheel step feels the same at every distance.
    public readonly struct ZoomScale
    {
        public ZoomScale(float nearest, float farthest, float minLevel = 0.1f)
        {
            Nearest = nearest;
            Farthest = farthest;
            MinLevel = minLevel;
        }

        public float Nearest { get; }
        public float Farthest { get; }
        public float MinLevel { get; }

        public float ClampLevel(float level) => Mathf.Clamp(level, MinLevel, 1f);

        public float OrthoAt(float level)
        {
            var t = (ClampLevel(level) - MinLevel) / (1f - MinLevel);
            return Farthest * Mathf.Pow(Nearest / Farthest, t);
        }

        public float LevelAt(float ortho)
        {
            var clamped = Mathf.Clamp(ortho, Nearest, Farthest);
            var t = Mathf.Log(clamped / Farthest) / Mathf.Log(Nearest / Farthest);
            return MinLevel + (1f - MinLevel) * Mathf.Clamp01(t);
        }

        public float LevelAfterScaling(float level, float orthoFactor) => LevelAt(OrthoAt(level) * orthoFactor);
    }
}
