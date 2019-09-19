using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;
using Citizens.Actions;
using Buildings;

public class Building_PublicService_MissileSilo : Building_PublicService {

	public GameObject missilePrefab;

	[HideInInspector]
	public int numberOfMissiles = 0;
	[HideInInspector]
	public List<GameObject> ActiveMissiles;

	Vector2 missileSpawnpos;

	bool lidOpen = false;

	protected override void OnEnable()
	{
		base.OnEnable ();

		MeteorHandler.MeteorFired += DispatchMissile;
	}

	protected override void OnDisable()
	{
		base.OnDisable ();

		MeteorHandler.MeteorFired -= DispatchMissile;
	}

	protected override void Start()
	{
		base.Start ();
		missileSpawnpos = transform.GetChild (3).position;
		ActiveMissiles = new List<GameObject> ();
		anim = GetComponent<Animator> ();
		var meteors = FindObjectOfType<MeteorImpact>();

		if (meteors != null)
		{
			DispatchMissile(meteors.gameObject);
		}
	}

	protected override void Update()
	{
		base.Update ();

	}

	public void DispatchMissile(GameObject target)
	{
		if (!lidOpen) {
			StartCoroutine (LidOpenClose ());
		}



		GameObject missile = null;


		if (ActiveMissiles.Count > 0) {
			missile = ActiveMissiles [0];
		} else if (numberOfMissiles < MaxWorkers) {
			missile = CreateMissile ();
			numberOfMissiles++;
		} else if (ActiveMissiles.Count == 0) {	
			
			return;
		}



		ActiveMissiles.Remove (missile);

	}


	protected GameObject CreateMissile()
	{
		GameObject newMissile = Instantiate (missilePrefab) as GameObject;
		var missileScript = newMissile.GetComponent<MissileBehaviour> ();

		missileScript.HomeBuilding = gameObject.GetComponent<Building>();

		newMissile.transform.position = missileSpawnpos;

		return newMissile;
	}

	IEnumerator LidOpenClose()
	{
		lidOpen = true;
		anim.SetBool("LidOpen", lidOpen);

		yield return new WaitForSeconds (2.5f);
		while (ActiveMissiles.Count != 0) {

			yield return new WaitForSeconds(1.5f);
		}
		lidOpen = false;
		anim.SetBool("LidOpen", lidOpen);

	}
		
}
