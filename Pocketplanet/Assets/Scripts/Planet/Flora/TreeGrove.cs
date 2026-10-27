using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public class TreeGrove : MonoBehaviour
    {
        [Serializable]
        public class BiomeFlora
        {
            public string biomeId;
            [Min(0f), Tooltip("Average trees per world unit inside a grove.")]
            public float density = 1.5f;
            public Sprite[] sprites = new Sprite[0];
        }

        sealed class PlantedTree
        {
            public GameObject Tree;
            public SurfaceRow Row;
            public ArcRange Range;
        }

        [SerializeField] int seed = 1;
        [SerializeField, Range(0f, 1f), Tooltip("Higher leaves more open land between groves.")]
        float groveThreshold = 0.55f;
        [SerializeField, Min(1f), Tooltip("Noise cells around the planet. Roughly a third of them become groves.")]
        float groveNoiseCells = 20f;
        [SerializeField] Vector2 scaleRange = new Vector2(1.1f, 1.7f);
        [SerializeField, Min(0.05f), Tooltip("Arc width a tree keeps clear. Buildings that overlap it remove the tree.")]
        float clearanceWidth = 0.25f;
        [SerializeField] BiomeFlora[] flora = new BiomeFlora[0];

        readonly List<PlantedTree> trees = new List<PlantedTree>();
        readonly Dictionary<SurfaceRow, Action<IPlanetOccupant, ArcRange>> clearHandlers =
            new Dictionary<SurfaceRow, Action<IPlanetOccupant, ArcRange>>();
        IPlanet planet;

        public int Count => trees.Count;

        void Start()
        {
            if (PlanetHost.Active == null)
                return;
            planet = PlanetHost.Active.Planet;
            Plant();
            foreach (var row in SurfaceRows.FrontToBack)
                planet.Surface.Row(row).OccupantAdded += ClearHandlerFor(row);
        }

        void OnDestroy()
        {
            if (planet == null)
                return;
            foreach (var row in SurfaceRows.FrontToBack)
                planet.Surface.Row(row).OccupantAdded -= ClearHandlerFor(row);
        }

        void Plant()
        {
            var rules = Array.ConvertAll(flora, f => new FloraRule(f.biomeId, f.density, f.sprites.Length));
            var settings = new FloraLayoutSettings
            {
                GroveThreshold = groveThreshold,
                GroveNoiseCells = groveNoiseCells,
                MinScale = scaleRange.x,
                MaxScale = scaleRange.y
            };
            foreach (var placement in FloraLayout.Generate(planet.Surface.Profile, planet.Biomes, rules, seed, settings))
                TryPlant(placement);
        }

        void TryPlant(FloraPlacement placement)
        {
            var sprites = flora[placement.RuleIndex].sprites;
            if (sprites.Length == 0 || sprites[placement.Variant] == null)
                return;
            var range = ArcRange.Centered(placement.Arc, clearanceWidth, planet.Frame.Circumference);
            if (!planet.Surface.Row(placement.Row).IsFree(range))
                return;
            var tree = TreeFactory.Create(transform, sprites[placement.Variant], placement, planet.Frame);
            trees.Add(new PlantedTree { Tree = tree, Row = placement.Row, Range = range });
        }

        Action<IPlanetOccupant, ArcRange> ClearHandlerFor(SurfaceRow row)
        {
            if (!clearHandlers.TryGetValue(row, out var handler))
                clearHandlers[row] = handler = (_, range) => RemoveTrees(row, range);
            return handler;
        }

        void RemoveTrees(SurfaceRow row, ArcRange range)
        {
            for (int i = trees.Count - 1; i >= 0; i--)
            {
                if (trees[i].Row != row || !trees[i].Range.Overlaps(range))
                    continue;
                if (trees[i].Tree != null)
                    Destroy(trees[i].Tree);
                trees.RemoveAt(i);
            }
        }
    }
}
