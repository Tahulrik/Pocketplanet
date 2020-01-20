using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;

namespace Buildings
{
	public class Building_Normal_Service_Restaurant_ItalianRestaurant : Building_Normal_Service_Restaurant {

		public int MaxMobsters = 1;
		[HideInInspector]
		public int currentMobsters = 0;

		void OnEnable()
		{
			EventHandler.CreateMafia += SpawnMobster;
           // ObjectSpawner.Released += Spawn;
		}
		
		void OnDisable()
		{
			EventHandler.CreateMafia -= SpawnMobster;
            //ObjectSpawner.Released -= Spawn;
        }

        // Use this for initialization
        protected override void Start () {
			base.Start ();
		}

		// Update is called once per frame
		protected override void Update () {
			base.Update ();
		}

		//make it so a gang of mobsters spawn instead of just 1 ?
		void SpawnMobster(GameObject building)
		{
            if (building == gameObject)
            {
			    currentMobsters++;
			    CreateMafia ();
            }
		}
		
		protected GameObject CreateMafia()
		{
			GameObject newPerson = Instantiate (personPrefab) as GameObject;
			var personScript = newPerson.GetComponent<Citizen> ();
			
			var newMafia = newPerson.AddComponent<Citizen_Criminal_Mafia>();
			newMafia.HomeBuilding = gameObject.GetComponent<Building> ();

			personScript.CreateSpecial (Special.SpecialType.Mafia);
			
			newPerson.transform.position = gameObject.transform.position;
			
			WorldObjects.People.Add (newMafia);
			WorldObjects.Targets.Add (newPerson);
			Inhabitants.Add (newPerson);

            newMafia.GunDamage = 40f;


			return newPerson;
		}

		void Spawn(EventCategory type, Vector2 position, GameObject prefab, bool fromPlayer)
        {
            if (type == EventCategory.Mafia)
            {
                CreateMafia();
            }
        }

    }
}
