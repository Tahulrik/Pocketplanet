using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Citizens;
using Citizens.Actions;

public class Building_PublicService_PoliceStation : Building_PublicService {

	int numberOfOfficers = 0;
	[HideInInspector]
	public List<GameObject> InactiveOfficers;

	protected override void OnEnable()
	{
		base.OnEnable ();
		SendUnit += DispatchOfficer;
	}

	protected override void OnDisable()
	{
		base.OnDisable ();

		SendUnit -= DispatchOfficer;
	}

	protected override void Start()
	{
		base.Start ();

		InactiveOfficers = new List<GameObject> ();
        var criminals = FindObjectsOfType<Citizen_Criminal>();
        try
        {
            DispatchOfficer(gameObject, criminals[Random.Range(0, criminals.Length)].gameObject);

        }
        catch (System.Exception)
        { }
    }

    protected override void Update()
	{
		base.Update ();
	}

	public void DispatchOfficer(GameObject department, GameObject target)
	{
		if (department != gameObject) {
			return;
		}

        if (target == null)
        {
            return;
        }

        GameObject officer = null;

		if (InactiveOfficers.Count > 0) {
			officer = InactiveOfficers [0];
		} else if (numberOfOfficers < MaxWorkers) {
			officer = CreatePerson ();
			numberOfOfficers++;
		} else if (InactiveOfficers.Count == 0) {					
			return;
		}

		InactiveOfficers.Remove (officer);


		var officerScript = officer.GetComponent<Citizen_PublicServant_PoliceOfficer>();
		officerScript.CurrentState = ActionState.FightCrime;

		officerScript.CurrentAction = new CatchCriminal(officerScript, target.GetComponent<CitizenBehaviour>());
	}

	protected GameObject CreatePerson()
	{
		GameObject newPerson = Instantiate (personPrefab) as GameObject;
		var personScript = newPerson.GetComponent<Citizen> ();
		var personPolice = newPerson.AddComponent<Citizen_PublicServant_PoliceOfficer>();
		personScript.CreateSpecial (Special.SpecialType.Police);

		newPerson.GetComponent<Citizen_PublicServant_PoliceOfficer> ();
		personPolice.HomeBuilding = gameObject.GetComponent<Building_PublicService_PoliceStation>();

		newPerson.transform.position = gameObject.transform.position;

		WorldObjects.People.Add (personPolice);
		WorldObjects.Targets.Add (newPerson);
		Inhabitants.Add (newPerson);
		InactiveOfficers.Add (newPerson);

		return newPerson;
	}


}
