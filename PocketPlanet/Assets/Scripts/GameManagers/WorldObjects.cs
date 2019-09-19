using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;
using Citizens;
using System.Linq;


public static class WorldObjects {

    public static int maxPeople = 100;

	public static Transform Planet;

	public static List<GameObject> Targets = new List<GameObject>(0);

	public static List<CitizenBehaviour> People = new List<CitizenBehaviour>(0);
	public static List<GameObject> ActivePeople = new List<GameObject>(0);

	public static List<GameObject> Buildings = new List<GameObject> (0);
	public static List<GameObject> ValidTargets = new List<GameObject>(0);


	//Remove active buildings at some point
	public static List<GameObject> ActiveBuildings = new List<GameObject>(0);
	public static List<GameObject> BuildingsOnFire = new List<GameObject>(0);

	public static CitizenBehaviour GetPersonTarget(CitizenBehaviour invoker)
	{
		try{
			var personTargets = new List<GameObject> (0);
			
			for (int i = 0; i < ValidTargets.Count; i++) {
				if (ValidTargets [i].GetComponent<CitizenBehaviour> ()) {
					personTargets.Add (ValidTargets [i]);
				}
			}
			
			if (personTargets.Count == 0) {
				return null;
			}
			
			if(personTargets.Contains(invoker.gameObject))
			{
				personTargets.Remove (invoker.gameObject);
			}
			
			var rand = Random.Range (0, personTargets.Count);
			
			return personTargets [rand].GetComponent<CitizenBehaviour>();
		}
		catch(System.Exception ex) {
			if (ex is MissingReferenceException || ex is System.ArgumentOutOfRangeException) {
				return null;

			}
		}
		return null;
	}

	public static Building GetBuildingTarget(CitizenBehaviour invoker)
	{
		

		try
		{
			var buildingTargets = new List<GameObject> (0);

			for (int i = 0; i < ValidTargets.Count; i++) {
				if (ValidTargets [i].GetComponent<Building> ()) {
					buildingTargets.Add (ValidTargets [i]);
				}
			}

			if(buildingTargets.Count > 0)
			{
				var rand = Random.Range (0, buildingTargets.Count);
				return buildingTargets [rand].GetComponent<Building>();
			}
		}
		catch(System.Exception ex) {
			if (ex is MissingReferenceException || ex is System.ArgumentOutOfRangeException) {
				return null;
			
			}
		}
		return null;
	}

	public static GameObject GetClosestTarget(GameObject Self, List<GameObject> targets)
	{
		GameObject tMin = null;
		float minDist = Mathf.Infinity;
		Vector3 currentPos = Self.transform.position;
		for (int i = 0; i < targets.Count; i++) {
			float dist = Vector3.Distance(targets[i].transform.position, currentPos);
			if (dist < minDist)
			{
				tMin = targets[i];
				minDist = dist;
			}
		}

		return tMin;
	}
}
