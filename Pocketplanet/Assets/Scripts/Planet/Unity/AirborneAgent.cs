using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    [ExecuteAlways]
    public class AirborneAgent : MonoBehaviour, IAirborne
    {
        [SerializeField] string[] tags = new string[0];

        AirTraffic traffic;
        PlanetFrame frame;
        Vector2 lastPosition;

        public string Id => GetEntityId().ToString();
        public PlanetCoord Position => frame != null ? frame.ToCoord(transform.position) : default;
        public Vector2 Velocity { get; private set; }
        public IReadOnlyCollection<string> Tags => tags;

        public void SetTags(params string[] newTags) => tags = newTags;

        void OnEnable()
        {
            var host = PlanetHost.Active;
            if (host == null)
                return;
            frame = host.Planet.Frame;
            traffic = host.Planet.Air.Traffic;
            traffic.Register(this);
            lastPosition = transform.position;
        }

        void OnDisable()
        {
            traffic?.Unregister(this);
            traffic = null;
        }

        void Update()
        {
            Vector2 position = transform.position;
            if (Time.deltaTime > 0f)
                Velocity = (position - lastPosition) / Time.deltaTime;
            lastPosition = position;
        }
    }
}
