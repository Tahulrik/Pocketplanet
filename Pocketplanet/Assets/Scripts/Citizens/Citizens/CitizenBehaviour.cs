using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Citizens.Actions;
using Buildings;

namespace Citizens
{
	//States need to be multi layered, eg such that you can talk in different way and such that you are unavailble while doing so.
	//Do this as a class with constant ints instead, such that it can be changed in children classes?
	public enum ActionState
	{
		Test,
		Idle,
		Talking,
		FightFire,
		FightCrime,
		HealPerson,
		RobStore,
		RobPerson,
		ExtortStore,
		ExtortPerson
	}

	public enum SubState
	{
		Moving,
		ExecutingAction,
		Waiting
	}

	[RequireComponent (typeof (CitizenStats))]
	[RequireComponent (typeof (PersonMood))]
	[RequireComponent (typeof (CitizenAnimationManager))]


	//Require all the necessary actions for each behaviour type

	public class CitizenBehaviour : MonoBehaviour {

		public ActionState CurrentState;
		public SubState CurrentSubState;
		//Create an inspecter view for this class?
		[HideInInspector]
		public CitizenAction CurrentAction = null;

		public Building HomeBuilding;

		//[HideInInspector]
		public Animator anim;

		public float WalkingSpeed = 1;
		public float RunningSpeed = 1.5f;
		public float CurrentMoveSpeed;
		public float ViewRange = 10f;

		bool isActive = false;
		public bool isRunning = false;
		public bool inBuilding;
		public bool isStunned = false;
		//only neccesarry for lookup methods, if this is not done we cannot use the lookup function
		public bool committedCrime;

		public CitizenStats stats;
		public PersonMood mood;

		void OnEnable()
		{
			Building.OnDestroyed += TargetDestroyed;
		}
		
		void OnDisable()
		{
			Building.OnDestroyed -= TargetDestroyed;
		}
		
		void Awake()
		{
			anim = GetComponentInChildren<Animator> ();
			stats = GetComponent<CitizenStats> ();
			if (SceneManager.GetActiveScene ().name == "CharacterGenerationTest") {
				CurrentState = ActionState.Test;
			}
			else
			{
				CurrentState = ActionState.Idle;
				CurrentAction = null;
				//Only do this for testing purposes
				if (!WorldObjects.ActivePeople.Contains (gameObject)) {
				
					WorldObjects.ActivePeople.Add (gameObject);
				}
			}
			if (CurrentState == ActionState.Test) {
				anim.enabled = false;
			}
		}

		// Use this for initialization
		protected virtual void Start () {
			if (CurrentState != ActionState.Test) {
				Planet.PlaceObjectOnSurface(gameObject);
				Planet.RotateObjectToSurface(gameObject);
			}

			if (!WorldObjects.People.Contains (this)) {
				WorldObjects.People.Add (this);
			}

			CurrentMoveSpeed = WalkingSpeed;

			SetPersonActive (true);
		}

		// Update is called once per frame
		protected virtual void Update () {
			if (CurrentState != ActionState.Test) {
				Planet.RotateObjectToSurface(gameObject);
			}

			if (stats.CurrentStatus == Status.Alive || stats.CurrentStatus == Status.Injured) {
				
				if (CurrentAction != null) {
					//Debug.Log (name + " " + CurrentAction.ToString());
					try
					{
						
						CurrentAction.ExecuteAction ();
					}
					catch(MissingReferenceException) {
                       // Debug.Log("No action to execute " + gameObject.name);
						//Destroy (gameObject);
					}
				}
			}

            //print(name + " has action " + CurrentAction);
		}

		//Find a solution for this problem
		void TargetDestroyed(GameObject GO)
		{
			if (CurrentAction != null) {
				if (GO == CurrentAction.CurrentActionTarget) {
					CurrentAction = new WaitAction(this, gameObject, 5);
					CurrentState = ActionState.Idle;
				}
			} else if (GO.GetComponent<Building> ().IsOnFire) {
				if (WorldObjects.BuildingsOnFire.Contains (GO)) {
					WorldObjects.BuildingsOnFire.Remove (GO);
				}
			}
		}
			
        public void SetPersonActive(bool value)
		{
			if (value) {
				if (!isActive) {
					WorldObjects.ActivePeople.Add (gameObject);
					transform.GetChild (0).gameObject.SetActive (value);			
				}

			} else {
				if (isActive) {
					WorldObjects.ActivePeople.Remove (gameObject);
					transform.GetChild (0).gameObject.SetActive (value);
				}
			}

			isActive = value;
		}

		public virtual void GetNewTask()
		{
            if (stats.MoneyAmount <= 5)
            {
                CurrentAction = new BuyFromStore(this, FindObjectOfType<Building_Normal_Service_Bank>());
                return;
            }


			var rand = Random.Range (1, 3);

			if(rand == 1)
			{
				CurrentState = ActionState.Talking;
            }
            if (rand == 2)
            {
				CurrentState = ActionState.Idle;
            }


        }

		void OnDestroy()
		{
			if (Application.isPlaying) {
				WorldObjects.People.Remove (gameObject.GetComponent<CitizenBehaviour>());

				if(WorldObjects.ActivePeople.Contains(gameObject))
					WorldObjects.ActivePeople.Remove (gameObject);
				
				if(WorldObjects.ValidTargets.Contains(gameObject))
					WorldObjects.ValidTargets.Remove (gameObject);
			}
		}
		
	}
}