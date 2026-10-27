using System.Collections.Generic;
using PocketPlanet.Planet;
using UnityEngine;
using UnityEngine.UI;

// A half circle that starts at the planet surface on one side of the target, arcs over it and lands on the other side.
public class RadialContextMenu : MonoBehaviour, IContextMenu
{
	[SerializeField] float buttonSize = 150f;
	[SerializeField] float minRadius = 190f;
	[SerializeField] float maxRadius = 520f;
	[SerializeField] float padding = 60f;
	[SerializeField] int fontSize = 26;
	[SerializeField] Color ringColor = new Color(1f, 1f, 1f, 0.55f);
	[SerializeField] Color buttonColor = new Color(1f, 0.92f, 0.55f);
	[SerializeField] Color unavailableColor = new Color(0.7f, 0.7f, 0.7f);
	[SerializeField] Color infoColor = new Color(1f, 1f, 1f, 0.92f);

	readonly Dictionary<RectTransform, Vector2> uprightParts = new Dictionary<RectTransform, Vector2>();
	Canvas canvas;
	RectTransform root;
	Image ring;
	Text info;
	GameObject target;
	float radius;

	public GameObject Target => target;
	public bool IsOpen => target != null;

	void Awake() => canvas = OverlayFactory.CreateOverlay(transform, "ContextMenuCanvas", 55);

	public void Open(GameObject newTarget, IReadOnlyList<ContextAction> actions)
	{
		Close();
		target = newTarget;
		radius = -1f;
		root = OverlayFactory.CreateRect(canvas.transform, "RadialMenu");
		ring = CreateRing(root);
		for (int i = 0; i < actions.Count; i++)
			CreateButton(actions[i], AngleFor(i, actions.Count));
		info = CreateInfo();
		Place();
	}

	public void ShowInfo(string text)
	{
		if (info == null)
			return;
		info.text = text;
		info.transform.parent.gameObject.SetActive(true);
	}

	public void Close()
	{
		if (root != null)
			Destroy(root.gameObject);
		root = null;
		target = null;
		uprightParts.Clear();
	}

	void LateUpdate()
	{
		if (root == null)
			return;
		if (target == null || !target.activeInHierarchy || !RowDepthStyle.IsInteractiveObject(target))
		{
			Close();
			return;
		}
		Place();
	}

	void Place()
	{
		var camera = Camera.main;
		var frame = PlanetHost.Active.Planet.Frame;
		var arc = frame.ToCoord(target.transform.position).Arc;
		var ground = frame.ToWorld(new PlanetCoord(arc, 0f));
		var outward = frame.Outward(arc);

		Vector2 screenGround = camera.WorldToScreenPoint(ground);
		var screenUp = (Vector2)camera.WorldToScreenPoint(ground + outward) - screenGround;
		root.position = screenGround;
		root.rotation = Quaternion.LookRotation(Vector3.forward, screenUp);
		foreach (var part in uprightParts.Keys)
			part.rotation = Quaternion.identity;

		SetRadius(RadiusFor(camera, frame, arc, screenGround));
	}

	float RadiusFor(Camera camera, PlanetFrame frame, float arc, Vector2 screenGround)
	{
		var bounds = WorldPicker.VisibleBounds(target);
		var height = frame.ToCoord(bounds.center).Height + bounds.extents.magnitude;
		var top = (Vector2)camera.WorldToScreenPoint(frame.ToWorld(new PlanetCoord(arc, height)));
		var inCanvasUnits = (top - screenGround).magnitude / canvas.scaleFactor;
		return Mathf.Clamp(inCanvasUnits + padding, minRadius, maxRadius);
	}

	void SetRadius(float newRadius)
	{
		if (Mathf.Approximately(newRadius, radius))
			return;
		radius = newRadius;
		ring.rectTransform.sizeDelta = Vector2.one * radius * 2f;
		foreach (var part in uprightParts)
			part.Key.anchoredPosition = PositionOnArc(part.Value.x, radius + part.Value.y);
	}

	Image CreateRing(Transform parent)
	{
		var image = OverlayFactory.CreateImage(parent, "Arc", UiShapes.Ring, Vector2.zero, ringColor);
		image.type = Image.Type.Filled;
		image.fillMethod = Image.FillMethod.Radial180;
		image.fillOrigin = (int)Image.Origin180.Bottom;
		image.raycastTarget = false;
		return image;
	}

	void CreateButton(ContextAction action, float angleDegrees)
	{
		var image = OverlayFactory.CreateImage(root, action.Label, UiShapes.OutlinedDisc, Vector2.one * buttonSize,
			action.IsAvailable ? buttonColor : unavailableColor);
		var button = image.gameObject.AddComponent<Button>();
		button.targetGraphic = image;
		button.interactable = action.IsAvailable;
		button.onClick.AddListener(() => Run(action));
		var text = action.Detail == null ? action.Label : $"{action.Label}\n<size={fontSize * 3 / 4}>{action.Detail}</size>";
		OverlayFactory.CreateText(image.transform, text, fontSize).supportRichText = true;
		uprightParts.Add(image.rectTransform, new Vector2(angleDegrees, 0f));
	}

	Text CreateInfo()
	{
		var pill = OverlayFactory.CreateImage(root, "Info", null, new Vector2(380f, 150f), infoColor);
		pill.raycastTarget = false;
		uprightParts.Add(pill.rectTransform, new Vector2(90f, buttonSize));
		pill.gameObject.SetActive(false);
		return OverlayFactory.CreateText(pill.transform, "", fontSize);
	}

	void Run(ContextAction action)
	{
		action.Execute(this);
		if (action.ClosesMenu)
			Close();
	}

	// Spreads actions evenly over the half circle, leaving the ends free where it meets the ground.
	static float AngleFor(int index, int count) => 180f - (index + 1) * 180f / (count + 1);

	static Vector2 PositionOnArc(float degrees, float distance)
	{
		var radians = degrees * Mathf.Deg2Rad;
		return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * distance;
	}
}
