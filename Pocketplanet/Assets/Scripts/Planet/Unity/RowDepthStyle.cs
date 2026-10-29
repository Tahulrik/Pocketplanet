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
        bool styled;

        public SurfaceRow Row { get; private set; }

        public bool IsInteractive => !styled || BuildRowSelection.IsInteractive(Row);

        public static void Apply(GameObject target, SurfaceRow row, RowDepthKind kind)
        {
            var style = target.GetComponent<RowDepthStyle>();
            if (style == null)
                style = target.AddComponent<RowDepthStyle>();
            style.Apply(row, kind);
        }

        public static bool IsInteractiveObject(GameObject target)
        {
            var style = target.GetComponent<RowDepthStyle>();
            return style == null || style.IsInteractive;
        }

        public void Apply(SurfaceRow row, RowDepthKind kind)
        {
            CaptureBaseOnce();
            Row = row;
            styled = true;
            SortingGroupOf(gameObject).sortingOrder = RowDepth.SortingOrder(row, kind);
            transform.localScale = baseScale * RowDepth.Scale(row);
            ApplyColors();
        }

        void OnEnable() => BuildRowSelection.ActiveRowChanged += OnActiveRowChanged;

        void OnDisable() => BuildRowSelection.ActiveRowChanged -= OnActiveRowChanged;

        void OnActiveRowChanged(SurfaceRow _)
        {
            if (styled)
                ApplyColors();
        }

        void ApplyColors()
        {
            var tint = RowDepth.Tint(Row);
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
