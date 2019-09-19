using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

public class EventHandler : MonoBehaviour {
	public GameObject blockzilla;


	public static EventHandler instance;

	public bool BlockzillaActive = false;

	public delegate void BlockzillaEvent();

	public static event BlockzillaEvent BlockzillaSpawned;
	public static event BlockzillaEvent BlockzillaDead;

    public delegate void SendEvent(GameObject spawnBuilding);

    public static event SendEvent CreateRobber;
    public static event SendEvent CreateCivilian;
    public static event SendEvent CreateMafia;




    void OnEnable()
    {
        ObjectSpawner.Released += TriggerBlockZilla;
		ObjectSpawner.Released += TriggerBlockman;
		BlockZillaBehaviour.BlockZillaDeath += BlockzillaDiedEvent;
		BlockZillaBehaviour.BlockZillaDespawn += BlockzillaDiedEvent;
		BlockZillaBehaviour.BlockZillaSpawned += BlockZillaSpawn;

    }

    void OnDisable()
    {
        ObjectSpawner.Released -= TriggerBlockZilla;
		ObjectSpawner.Released -= TriggerBlockman;
		BlockZillaBehaviour.BlockZillaDeath -= BlockzillaDiedEvent;
		BlockZillaBehaviour.BlockZillaDespawn -= BlockzillaDiedEvent;
		BlockZillaBehaviour.BlockZillaSpawned -= BlockZillaSpawn;
    }


    // Use this for initialization
    void Awake () {
		instance = this;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            TriggerRobber();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            TriggerCivilian();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            TriggerMafia();
        }
		if (Input.GetKeyDown (KeyCode.B))
		{
			TriggerBlockZilla (EventCategory.Blockzilla,new Vector2(0f, 0f), blockzilla, false);
		}
    }

    public static void TriggerMafia()
    {
        if (CreateMafia != null)
        {
            var availableBuildings = FindObjectsOfType<Building_Normal_Service_Restaurant_ItalianRestaurant>();
            var spawnBuilding = availableBuildings[Random.Range(0, availableBuildings.Length)].gameObject;

            CreateMafia(spawnBuilding);
        }
    }

    public static void TriggerRobber()
    {
        //print("attempting spawn");
        if (CreateRobber != null)
        {
            var availableBuildings = FindObjectsOfType<Building_Normal_Residence>();
            var spawnBuilding = availableBuildings[Random.Range(0, availableBuildings.Length)].gameObject;
            //print("spawner available");

            CreateRobber(spawnBuilding);
        }
    }

    public static void TriggerCivilian()
    {
        if (CreateCivilian != null)
        {
            var availableBuildings = FindObjectsOfType<Building_Normal_Residence>();
            var spawnBuilding = availableBuildings[Random.Range(0, availableBuildings.Length)].gameObject;

            CreateCivilian(spawnBuilding);
        }
    }

	public static void TriggerBlockZilla(EventCategory type, Vector2 position, GameObject prefab, bool fromPlayer)
    {
        if(type == EventCategory.Blockzilla)
        {

            var blockZilla = Instantiate(prefab);

			var spawnPoint = Planet.GetBlockzillaSpawnpoint (position);

			if (fromPlayer) {
				blockZilla.transform.position = position;
			} else {
				blockZilla.transform.position = spawnPoint.position;
			}	


            Planet.PlaceObjectOnSurface(blockZilla);
            Planet.RotateObjectToSurface(blockZilla);

			blockZilla.GetComponent<BlockZillaBehaviour> ().SpawnPoint = spawnPoint.gameObject;
        }
    }

	public static void TriggerBlockman(EventCategory type, Vector2 position, GameObject prefab, bool fromPlayer)
	{
		if (type == EventCategory.BlockMan) {
			var blockman = Instantiate (prefab);

			Destroy (blockman.GetComponent<SpriteRenderer> ());
			blockman.GetComponent<Citizen> ().CreateSpecial (Special.SpecialType.Blockman);

			var blockmanScript = blockman.AddComponent<Citizen_Blockman> ();

			blockmanScript.RunningSpeed = 16;
			blockmanScript.WalkingSpeed = 8;
			blockmanScript.CurrentMoveSpeed = blockmanScript.WalkingSpeed;

			blockman.transform.position = position;
		}
	}

	public void BlockZillaSpawn(GameObject blockzilla)
	{
		BlockzillaActive = true;
		if (BlockzillaSpawned != null) {
			BlockzillaSpawned ();
		}
	}

	void BlockzillaDiedEvent(GameObject blockzilla)
	{
		if (BlockzillaDead != null) {
			if (!CheckForBlockzillaEvent ()) {
				BlockzillaActive = false;
				BlockzillaDead ();
			}
		}
	}

	public bool CheckForBlockzillaEvent()
	{
		var Blockzillas = GameObject.FindObjectsOfType<BlockZillaBehaviour> ();

		foreach (BlockZillaBehaviour zilla in Blockzillas) {
			if (zilla.Health > 0  && !zilla.isDespawning || zilla.isSpawning) {
				return true;
			} else {
				return false;
			}
		}



		return false;
	}

}
