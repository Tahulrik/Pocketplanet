using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using Buildings;

public class UpgradeUIHandler : MonoBehaviour {

	public int smallToMedCost = 1;
	public int medToLargeCost = 2;
	public int spawnCost = 1;
	public int increaseWorkersCost = 1;


	public void Upgrade(LeanSelectable selected)
	{
		if (selected.GetComponent<Building>().buildingSize == BuildingSize.Small)
		{
			selected.Deselect();
			GameHandler.Instance.ConstructionHandler.UpgradeSelectedBuilding(selected.GetComponent<Building_Normal_Residence>(),
																			 GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)]);
			GameHandler.Instance.SpendGamePoint(smallToMedCost);
		}
		else if (selected.GetComponent<Building>().buildingSize == BuildingSize.Medium)
		{
			selected.Deselect();
			GameHandler.Instance.ConstructionHandler.UpgradeSelectedBuilding(selected.GetComponent<Building_Normal_Residence>(),
																			 GameHandler.Instance.LargeHouses[Random.Range(0, GameHandler.Instance.LargeHouses.Length)]);
			GameHandler.Instance.SpendGamePoint(medToLargeCost);
		}
	}

	public void Spawn(LeanSelectable selected)
	{
		if (selected.GetComponent<Building_PublicService_FireDepartment>() != null)
		{
			var fireDepartment = selected.GetComponent<Building_PublicService_FireDepartment>();
			fireDepartment.DispatchFireFighter(fireDepartment.gameObject, null);
			GameHandler.Instance.SpendGamePoint(spawnCost);
		}
		else if (selected.GetComponent<Building_PublicService_PoliceStation>() != null)
		{
			var policeStation = selected.GetComponent<Building_PublicService_PoliceStation>();
			policeStation.DispatchOfficer(policeStation.gameObject, null);
			GameHandler.Instance.SpendGamePoint(spawnCost);
		}
		else if (selected.GetComponent<Building_PublicService_Hospital>() != null)
		{
			var hospital = selected.GetComponent<Building_PublicService_Hospital>();
			hospital.DispatchParamedic(hospital.gameObject, null);
			GameHandler.Instance.SpendGamePoint(spawnCost);
		}
		else if (selected.GetComponent<Building_PublicService_MissileSilo>() != null)
		{
			var silo = selected.GetComponent<Building_PublicService_MissileSilo>();
			silo.DispatchMissile(null);
			GameHandler.Instance.SpendGamePoint(spawnCost);
		}
		SetButtonInteractive(spawnCost, GameHandler.Instance.UpgradeUI.GetComponent<Button>());
	}

	public void IncreaseWorkerAmount(LeanSelectable selected)
	{ 
		if (selected.GetComponent<Building_PublicService>() != null)
		{
			var department = selected.GetComponent<Building_PublicService>();
			department.MaxWorkers++;
			GameHandler.Instance.SpendGamePoint(increaseWorkersCost);

			SetButtonInteractive(increaseWorkersCost, GameHandler.Instance.IncreaseWorkersUI.GetComponent<Button>());
		}
	}

	public void Demolish(LeanSelectable selected)
	{
		selected.Deselect();
		selected.GetComponent<Building>().Kill();
	}

	void SetButtonInteractive(int cost, Button button)
	{ 
		if (GameHandler.Instance.gamePoints < cost)
		{
			button.interactable = false;
		}
		else
		{
			button.interactable = true;
		}	
	}
}
