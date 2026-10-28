using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    // Decorative trees: spread evenly at start, removed by anything built over them,
    // and slowly regrown on free land of a fitting biome until the planet has its starting number again.
    public class TreeGrove : MonoBehaviour
    {
        [Serializable]
        public class BiomeFlora
        {
            public string biomeId;
            [Min(0f), Tooltip("Trees per world unit along the rim, across all layers.")]
            public float density = 1f;
            public Sprite[] sprites = new Sprite[0];
        }

        sealed class PlantedTree
        {
            public GameObject Tree;
            public Transform GrowPivot;
            public SurfaceRow Row;
            public ArcRange Range;
            public float PlantedAt;
        }

        [SerializeField] int seed = 1;
        [SerializeField] Vector2 scaleRange = new Vector2(1.1f, 1.7f);
        [SerializeField, Range(0f, 0.9f), Tooltip("How much tree spacing varies around the even step.")]
        float jitter = 0.45f;
        [SerializeField, Min(0.05f), Tooltip("Arc width a tree keeps clear. Buildings that overlap it remove the tree.")]
        float clearanceWidth = 0.25f;
        [SerializeField, Min(0f), Tooltip("Closest a regrown tree may stand to another tree in the same layer.")]
        float minTreeSpacing = 0.4f;
        [SerializeField, Tooltip("Seconds between regrowth attempts.")]
        Vector2 regrowInterval = new Vector2(6f, 14f);
        [SerializeField, Min(0.01f)] float growSeconds = 8f;
        [SerializeField] BiomeFlora[] flora = new BiomeFlora[0];

        readonly List<PlantedTree> trees = new List<PlantedTree>();
        readonly Dictionary<SurfaceRow, Action<IPlanetOccupant, ArcRange>> clearHandlers =
            new Dictionary<SurfaceRow, Action<IPlanetOccupant, ArcRange>>();
        IPlanet planet;
        FloraRule[] rules;
        FloraLayoutSettings settings;
        System.Random regrowthRng;
        int targetCount;
        float nextRegrowth;

        public int Count => trees.Count;

        void Start()
        {
            if (PlanetHost.Active == null)
                return;
            planet = PlanetHost.Active.Planet;
            rules = Array.ConvertAll(flora, f => new FloraRule(f.biomeId, f.density, f.sprites.Length));
            settings = new FloraLayoutSettings { Jitter = jitter, MinScale = scaleRange.x, MaxScale = scaleRange.y };
            regrowthRng = new System.Random(seed + 1);

            foreach (var placement in FloraLayout.Generate(planet.Surface.Profile, planet.Biomes, rules, seed, settings))
                TryPlant(placement, fullyGrown: true);
            targetCount = trees.Count;
            ScheduleRegrowth();

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

        void Update()
        {
            if (planet == null)
                return;
            GrowSaplings();
            if (Time.time >= nextRegrowth)
            {
                Regrow();
                ScheduleRegrowth();
            }
        }

        void Regrow()
        {
            if (trees.Count >= targetCount)
                return;
            var spot = FloraLayout.FindRegrowthSpot(planet.Surface.Profile, planet.Biomes, rules, regrowthRng, settings, IsFreeForTree);
            if (spot.HasValue)
                TryPlant(spot.Value, fullyGrown: false);
        }

        bool IsFreeForTree(float arc, SurfaceRow row)
        {
            if (!planet.Surface.Row(row).IsFree(RangeAt(arc)))
                return false;
            foreach (var tree in trees)
                if (tree.Row == row && ArcRange.Distance(tree.Range.Center, arc, planet.Frame.Circumference) < minTreeSpacing)
                    return false;
            return true;
        }

        void TryPlant(FloraPlacement placement, bool fullyGrown)
        {
            var sprites = flora[placement.RuleIndex].sprites;
            if (sprites.Length == 0 || sprites[placement.Variant] == null)
                return;
            var range = RangeAt(placement.Arc);
            if (!planet.Surface.Row(placement.Row).IsFree(range))
                return;
            var tree = TreeFactory.Create(transform, sprites[placement.Variant], placement, planet.Frame);
            var planted = new PlantedTree
            {
                Tree = tree,
                GrowPivot = tree.transform.Find(TreeFactory.GrowPivotName),
                Row = placement.Row,
                Range = range,
                PlantedAt = fullyGrown ? float.NegativeInfinity : Time.time
            };
            trees.Add(planted);
            SetGrowth(planted);
        }

        void GrowSaplings()
        {
            foreach (var tree in trees)
                if (Time.time - tree.PlantedAt < growSeconds + 0.1f)
                    SetGrowth(tree);
        }

        void SetGrowth(PlantedTree tree)
        {
            if (tree.GrowPivot == null)
                return;
            var progress = Mathf.Clamp01((Time.time - tree.PlantedAt) / growSeconds);
            tree.GrowPivot.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, progress);
        }

        ArcRange RangeAt(float arc) => ArcRange.Centered(arc, clearanceWidth, planet.Frame.Circumference);

        void ScheduleRegrowth() => nextRegrowth = Time.time + Mathf.Lerp(regrowInterval.x, regrowInterval.y, (float)regrowthRng.NextDouble());

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
