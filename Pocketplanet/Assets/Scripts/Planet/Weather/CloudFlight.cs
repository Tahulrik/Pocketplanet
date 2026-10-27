namespace PocketPlanet.Planet
{
    public readonly struct CloudFlight
    {
        public CloudFlight(float arc, float height, float speed, float lifetime, float growSeconds, float startAge)
        {
            Arc = arc;
            Height = height;
            Speed = speed;
            Lifetime = lifetime;
            GrowSeconds = growSeconds;
            StartAge = startAge;
        }

        public float Arc { get; }
        public float Height { get; }
        public float Speed { get; }
        public float Lifetime { get; }
        public float GrowSeconds { get; }
        public float StartAge { get; }
    }
}
