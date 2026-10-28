using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public class CloudSky : MonoBehaviour
    {
        const float VisibleBottomPerScale = 0.2f;

        [Header("Clouds")]
        [SerializeField] Sprite[] cloudSprites = new Sprite[0];
        [SerializeField, Min(0)] int maxFormations = 16;
        [SerializeField, Min(0), Tooltip("Clouds already in the sky when the game starts.")]
        int startingFormations = 12;
        [SerializeField] Vector2 spawnInterval = new Vector2(1.5f, 4f);
        [SerializeField] Vector2 lifetime = new Vector2(30f, 70f);
        [SerializeField, Min(0f), Tooltip("Seconds for all puffs of a cloud to grow in (and shrink out).")]
        float growSeconds = 4f;
        [SerializeField, Tooltip("Height above the surface, in world units.")]
        Vector2 heightRange = new Vector2(2.6f, 6.2f);
        [SerializeField, Tooltip("Arc units per second. All clouds share one wind direction.")]
        Vector2 driftSpeed = new Vector2(0.025f, 0.2f);
        [SerializeField] Vector2 scaleRange = new Vector2(0.8f, 2.8f);
        [SerializeField, Range(0f, 1f)] float groupChance = 0.35f;
        [SerializeField] Vector2Int groupSize = new Vector2Int(2, 5);
        [SerializeField] int sortingOrder = 30;

        [Header("Rain")]
        [SerializeField, Min(0f), Tooltip("Narrower clouds never rain. A single puff is at most about 2.4 wide.")]
        float minRainCloudWidth = 2.4f;
        [SerializeField, Range(0f, 1f), Tooltip("Chance that a cloud wide enough to rain does.")]
        float rainChance = 0.35f;
        [SerializeField] Color rainCloudTint = new Color(0.58f, 0.6f, 0.66f, 1f);
        [SerializeField, Range(0f, 1f), Tooltip("Extra rain chance at full doom mood.")]
        float doomRainBonus = 0.4f;
        [SerializeField] WorldMoodController mood;
        [SerializeField, Min(1f)] float dropsPerUnit = 18f;
        [SerializeField] float dropSpeed = 4.5f;
        [SerializeField] Vector2 dropSize = new Vector2(0.02f, 0.16f);
        [SerializeField] Color dropColor = new Color(0.62f, 0.78f, 0.95f, 0.7f);

        readonly List<CloudFormation> formations = new List<CloudFormation>();
        System.Random rng;
        PlanetFrame frame;
        float wind;
        float nextSpawn;

        void Start()
        {
            if (PlanetHost.Active == null || cloudSprites.Length == 0)
            {
                enabled = false;
                return;
            }
            frame = PlanetHost.Active.Planet.Frame;
            rng = new System.Random(Random.Range(int.MinValue, int.MaxValue));
            wind = rng.Next(2) == 0 ? 1f : -1f;
            FillSky();
            ScheduleNextSpawn();
        }

        // Spreads the starting clouds evenly around the planet at staggered ages,
        // so any part of the sky has clouds and they don't all fade out together.
        void FillSky()
        {
            var count = Mathf.Min(startingFormations, maxFormations);
            for (int i = 0; i < count; i++)
            {
                var arc = (i + (float)rng.NextDouble()) / count * frame.Circumference;
                var lifeShare = Range(new Vector2(0.05f, 0.85f));
                Spawn(arc, lifeShare);
            }
        }

        void Update()
        {
            formations.RemoveAll(f => f == null);
            if (Time.time < nextSpawn)
                return;
            if (formations.Count < maxFormations)
                Spawn(Range(new Vector2(0f, frame.Circumference)), 0f);
            ScheduleNextSpawn();
        }

        void Spawn(float arc, float lifeAlreadyLived)
        {
            var shape = CloudShape.Create(rng, ShapeSettings(), cloudSprites.Length);
            var life = Range(lifetime);
            var flight = new CloudFlight(
                arc,
                Range(heightRange),
                wind * Range(driftSpeed),
                life,
                growSeconds,
                life * lifeAlreadyLived);

            var formation = new GameObject("CloudFormation").AddComponent<CloudFormation>();
            formation.transform.SetParent(transform, false);
            var raining = shape.Width >= minRainCloudWidth && rng.NextDouble() < CurrentRainChance();
            formation.Begin(frame, shape, cloudSprites, flight, raining ? rainCloudTint : Color.white, sortingOrder);
            if (raining)
                StartRain(formation, shape);
            formations.Add(formation);
        }

        void StartRain(CloudFormation formation, CloudShape shape)
        {
            var dropCount = Mathf.CeilToInt(shape.Width * dropsPerUnit);
            var cloudBottom = -VisibleBottomPerScale * AverageScale(shape);
            formation.StartRain().Begin(shape.Width * 0.8f, cloudBottom, formation.Height, dropCount, dropSpeed,
                dropColor, dropSize, sortingOrder - 1);
        }

        static float AverageScale(CloudShape shape)
        {
            var total = 0f;
            foreach (var puff in shape.Puffs)
                total += puff.Scale;
            return total / shape.Puffs.Count;
        }

        float CurrentRainChance()
        {
            var doom = mood != null ? Mathf.Max(0f, -mood.CurrentMood) : 0f;
            return Mathf.Clamp01(rainChance + doomRainBonus * doom);
        }

        CloudShapeSettings ShapeSettings() => new CloudShapeSettings
        {
            PuffWidth = cloudSprites[0].bounds.size.x,
            MinScale = scaleRange.x,
            MaxScale = scaleRange.y,
            GroupChance = groupChance,
            MinGroupSize = groupSize.x,
            MaxGroupSize = groupSize.y
        };

        void ScheduleNextSpawn() => nextSpawn = Time.time + Range(spawnInterval);

        float Range(Vector2 range) => Mathf.Lerp(range.x, range.y, (float)rng.NextDouble());
    }
}
