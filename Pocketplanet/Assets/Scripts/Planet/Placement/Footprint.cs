using System.Collections.Generic;

namespace PocketPlanet.Planet
{
    // Values are serialized by index in prefabs, so new members go at the end.
    public enum RowPreference
    {
        Back,
        Front,
        Any,
        Middle
    }

    public sealed class Footprint
    {
        static readonly string[] NoTags = new string[0];

        public Footprint(float width, RowPreference row, IPlacementRule rule,
            IReadOnlyCollection<string> requiredTags = null, IReadOnlyCollection<string> forbiddenTags = null)
        {
            Width = width;
            Row = row;
            Rule = rule;
            RequiredTags = requiredTags ?? NoTags;
            ForbiddenTags = forbiddenTags ?? NoTags;
        }

        public float Width { get; }
        public RowPreference Row { get; }
        public IPlacementRule Rule { get; }
        public IReadOnlyCollection<string> RequiredTags { get; }
        public IReadOnlyCollection<string> ForbiddenTags { get; }
    }
}
