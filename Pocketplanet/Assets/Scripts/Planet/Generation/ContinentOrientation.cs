using System.Collections.Generic;
using System.Linq;

namespace PocketPlanet.Planet
{
    public static class ContinentOrientation
    {
        // Arc 0 points along world +X and arcs run counter-clockwise, so world up is a quarter turn.
        public static float UpArc(float circumference) => circumference / 4f;

        public static IReadOnlyList<ArcRange> CenterLargestOn(IReadOnlyList<ArcRange> arcs, float targetArc)
        {
            if (arcs.Count == 0)
                return arcs;
            var largest = arcs.Aggregate((best, arc) => arc.Length > best.Length ? arc : best);
            var shift = targetArc - largest.Center;
            return arcs
                .Select(arc => new ArcRange(arc.Start + shift, arc.Length, arc.Circumference))
                .OrderBy(arc => arc.Start)
                .ToList();
        }
    }
}
