using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class RowSorting
    {
        public const int BackRowOffset = -10;

        public static void Apply(GameObject target, SurfaceRow row)
        {
            if (row != SurfaceRow.Back)
                return;
            foreach (var sprite in target.GetComponentsInChildren<SpriteRenderer>(true))
                sprite.sortingOrder += BackRowOffset;
        }
    }
}
