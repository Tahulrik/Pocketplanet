using System.Collections.Generic;
using Citizens;
using UnityEngine;

public class CitizenActionProvider : IContextActionProvider
{
	public bool Handles(GameObject target) => target.GetComponent<CitizenBehaviour>() != null;

	public IEnumerable<ContextAction> ActionsFor(GameObject target)
	{
		var citizen = target.GetComponent<CitizenBehaviour>();
		yield return new ContextAction("Follow", _ => GameHandler.Instance.CameraController.Follow(citizen.transform));
		yield return new ContextAction("Info", menu => menu.ShowInfo(Describe(citizen)), closesMenu: false);
	}

	static string Describe(CitizenBehaviour citizen)
	{
		var role = RoleName(citizen);
		if (citizen.stats == null)
			return role;
		return $"{role}\n{citizen.stats.CurrentStatus} · {citizen.stats.MoneyAmount:0} money\n{citizen.CurrentState}";
	}

	// Class names look like "Citizen_Criminal_Robber"; the last part is the role.
	static string RoleName(CitizenBehaviour citizen)
	{
		var typeName = citizen.GetType().Name;
		return typeName.Substring(typeName.LastIndexOf('_') + 1);
	}
}
