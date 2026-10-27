namespace PocketPlanet.Planet
{
    public enum PlacementReason
    {
        None,
        Water,
        BiomeNotBuildable,
        MissingBiomeTag,
        ForbiddenBiomeTag,
        Occupied,
        RuleViolation,
        RowNotAllowed
    }
}
