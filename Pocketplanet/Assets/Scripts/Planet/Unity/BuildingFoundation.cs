using UnityEngine;

namespace PocketPlanet.Planet
{
    // A stone block under a building that reaches into the ground, so where the rim curves away
    // beneath the building's flat base the gap shows foundation instead of sky.
    public static class BuildingFoundation
    {
        public const string ChildName = "Foundation";
        const float ExtraDepth = 0.25f;
        const float TopInset = 0.03f;
        static readonly Color Stone = new Color(0.36f, 0.34f, 0.33f);

        public static void Ensure(GameObject building, float width, float radius)
        {
            if (building.transform.Find(ChildName) != null)
                return;
            var depth = RimFit.Sag(width, radius) + ExtraDepth;
            var foundation = new GameObject(ChildName).AddComponent<SpriteRenderer>();
            foundation.transform.SetParent(building.transform, false);
            var rootScale = building.transform.lossyScale;
            foundation.transform.localPosition = new Vector3(0f, (TopInset - depth / 2f) / rootScale.y, 0f);
            foundation.transform.localScale = new Vector3(width / rootScale.x, depth / rootScale.y, 1f);
            foundation.sprite = SolidSprite.White;
            foundation.color = Stone;
            foundation.sortingOrder = LowestSortingOrder(building) - 1;
        }

        static int LowestSortingOrder(GameObject building)
        {
            var lowest = 0;
            foreach (var sprite in building.GetComponentsInChildren<SpriteRenderer>(true))
                lowest = Mathf.Min(lowest, sprite.sortingOrder);
            return lowest;
        }
    }
}
