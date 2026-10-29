using System.Collections.Generic;
using PocketPlanet.Planet;
using UnityEngine;
using UnityEngine.UI;

// Three stacked bars, back layer on top, mirroring the rows on the planet.
// The active layer's bar lights up, and tapping a bar jumps straight to that layer.
[RequireComponent(typeof(BuildRowSelection))]
public class BuildRowIndicator : MonoBehaviour
{
	[SerializeField] Vector2 anchor = new Vector2(0f, 1f);
	[SerializeField] Vector2 offset = new Vector2(40f, -40f);
	[SerializeField] float barLength = 150f;
	[SerializeField] float barThickness = 16f;
	[SerializeField] float rowHeight = 46f;
	[SerializeField] float padding = 14f;
	[SerializeField] Color activeColor = new Color(0.25f, 1f, 0.3f, 1f);
	[SerializeField] Color idleColor = new Color(1f, 1f, 1f, 0.4f);
	[SerializeField] Color panelColor = new Color(0f, 0f, 0f, 0.35f);

	readonly Dictionary<SurfaceRow, Image> bars = new Dictionary<SurfaceRow, Image>();
	BuildRowSelection selection;

	void Awake()
	{
		selection = GetComponent<BuildRowSelection>();
		var canvas = OverlayFactory.CreateOverlay(transform, "BuildRowIndicator", 55);
		var panel = CreatePanel(canvas.transform);
		var frontToBack = SurfaceRows.FrontToBack;
		for (int slot = 0; slot < frontToBack.Count; slot++)
			CreateRow(panel, frontToBack[frontToBack.Count - 1 - slot], slot);
	}

	void OnEnable()
	{
		BuildRowSelection.ActiveRowChanged += Highlight;
		Highlight(selection.Current);
	}

	void OnDisable() => BuildRowSelection.ActiveRowChanged -= Highlight;

	void Highlight(SurfaceRow active)
	{
		foreach (var pair in bars)
			pair.Value.color = pair.Key == active ? activeColor : idleColor;
	}

	RectTransform CreatePanel(Transform canvas)
	{
		var size = new Vector2(barLength + padding * 2f, rowHeight * SurfaceRows.FrontToBack.Count + padding * 2f);
		var panel = OverlayFactory.CreateImage(canvas, "Panel", null, size, panelColor);
		var rect = panel.rectTransform;
		rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
		rect.anchoredPosition = offset;
		panel.raycastTarget = false;
		return rect;
	}

	void CreateRow(RectTransform panel, SurfaceRow row, int slot)
	{
		var hitArea = OverlayFactory.CreateImage(panel, $"{row} layer", null, new Vector2(barLength + padding * 2f, rowHeight), Color.clear);
		var rect = hitArea.rectTransform;
		rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
		rect.anchoredPosition = new Vector2(0f, -(padding + rowHeight * (slot + 0.5f)));
		var button = hitArea.gameObject.AddComponent<Button>();
		button.transition = Selectable.Transition.None;
		button.onClick.AddListener(() => selection.Select(row));

		var bar = OverlayFactory.CreateImage(rect, "Bar", null, new Vector2(barLength * RowDepth.Scale(row), barThickness), idleColor);
		bar.raycastTarget = false;
		bars[row] = bar;
	}
}
