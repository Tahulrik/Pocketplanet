using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public class CloudFormation : MonoBehaviour
    {
        public const string CloudTag = "Cloud";
        public const string RainTag = "Rain";

        readonly List<SpriteRenderer> puffs = new List<SpriteRenderer>();
        PlanetFrame frame;
        RainShower rain;
        float arc;
        float height;
        float speed;
        float age;
        float lifetime;
        float fadeSeconds;

        public float Height => height;

        public void Begin(PlanetFrame planetFrame, CloudShape shape, IReadOnlyList<Sprite> sprites, CloudFlight flight, int sortingOrder)
        {
            frame = planetFrame;
            arc = flight.Arc;
            height = flight.Height;
            speed = flight.Speed;
            lifetime = flight.Lifetime;
            fadeSeconds = flight.FadeSeconds;
            age = flight.StartAge;

            for (int i = 0; i < shape.Puffs.Count; i++)
                puffs.Add(CreatePuff(shape.Puffs[i], sprites, sortingOrder + i));

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
            var alpha = FadeEnvelope.Alpha(age, lifetime, fadeSeconds);
            foreach (var puff in puffs)
                puff.color = new Color(1f, 1f, 1f, alpha);
            if (rain != null)
                rain.SetIntensity(alpha);
        }

        SpriteRenderer CreatePuff(CloudPuff puff, IReadOnlyList<Sprite> sprites, int sortingOrder)
        {
            var renderer = new GameObject("Puff").AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(transform, false);
            renderer.transform.localPosition = puff.Offset;
            renderer.transform.localScale = Vector3.one * puff.Scale;
            renderer.sprite = sprites[puff.SpriteIndex];
            renderer.flipX = puff.Flipped;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }
    }
}
