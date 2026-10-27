using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PocketPlanet.Planet
{
    [DisallowMultipleComponent]
    public class RowDepthStyle : MonoBehaviour
    {
        readonly Dictionary<SpriteRenderer, Color> baseColors = new Dictionary<SpriteRenderer, Color>();
        Vector3 baseScale;
        bool captured;

        public static void Apply(GameObject target, SurfaceRow row, RowDepthKind kind)
        {
            var style = target.GetComponent<RowDepthStyle>();
            if (style == null)
                style = target.AddComponent<RowDepthStyle>();
            style.Apply(row, kind);
        }

        public void Apply(SurfaceRow row, RowDepthKind kind)
        {
            CaptureBaseOnce();
            SortingGroupOf(gameObject).sortingOrder = RowDepth.SortingOrder(row, kind);
            transform.localScale = baseScale * RowDepth.Scale(row);
            var tint = RowDepth.Tint(row);
            foreach (var pair in baseColors)
                if (pair.Key != null)
                    pair.Key.color = pair.Value * tint;
        }

        void CaptureBaseOnce()
        {
            if (captured)
                return;
            captured = true;
            baseScale = transform.localScale;
            foreach (var sprite in GetComponentsInChildren<SpriteRenderer>(true))
                baseColors[sprite] = sprite.color;
        }

        static SortingGroup SortingGroupOf(GameObject target)
        {
            var group = target.GetComponent<SortingGroup>();
            return group != null ? group : target.AddComponent<SortingGroup>();
        }
    }
}
