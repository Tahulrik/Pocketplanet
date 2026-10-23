namespace PocketPlanet.Planet
{
    public readonly struct PlanetCoord
    {
        public float Arc { get; }
        public float Height { get; }

        public PlanetCoord(float arc, float height)
        {
            Arc = arc;
            Height = height;
        }

        public override string ToString() => $"(arc {Arc:0.##}, height {Height:0.##})";
    }
}
