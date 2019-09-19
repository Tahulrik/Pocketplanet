using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;

namespace Buildings{
	public class Building_Normal_Residence : Building_Normal {

		public delegate void DispatchedUnit (bool value);

		bool createdRobber = false;


		public int MaxRobbers = 1;
		public int currentRobbers = 0;

		void OnEnable()
		{
            //Listen to crime events

            ObjectSpawner.Released += Spawn;
			EventHandler.CreateCivilian += EventCitizen;
			EventHandler.CreateRobber += EventRobber;
		}

		void OnDisable()
		{
            ObjectSpawner.Released -= Spawn;

            EventHandler.CreateCivilian -= EventCitizen;
			EventHandler.CreateRobber -= EventRobber;
		}

		protected override void Start()
		{
			if (!fromConstruction) {
				SpawnCitizens ();
			}	
			base.Start ();
		}

		protected override void Update ()
		{
			base.Update ();
		}

		protected void CreateRobber()
		{
			GameObject newPerson = Instantiate (personPrefab) as GameObject;
			var personScript = newPerson.GetComponent<Citizen> ();
			var RobberScript = newPerson.AddComponent<Citizen_Criminal_Robber> ();

			personScript.CreateSpecial (Special.SpecialType.Robber);

			RobberScript.HomeBuilding = gameObject.GetComponent<Building> ();

			newPerson.transform.position = gameObject.transform.position;

			PeopleInside.Add (newPerson);

			WorldObjects.People.Add (RobberScript);

			Inhabitants.Add (newPerson);


            RobberScript.RobbingSpeed = 3f;

		}

		protected virtual GameObject CreatePerson()
		{
			//print ("Creating person from: " + gameObject.name);
			int rand = Random.Range(0, 100);

			GameObject newPerson = Instantiate (personPrefab) as GameObject;
			var citizenScript = newPerson.GetComponent<Citizen> ();
			var personScript = newPerson.AddComponent<Citizen_Civilian>();


			var civilian = newPerson.GetComponent<Citizen_Civilian> ();
			//newPerson.AddComponent<HostileCitizenTarget> ();
			//This must be as an event instead
			if(rand == 42)
			{
				citizenScript.CreateSpecial (Special.SpecialType.BonusCitizen);
			}
			else {
				citizenScript.CreateCitizen ();
			}

			PeopleInside.Add (newPerson);

			newPerson.transform.position = gameObject.transform.position;
			personScript.HomeBuilding = this;


			WorldObjects.People.Add (civilian);
			WorldObjects.Targets.Add (newPerson);

			Inhabitants.Add (newPerson);

			return newPerson;
		}
			
		void EventCitizen(GameObject building)
		{

			print ("Running event");
            if (building == gameObject)
            {
                if (Inhabitants.Count < MaxInhabitants)
                {
                    CreatePerson();
                }
            }
		}

		void EventRobber(GameObject building)
		{
            if(building == gameObject)
			    CreateRobber ();
		}

		void Spawn(EventCategory type, Vector2 position, GameObject prefab, bool fromPlayer)
        {
            if(type == EventCategory.Robber)
            {
                CreateRobber();
            }
        }
			
		void InitializeCitizenStats(CitizenBehaviour citizen)
		{
			citizen.stats.MaxHealth = 25;
			citizen.stats.Health = citizen.stats.MaxHealth;
			citizen.stats.MoneyAmount = 50;

			citizen.HomeBuilding = gameObject.GetComponent<Building_Normal_Residence>();


		}

		protected override void SpawnCitizens()
		{
			base.SpawnCitizens ();
			for (int i = 0; i < MaxInhabitants; i++)
			{
				if(WorldObjects.People.Count < WorldObjects.maxPeople)
					//All people should start inside buildings
					//print ("Spawn Citizen from " + gameObject.transform.position);
				
					//if a person dies, building should create a new one, (as long as it is not on fire?)
					CreatePerson ();
			}
		}

	}
}

