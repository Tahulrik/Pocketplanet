using UnityEngine;
using UnityEngine.UI;

public static class OverlayFactory
{
	const int ReferenceWidth = 1080;
	const int ReferenceHeight = 1920;
	static readonly Color TextColor = new Color(0.15f, 0.15f, 0.15f);

	public static Canvas CreateOverlay(Transform parent, string name, int sortingOrder)
	{
		var canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		canvasObject.transform.SetParent(parent, false);
		var canvas = canvasObject.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = sortingOrder;
		var scaler = canvasObject.GetComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
		scaler.matchWidthOrHeight = 0.5f;
		return canvas;
	}

	public static RectTransform CreateRect(Transform parent, string name)
	{
		var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
		rect.SetParent(parent, false);
		return rect;
	}

	public static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 size, Color color)
	{
		var rect = CreateRect(parent, name);
		rect.sizeDelta = size;
		var image = rect.gameObject.AddComponent<Image>();
		image.sprite = sprite;
		image.color = color;
		return image;
	}

	public static Text CreateText(Transform parent, string text, int fontSize)
	{
		var rect = CreateRect(parent, "Label");
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = rect.offsetMax = Vector2.zero;
		var label = rect.gameObject.AddComponent<Text>();
		label.text = text;
		label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		label.fontSize = fontSize;
		label.fontStyle = FontStyle.Bold;
		label.alignment = TextAnchor.MiddleCenter;
		label.horizontalOverflow = HorizontalWrapMode.Overflow;
		label.verticalOverflow = VerticalWrapMode.Overflow;
		label.color = TextColor;
		label.raycastTarget = false;
		return label;
	}
}
