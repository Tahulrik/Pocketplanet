using UnityEngine;

namespace PocketPlanet.Planet
{
    // A 1x1 world-unit white sprite, tinted and scaled for plain blocks like rain drops and foundations.
    public static class SolidSprite
    {
        static Sprite white;

        public static Sprite White
        {
            get
            {
                if (white == null)
                {
                    var texture = Texture2D.whiteTexture;
                    white = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
                }
                return white;
            }
        }
    }
}
