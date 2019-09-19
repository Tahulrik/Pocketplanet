using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;
using Citizens;
using Citizens.Actions;

public abstract class Building_PublicService : Building
{
	public delegate void UnitDispatcher(GameObject department, GameObject target);

	public static event UnitDispatcher SendUnit;

	public int MaxWorkers = 1;

	protected virtual void OnEnable()
	{
		FireStarted += DispatchFromFireDepartment;

		ExtortPersonAction.ExtortingPerson += DispatchFromPoliceStation;
		ExtortStoreAction.ExtortingStore += DispatchFromPoliceStation;

		RobPersonAction.RobbingPerson += DispatchFromPoliceStation;
		RobStoreAction.RobbingStore += DispatchFromPoliceStation;

		CitizenStats.Injured += DispatchFromHospital;
		CitizenStats.Incapacitated += DispatchFromHospital;
		CitizenStats.Dead += DispatchFromHospital;
	}

	protected virtual void OnDisable()
	{
		FireStarted -= DispatchFromFireDepartment;

		ExtortPersonAction.ExtortingPerson -= DispatchFromPoliceStation;
		ExtortStoreAction.ExtortingStore -= DispatchFromPoliceStation;

		RobPersonAction.RobbingPerson -= DispatchFromPoliceStation;
		RobStoreAction.RobbingStore -= DispatchFromPoliceStation;

		CitizenStats.Injured -= DispatchFromHospital;
		CitizenStats.Incapacitated -= DispatchFromHospital;
		CitizenStats.Dead -= DispatchFromHospital;
	}

	protected override void Start()
	{
		base.Start ();

		buildingSize = BuildingSize.Special;
	}

    protected GameObject GetNewTarget(List<GameObject> targets)
    {

        Vector3 currentPos = gameObject.transform.position;

        GameObject tMin = null;
        float minDist = Mathf.Infinity;

        foreach (var target in targets)
        {
            float dist = Vector3.Distance(target.transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = target;
                minDist = dist;
            }
        }

        if (targets.Count == 0)
        {
            return null;
        }

        return tMin;

    }

    protected void DispatchFromHospital(GameObject target)
	{
		var hospitals = FindObjectsOfType<Building_PublicService_Hospital> ();

		var nearest = FindClosestDepartment (hospitals, target);

		if (SendUnit != null) {
			SendUnit (nearest, target);
		}
	}

	protected void DispatchFromFireDepartment(GameObject target)
	{
		var fireDepartments = FindObjectsOfType<Building_PublicService_FireDepartment> ();

		var nearest = FindClosestDepartment (fireDepartments, target);

		if (SendUnit != null) {
			SendUnit (nearest, target);
		}
	}

	protected void DispatchFromPoliceStation(GameObject target)
	{
		var policeStations = FindObjectsOfType<Building_PublicService_PoliceStation> ();

		var nearest = FindClosestDepartment (policeStations, target);

		if (SendUnit != null) {
			SendUnit (nearest, target);
		}
	}

	protected GameObject FindClosestDepartment(Building_PublicService[] departments, GameObject target)
	{
		Vector3 currentPos = target.transform.position;

		GameObject tMin = null;
		float minDist = Mathf.Infinity;

		foreach (var department in departments) {
			float dist = Vector3.Distance (department.gameObject.transform.position, currentPos);
			if (dist < minDist) {
				tMin = department.gameObject;
				minDist = dist;
			}
		}

		return tMin;
	}
}
