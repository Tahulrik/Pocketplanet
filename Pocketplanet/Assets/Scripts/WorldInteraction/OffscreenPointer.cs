using UnityEngine;
using UnityEngine.UI;

// An arrow on the screen edge pointing at the selection while it is out of view. Tapping it brings the camera there.
[RequireComponent(typeof(WorldTapSelector))]
public class OffscreenPointer : MonoBehaviour
{
	[SerializeField] float size = 90f;
	[SerializeField] float edgeMargin = 80f;
	[SerializeField] Color color = new Color(1f, 0.92f, 0.55f, 0.95f);

	WorldTapSelector selector;
	Canvas canvas;
	Image arrow;

	void Awake()
	{
		selector = GetComponent<WorldTapSelector>();
		canvas = OverlayFactory.CreateOverlay(transform, "OffscreenPointerCanvas", 56);
		arrow = OverlayFactory.CreateImage(canvas.transform, "Pointer", UiShapes.Arrow, Vector2.one * size, color);
		var button = arrow.gameObject.AddComponent<Button>();
		button.targetGraphic = arrow;
		button.onClick.AddListener(JumpToSelection);
		arrow.gameObject.SetActive(false);
	}

	void LateUpdate()
	{
		var target = selector.Selection?.Current;
		var camera = Camera.main;
		var shown = target != null && camera != null && TryPlace(camera.WorldToViewportPoint(WorldPicker.VisibleBounds(target).center));
		arrow.gameObject.SetActive(shown);
	}

	// Walks from the screen centre toward the target and stops at the margin-inset screen edge.
	bool TryPlace(Vector2 viewport)
	{
		if (viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f)
			return false;
		var canvasSize = ((RectTransform)canvas.transform).rect.size;
		var direction = Vector2.Scale(viewport - new Vector2(0.5f, 0.5f), canvasSize);
		var reach = canvasSize / 2f - Vector2.one * edgeMargin;
		var scale = Mathf.Min(
			direction.x != 0f ? reach.x / Mathf.Abs(direction.x) : float.PositiveInfinity,
			direction.y != 0f ? reach.y / Mathf.Abs(direction.y) : float.PositiveInfinity);
		arrow.rectTransform.anchoredPosition = direction * scale;
		arrow.rectTransform.localRotation = Quaternion.LookRotation(Vector3.forward, direction);
		return true;
	}

	void JumpToSelection()
	{
		var target = selector.Selection.Current;
		if (target != null)
			GameHandler.Instance.CameraController.FocusOn(target.transform);
	}
}
