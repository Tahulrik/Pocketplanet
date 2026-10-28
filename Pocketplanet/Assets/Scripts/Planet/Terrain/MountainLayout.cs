using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class MountainLayout
    {
        // Matches the flat top of the mountain sprites.
        public const float PeakShare = 0.3f;
        const int AttemptsPerMountain = 12;

        public static List<MountainFeature> Generate(SurfaceProfile profile, PlanetSettings settings, int seed)
        {
            var rng = new System.Random(seed);
            var mountains = new List<MountainFeature>();
            var target = Mathf.RoundToInt(LandLength(profile) * settings.MountainsPerLandUnit);

            for (int attempt = 0; attempt < target * AttemptsPerMountain && mountains.Count < target; attempt++)
            {
                var width = Range(rng, settings.MountainWidth);
                var range = ArcRange.Centered((float)rng.NextDouble() * profile.Circumference, width, profile.Circumference);
                var row = SurfaceRows.FrontToBack[rng.Next(SurfaceRows.FrontToBack.Count)];
                if (!profile.IsAll(range, s => s.Terrain == TerrainKind.Land) || OverlapsInRow(mountains, range, row))
                    continue;
                mountains.Add(new MountainFeature(mountains.Count, range, row, Range(rng, settings.MountainHeight), PeakShare, rng.Next(3)));
            }
            return mountains;
        }

        static bool OverlapsInRow(List<MountainFeature> mountains, ArcRange range, SurfaceRow row)
        {
            foreach (var mountain in mountains)
                if (mountain.Row == row && mountain.Range.Overlaps(range))
                    return true;
            return false;
        }

        static float LandLength(SurfaceProfile profile)
        {
            var total = 0f;
            foreach (var segment in profile.Segments)
                if (segment.Terrain == TerrainKind.Land)
                    total += segment.Range.Length;
            return total;
        }

        static float Range(System.Random rng, Vector2 range) => Mathf.Lerp(range.x, range.y, (float)rng.NextDouble());
    }
}
