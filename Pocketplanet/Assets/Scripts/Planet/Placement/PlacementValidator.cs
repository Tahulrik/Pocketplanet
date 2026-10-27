using System.Collections.Generic;
using System.Linq;

namespace PocketPlanet.Planet
{
    public class PlacementValidator
    {
        static readonly SurfaceRow[] FrontOnly = { SurfaceRow.Front };
        static readonly SurfaceRow[] MiddleOnly = { SurfaceRow.Middle };
        static readonly SurfaceRow[] BackOnly = { SurfaceRow.Back };
        static readonly SurfaceRow[] AnyRow = SurfaceRows.FrontToBack.ToArray();

        readonly SurfaceLayer surface;
        readonly BiomeTable biomes;

        public PlacementValidator(SurfaceLayer surface, BiomeTable biomes)
        {
            this.surface = surface;
            this.biomes = biomes;
        }

        SurfaceProfile Profile => surface.Profile;

        public ArcRange RangeFor(Footprint footprint, float arc) =>
            ArcRange.Centered(arc, footprint.Width, Profile.Circumference);

        public PlacementResult Check(Footprint footprint, float arc) =>
            Check(footprint, arc, RowArrayFor(footprint.Row));

        public PlacementResult Check(Footprint footprint, float arc, SurfaceRow row)
        {
            if (!Allows(footprint.Row, row))
                return new PlacementResult(PlacementReason.RowNotAllowed, row, RangeFor(footprint, arc));
            return Check(footprint, arc, new[] { row });
        }

        public PlacementResult? FindNearestValid(Footprint footprint, float arc, float maxNudge, SurfaceRow? row = null)
        {
            var steps = (int)(maxNudge / Profile.Spacing);
            for (int step = 0; step <= steps; step++)
            {
                var forward = CheckIn(footprint, arc + step * Profile.Spacing, row);
                if (forward.IsValid)
                    return forward;
                var backward = CheckIn(footprint, arc - step * Profile.Spacing, row);
                if (backward.IsValid)
                    return backward;
            }
            return null;
        }

        public PlacementResult? FindAnywhere(Footprint footprint, float startArc) =>
            FindNearestValid(footprint, startArc, Profile.Circumference / 2f);

        public static IReadOnlyList<SurfaceRow> RowsFor(RowPreference preference) => RowArrayFor(preference);

        public static bool Allows(RowPreference preference, SurfaceRow row) => RowArrayFor(preference).Contains(row);

        PlacementResult CheckIn(Footprint footprint, float arc, SurfaceRow? row) =>
            row.HasValue ? Check(footprint, arc, row.Value) : Check(footprint, arc);

        PlacementResult Check(Footprint footprint, float arc, IReadOnlyList<SurfaceRow> rows)
        {
            var range = RangeFor(footprint, arc);

            var reason = footprint.Rule.Evaluate(range, Profile, biomes);
            if (reason == PlacementReason.None)
                reason = CheckBiomeTags(footprint, range);
            if (reason != PlacementReason.None)
                return new PlacementResult(reason, rows[0], range);

            foreach (var row in rows)
                if (surface.Row(row).IsFree(range))
                    return new PlacementResult(PlacementReason.None, row, range);
            return new PlacementResult(PlacementReason.Occupied, rows[0], range);
        }

        PlacementReason CheckBiomeTags(Footprint footprint, ArcRange range)
        {
            foreach (var sample in Profile.SamplesIn(range))
            {
                if (sample.Terrain != TerrainKind.Land)
                    continue;
                var biome = biomes[sample.Biome];
                foreach (var tag in footprint.RequiredTags)
                    if (!biome.HasTag(tag))
                        return PlacementReason.MissingBiomeTag;
                foreach (var tag in footprint.ForbiddenTags)
                    if (biome.HasTag(tag))
                        return PlacementReason.ForbiddenBiomeTag;
            }
            return PlacementReason.None;
        }

        static SurfaceRow[] RowArrayFor(RowPreference preference)
        {
            switch (preference)
            {
                case RowPreference.Back: return BackOnly;
                case RowPreference.Middle: return MiddleOnly;
                case RowPreference.Any: return AnyRow;
                default: return FrontOnly;
            }
        }
    }
}
