using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class BlockMapGeneratorTests
    {
        static BlockMap Generate(int seed)
        {
            var settings = TestPlanets.Settings(seed);
            var arcs = RimLayout.BuildLandArcs(settings, new System.Random(seed));
            return BlockMapGenerator.Generate(settings, new BiomeTable(settings.BiomeTable), arcs, seed);
        }

        [Test]
        public void SameSeed_IdenticalMap()
        {
            var a = Generate(5);
            var b = Generate(5);
            for (int x = 0; x < a.Size; x++)
                for (int y = 0; y < a.Size; y++)
                    Assert.AreEqual(a[x, y], b[x, y]);
        }

        [Test]
        public void DifferentSeed_DifferentMap()
        {
            var a = Generate(5);
            var b = Generate(6);
            var differs = false;
            for (int x = 0; x < a.Size && !differs; x++)
                for (int y = 0; y < a.Size && !differs; y++)
                    differs = a[x, y] != b[x, y];
            Assert.IsTrue(differs);
        }

        [Test]
        public void OutsideCircle_IsEmpty_InsideIsNot()
        {
            var map = Generate(5);
            Assert.AreEqual(BlockMap.Empty, map[0, 0]);
            Assert.AreNotEqual(BlockMap.Empty, map[map.Size / 2, map.Size / 2]);
        }

        [TestCase(5)]
        [TestCase(8)]
        public void BlocksTouchingTheDisc_AreNeverEmpty(int seed)
        {
            var settings = TestPlanets.Settings(seed);
            var map = Generate(seed);
            var half = map.BlockSize / 2f;
            for (int x = 0; x < map.Size; x++)
                for (int y = 0; y < map.Size; y++)
                {
                    var c = map.BlockCenter(x, y);
                    var closest = new UnityEngine.Vector2(
                        UnityEngine.Mathf.Clamp(0f, c.x - half, c.x + half),
                        UnityEngine.Mathf.Clamp(0f, c.y - half, c.y + half));
                    if (closest.magnitude < settings.Radius - 1e-3f)
                        Assert.AreNotEqual(BlockMap.Empty, map[x, y], $"block {x},{y}");
                }
        }

        [Test]
        public void HasBothLandAndWater()
        {
            var settings = TestPlanets.Settings(5);
            var table = new BiomeTable(settings.BiomeTable);
            var map = Generate(5);
            int land = 0, water = 0;
            for (int x = 0; x < map.Size; x++)
                for (int y = 0; y < map.Size; y++)
                {
                    if (map[x, y] == BlockMap.Empty) continue;
                    if (table.IsLand(map[x, y])) land++; else water++;
                }
            Assert.Greater(land, 0);
            Assert.Greater(water, 0);
        }

        [Test]
        public void TryGetBlock_RoundTripsBlockCenter()
        {
            var map = new BlockMap(10, 0.5f);
            Assert.IsTrue(map.TryGetBlock(map.BlockCenter(3, 7), out var x, out var y));
            Assert.AreEqual(3, x);
            Assert.AreEqual(7, y);
            Assert.IsFalse(map.TryGetBlock(new UnityEngine.Vector2(100f, 0f), out _, out _));
        }

        [Test]
        public void GridGrowsWithCircumference() =>
            Assert.Greater(TestPlanets.Settings(circumference: 200f).GridSize, TestPlanets.Settings(circumference: 48f).GridSize);
    }
}
