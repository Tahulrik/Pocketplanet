using System.Collections.Generic;

namespace PocketPlanet.Planet
{
    public class PlacementValidator
    {
        static readonly SurfaceRow[] FrontOnly = { SurfaceRow.Front };
        static readonly SurfaceRow[] BackOnly = { SurfaceRow.Back };
        static readonly SurfaceRow[] FrontThenBack = { SurfaceRow.Front, SurfaceRow.Back };

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

        public PlacementResult Check(Footprint footprint, float arc)
        {
            var range = RangeFor(footprint, arc);
            var rows = RowArrayFor(footprint.Row);

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

        public PlacementResult? FindNearestValid(Footprint footprint, float arc, float maxNudge)
        {
            var steps = (int)(maxNudge / Profile.Spacing);
            for (int step = 0; step <= steps; step++)
            {
                var forward = Check(footprint, arc + step * Profile.Spacing);
                if (forward.IsValid)
                    return forward;
                var backward = Check(footprint, arc - step * Profile.Spacing);
                if (backward.IsValid)
                    return backward;
            }
            return null;
        }

        public PlacementResult? FindAnywhere(Footprint footprint, float startArc) =>
            FindNearestValid(footprint, startArc, Profile.Circumference / 2f);

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

        public static IReadOnlyList<SurfaceRow> RowsFor(RowPreference preference) => RowArrayFor(preference);

        static SurfaceRow[] RowArrayFor(RowPreference preference)
        {
            switch (preference)
            {
                case RowPreference.Back: return BackOnly;
                case RowPreference.Either: return FrontThenBack;
                default: return FrontOnly;
            }
        }
    }
}
