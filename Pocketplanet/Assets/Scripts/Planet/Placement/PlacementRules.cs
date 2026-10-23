namespace PocketPlanet.Planet
{
    public enum PlacementRuleKind
    {
        LandOnly,
        Coastal,
        Spanning
    }

    public static class PlacementRules
    {
        public static IPlacementRule Create(PlacementRuleKind kind, float maxSpan)
        {
            switch (kind)
            {
                case PlacementRuleKind.Coastal: return new CoastalRule();
                case PlacementRuleKind.Spanning: return new SpanningRule(maxSpan);
                default: return new LandOnlyRule();
            }
        }
    }
}
