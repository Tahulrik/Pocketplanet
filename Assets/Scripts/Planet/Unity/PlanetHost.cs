using System;
using UnityEngine;

namespace PocketPlanet.Planet
{
    [ExecuteAlways]
    [RequireComponent(typeof(CircleCollider2D))]
    public class PlanetHost : MonoBehaviour
    {
        [SerializeField] PlanetSettings settings;
        [SerializeField] PlanetView view;

        IPlanet planet;
        bool regenerateRequested;

        public static PlanetHost Active { get; private set; }

        public event Action<IPlanet> Generated;

        public IPlanet Planet => planet ?? Regenerate();

        public void Configure(PlanetSettings planetSettings, PlanetView planetView)
        {
            settings = planetSettings;
            view = planetView;
            planet = null;
        }

        public IPlanet Regenerate()
        {
            if (settings == null)
                throw new InvalidOperationException($"{name} has no {nameof(PlanetSettings)} assigned.");
            planet = PlanetGenerator.Generate(settings, transform.position);
            if (view != null)
                view.Render(planet);
            GetComponent<CircleCollider2D>().radius = planet.Frame.Radius;
            Generated?.Invoke(planet);
            return planet;
        }

        public void SyncFrame()
        {
            if (planet != null)
                planet.Frame.Center = transform.position;
        }

        void OnEnable() => Active = this;

        void OnDisable()
        {
            if (Active == this)
                Active = null;
        }

        void Awake()
        {
            if (settings != null)
                Regenerate();
        }

        void OnValidate() => regenerateRequested = true;

        void Update()
        {
            if (!regenerateRequested || settings == null)
                return;
            regenerateRequested = false;
            Regenerate();
        }

        void LateUpdate() => SyncFrame();

        void OnDrawGizmos()
        {
            if (planet != null)
                PlanetGizmos.Draw(planet);
        }
    }
}
