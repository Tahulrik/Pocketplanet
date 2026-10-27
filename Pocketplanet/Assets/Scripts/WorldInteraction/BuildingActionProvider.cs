using System.Collections.Generic;
using Buildings;
using UnityEngine;

public class BuildingActionProvider : IContextActionProvider
{
	const int SmallToMediumCost = 1;
	const int MediumToLargeCost = 2;

	public bool Handles(GameObject target) => target.GetComponent<Building>() != null;

	public IEnumerable<ContextAction> ActionsFor(GameObject target)
	{
		var house = target.GetComponent<Building_Normal_Residence>();
		if (house != null && house.buildingSize == BuildingSize.Small)
			yield return Upgrade(house, GameHandler.Instance.MediumHouses, SmallToMediumCost);
		else if (house != null && house.buildingSize == BuildingSize.Medium)
			yield return Upgrade(house, GameHandler.Instance.LargeHouses, MediumToLargeCost);

		var building = target.GetComponent<Building>();
		yield return new ContextAction("Demolish", _ => building.Kill());
	}

	static ContextAction Upgrade(Building_Normal_Residence house, GameObject[] options, int cost)
	{
		var affordable = GameHandler.Instance.gamePoints >= cost && options.Length > 0;
		return new ContextAction("Upgrade", _ =>
		{
			GameHandler.Instance.ConstructionHandler.UpgradeSelectedBuilding(house, options[Random.Range(0, options.Length)]);
			GameHandler.Instance.SpendGamePoint(cost);
		}, $"{cost} pt", affordable);
	}
}
