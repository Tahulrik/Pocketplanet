using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public class CloudFormation : MonoBehaviour
    {
        public const string CloudTag = "Cloud";
        public const string RainTag = "Rain";

        const float AlphaRampShare = 0.3f;

        readonly List<SpriteRenderer> puffs = new List<SpriteRenderer>();
        readonly List<float> puffScales = new List<float>();
        PlanetFrame frame;
        RainShower rain;
        Color tint;
        float arc;
        float height;
        float speed;
        float age;
        float lifetime;
        float growSeconds;

        public float Height => height;

        public void Begin(PlanetFrame planetFrame, CloudShape shape, IReadOnlyList<Sprite> sprites, CloudFlight flight,
            Color cloudTint, int sortingOrder)
        {
            frame = planetFrame;
            tint = cloudTint;
            arc = flight.Arc;
            height = flight.Height;
            speed = flight.Speed;
            lifetime = flight.Lifetime;
            growSeconds = flight.GrowSeconds;
            age = flight.StartAge;

            for (int i = 0; i < shape.Puffs.Count; i++)
            {
                puffs.Add(CreatePuff(shape.Puffs[i], sprites, sortingOrder + i));
                puffScales.Add(shape.Puffs[i].Scale);
            }

            var agent = gameObject.AddComponent<AirborneAgent>();
            agent.SetTags(CloudTag);
            Apply();
        }

        public RainShower StartRain()
        {
            rain = gameObject.AddComponent<RainShower>();
            GetComponent<AirborneAgent>().SetTags(CloudTag, RainTag);
            return rain;
        }

        void Update()
        {
            age += Time.deltaTime;
            arc = ArcRange.Wrap(arc + speed * Time.deltaTime, frame.Circumference);
            if (age >= lifetime)
            {
                Destroy(gameObject);
                return;
            }
            Apply();
        }

        void Apply()
        {
            transform.SetPositionAndRotation(frame.ToWorld(new PlanetCoord(arc, height)), frame.UprightRotation(arc));
            var totalProgress = 0f;
            for (int i = 0; i < puffs.Count; i++)
            {
                var progress = CloudGrowth.PuffProgress(age, lifetime, growSeconds, i, puffs.Count);
                ShowPuff(i, progress);
                totalProgress += progress;
            }
            if (rain != null)
                rain.SetIntensity(totalProgress / puffs.Count);
        }

        void ShowPuff(int index, float progress)
        {
            var puff = puffs[index];
            puff.enabled = progress > 0f;
            puff.transform.localScale = Vector3.one * (puffScales[index] * CloudGrowth.PopScale(progress));
            puff.color = new Color(tint.r, tint.g, tint.b, tint.a * Mathf.Clamp01(progress / AlphaRampShare));
        }

        SpriteRenderer CreatePuff(CloudPuff puff, IReadOnlyList<Sprite> sprites, int sortingOrder)
        {
            var renderer = new GameObject("Puff").AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(transform, false);
            renderer.transform.localPosition = puff.Offset;
            renderer.sprite = sprites[puff.SpriteIndex];
            renderer.flipX = puff.Flipped;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }
    }
}
