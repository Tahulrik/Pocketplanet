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
        RowDepthKind kind;

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

        public void Apply(SurfaceRow row, RowDepthKind depthKind)
        {
            CaptureBaseOnce();
            Row = row;
            kind = depthKind;
            styled = true;
            ApplyDepth();
        }

        void OnEnable() => BuildRowSelection.ActiveRowChanged += OnActiveRowChanged;

        void OnDisable() => BuildRowSelection.ActiveRowChanged -= OnActiveRowChanged;

        void OnActiveRowChanged(SurfaceRow _)
        {
            if (styled)
                ApplyDepth();
        }

        void ApplyDepth()
        {
            SortingGroupOf(gameObject).sortingOrder = RowDepth.SortingOrder(Row, kind);
            transform.localScale = baseScale * RowDepth.Scale(Row);
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
