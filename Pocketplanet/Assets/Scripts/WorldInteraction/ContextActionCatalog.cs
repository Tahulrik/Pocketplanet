using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContextActionCatalog
{
	readonly List<IContextActionProvider> providers = new List<IContextActionProvider>
	{
		new BuildingActionProvider(),
		new CitizenActionProvider()
	};

	public List<ContextAction> For(GameObject target) =>
		providers.Where(p => p.Handles(target)).SelectMany(p => p.ActionsFor(target)).ToList();

	public bool HasActions(GameObject target) => providers.Any(p => p.Handles(target));
}
