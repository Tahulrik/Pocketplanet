using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class TreeFactory
    {
        public static GameObject Create(Transform parent, Sprite sprite, FloraPlacement placement, PlanetFrame frame)
        {
            var tree = new GameObject(sprite.name);
            tree.transform.SetParent(parent, false);
            var surface = frame.ToWorld(new PlanetCoord(placement.Arc, 0f));
            tree.transform.SetPositionAndRotation(surface, frame.UprightRotation(placement.Arc));
            tree.transform.localScale = Vector3.one * placement.Scale;

            var body = new GameObject("Sprite");
            body.transform.SetParent(tree.transform, false);
            body.transform.localPosition = new Vector3(0f, -sprite.bounds.min.y, 0f);
            var renderer = body.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.flipX = placement.Flipped;

            RowDepthStyle.Apply(tree, placement.Row, RowDepthKind.Flora);
            return tree;
        }
    }
}
