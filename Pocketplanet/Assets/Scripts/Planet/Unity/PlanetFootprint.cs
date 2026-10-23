using UnityEngine;

namespace PocketPlanet.Planet
{
    public class PlanetFootprint : MonoBehaviour
    {
        const float FallbackWidth = 1f;

        [SerializeField, Min(0f), Tooltip("Arc width in world units. 0 = use the sprite width.")]
        float width;
        [SerializeField] RowPreference row = RowPreference.Front;
        [SerializeField] PlacementRuleKind rule = PlacementRuleKind.LandOnly;
        [SerializeField, Min(0f)] float maxSpan = 3f;
        [SerializeField] string[] requiredBiomeTags = new string[0];
        [SerializeField] string[] forbiddenBiomeTags = new string[0];

        public void Configure(float arcWidth, RowPreference rowPreference, PlacementRuleKind ruleKind)
        {
            width = arcWidth;
            row = rowPreference;
            rule = ruleKind;
        }

        public Footprint ToFootprint() => new Footprint(
            width > 0f ? width : SpriteWidth(gameObject), row, PlacementRules.Create(rule, maxSpan),
            requiredBiomeTags, forbiddenBiomeTags);

        public static Footprint For(GameObject prefab)
        {
            var footprint = prefab.GetComponent<PlanetFootprint>();
            return footprint != null
                ? footprint.ToFootprint()
                : new Footprint(SpriteWidth(prefab), RowPreference.Front, new LandOnlyRule());
        }

        static float SpriteWidth(GameObject target)
        {
            var widest = 0f;
            foreach (var sprite in target.GetComponentsInChildren<SpriteRenderer>(true))
                if (sprite.sprite != null)
                    widest = Mathf.Max(widest, sprite.sprite.bounds.size.x * Mathf.Abs(sprite.transform.lossyScale.x));
            return widest > 0f ? widest : FallbackWidth;
        }
    }
}
