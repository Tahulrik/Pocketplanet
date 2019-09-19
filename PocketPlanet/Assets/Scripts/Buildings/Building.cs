using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;
using Citizens.Actions;

namespace Buildings
{
	public enum BuildingSize
	{
		Small = 2, Medium = 1, Large = 0, Special = 3, Store = 4
	}

	[RequireComponent(typeof(BuildingSelectHandler))]
	public abstract class Building : Damageable {

		public delegate void ConstructedBuilding (GameObject GO);
		public delegate void DestroyedBuilding (GameObject GO);

		public delegate void FireEvent(GameObject GO);

		public static event FireEvent FireStarted;
		public static event FireEvent FireStopped;


		public static event ConstructedBuilding OnConstructed;

		public static event DestroyedBuilding OnDestroyed;

		public bool IsOnFire = false;

		public int MaxInhabitants = 5;

		[HideInInspector]
		public List<GameObject> Inhabitants = new List<GameObject>();

		public List<GameObject> PeopleInside = new List<GameObject>();

		public GameObject personPrefab;

		GameObject Fire;
		GameObject Destruction;

		public float CurrentFireHealth;
		public float MaxFireHealth = 100f;
		public int FireDamage = 1;
		[HideInInspector]
		public bool fromConstruction = false;
		protected Animator anim;
		public BuildingSize buildingSize;
        public GameObject SoundHolder;


		protected override void Start () 
	    {
			base.Start ();

			anim = GetComponent<Animator> ();
	        Planet.PlaceObjectOnSurface(gameObject);
	        Planet.RotateObjectToSurface(gameObject);

	        //FinishedConstruction ();

			CurrentFireHealth = MaxFireHealth;

            GameObject firePrefab = Resources.Load<GameObject>("Particles/Fire");
			GameObject destructionPrefab = Resources.Load<GameObject>("Particles/Destruction");
            Collider2D col = GetComponentInChildren<Collider2D>();
			float randHeight = Random.Range(0, col.bounds.size.y / 2);

			GameObject fireInstance = Instantiate(firePrefab, transform.GetChild(0));
			fireInstance.name = "Fire";
			fireInstance.transform.localPosition = new Vector3(0, randHeight, 0);

			GameObject destructionInstance = Instantiate (destructionPrefab, transform.GetChild (0));
			destructionInstance.transform.position = transform.position;
			destructionInstance.transform.rotation = transform.rotation;

			Fire = fireInstance;
			Destruction = destructionInstance;


		}
		
		// Update is called once per frame
		protected virtual void Update () 
		{
			if (!IsOnFire) {
				StopFire ();
			}
		}
			

		public override void TakeDamage(float amount)
		{
			Health -= amount;
				
			if (Health <= 0) {
				Kill ();
				return;
			}
			else if (Health <= (MaxHealth/100)*0.5f) {
				StartFire ();
			}	
		}

		void FinishedConstruction()
		{
			WorldObjects.ActiveBuildings.Add (gameObject);

			if (OnConstructed != null) {
				OnConstructed (gameObject);
			}
		}

		public void Kill(){

			if (OnDestroyed != null) {
				//print ("destroyed " + name);
				OnDestroyed (gameObject);
			}
            Instantiate(SoundHolder,transform.position, Quaternion.identity);


            WorldObjects.ActiveBuildings.Remove (gameObject);
			WorldObjects.Buildings.Remove (gameObject);

			//this probably isnt very fast
			if (WorldObjects.BuildingsOnFire.Contains (gameObject)) {
				WorldObjects.BuildingsOnFire.Remove (gameObject);
			}

			WorldObjects.Targets.Remove (gameObject);

			for (int i = 0; i < PeopleInside.Count; i++) {
				try
				{
					var currentPerson = PeopleInside [i].GetComponent<CitizenBehaviour> ();
					currentPerson.SetPersonActive (true);
					currentPerson.CurrentAction = new StunAction (currentPerson, currentPerson.gameObject, Random.Range (5f, 10f), transform.position, currentPerson.CurrentAction);
					
					PeopleInside.Remove (currentPerson.gameObject);
				}
				catch(System.Exception) {
					continue;
				}

			}


			RemoveCitizens ();

			anim.SetTrigger ("Destroyed");
			Destruction.GetComponent<ParticleSystem> ().Play ();
			IsOnFire = false;
			Destroy (gameObject, 5);
		}

		public void RemoveCitizens()
		{
			
			foreach (GameObject person in Inhabitants) {
				try{
					//Debug.LogWarning (person.name);
					Destroy (person);
				}
				catch(MissingReferenceException) {
					continue;
				}
			}
		
		}
		public void StartFire()
		{
			if (IsOnFire) {
				return;
			}
			if (Fire == null) {
				return;
			}
			for (int i = 0; i < Fire.transform.childCount; i++) {
				var particle = Fire.transform.GetChild (i).GetComponent<ParticleSystem> ();
				particle.Play ();
			}

			WorldObjects.BuildingsOnFire.Add (gameObject);

			Fire.GetComponent<AudioSource> ().Play ();
			IsOnFire = true;
			StartCoroutine (OnFire ());

			//Setting the fire to full health on firestart
			CurrentFireHealth = MaxFireHealth;

			if (FireStarted != null) {
				FireStarted (gameObject);
			}
		}

		public void StopFire()
		{
			WorldObjects.BuildingsOnFire.Remove(gameObject);

			if (FireStopped != null) {
				FireStopped (gameObject);
			}

			for (int i = 0; i < Fire.transform.childCount; i++) {
				var particle = Fire.transform.GetChild (i).GetComponent<ParticleSystem> ();
				particle.Stop ();
			}
			StopCoroutine (OnFire ());
			Fire.GetComponent<AudioSource> ().Stop();
			IsOnFire = false;
		}

		public void RemoveFireHealth(int value)
		{
			CurrentFireHealth -= value * Time.deltaTime;

			if (CurrentFireHealth <= 0) {
				StopFire ();
			}
		}

		IEnumerator OnFire()
		{	
			while (IsOnFire) 
			{
				TakeDamage (FireDamage);
				yield return new WaitForSeconds (2);
			}
		}

		protected virtual void InitializeCitizenStats(CitizenBehaviour citizen)
		{
			citizen.CurrentState = ActionState.Idle;
			citizen.CurrentSubState = SubState.Waiting;
		}

		public void BuildingConstructed()
		{
			//fromConstruction = true;
			SpawnCitizens ();
		}

		protected virtual void SpawnCitizens ()
		{
		}
	}
}
