using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionTypes;
using Buildings;
using PocketPlanet.Planet;

public class ConstructionHandler : MonoBehaviour {

	public delegate void ConstructionLimiter(bool value);

	public static event ConstructionLimiter AllowBuilding;

	public GameObject[] Buildings;

	public EventCategory Type;

	//TileInfo[] tiles;
	public int MaxBuilding = 40;
	const float MaxPlacementNudge = 1.5f;

	//resources.load these sites
	GameObject[] ConstructionSite;

	void OnEnable()
	{
		//LeanTouch.OnFingerDown += PlaceBuilding;
		Building.OnDestroyed += CheckBuildingAmount;
		Building.OnConstructed += CheckBuildingAmount;


		ObjectSpawner.Released += PlaceBuilding;

	}

	void Awake()
	{
		Type = EventCategory.Building;
		Buildings = Resources.LoadAll<GameObject>("Buildings");
		ConstructionSite = Resources.LoadAll<GameObject> ("ConstructionSites");
	}
	// Use this for initialization
	void Start () {
		//CreateNewBuilding ();
		//StartCoroutine (ContinousBuilding ());

	}

	void CheckBuildingAmount(GameObject GO)
	{
		if (MaxBuilding == 0) {
			AllowBuilding (false);
		}

		if (AllowBuilding != null) {
			if (WorldObjects.ActiveBuildings.Count >= MaxBuilding) {
				AllowBuilding (false);

			} else if (WorldObjects.ActiveBuildings.Count < MaxBuilding) {

                AllowBuilding(true);
				StartCoroutine (ContinousBuilding ());
			}
		}
	}
	


	IEnumerator ContinousBuilding()
	{
		for (int i = WorldObjects.ActiveBuildings.Count; i < MaxBuilding; i++) {
			if (i >= MaxBuilding) {
				StopCoroutine (ContinousBuilding());
				break;
			}
				
			yield return new WaitForSeconds (Random.Range (60, 120));

			if (!CreateNewBuilding ()) {
				i = WorldObjects.ActiveBuildings.Count;
			}

		}
	}

	// Update is called once per frame
	void Update () {


	}
		
	void PlaceBuilding(EventCategory type, Vector2 position, GameObject prefab, bool fromPlayer)
	{
		if (Type != type) {
			return;
		}
		//print (true);

		CreateNewBuildingAtPosition (position, prefab);
	}

	public void CreateNewBuildingAtPosition(Vector2 position, GameObject buildingType)
	{
		TryPlaceConstructionSite(position, buildingType);
	}

	public void CreateNewBuildingAtPosition(Vector2 position, GameObject buildingType, bool intersectionCheck)
	{
		TryPlaceConstructionSite(position, buildingType);
	}

	public void CreateNewBuildingsAtRandomPosition(GameObject buildingType, int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			var placement = FindRandomPlacement(buildingType);
			if (!placement.HasValue)
				return;
			PlaceConstructionSite(buildingType, GetConstructionSiteSize(buildingType), placement.Value);
		}
	}

	public void UpgradeBuilding(BuildingSize initialSize, GameObject newBuilding, ref int localVar, int upgradeAmount)
	{
		Building_Normal_Residence[] allHouses = FindObjectsOfType<Building_Normal_Residence>();

		foreach (Building_Normal_Residence b in allHouses)
		{
			if (b.buildingSize == initialSize && TryUpgrade(b, newBuilding))
			{
				localVar++;
				if (localVar >= upgradeAmount)
					return;
			}
		}
	}

	public void UpgradeSelectedBuilding(Building_Normal_Residence house, GameObject newBuilding)
	{
		TryUpgrade(house, newBuilding);
	}

	public void CreateNewBuildingAtPosition(Vector2 position)
	{
		var prefab = Buildings[Random.Range(0, Buildings.Length)];
		var placement = FindPlacement(position, prefab);
		if (placement.HasValue)
			PlaceBuilding(prefab, placement.Value);
	}

	public bool CreateNewBuilding()
	{
		var prefab = Buildings[Random.Range(0, Buildings.Length)];
		var placement = FindRandomPlacement(prefab);
		if (!placement.HasValue)
			return false;
		PlaceBuilding(prefab, placement.Value);
		return true;
	}

	bool TryUpgrade(Building oldBuilding, GameObject newBuilding)
	{
		var oldOccupant = oldBuilding.GetComponent<PlanetOccupant>();
		var oldPlacement = CurrentPlacement(oldOccupant);
		if (oldOccupant != null)
			oldOccupant.Release();

		var placement = FindPlacement(oldBuilding.transform.position, newBuilding);
		if (!placement.HasValue)
		{
			if (oldPlacement.HasValue)
				oldOccupant.Reserve(oldPlacement.Value);
			return false;
		}

		oldBuilding.GetComponentInChildren<Collider2D>().enabled = false;
		RemoveForUpgradeBuilding(oldBuilding);
		PlaceConstructionSite(newBuilding, GetConstructionSiteSize(newBuilding), placement.Value);
		return true;
	}

	PlacementResult? CurrentPlacement(PlanetOccupant occupant)
	{
		if (occupant == null)
			return null;
		var surface = PlanetHost.Active.Planet.Surface;
		foreach (SurfaceRow row in System.Enum.GetValues(typeof(SurfaceRow)))
		{
			if (surface.Row(row).TryGetRange(occupant, out var range))
				return new PlacementResult(PlacementReason.None, row, range);
		}
		return null;
	}

	bool TryPlaceConstructionSite(Vector2 position, GameObject buildingType)
	{
		var placement = FindPlacement(position, buildingType);
		if (!placement.HasValue)
			return false;
		PlaceConstructionSite(buildingType, GetConstructionSiteSize(buildingType), placement.Value);
		return true;
	}

	PlacementResult? FindPlacement(Vector2 position, GameObject buildingType)
	{
		var planet = PlanetHost.Active.Planet;
		var arc = planet.Frame.ToCoord(position).Arc;
		return planet.Placement.FindNearestValid(PlanetFootprint.For(buildingType), arc, MaxPlacementNudge);
	}

	PlacementResult? FindRandomPlacement(GameObject buildingType)
	{
		var planet = PlanetHost.Active.Planet;
		var startArc = Random.value * planet.Frame.Circumference;
		return planet.Placement.FindAnywhere(PlanetFootprint.For(buildingType), startArc);
	}

	public void PlaceConstructionSite(GameObject buildingPrefab, GameObject constructionSize, PlacementResult placement)
	{
		var newConstructionSite = Instantiate (constructionSize);
		PositionOnSurface(newConstructionSite, placement);
		newConstructionSite.GetComponent<Building_ConstructionSite> ().buildingPrefab = buildingPrefab;
		PlanetOccupant.On(newConstructionSite, OccupantKind.ConstructionSite).Reserve(placement);
	}

	void PlaceBuilding(GameObject buildingPrefab, PlacementResult placement)
	{
		var building = Instantiate (buildingPrefab);
		PositionOnSurface(building, placement);
		PlanetOccupant.On(building, OccupantKind.Building).Reserve(placement);
		WorldObjects.Targets.Add (building);
	}

	static void PositionOnSurface(GameObject target, PlacementResult placement)
	{
		var frame = PlanetHost.Active.Planet.Frame;
		target.transform.position = frame.ToWorld(new PlanetCoord(placement.Range.Center, 0f));
		target.transform.rotation = frame.UprightRotation(placement.Range.Center);
	}

	public GameObject GetConstructionSiteSize(GameObject buildingPrefab)
	{
		var size = buildingPrefab.GetComponent<Building>().buildingSize;

		return ConstructionSite [(int)size];
	}

	public void RemoveForUpgradeBuilding(Building building)
	{
		PlanetOccupant.ReleaseFrom(building.gameObject);
		Destroy (building.gameObject, 5);
		WorldObjects.ActiveBuildings.Remove (building.gameObject);
		WorldObjects.Buildings.Remove (building.gameObject);

		if (WorldObjects.BuildingsOnFire.Contains (building.gameObject)) {
			WorldObjects.BuildingsOnFire.Remove (building.gameObject);
		}

		WorldObjects.Targets.Remove (building.gameObject);

		building.RemoveCitizens ();

	}
}
