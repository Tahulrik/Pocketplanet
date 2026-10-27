using PocketPlanet.Planet;
using UnityEngine;
using UnityEngine.UI;

public class BuildRowToast : MonoBehaviour
{
	[SerializeField] float holdSeconds = 0.9f;
	[SerializeField] float fadeSeconds = 0.5f;
	[SerializeField] int fontSize = 48;
	[SerializeField] Vector2 anchor = new Vector2(0.5f, 0.82f);
	[SerializeField] Color pillColor = new Color(1f, 1f, 1f, 0.85f);

	CanvasGroup group;
	Text label;
	float shownAt = float.NegativeInfinity;

	void Awake()
	{
		var canvas = OverlayFactory.CreateOverlay(transform, "BuildRowToast", 60);
		var pill = OverlayFactory.CreateImage(canvas.transform, "Pill", null, new Vector2(420f, 90f), pillColor);
		pill.rectTransform.anchorMin = pill.rectTransform.anchorMax = anchor;
		pill.raycastTarget = false;
		label = OverlayFactory.CreateText(pill.transform, "", fontSize);
		group = pill.gameObject.AddComponent<CanvasGroup>();
		group.alpha = 0f;
		group.blocksRaycasts = false;
	}

	void OnEnable() => BuildRowSelection.ActiveRowChanged += Show;

	void OnDisable() => BuildRowSelection.ActiveRowChanged -= Show;

	void Show(SurfaceRow row)
	{
		if (Time.timeSinceLevelLoad < 0.1f)
			return;
		label.text = $"{row} layer";
		shownAt = Time.unscaledTime;
	}

	void Update()
	{
		var age = Time.unscaledTime - shownAt;
		group.alpha = age < holdSeconds ? 1f : Mathf.Clamp01(1f - (age - holdSeconds) / fadeSeconds);
	}
}
