using System;
using UnityEngine;

// Flat shapes drawn once at runtime, so the menu needs no extra art assets.
public static class UiShapes
{
	const int Size = 128;
	const float Sharpness = Size / 2f;

	static Sprite disc;
	static Sprite outlinedDisc;
	static Sprite ring;
	static Sprite arrow;

	public static Sprite Disc => disc != null ? disc : disc = CreateRadial(DiscAlpha, Color.white, Color.white, 0f);

	public static Sprite OutlinedDisc => outlinedDisc != null
		? outlinedDisc
		: outlinedDisc = CreateRadial(DiscAlpha, Color.white, new Color(0.15f, 0.15f, 0.15f), 0.1f);

	public static Sprite Ring => ring != null ? ring : ring = CreateRadial(RingAlpha, Color.white, Color.white, 0f);

	// A triangle pointing up, for direction markers.
	public static Sprite Arrow => arrow != null ? arrow : arrow = Create(point => WithAlpha(Color.white, ArrowAlpha(point)));

	static float DiscAlpha(float distance) => Mathf.Clamp01((1f - distance) * Sharpness);

	static float RingAlpha(float distance) => Mathf.Clamp01((1f - distance) * Sharpness) * Mathf.Clamp01((distance - 0.9f) * Sharpness);

	// Tip at the top, base near the bottom; alpha fades over about a pixel at each edge.
	static float ArrowAlpha(Vector2 point)
	{
		const float tip = 0.9f;
		const float baseLine = -0.7f;
		const float halfBase = 0.8f;
		var halfWidthHere = halfBase * (tip - point.y) / (tip - baseLine);
		var insideBase = point.y - baseLine;
		var insideSides = halfWidthHere - Mathf.Abs(point.x);
		return Mathf.Clamp01(Mathf.Min(insideBase, insideSides) * Sharpness);
	}

	static Sprite CreateRadial(Func<float, float> alphaAt, Color fill, Color outline, float outlineWidth) => Create(point =>
	{
		var distance = point.magnitude;
		return WithAlpha(distance > 1f - outlineWidth ? outline : fill, alphaAt(distance));
	});

	static Color WithAlpha(Color color, float alpha)
	{
		color.a = alpha;
		return color;
	}

	// colorAt receives each pixel centre in the square from (-1, -1) to (1, 1).
	static Sprite Create(Func<Vector2, Color> colorAt)
	{
		var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
		var pixels = new Color[Size * Size];
		var half = Size / 2f;
		for (int y = 0; y < Size; y++)
			for (int x = 0; x < Size; x++)
				pixels[y * Size + x] = colorAt(new Vector2(x + 0.5f - half, y + 0.5f - half) / half);
		texture.SetPixels(pixels);
		texture.Apply();
		return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
	}
}
