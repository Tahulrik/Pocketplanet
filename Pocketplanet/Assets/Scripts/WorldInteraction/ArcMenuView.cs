using System.Collections.Generic;
using PocketPlanet.Planet;
using UnityEngine;
using UnityEngine.UI;

// A fixed-size arch of action buttons around the selected object, shown only in city view.
// Ground objects get an arch rising from their ground point; flying objects get a full ring.
[RequireComponent(typeof(WorldTapSelector))]
public class ArcMenuView : MonoBehaviour, IContextMenu
{
	[SerializeField] float radius = 300f;
	[SerializeField] float buttonSize = 140f;
	[SerializeField] float buttonGap = 12f;
	[SerializeField] int fontSize = 26;
	[SerializeField] Color ringColor = new Color(1f, 1f, 1f, 0.55f);
	[SerializeField] Color buttonColor = new Color(1f, 0.92f, 0.55f);
	[SerializeField] Color arrowColor = new Color(1f, 1f, 1f, 0.9f);
	[SerializeField] Color unavailableColor = new Color(0.7f, 0.7f, 0.7f);
	[SerializeField] Color infoColor = new Color(1f, 1f, 1f, 0.92f);

	readonly List<RectTransform> uprightParts = new List<RectTransform>();
	readonly List<GameObject> parts = new List<GameObject>();
	WorldTapSelector selector;
	Canvas canvas;
	RectTransform root;
	Image ring;
	Text info;
	List<ContextAction> actions = new List<ContextAction>();
	ArcShape shape;
	float slotAngle;
	int page;
	bool dismissed;
	bool wasShown;

	Selection Selection => selector != null ? selector.Selection : null;

	void Awake()
	{
		selector = GetComponent<WorldTapSelector>();
		canvas = OverlayFactory.CreateOverlay(transform, "ArcMenuCanvas", 55);
		root = OverlayFactory.CreateRect(canvas.transform, "ArcMenu");
		ring = CreateRing();
		slotAngle = 2f * Mathf.Asin((buttonSize + buttonGap) / 2f / radius) * Mathf.Rad2Deg;
		root.gameObject.SetActive(false);
	}

	void Start()
	{
		if (Selection == null)
			return;
		Selection.Changed += OnSelectionChanged;
		Selection.Reselected += OnReselected;
	}

	void OnDestroy()
	{
		if (selector == null || Selection == null)
			return;
		Selection.Changed -= OnSelectionChanged;
		Selection.Reselected -= OnReselected;
	}

	public void ShowInfo(string text)
	{
		if (info == null)
			return;
		info.text = text;
		info.transform.parent.gameObject.SetActive(true);
	}

	public void Close() => dismissed = true;

	void OnSelectionChanged()
	{
		page = 0;
		dismissed = false;
		wasShown = false;
	}

	void OnReselected() => dismissed = false;

	void LateUpdate()
	{
		var shown = ShouldShow();
		root.gameObject.SetActive(shown);
		if (!shown)
		{
			wasShown = false;
			return;
		}
		// Rebuilding when the menu reappears keeps action availability (such as affordable upgrades) current.
		if (!wasShown)
			Rebuild();
		wasShown = true;
		Place();
	}

	bool ShouldShow()
	{
		var target = Selection?.Current;
		return target != null
			&& !dismissed
			&& Camera.main != null
			&& PlanetHost.Active != null
			&& CameraRig.IsInCityView()
			&& selector.Actions.HasActions(target)
			&& RowDepthStyle.IsInteractiveObject(target);
	}

	void Rebuild()
	{
		ClearParts();
		var target = Selection.Current;
		actions = selector.Actions.For(target);
		shape = target.GetComponent<AirborneAgent>() != null ? ArcShape.Ring : ArcShape.Ground;
		page = ArcLayout.NormalizePage(shape, slotAngle, actions.Count, page);
		ShapeRing();
		foreach (var slot in ArcLayout.Layout(shape, slotAngle, actions.Count, page))
			CreateSlot(slot);
		info = CreateInfo();
	}

	void ClearParts()
	{
		foreach (var part in parts)
			Destroy(part);
		parts.Clear();
		uprightParts.Clear();
		info = null;
	}

	void Place()
	{
		var camera = Camera.main;
		var target = Selection.Current;
		if (shape == ArcShape.Ring)
		{
			root.position = camera.WorldToScreenPoint(WorldPicker.VisibleBounds(target).center);
			root.rotation = Quaternion.identity;
		}
		else
			PlaceOnGround(camera, target);
		foreach (var part in uprightParts)
			part.rotation = Quaternion.identity;
	}

	void PlaceOnGround(Camera camera, GameObject target)
	{
		var frame = PlanetHost.Active.Planet.Frame;
		var arc = frame.ToCoord(target.transform.position).Arc;
		var ground = frame.ToWorld(new PlanetCoord(arc, 0f));
		Vector2 screenGround = camera.WorldToScreenPoint(ground);
		var screenUp = (Vector2)camera.WorldToScreenPoint(ground + frame.Outward(arc)) - screenGround;
		root.position = screenGround;
		root.rotation = Quaternion.LookRotation(Vector3.forward, screenUp);
	}

	Image CreateRing()
	{
		var image = OverlayFactory.CreateImage(root, "Arc", UiShapes.Ring, Vector2.one * radius * 2f, ringColor);
		image.type = Image.Type.Filled;
		image.fillMethod = Image.FillMethod.Radial360;
		image.fillOrigin = (int)Image.Origin360.Top;
		image.fillClockwise = true;
		image.raycastTarget = false;
		return image;
	}

	// The fill starts at the top and runs clockwise, so turning it back by half the span centres it on up.
	void ShapeRing()
	{
		var span = ArcLayout.SpanOf(shape);
		ring.fillAmount = span / 360f;
		ring.rectTransform.localRotation = Quaternion.Euler(0f, 0f, span / 2f);
	}

	void CreateSlot(ArcSlot slot)
	{
		switch (slot.Kind)
		{
			case ArcSlotKind.PreviousPage:
				CreateButton("<", null, arrowColor, true, slot.Angle, () => TurnPage(-1));
				break;
			case ArcSlotKind.NextPage:
				CreateButton(">", null, arrowColor, true, slot.Angle, () => TurnPage(1));
				break;
			default:
				var action = actions[slot.ActionIndex];
				CreateButton(action.Label, action.Detail, action.IsAvailable ? buttonColor : unavailableColor,
					action.IsAvailable, slot.Angle, () => Run(action));
				break;
		}
	}

	void CreateButton(string label, string detail, Color color, bool interactable, float angle, UnityEngine.Events.UnityAction onClick)
	{
		var image = OverlayFactory.CreateImage(root, label, UiShapes.OutlinedDisc, Vector2.one * buttonSize, color);
		var button = image.gameObject.AddComponent<Button>();
		button.targetGraphic = image;
		button.interactable = interactable;
		button.onClick.AddListener(onClick);
		var text = detail == null ? label : $"{label}\n<size={fontSize * 3 / 4}>{detail}</size>";
		OverlayFactory.CreateText(image.transform, text, fontSize).supportRichText = true;
		image.rectTransform.anchoredPosition = PositionOnArc(angle, radius);
		AddPart(image.rectTransform);
	}

	Text CreateInfo()
	{
		var pill = OverlayFactory.CreateImage(root, "Info", null, new Vector2(380f, 150f), infoColor);
		pill.raycastTarget = false;
		pill.rectTransform.anchoredPosition = PositionOnArc(0f, radius + buttonSize);
		AddPart(pill.rectTransform);
		pill.gameObject.SetActive(false);
		return OverlayFactory.CreateText(pill.transform, "", fontSize);
	}

	void AddPart(RectTransform part)
	{
		parts.Add(part.gameObject);
		uprightParts.Add(part);
	}

	void TurnPage(int step)
	{
		page = ArcLayout.NormalizePage(shape, slotAngle, actions.Count, page + step);
		Rebuild();
		Place();
	}

	void Run(ContextAction action)
	{
		action.Execute(this);
		if (action.ClosesMenu)
			Close();
	}

	// Angles are clockwise from the menu's up.
	static Vector2 PositionOnArc(float degrees, float distance)
	{
		var radians = degrees * Mathf.Deg2Rad;
		return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) * distance;
	}
}
