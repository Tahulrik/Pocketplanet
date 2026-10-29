using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class TreeFactory
    {
        public const string GrowPivotName = "Grow";

        // Tree root (row depth) > grow pivot at the ground (animated scale) > sprite lifted so its base sits on the ground.
        public static GameObject Create(Transform parent, Sprite sprite, FloraPlacement placement, PlanetFrame frame)
        {
            var tree = new GameObject(sprite.name);
            tree.transform.SetParent(parent, false);
            var drawnWidth = sprite.bounds.size.x * placement.Scale;
            var ground = frame.ToWorld(new PlanetCoord(placement.Arc, -RimFit.SinkDepth(drawnWidth, frame.Radius)));
            tree.transform.SetPositionAndRotation(ground, frame.UprightRotation(placement.Arc));
            tree.transform.localScale = Vector3.one * placement.Scale;

            var grow = new GameObject(GrowPivotName);
            grow.transform.SetParent(tree.transform, false);

            var body = new GameObject("Sprite");
            body.transform.SetParent(grow.transform, false);
            body.transform.localPosition = new Vector3(0f, -sprite.bounds.min.y, 0f);
            var renderer = body.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.flipX = placement.Flipped;

            RowDepthStyle.Apply(tree, placement.Row, RowDepthKind.Flora);
            return tree;
        }
    }
}
