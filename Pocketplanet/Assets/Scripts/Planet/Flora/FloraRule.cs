namespace PocketPlanet.Planet
{
    public readonly struct FloraRule
    {
        public FloraRule(string biomeId, float density, int variantCount)
        {
            BiomeId = biomeId;
            Density = density;
            VariantCount = variantCount;
        }

        public string BiomeId { get; }
        public float Density { get; }
        public int VariantCount { get; }
    }
}
