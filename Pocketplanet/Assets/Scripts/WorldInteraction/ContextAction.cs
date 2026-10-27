using System;
using System.Collections.Generic;
using UnityEngine;

public interface IContextMenu
{
	void ShowInfo(string text);
	void Close();
}

public interface IContextActionProvider
{
	bool Handles(GameObject target);
	IEnumerable<ContextAction> ActionsFor(GameObject target);
}

public sealed class ContextAction
{
	readonly Action<IContextMenu> execute;

	public ContextAction(string label, Action<IContextMenu> execute, string detail = null, bool available = true, bool closesMenu = true)
	{
		Label = label;
		Detail = detail;
		IsAvailable = available;
		ClosesMenu = closesMenu;
		this.execute = execute;
	}

	public string Label { get; }
	public string Detail { get; }
	public bool IsAvailable { get; }
	public bool ClosesMenu { get; }

	public void Execute(IContextMenu menu) => execute(menu);
}
