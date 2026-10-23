using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;
using UnityEngine.UI;
/*
[RequireComponent(typeof(LeanSelectable))]
public class BuildingSelectHandler : LeanSelectableBehaviour 
{
	Building thisType;
	UpgradeUIHandler UIHandler;
	LeanTapSelect selected;

	public float xOffset = 0;
	public float yOffset = -0.1f;

	Vector3 tempPos;

	bool isShown;

	GameObject PlanetObject;
	float radius;


	void Start()
	{
		thisType = GetComponent<Building>();
		UIHandler = FindObjectOfType<UpgradeUIHandler>();
		selected = FindObjectOfType<LeanTapSelect>();

		PlanetObject = GameObject.Find("Planet");
		radius = PlanetObject.GetComponent<CircleCollider2D>().radius;

	}

	void Update()
	{
		if (isShown)
		{
			SetUITransform(GameHandler.Instance.ContextMenuPanel.transform, yOffset);
		}
	}

	protected override void OnSelect(LeanFinger finger)
	{
		ShowUI(true);
	}

	protected override void OnDeselect()
	{
		ShowUI(false);
	}

	void ShowUI(bool show)
	{
		if (show)
		{
			tempPos = transform.position;

			isShown = true;
			GameHandler.Instance.UpgradeUI.SetActive(true);
			GameHandler.Instance.DemolishUI.SetActive(true);
			GameHandler.Instance.DemolishUI.GetComponent<Button>().onClick.AddListener(() => { UIHandler.Demolish(selected.CurrentSelectable);});

			if (thisType.buildingSize == BuildingSize.Special
				&& GetComponent<Building_Normal_Service_Store>() == null
				&& GetComponent<Building_Normal_Service_Bank>() == null
				&& GetComponent<Building_Normal_Service_Restaurant>() == null)
			{
				GameHandler.Instance.IncreaseWorkersUI.SetActive(true);
				SetInteractive(UIHandler.spawnCost, GameHandler.Instance.UpgradeUI.GetComponent<Button>());
				SetInteractive(UIHandler.increaseWorkersCost, GameHandler.Instance.IncreaseWorkersUI.GetComponent<Button>());
				GameHandler.Instance.UpgradeUI.GetComponentInChildren<Text>().text = "SPAWN: COST " + UIHandler.spawnCost;
				GameHandler.Instance.UpgradeUI.GetComponent<Image>().sprite = GameHandler.Instance.SpawnSprite;
				GameHandler.Instance.UpgradeUI.GetComponent<Button>().onClick.AddListener(() => { UIHandler.Spawn(selected.CurrentSelectable); });

				if (GetComponent<Building_PublicService_MissileSilo>() == null)
				{
					GameHandler.Instance.IncreaseWorkersUI.GetComponentInChildren<Text>().text = "INCREASE WORKERS: COST " + UIHandler.increaseWorkersCost;
					GameHandler.Instance.IncreaseWorkersUI.GetComponent<Button>().onClick.AddListener(() => { UIHandler.IncreaseWorkerAmount(selected.CurrentSelectable); });
				}
				else
				{ 
					GameHandler.Instance.IncreaseWorkersUI.GetComponentInChildren<Text>().text = "INCREASE MAX MISSILES: COST " + UIHandler.increaseWorkersCost;
					GameHandler.Instance.IncreaseWorkersUI.GetComponent<Button>().onClick.AddListener(() => { UIHandler.IncreaseWorkerAmount(selected.CurrentSelectable); });
				}

			}
			else if (thisType.buildingSize == BuildingSize.Small)
			{
                SetInteractive(UIHandler.smallToMedCost, GameHandler.Instance.UpgradeUI.GetComponent<Button>());
				GameHandler.Instance.UpgradeUI.GetComponentInChildren<Text>().text = "UPGRADE: COST " + UIHandler.smallToMedCost;
				GameHandler.Instance.UpgradeUI.GetComponent<Image>().sprite = GameHandler.Instance.UpgradeSprite;
				GameHandler.Instance.UpgradeUI.GetComponent<Button>().onClick.AddListener(() => { UIHandler.Upgrade(selected.CurrentSelectable); });
			}
			else if (thisType.buildingSize == BuildingSize.Medium)
			{
                SetInteractive(UIHandler.medToLargeCost, GameHandler.Instance.UpgradeUI.GetComponent<Button>());
				GameHandler.Instance.UpgradeUI.GetComponentInChildren<Text>().text = "UPGRADE: COST " + UIHandler.medToLargeCost;
				GameHandler.Instance.UpgradeUI.GetComponent<Image>().sprite = GameHandler.Instance.UpgradeSprite;
				GameHandler.Instance.UpgradeUI.GetComponent<Button>().onClick.AddListener(() => { UIHandler.Upgrade(selected.CurrentSelectable); });
			}
			else
			{
				GameHandler.Instance.UpgradeUI.GetComponent<Button>().onClick.RemoveAllListeners();
				GameHandler.Instance.UpgradeUI.SetActive(false);
				GameHandler.Instance.IncreaseWorkersUI.SetActive(false);
			}
		}
		else
		{
			isShown = false;
			GameHandler.Instance.UpgradeUI.GetComponent<Button>().onClick.RemoveAllListeners();
			GameHandler.Instance.UpgradeUI.SetActive(false);
			GameHandler.Instance.DemolishUI.GetComponent<Button>().onClick.RemoveAllListeners();
			GameHandler.Instance.DemolishUI.SetActive(false);
			GameHandler.Instance.IncreaseWorkersUI.GetComponent<Button>().onClick.RemoveAllListeners();
			GameHandler.Instance.IncreaseWorkersUI.SetActive(false);
		}
	}

	static Vector2 FindPointOnSurface(float radius, Vector2 position)
	{
		GameObject PlanetObject = GameObject.Find("Planet");

		var planetVec = new Vector2(PlanetObject.transform.position.x, PlanetObject.transform.position.y);
		var toVector = (position.normalized * radius) - planetVec;

		var newRes = planetVec + toVector;

		return newRes;	
	}

	void SetUITransform(Transform UITransform, float radiusOffset)
	{ 
		UITransform.position = Camera.main.WorldToScreenPoint(FindPointOnSurface(radius + radiusOffset, tempPos));

		var direction = Camera.main.WorldToScreenPoint(PlanetObject.transform.position) - UITransform.position;
		var lookDir = Quaternion.LookRotation(Vector3.forward, -direction);
		UITransform.rotation = lookDir;

		UITransform.localScale = new Vector3(2, 2, 2) / Camera.main.orthographicSize;
	}

	void SetInteractive(int cost, Button button)
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
*/