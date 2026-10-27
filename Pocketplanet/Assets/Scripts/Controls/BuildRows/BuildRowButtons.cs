using System.Collections.Generic;
using PocketPlanet.Planet;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BuildRowSelection))]
public class BuildRowButtons : MonoBehaviour
{
	static readonly SurfaceRow[] TopToBottom = { SurfaceRow.Back, SurfaceRow.Middle, SurfaceRow.Front };

	[SerializeField] Sprite buttonBackground;
	[SerializeField] Vector2 buttonSize = new Vector2(190f, 96f);
	[SerializeField] float spacing = 14f;
	[SerializeField] Vector2 screenAnchor = new Vector2(1f, 0.5f);
	[SerializeField] Vector2 offset = new Vector2(-36f, 0f);
	[SerializeField] int fontSize = 34;
	[SerializeField] int canvasSortingOrder = 50;
	[SerializeField] Color selectedColor = new Color(1f, 0.83f, 0.25f);
	[SerializeField] Color idleColor = new Color(1f, 1f, 1f, 0.75f);

	readonly Dictionary<SurfaceRow, Image> backgrounds = new Dictionary<SurfaceRow, Image>();
	BuildRowSelection selection;

	void Awake()
	{
		selection = GetComponent<BuildRowSelection>();
		var canvas = BuildRowButtonFactory.CreateOverlay(transform, canvasSortingOrder);
		var column = BuildRowButtonFactory.CreateColumn(canvas, screenAnchor, offset, spacing);
		foreach (var row in TopToBottom)
			AddButton(column, row);
	}

	void OnEnable()
	{
		selection.Changed += Highlight;
		Highlight(selection.Current);
	}

	void OnDisable() => selection.Changed -= Highlight;

	void AddButton(RectTransform column, SurfaceRow row)
	{
		var button = BuildRowButtonFactory.CreateButton(column, LabelFor(row), buttonSize, buttonBackground, fontSize);
		button.onClick.AddListener(() => selection.Select(row));
		backgrounds[row] = button.GetComponent<Image>();
	}

	void Highlight(SurfaceRow current)
	{
		foreach (var pair in backgrounds)
			pair.Value.color = pair.Key == current ? selectedColor : idleColor;
	}

	string LabelFor(SurfaceRow row)
	{
		var name = row.ToString();
		return Application.isMobilePlatform ? name : $"{name}  {KeyHint(row)}";
	}

	static int KeyHint(SurfaceRow row) =>
		row == SurfaceRow.Front ? 1 : row == SurfaceRow.Middle ? 2 : 3;
}
