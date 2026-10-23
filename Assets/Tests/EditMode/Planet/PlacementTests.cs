using System.Linq;
using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class PlacementTests
    {
        static readonly IPlacementRule LandOnly = new LandOnlyRule();

        static float CenterOfLongest(IPlanet planet, TerrainKind terrain) =>
            planet.Surface.Profile.Segments.Where(s => s.Terrain == terrain).OrderByDescending(s => s.Range.Length).First().Range.Center;

        static float CoastBoundary(IPlanet planet)
        {
            var profile = planet.Surface.Profile;
            for (int i = 0; i < profile.SampleCount; i++)
            {
                var previous = profile.SampleAtIndex((i - 1 + profile.SampleCount) % profile.SampleCount);
                if (profile.SampleAtIndex(i).Terrain == TerrainKind.Land && previous.Terrain == TerrainKind.Water)
                    return i * profile.Spacing;
            }
            Assert.Fail("No coast found.");
            return 0f;
        }

        static (PlacementValidator validator, SurfaceLayer surface) HandBuilt(params TerrainKind[] terrains)
        {
            var biomes = new BiomeTable(DefaultBiomes.Create());
            var samples = terrains.Select(t => new SurfaceSample(t, t == TerrainKind.Land ? (byte)0 : biomes.ShallowWater, false)).ToArray();
            var surface = new SurfaceLayer(new SurfaceProfile(samples, 1f));
            return (new PlacementValidator(surface, biomes), surface);
        }

        static TerrainKind[] Repeat(TerrainKind terrain, int count) => Enumerable.Repeat(terrain, count).ToArray();

        [Test]
        public void LandOnly_OnLand_Valid_FrontRow()
        {
            var planet = TestPlanets.Generate(3);
            var result = planet.Placement.Check(new Footprint(1f, RowPreference.Front, LandOnly), CenterOfLongest(planet, TerrainKind.Land));
            Assert.IsTrue(result.IsValid, result.Reason.ToString());
            Assert.AreEqual(SurfaceRow.Front, result.Row);
        }

        [Test]
        public void LandOnly_OnWater_ReasonWater()
        {
            var planet = TestPlanets.Generate(3);
            var result = planet.Placement.Check(new Footprint(1f, RowPreference.Front, LandOnly), CenterOfLongest(planet, TerrainKind.Water));
            Assert.AreEqual(PlacementReason.Water, result.Reason);
        }

        [Test]
        public void LandOnly_UnbuildableBiome_ReasonBiomeNotBuildable()
        {
            var settings = TestPlanets.Settings(3);
            foreach (var biome in settings.BiomeTable.Where(b => b.Kind == BiomeKind.Land))
                biome.Buildable = false;
            var planet = PlanetGenerator.Generate(settings);
            var result = planet.Placement.Check(new Footprint(1f, RowPreference.Front, LandOnly), CenterOfLongest(planet, TerrainKind.Land));
            Assert.AreEqual(PlacementReason.BiomeNotBuildable, result.Reason);
        }

        [Test]
        public void RequiredTag_Missing_ReasonMissingBiomeTag()
        {
            var planet = TestPlanets.Generate(3);
            var footprint = new Footprint(1f, RowPreference.Front, LandOnly, requiredTags: new[] { "Volcanic" });
            Assert.AreEqual(PlacementReason.MissingBiomeTag, planet.Placement.Check(footprint, CenterOfLongest(planet, TerrainKind.Land)).Reason);
        }

        [Test]
        public void ForbiddenTag_Present_ReasonForbiddenBiomeTag()
        {
            var planet = TestPlanets.Generate(3);
            var arc = CenterOfLongest(planet, TerrainKind.Land);
            var tag = planet.Biomes[planet.Surface.Profile.BiomeAt(arc)].Tags[0];
            var footprint = new Footprint(0.1f, RowPreference.Front, LandOnly, forbiddenTags: new[] { tag });
            Assert.AreEqual(PlacementReason.ForbiddenBiomeTag, planet.Placement.Check(footprint, arc).Reason);
        }

        [Test]
        public void Coastal_AtCoast_Valid()
        {
            var planet = TestPlanets.Generate(3);
            var result = planet.Placement.Check(new Footprint(2f, RowPreference.Front, new CoastalRule()), CoastBoundary(planet));
            Assert.IsTrue(result.IsValid, result.Reason.ToString());
        }

        [Test]
        public void Coastal_Inland_RuleViolation()
        {
            var planet = TestPlanets.Generate(3);
            var result = planet.Placement.Check(new Footprint(1f, RowPreference.Front, new CoastalRule()), CenterOfLongest(planet, TerrainKind.Land));
            Assert.AreEqual(PlacementReason.RuleViolation, result.Reason);
        }

        [Test]
        public void Spanning_NarrowStrait_Valid()
        {
            var (validator, _) = HandBuilt(Repeat(TerrainKind.Land, 4).Concat(Repeat(TerrainKind.Water, 4)).Concat(Repeat(TerrainKind.Land, 4)).ToArray());
            Assert.IsTrue(validator.Check(new Footprint(10f, RowPreference.Front, new SpanningRule(5f)), 6f).IsValid);
        }

        [Test]
        public void Spanning_TooWide_RuleViolation()
        {
            var (validator, _) = HandBuilt(Repeat(TerrainKind.Land, 4).Concat(Repeat(TerrainKind.Water, 4)).Concat(Repeat(TerrainKind.Land, 4)).ToArray());
            Assert.AreEqual(PlacementReason.RuleViolation, validator.Check(new Footprint(10f, RowPreference.Front, new SpanningRule(3f)), 6f).Reason);
        }

        [Test]
        public void Spanning_EndInWater_ReasonWater()
        {
            var (validator, _) = HandBuilt(Repeat(TerrainKind.Land, 4).Concat(Repeat(TerrainKind.Water, 8)).ToArray());
            Assert.AreEqual(PlacementReason.Water, validator.Check(new Footprint(6f, RowPreference.Front, new SpanningRule(5f)), 5f).Reason);
        }

        [Test]
        public void Either_FrontTaken_FallsBackToBack()
        {
            var (validator, surface) = HandBuilt(Repeat(TerrainKind.Land, 10));
            surface.Row(SurfaceRow.Front).TryAdd(new FakeOccupant("a"), ArcRange.Centered(5f, 2f, 10f));
            var result = validator.Check(new Footprint(2f, RowPreference.Either, LandOnly), 5f);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(SurfaceRow.Back, result.Row);
        }

        [Test]
        public void Either_BothTaken_ReasonOccupied()
        {
            var (validator, surface) = HandBuilt(Repeat(TerrainKind.Land, 10));
            surface.Row(SurfaceRow.Front).TryAdd(new FakeOccupant("a"), ArcRange.Centered(5f, 2f, 10f));
            surface.Row(SurfaceRow.Back).TryAdd(new FakeOccupant("b"), ArcRange.Centered(5f, 2f, 10f));
            Assert.AreEqual(PlacementReason.Occupied, validator.Check(new Footprint(2f, RowPreference.Either, LandOnly), 5f).Reason);
        }

        [Test]
        public void Order_RuleBeforeOccupancy()
        {
            var (validator, surface) = HandBuilt(Repeat(TerrainKind.Water, 10));
            surface.Row(SurfaceRow.Front).TryAdd(new FakeOccupant("a"), ArcRange.Centered(5f, 2f, 10f));
            Assert.AreEqual(PlacementReason.Water, validator.Check(new Footprint(2f, RowPreference.Front, LandOnly), 5f).Reason);
        }

        [Test]
        public void AcrossSeam_OccupiedDetected()
        {
            var (validator, surface) = HandBuilt(Repeat(TerrainKind.Land, 10));
            surface.Row(SurfaceRow.Front).TryAdd(new FakeOccupant("a"), ArcRange.Centered(9.7f, 1f, 10f));
            Assert.AreEqual(PlacementReason.Occupied, validator.Check(new Footprint(2f, RowPreference.Front, LandOnly), 0.1f).Reason);
        }

        [Test]
        public void FindNearestValid_NudgesOffOccupant()
        {
            var planet = TestPlanets.Generate(3);
            var center = CenterOfLongest(planet, TerrainKind.Land);
            var taken = ArcRange.Centered(center, 2f, planet.Frame.Circumference);
            planet.Surface.Row(SurfaceRow.Front).TryAdd(new FakeOccupant("a"), taken);
            var result = planet.Placement.FindNearestValid(new Footprint(2f, RowPreference.Front, LandOnly), center, 3f);
            Assert.IsTrue(result.HasValue);
            Assert.IsTrue(result.Value.IsValid);
            Assert.IsFalse(result.Value.Range.Overlaps(taken));
            Assert.LessOrEqual(ArcRange.Distance(result.Value.Range.Center, center, planet.Frame.Circumference), 3f);
        }

        [Test]
        public void FindNearestValid_NoRoom_ReturnsNull()
        {
            var (validator, _) = HandBuilt(Repeat(TerrainKind.Land, 3).Concat(Repeat(TerrainKind.Water, 7)).ToArray());
            Assert.IsNull(validator.FindNearestValid(new Footprint(4f, RowPreference.Either, LandOnly), 1.5f, 10f));
        }

        [Test]
        public void FindNearestValid_FullyOccupied_ReturnsNull()
        {
            var (validator, surface) = HandBuilt(Repeat(TerrainKind.Land, 10));
            surface.Row(SurfaceRow.Front).TryAdd(new FakeOccupant("a"), new ArcRange(0f, 10f, 10f));
            surface.Row(SurfaceRow.Back).TryAdd(new FakeOccupant("b"), new ArcRange(0f, 10f, 10f));
            Assert.IsNull(validator.FindNearestValid(new Footprint(1f, RowPreference.Either, LandOnly), 5f, 10f));
        }

        [Test]
        public void PlacementRules_CreateMatchesKind()
        {
            Assert.IsInstanceOf<LandOnlyRule>(PlacementRules.Create(PlacementRuleKind.LandOnly, 3f));
            Assert.IsInstanceOf<CoastalRule>(PlacementRules.Create(PlacementRuleKind.Coastal, 3f));
            Assert.IsInstanceOf<SpanningRule>(PlacementRules.Create(PlacementRuleKind.Spanning, 3f));
        }
    }
}
