using System;
using System.Linq;
using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class BiomeTableTests
    {
        static BiomeTable DefaultTable() => new BiomeTable(TestPlanets.Settings().BiomeTable);

        [Test]
        public void Classify_Cold_IsSnow()
        {
            var table = DefaultTable();
            Assert.AreEqual("Snow", table[table.ClassifyLand(0.05f, 0.5f)].Id);
        }

        [Test]
        public void Classify_HotDry_IsDesert()
        {
            var table = DefaultTable();
            Assert.AreEqual("Desert", table[table.ClassifyLand(0.9f, 0.1f)].Id);
        }

        [Test]
        public void Classify_MildWet_IsForest()
        {
            var table = DefaultTable();
            Assert.AreEqual("Forest", table[table.ClassifyLand(0.45f, 0.8f)].Id);
        }

        [Test]
        public void Classify_NoMatch_FallsBackToFirstLand()
        {
            var table = DefaultTable();
            Assert.AreEqual("Grassland", table[table.ClassifyLand(2f, 2f)].Id);
        }

        [Test]
        public void WaterIndices_PointAtWaterBiomes()
        {
            var table = DefaultTable();
            Assert.AreEqual(BiomeTable.ShallowWaterId, table[table.ShallowWater].Id);
            Assert.AreEqual(BiomeTable.DeepWaterId, table[table.DeepWater].Id);
            Assert.IsFalse(table.IsLand(table.DeepWater));
            Assert.IsTrue(table.IsLand(table.ClassifyLand(0.5f, 0.5f)));
        }

        [Test]
        public void DefaultSet_HasNoRock_AndAllLandBuildable()
        {
            var biomes = TestPlanets.Settings().BiomeTable;
            Assert.IsFalse(biomes.Any(b => b.Id == "Rock"));
            Assert.IsTrue(biomes.Where(b => b.Kind == BiomeKind.Land).All(b => b.Buildable));
        }

        [Test]
        public void MissingWater_Throws() =>
            Assert.Throws<ArgumentException>(() =>
                new BiomeTable(TestPlanets.Settings().BiomeTable.Where(b => b.Id != "DeepWater").ToList()));

        [Test]
        public void Empty_Throws() =>
            Assert.Throws<ArgumentException>(() => new BiomeTable(new BiomeDefinition[0]));
    }
}
