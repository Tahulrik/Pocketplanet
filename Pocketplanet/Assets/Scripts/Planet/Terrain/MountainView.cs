using UnityEngine;

namespace PocketPlanet.Planet
{
    public class MountainView : MonoBehaviour
    {
        [SerializeField] Sprite[] sprites = new Sprite[0];

        void Start()
        {
            if (PlanetHost.Active == null || sprites.Length == 0)
                return;
            var planet = PlanetHost.Active.Planet;
            foreach (var mountain in planet.Surface.Mountains)
                Create(mountain, planet.Frame);
        }

        void Create(MountainFeature mountain, PlanetFrame frame)
        {
            var sprite = sprites[mountain.Variant % sprites.Length];
            var arc = mountain.Range.Center;
            var root = new GameObject(mountain.Id);
            root.transform.SetParent(transform, false);
            var sink = RimFit.SinkDepth(mountain.Range.Length, frame.Radius);
            root.transform.SetPositionAndRotation(frame.ToWorld(new PlanetCoord(arc, -sink)), frame.UprightRotation(arc));

            // Row depth shrinks far rows, so pre-scale up to make the drawn mountain match the range it blocks.
            // The sunk part hides behind the planet, so it's added to the height to keep the peak where it belongs.
            var depthScale = RowDepth.Scale(mountain.Row);
            var size = sprite.bounds.size;
            root.transform.localScale = new Vector3(
                mountain.Range.Length / size.x / depthScale,
                (mountain.Height + sink) / size.y / depthScale,
                1f);

            var body = new GameObject("Sprite");
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, -sprite.bounds.min.y, 0f);
            body.AddComponent<SpriteRenderer>().sprite = sprite;

            RowDepthStyle.Apply(root, mountain.Row, RowDepthKind.Building);
        }
    }
}
