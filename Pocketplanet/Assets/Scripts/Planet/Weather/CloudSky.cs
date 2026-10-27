using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public class CloudSky : MonoBehaviour
    {
        const float VisibleBottomPerScale = 0.2f;

        [Header("Clouds")]
        [SerializeField] Sprite[] cloudSprites = new Sprite[0];
        [SerializeField, Min(0)] int maxFormations = 7;
        [SerializeField] Vector2 spawnInterval = new Vector2(4f, 12f);
        [SerializeField] Vector2 lifetime = new Vector2(30f, 70f);
        [SerializeField, Min(0f)] float fadeSeconds = 5f;
        [SerializeField, Tooltip("Height above the surface, in world units.")]
        Vector2 heightRange = new Vector2(3f, 5.2f);
        [SerializeField, Tooltip("Arc units per second. All clouds share one wind direction.")]
        Vector2 driftSpeed = new Vector2(0.04f, 0.14f);
        [SerializeField] Vector2 scaleRange = new Vector2(1.2f, 2.6f);
        [SerializeField, Range(0f, 1f)] float groupChance = 0.35f;
        [SerializeField] Vector2Int groupSize = new Vector2Int(2, 5);
        [SerializeField] int sortingOrder = 30;

        [Header("Rain")]
        [SerializeField, Range(0f, 1f)] float rainChance = 0.2f;
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
            for (int i = 0; i < maxFormations / 2; i++)
                Spawn(prewarmed: true);
            ScheduleNextSpawn();
        }

        void Update()
        {
            formations.RemoveAll(f => f == null);
            if (Time.time < nextSpawn)
                return;
            if (formations.Count < maxFormations)
                Spawn(prewarmed: false);
            ScheduleNextSpawn();
        }

        void Spawn(bool prewarmed)
        {
            var shape = CloudShape.Create(rng, ShapeSettings(), cloudSprites.Length);
            var life = Range(lifetime);
            var flight = new CloudFlight(
                Range(new Vector2(0f, frame.Circumference)),
                Range(heightRange),
                wind * Range(driftSpeed),
                life,
                fadeSeconds,
                prewarmed ? life * Range(new Vector2(0.15f, 0.6f)) : 0f);

            var formation = new GameObject("CloudFormation").AddComponent<CloudFormation>();
            formation.transform.SetParent(transform, false);
            formation.Begin(frame, shape, cloudSprites, flight, sortingOrder);
            if (rng.NextDouble() < CurrentRainChance())
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
