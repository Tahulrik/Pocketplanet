using UnityEngine;
using UnityEngine.UI;

// A ring around the selected object at every zoom level, never smaller than minDiameter so it stays visible from afar.
[RequireComponent(typeof(WorldTapSelector))]
public class SelectionHighlight : MonoBehaviour
{
	[SerializeField] float minDiameter = 90f;
	[SerializeField] float padding = 30f;
	[SerializeField] Color color = new Color(1f, 0.92f, 0.55f, 0.9f);

	WorldTapSelector selector;
	Canvas canvas;
	Image ring;

	void Awake()
	{
		selector = GetComponent<WorldTapSelector>();
		canvas = OverlayFactory.CreateOverlay(transform, "SelectionHighlightCanvas", 54);
		ring = OverlayFactory.CreateImage(canvas.transform, "Highlight", UiShapes.Ring, Vector2.one * minDiameter, color);
		ring.raycastTarget = false;
		ring.gameObject.SetActive(false);
	}

	void LateUpdate()
	{
		var target = selector.Selection?.Current;
		var camera = Camera.main;
		var visible = target != null && camera != null && TryPlace(camera, WorldPicker.VisibleBounds(target));
		ring.gameObject.SetActive(visible);
	}

	bool TryPlace(Camera camera, Bounds bounds)
	{
		var viewport = camera.WorldToViewportPoint(bounds.center);
		if (viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f)
			return false;
		var onScreenDiagonal = ((Vector2)camera.WorldToScreenPoint(bounds.max) - (Vector2)camera.WorldToScreenPoint(bounds.min)).magnitude;
		ring.rectTransform.sizeDelta = Vector2.one * Mathf.Max(minDiameter, onScreenDiagonal / canvas.scaleFactor + padding);
		ring.rectTransform.position = camera.WorldToScreenPoint(bounds.center);
		return true;
	}
}
