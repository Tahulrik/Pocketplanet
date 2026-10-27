using System;
using PocketPlanet.Planet;
using UnityEngine;

public class BuildRowSelection : MonoBehaviour
{
	[SerializeField] SurfaceRow current = SurfaceRow.Front;

	public static BuildRowSelection Active { get; private set; }

	public event Action<SurfaceRow> Changed;

	public SurfaceRow Current => current;

	public static SurfaceRow? ActiveRow => Active != null ? Active.Current : (SurfaceRow?)null;

	public void Select(SurfaceRow row)
	{
		if (row == current)
			return;
		current = row;
		Changed?.Invoke(row);
	}

	public void CycleBackward()
	{
		var rows = SurfaceRows.FrontToBack;
		var index = 0;
		for (int i = 0; i < rows.Count; i++)
			if (rows[i] == Current)
				index = i;
		Select(rows[(index + 1) % rows.Count]);
	}

	void OnEnable() => Active = this;

	void OnDisable()
	{
		if (Active == this)
			Active = null;
	}
}
