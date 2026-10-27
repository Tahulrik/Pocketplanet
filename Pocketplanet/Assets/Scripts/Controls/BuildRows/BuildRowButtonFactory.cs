using UnityEngine;
using UnityEngine.UI;

public static class BuildRowButtonFactory
{
	const int ReferenceWidth = 1080;
	const int ReferenceHeight = 1920;

	public static RectTransform CreateOverlay(Transform parent, int sortingOrder)
	{
		var canvasObject = new GameObject("BuildRowCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		canvasObject.transform.SetParent(parent, false);
		var canvas = canvasObject.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = sortingOrder;
		var scaler = canvasObject.GetComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
		scaler.matchWidthOrHeight = 0.5f;
		return (RectTransform)canvasObject.transform;
	}

	public static RectTransform CreateColumn(RectTransform canvas, Vector2 anchor, Vector2 offset, float spacing)
	{
		var column = new GameObject("BuildRowButtons", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
		var rect = (RectTransform)column.transform;
		rect.SetParent(canvas, false);
		rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
		rect.anchoredPosition = offset;
		var layout = column.GetComponent<VerticalLayoutGroup>();
		layout.spacing = spacing;
		layout.childControlWidth = layout.childControlHeight = false;
		layout.childForceExpandWidth = layout.childForceExpandHeight = false;
		var fitter = column.GetComponent<ContentSizeFitter>();
		fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		return rect;
	}

	public static Button CreateButton(RectTransform column, string label, Vector2 size, Sprite background, int fontSize)
	{
		var buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
		var rect = (RectTransform)buttonObject.transform;
		rect.SetParent(column, false);
		rect.sizeDelta = size;
		var image = buttonObject.GetComponent<Image>();
		image.sprite = background;
		var button = buttonObject.GetComponent<Button>();
		button.targetGraphic = image;
		CreateLabel(rect, label, fontSize);
		return button;
	}

	static void CreateLabel(RectTransform button, string text, int fontSize)
	{
		var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
		var rect = (RectTransform)labelObject.transform;
		rect.SetParent(button, false);
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = rect.offsetMax = Vector2.zero;
		var label = labelObject.GetComponent<Text>();
		label.text = text;
		label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		label.fontSize = fontSize;
		label.fontStyle = FontStyle.Bold;
		label.alignment = TextAnchor.MiddleCenter;
		label.color = new Color(0.15f, 0.15f, 0.15f);
		label.raycastTarget = false;
	}
}
