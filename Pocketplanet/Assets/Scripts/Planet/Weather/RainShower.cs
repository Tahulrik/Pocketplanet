using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    // Lives on a cloud formation, whose local -Y points at the planet centre.
    public class RainShower : MonoBehaviour
    {
        static Sprite dropSprite;

        readonly List<SpriteRenderer> drops = new List<SpriteRenderer>();
        float width;
        float top;
        float bottom;
        float speed;
        Color color;

        public void Begin(float showerWidth, float cloudBottom, float fallDistance, int dropCount, float fallSpeed,
            Color dropColor, Vector2 dropSize, int sortingOrder)
        {
            width = showerWidth;
            top = cloudBottom;
            bottom = cloudBottom - fallDistance;
            speed = fallSpeed;
            color = dropColor;
            for (int i = 0; i < dropCount; i++)
                drops.Add(CreateDrop(dropSize, sortingOrder, Mathf.Lerp(bottom, top, Random.value)));
        }

        public void SetIntensity(float alpha)
        {
            var tinted = new Color(color.r, color.g, color.b, color.a * alpha);
            foreach (var drop in drops)
                drop.color = tinted;
        }

        void Update()
        {
            var fall = speed * Time.deltaTime;
            foreach (var drop in drops)
            {
                var position = drop.transform.localPosition;
                position.y -= fall;
                if (position.y < bottom)
                    position = new Vector3(RandomX(), top, 0f);
                drop.transform.localPosition = position;
            }
        }

        SpriteRenderer CreateDrop(Vector2 size, int sortingOrder, float startY)
        {
            var drop = new GameObject("Drop").AddComponent<SpriteRenderer>();
            drop.transform.SetParent(transform, false);
            drop.transform.localPosition = new Vector3(RandomX(), startY, 0f);
            drop.transform.localScale = new Vector3(size.x, size.y, 1f);
            drop.sprite = DropSprite();
            drop.sortingOrder = sortingOrder;
            drop.color = color;
            return drop;
        }

        float RandomX() => (Random.value - 0.5f) * width;

        static Sprite DropSprite()
        {
            if (dropSprite == null)
            {
                var texture = Texture2D.whiteTexture;
                dropSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), texture.width);
            }
            return dropSprite;
        }
    }
}
