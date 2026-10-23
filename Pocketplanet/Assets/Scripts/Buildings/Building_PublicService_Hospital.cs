using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;
using Citizens.Actions;
using Buildings;

public class Building_PublicService_Hospital : Building_PublicService {

	int numberOfParamedics = 0;
	[HideInInspector]
	public List<GameObject> InactiveParamedics;


	protected override void OnEnable()
	{
		base.OnEnable ();

		SendUnit += DispatchParamedic;
	}

	protected override void OnDisable()
	{
		base.OnDisable ();

		SendUnit -= DispatchParamedic;
	}

	protected override void Start()
	{
		base.Start ();

		InactiveParamedics = new List<GameObject> ();

		DispatchParamedic(gameObject, null);
	}

	protected override void Update()
	{
		base.Update ();
	}

	public void DispatchParamedic(GameObject department, GameObject target)
	{
        if (department != gameObject)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

		GameObject paramedic = null;

		if (InactiveParamedics.Count > 0) {
			paramedic = InactiveParamedics [0];
		} else if (numberOfParamedics < MaxWorkers) {
			paramedic = CreatePerson ();
			numberOfParamedics++;
		} else if (InactiveParamedics.Count == 0) {					
			return;
		}

		InactiveParamedics.Remove (paramedic);

		var paramedicScript = paramedic.GetComponent<Citizen_PublicServant_Paramedic>();
		paramedicScript.CurrentState = ActionState.HealPerson;

		paramedicScript.CurrentAction = new HealPerson(paramedicScript, target);
	}


	protected GameObject CreatePerson()
	{
		GameObject newPerson = Instantiate (personPrefab) as GameObject;
		var personScript = newPerson.GetComponent<Citizen> ();
		var paramedicScript = newPerson.AddComponent<Citizen_PublicServant_Paramedic>();
		personScript.CreateSpecial (Special.SpecialType.Doctor);

		paramedicScript.HomeBuilding = gameObject.GetComponent<Building>();

		newPerson.transform.position = gameObject.transform.position;

		WorldObjects.People.Add (paramedicScript);
		WorldObjects.Targets.Add (newPerson);
		Inhabitants.Add (newPerson);

		return newPerson;
	}
		
}
