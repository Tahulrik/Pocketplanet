using UnityEngine;

// Flat shapes drawn once at runtime, so the menu needs no extra art assets.
public static class UiShapes
{
	const int Size = 128;

	static Sprite disc;
	static Sprite outlinedDisc;
	static Sprite ring;

	public static Sprite Disc => disc != null ? disc : disc = Create(DiscAlpha, Color.white, Color.white, 0f);

	public static Sprite OutlinedDisc => outlinedDisc != null
		? outlinedDisc
		: outlinedDisc = Create(DiscAlpha, Color.white, new Color(0.15f, 0.15f, 0.15f), 0.1f);

	public static Sprite Ring => ring != null ? ring : ring = Create(RingAlpha, Color.white, Color.white, 0f);

	static float DiscAlpha(float distance) => Mathf.Clamp01((1f - distance) * Size / 2f);

	static float RingAlpha(float distance) => Mathf.Clamp01((1f - distance) * Size / 2f) * Mathf.Clamp01((distance - 0.9f) * Size / 2f);

	static Sprite Create(System.Func<float, float> alphaAt, Color fill, Color outline, float outlineWidth)
	{
		var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
		var pixels = new Color[Size * Size];
		var half = Size / 2f;
		for (int y = 0; y < Size; y++)
			for (int x = 0; x < Size; x++)
			{
				var distance = new Vector2(x + 0.5f - half, y + 0.5f - half).magnitude / half;
				var color = distance > 1f - outlineWidth ? outline : fill;
				color.a = alphaAt(distance);
				pixels[y * Size + x] = color;
			}
		texture.SetPixels(pixels);
		texture.Apply();
		return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
	}
}
