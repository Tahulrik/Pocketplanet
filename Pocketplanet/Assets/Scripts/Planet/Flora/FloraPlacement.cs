namespace PocketPlanet.Planet
{
    public readonly struct FloraPlacement
    {
        public FloraPlacement(float arc, SurfaceRow row, int ruleIndex, int variant, float scale, bool flipped)
        {
            Arc = arc;
            Row = row;
            RuleIndex = ruleIndex;
            Variant = variant;
            Scale = scale;
            Flipped = flipped;
        }

        public float Arc { get; }
        public SurfaceRow Row { get; }
        public int RuleIndex { get; }
        public int Variant { get; }
        public float Scale { get; }
        public bool Flipped { get; }
    }
}
