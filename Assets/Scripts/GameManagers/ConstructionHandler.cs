using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionTypes;
using Buildings;

public class ConstructionHandler : MonoBehaviour {

	public delegate void ConstructionLimiter(bool value);

	public static event ConstructionLimiter AllowBuilding;

	public GameObject[] Buildings;

	public EventCategory Type;

	//TileInfo[] tiles;
	public int MaxBuilding = 40;

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

	public void CreateNewBuildingAtPosition(Vector2 position, GameObject buildingType){
		var planetRadius = Planet.Radius;
		var placementPos = Planet.FindPointOnSurface (planetRadius, position);

		var constructionSiteSize = GetConstructionSiteSize (buildingType);
		//if (Intersecting(placementPos, buildingType) || InWater(placementPos, buildingType)) {
		//	return;
		//}

		PlaceConstructionSite (buildingType, constructionSiteSize, placementPos);
	}

	public void CreateNewBuildingAtPosition(Vector2 position, GameObject buildingType, bool intersectionCheck)
	{
		var planetRadius = Planet.Radius;
		var placementPos = Planet.FindPointOnSurface(planetRadius, position);

		var constructionSiteSize = GetConstructionSiteSize(buildingType);
		if (intersectionCheck)
		{ 
			if (Intersecting(placementPos, buildingType) || InWater(placementPos, buildingType))
			{
				return;
			}
		}

		PlaceConstructionSite(buildingType, constructionSiteSize, placementPos);	
	}

	public void CreateNewBuildingsAtRandomPosition(GameObject buildingType, int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			var planetRadius = Planet.Radius;
			var placementPos = Planet.FindPointOnSurface(planetRadius, Random.insideUnitCircle);
			
			var constructionSiteSize = GetConstructionSiteSize(buildingType);
			if (Intersecting(placementPos, buildingType) || InWater(placementPos, buildingType))
			{
				return;
			}
			
			PlaceConstructionSite(buildingType, constructionSiteSize, placementPos);
		}
	}

	public void UpgradeBuilding(BuildingSize initialSize, GameObject newBuilding, ref int localVar, int upgradeAmount)
	{
		Building_Normal_Residence[] allHouses = FindObjectsOfType<Building_Normal_Residence>();

		foreach (Building_Normal_Residence b in allHouses)
		{
			if (b.buildingSize == initialSize)
			{
				RemoveForUpgradeBuilding (b);
				localVar++;
				b.GetComponentInChildren<Collider2D>().enabled = false;
				Vector2 tempPos = b.transform.position;
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingAtPosition(tempPos, newBuilding, false);
				if (localVar >= upgradeAmount)
					return;
			}
		}
	}
	public void UpgradeSelectedBuilding(Building_Normal_Residence house, GameObject newBuilding)
	{
		house.GetComponentInChildren<Collider2D>().enabled = false;
		Vector2 tempPos = house.transform.position;
		RemoveForUpgradeBuilding (house);
		GameHandler.Instance.ConstructionHandler.CreateNewBuildingAtPosition(tempPos, newBuilding, false);
	}
	public void CreateNewBuildingAtPosition(Vector2 position){
		var planetRadius = Planet.Radius;
		var placementPos = Planet.FindPointOnSurface (planetRadius, position);
		var randomNumber = Random.Range (0, Buildings.Length);


		//if (Intersecting(placementPos, Buildings[randomNumber]) || InWater(placementPos, Buildings[randomNumber])) {
		//	return;
		//}

		var building = Instantiate (Buildings [randomNumber]) as GameObject;

		building.transform.position = placementPos;
		Planet.RotateObjectToSurface (building);

		WorldObjects.Targets.Add (building);

	}

	public bool CreateNewBuilding(){
		
		var planetRadius = Planet.Radius;
		var placementPos = FindRandomPointOnSurface (planetRadius);
		var randomNumber = Random.Range (0, Buildings.Length);

		//if (Intersecting(placementPos, Buildings[randomNumber]) || InWater(placementPos, Buildings[randomNumber])) {
		//	return false;
		//}

		var building = Instantiate (Buildings [randomNumber]) as GameObject;

		building.transform.position = placementPos;

		WorldObjects.Targets.Add (building);
		return true;
	}
		
	void RotateToSurface(Transform GO)
	{
		var GOrot = GO.transform.rotation;

		var dir = GO.position - Planet.PlanetObject.transform.position;

		GOrot = Quaternion.LookRotation (Vector3.forward, dir);

		GO.transform.rotation = GOrot;
	}

	bool Intersecting(Vector2 position, GameObject building)
	{

		var buildingBounds = building.GetComponentInChildren<SpriteRenderer> ().bounds;
		var size = new Vector2 (buildingBounds.extents.x, buildingBounds.extents.y);

		Collider2D[] hits = Physics2D.OverlapCircleAll (position, size.x);
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].gameObject.transform.root.tag == "Building") {
				return true;
			}
		
		}

		return false;
	}

	bool InWater(Vector2 position, GameObject building)
	{
		bool hitWater = false;
		bool hitGround = false;

		var buildingBounds = building.GetComponentInChildren<SpriteRenderer> ().bounds;
		var size = new Vector2 (buildingBounds.extents.x, 0.2f);

		Collider2D[] hits = Physics2D.OverlapCircleAll (position, size.x);

		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].gameObject.tag == "Water") {
				hitWater = true;
			}
			if (hits [i].gameObject.tag == "Ground") {
				hitGround = true;
			}

			if (hitGround && hitWater) {
				return false;
			}

		}

		return true;
	}

	Vector2 FindRandomPointOnSurface(float radius)
	{
		Vector2 spawnPoint;

		var randVal = Random.value;
		var angle = randVal*Mathf.PI*2;

		spawnPoint.x = Mathf.Cos(angle)*radius + Planet.PlanetObject.transform.position.x;
		spawnPoint.y = Mathf.Sin(angle)*radius + Planet.PlanetObject.transform.position.y;

		return spawnPoint;
	}

	public void PlaceConstructionSite(GameObject buildingPrefab, GameObject constructionSize, Vector2 position)
	{
		var newConstructionSite = Instantiate (constructionSize);

		newConstructionSite.transform.position = position;

		Planet.RotateObjectToSurface (newConstructionSite);

		newConstructionSite.GetComponent<Building_ConstructionSite> ().buildingPrefab = buildingPrefab;
	}

	public GameObject GetConstructionSiteSize(GameObject buildingPrefab)
	{
		var size = buildingPrefab.GetComponent<Building>().buildingSize;

		return ConstructionSite [(int)size];
	}

	public void RemoveForUpgradeBuilding(Building building)
	{
		Destroy (building.gameObject, 5);
		WorldObjects.ActiveBuildings.Remove (building.gameObject);
		WorldObjects.Buildings.Remove (building.gameObject);

		//this probably isnt very fast
		if (WorldObjects.BuildingsOnFire.Contains (building.gameObject)) {
			WorldObjects.BuildingsOnFire.Remove (building.gameObject);
		}

		WorldObjects.Targets.Remove (building.gameObject);

		building.RemoveCitizens ();

	}
}
