using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace PocketPlanet.Planet.Tests
{
    public class IntegrationTests
    {
        readonly List<Object> created = new List<Object>();
        PlanetHost host;

        IPlanet Planet => host.Planet;

        [SetUp]
        public void SetUp()
        {
            var planetObject = Track(new GameObject("Planet"));
            host = planetObject.AddComponent<PlanetHost>();
            host.Configure(TestPlanets.Settings(3), null);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in created)
                if (obj != null)
                    Object.DestroyImmediate(obj);
            created.Clear();
        }

        T Track<T>(T obj) where T : Object
        {
            created.Add(obj);
            return obj;
        }

        float LandArc() => Planet.Surface.Profile.Segments.Where(s => s.Terrain == TerrainKind.Land)
            .OrderByDescending(s => s.Range.Length).First().Range.Center;

        PlacementResult LandPlacement(float width = 1f) =>
            Planet.Placement.Check(new Footprint(width, RowPreference.Front, new LandOnlyRule()), LandArc());

        PlanetOccupant NewOccupant(OccupantKind kind = OccupantKind.Building)
        {
            var occupant = Track(new GameObject("occupant")).AddComponent<PlanetOccupant>();
            occupant.Kind = kind;
            return occupant;
        }

        [Test]
        public void LegacyPlanet_FindPointOnSurface_SnapsFromAnywhere()
        {
            foreach (var position in new[] { new Vector2(0f, 0.001f), new Vector2(0f, 500f) })
            {
                var point = LegacyPlanetApi.FindPointOnSurface(LegacyPlanetApi.Radius, position);
                Assert.AreEqual(Planet.Frame.Radius, point.magnitude, 1e-3);
                Assert.AreEqual(Planet.Frame.Circumference / 4f, Planet.Frame.ToCoord(point).Arc, 1e-3);
            }
        }

        [Test]
        public void LegacyPlanet_RotateObjectToSurface_UpIsOutward()
        {
            var go = Track(new GameObject("thing"));
            go.transform.position = new Vector3(0f, 5f, 0f);
            LegacyPlanetApi.RotateObjectToSurface(go);
            Assert.AreEqual(0f, go.transform.up.x, 1e-4);
            Assert.AreEqual(1f, go.transform.up.y, 1e-4);
        }

        [Test]
        public void LegacyPlanet_BlockzillaSpawnpoint_IsOnDeepWater()
        {
            var spawn = LegacyPlanetApi.GetBlockzillaSpawnpoint(Vector2.right);
            var arc = Planet.Frame.ToCoord(spawn.position).Arc;
            Assume.That(Planet.Queries.FindNearestDeepWater(0f).HasValue);
            Assert.AreEqual(Planet.Biomes.DeepWater, Planet.Surface.Profile.BiomeAt(arc));
        }

        [Test]
        public void Occupant_ReserveAndRelease()
        {
            var occupant = NewOccupant();
            var placement = LandPlacement();
            Assert.IsTrue(occupant.Reserve(placement));
            Assert.IsTrue(Planet.Surface.Row(placement.Row).TryGetRange(occupant, out _));
            occupant.Release();
            Assert.IsFalse(Planet.Surface.Row(placement.Row).TryGetRange(occupant, out _));
        }

        [Test]
        public void Occupant_TransferKeepsRange()
        {
            var site = NewOccupant(OccupantKind.ConstructionSite);
            var building = NewOccupant();
            var placement = LandPlacement();
            site.Reserve(placement);
            Assert.IsTrue(site.TransferTo(building));
            var row = Planet.Surface.Row(placement.Row);
            Assert.IsFalse(row.TryGetRange(site, out _));
            Assert.IsTrue(row.TryGetRange(building, out var range));
            Assert.AreEqual(placement.Range.Start, range.Start, 1e-4);
        }

        [Test]
        public void UpgradeInPlace_ReleaseThenReserve_Succeeds()
        {
            var oldHouse = NewOccupant();
            var newSite = NewOccupant(OccupantKind.ConstructionSite);
            var placement = LandPlacement();
            oldHouse.Reserve(placement);
            Assert.IsFalse(newSite.Reserve(placement));
            oldHouse.Release();
            Assert.IsTrue(newSite.Reserve(placement));
        }

        [Test]
        public void PreplacedBuilding_OnLand_KeepsItsSpot_AndBlocksPlacement()
        {
            var occupant = NewOccupant();
            var arc = LandArc();
            occupant.transform.position = Planet.Frame.ToWorld(new PlanetCoord(arc, 0f));
            var footprint = new Footprint(1f, RowPreference.Front, new LandOnlyRule());

            Assert.IsFalse(occupant.IsReserved);
            Assert.IsTrue(occupant.SettleOnNearestValidSpot(footprint));
            Assert.IsTrue(occupant.IsReserved);
            Assert.AreEqual(arc, Planet.Frame.ToCoord(occupant.transform.position).Arc, Planet.Surface.Profile.Spacing);
            Assert.AreEqual(PlacementReason.Occupied, Planet.Placement.Check(footprint, arc).Reason);
        }

        [Test]
        public void PreplacedBuilding_OverWater_MovesOntoLand_StandingUpright()
        {
            var occupant = NewOccupant();
            var waterArc = Planet.Surface.Profile.Segments.First(s => s.Terrain == TerrainKind.Water).Range.Center;
            occupant.transform.position = Planet.Frame.ToWorld(new PlanetCoord(waterArc, 2f));
            var footprint = new Footprint(1f, RowPreference.Front, new LandOnlyRule());

            Assert.IsTrue(occupant.SettleOnNearestValidSpot(footprint));
            var coord = Planet.Frame.ToCoord(occupant.transform.position);
            Assert.AreEqual(TerrainKind.Land, Planet.Surface.Profile.TerrainAt(coord.Arc));
            Assert.AreEqual(0f, coord.Height, 1e-3);
            Assert.AreEqual(0f, Vector2.Distance(Planet.Frame.Outward(coord.Arc), occupant.transform.up), 1e-3);
        }

        [Test]
        public void PreplacedBuildings_AtSameSpot_DoNotOverlap()
        {
            var first = NewOccupant();
            var second = NewOccupant();
            var position = Planet.Frame.ToWorld(new PlanetCoord(LandArc(), 0f));
            first.transform.position = position;
            second.transform.position = position;
            var footprint = new Footprint(1f, RowPreference.Front, new LandOnlyRule());

            Assert.IsTrue(first.SettleOnNearestValidSpot(footprint));
            Assert.IsTrue(second.SettleOnNearestValidSpot(footprint));
            var row = Planet.Surface.Row(SurfaceRow.Front);
            row.TryGetRange(first, out var firstRange);
            row.TryGetRange(second, out var secondRange);
            Assert.IsFalse(firstRange.Overlaps(secondRange));
        }

        [Test]
        public void GeneratedPlanet_LandAtTop()
        {
            var up = ContinentOrientation.UpArc(Planet.Frame.Circumference);
            Assert.AreEqual(TerrainKind.Land, Planet.Surface.Profile.TerrainAt(up));
        }

        [Test]
        public void BackRowReservation_SortsBehindFrontRow()
        {
            var back = ReserveWithSprite(RowPreference.Back, LandArc());
            var front = ReserveWithSprite(RowPreference.Front, LandArc());
            Assert.Less(back.GetComponent<SortingGroup>().sortingOrder, front.GetComponent<SortingGroup>().sortingOrder);
            Assert.Less(back.transform.localScale.x, front.transform.localScale.x);
        }

        [Test]
        public void RowDepthStyle_ReapplyingDoesNotCompound()
        {
            var occupant = ReserveWithSprite(RowPreference.Back, LandArc());
            var sprite = occupant.GetComponentInChildren<SpriteRenderer>();
            var scale = occupant.transform.localScale;
            var color = sprite.color;
            RowDepthStyle.Apply(occupant.gameObject, SurfaceRow.Back, RowDepthKind.Building);
            Assert.AreEqual(scale, occupant.transform.localScale);
            Assert.AreEqual(color, sprite.color);
        }

        [Test]
        public void RowDepth_FloraSortsJustBehindBuildingsInSameRow()
        {
            foreach (var row in SurfaceRows.FrontToBack)
                Assert.AreEqual(RowDepth.SortingOrder(row, RowDepthKind.Building) - 1, RowDepth.SortingOrder(row, RowDepthKind.Flora));
            Assert.Less(RowDepth.SortingOrder(SurfaceRow.Middle, RowDepthKind.Building), RowDepth.SortingOrder(SurfaceRow.Front, RowDepthKind.Flora));
        }

        PlanetOccupant ReserveWithSprite(RowPreference row, float arc)
        {
            var occupant = NewOccupant();
            var sprite = new GameObject("sprite").AddComponent<SpriteRenderer>();
            sprite.transform.SetParent(occupant.transform);
            occupant.Reserve(Planet.Placement.Check(new Footprint(1f, row, new LandOnlyRule()), arc));
            return occupant;
        }

        [Test]
        public void Airborne_RegistersWhileEnabled()
        {
            var agent = Track(new GameObject("blockman")).AddComponent<AirborneAgent>();
            Assert.IsTrue(Planet.Air.Traffic.All.Contains(agent));
            agent.enabled = false;
            Assert.IsFalse(Planet.Air.Traffic.All.Contains(agent));
        }

        [Test]
        public void Airborne_PositionIsPlanetCoord()
        {
            var agent = Track(new GameObject("blockman")).AddComponent<AirborneAgent>();
            agent.transform.position = new Vector3(0f, Planet.Frame.Radius + 3f, 0f);
            Assert.AreEqual(3f, agent.Position.Height, 1e-3);
            Assert.AreEqual(1, Planet.Air.BandAt(agent.Position.Height));
        }

        [Test]
        public void Footprint_AutoWidthFromSprite()
        {
            var prefab = Track(new GameObject("house"));
            var texture = Track(new Texture2D(200, 10));
            var sprite = Track(Sprite.Create(texture, new Rect(0, 0, 200, 10), new Vector2(0.5f, 0f), 100f));
            var child = new GameObject("sprite");
            child.transform.SetParent(prefab.transform);
            child.AddComponent<SpriteRenderer>().sprite = sprite;
            Assert.AreEqual(2f, PlanetFootprint.For(prefab).Width, 1e-3);
        }

        [Test]
        public void Footprint_Component_OverridesWidthAndRule()
        {
            var prefab = Track(new GameObject("port"));
            var footprint = prefab.AddComponent<PlanetFootprint>();
            footprint.Configure(3f, RowPreference.Any, PlacementRuleKind.Coastal);
            var result = PlanetFootprint.For(prefab);
            Assert.AreEqual(3f, result.Width, 1e-4);
            Assert.AreEqual(RowPreference.Any, result.Row);
            Assert.IsInstanceOf<CoastalRule>(result.Rule);
        }

        [Test]
        public void NoHost_LegacyApiThrowsClearError()
        {
            host.enabled = false;
            var error = Assert.Throws<System.InvalidOperationException>(() => _ = LegacyPlanetApi.Radius);
            StringAssert.Contains("PlanetHost", error.Message);
        }
    }
}
