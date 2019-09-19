using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;
using Citizens.Actions;

public class Building_PublicService_FireDepartment : Building_PublicService {

	int numberOfFirefighters = 0;
	[HideInInspector]
	public List<GameObject> InactiveFireFighters;

	protected override void OnEnable()
	{
		base.OnEnable ();
		SendUnit += DispatchFireFighter;
	}

	protected override void OnDisable()
	{
		base.OnDisable ();
		SendUnit -= DispatchFireFighter;

	}



    protected override void Start()
	{
		base.Start ();

		InactiveFireFighters = new List<GameObject> ();

        DispatchFireFighter(gameObject, GetNewTarget(WorldObjects.BuildingsOnFire));
    }

	protected override void Update()
	{
		base.Update ();
	}



    public void DispatchFireFighter(GameObject department, GameObject target)
	{
		if (department != gameObject) {
			return;
		}

		if (WorldObjects.BuildingsOnFire.Count > 0) {
			GameObject fireFighter = null;
			
			if (InactiveFireFighters.Count > 0) {
				fireFighter = InactiveFireFighters [0];
			} else if (numberOfFirefighters < MaxWorkers) {
				fireFighter = CreatePerson ();
				numberOfFirefighters++;
			} else if (InactiveFireFighters.Count == 0) {
				return;
			}

			InactiveFireFighters.Remove (fireFighter);

			var fighterScript = fireFighter.GetComponent<Citizen_PublicServant_FireFighter>();
			fighterScript.CurrentState = ActionState.FightFire;
				
			fighterScript.CurrentAction = new FireFightAction(fighterScript, target);

		}
	}
		
	protected GameObject CreatePerson()
	{
		GameObject newPerson = Instantiate (personPrefab) as GameObject;
		var personScript = newPerson.GetComponent<Citizen> ();
		var firefighterScript = newPerson.AddComponent<Citizen_PublicServant_FireFighter>();

		InitializeCitizenStats (firefighterScript);

		personScript.CreateSpecial (Special.SpecialType.Firefighter);

		firefighterScript.waterParticle = newPerson.GetComponentInChildren<ParticleSystem> ();
		newPerson.transform.position = gameObject.transform.position;

		WorldObjects.People.Add (firefighterScript);
		WorldObjects.Targets.Add (newPerson);

		InactiveFireFighters.Add (newPerson);

		Inhabitants.Add (newPerson);

		return newPerson;
	}

	protected override void InitializeCitizenStats(CitizenBehaviour citizen)
	{
		citizen.stats.MaxHealth = 25;
		citizen.stats.Health = citizen.stats.MaxHealth;
		citizen.stats.MoneyAmount = 50;

		citizen.HomeBuilding = gameObject.GetComponent<Building_PublicService_FireDepartment>();
	}

}
