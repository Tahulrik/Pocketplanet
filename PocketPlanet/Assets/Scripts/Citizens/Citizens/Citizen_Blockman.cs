using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens.Actions;
using Citizens;
using Buildings;

public class Citizen_Blockman : CitizenBehaviour {


	bool moveIdle = false;
	bool fighting = false;
	bool endingfight = false;

	public float currentOrbitDistance = 0;
	public float AttackCooldown = 2;
	public float AttackDamage = 60f;
	Vector3 startPosition;

	public BlockZillaBehaviour currentTarget;

	WalkDirection walkDirection = WalkDirection.Stationary;

	ParticleSystem moveParticle;

	void OnEnable()
	{

		EventHandler.BlockzillaSpawned += BlockzillaSpawned;
		BlockZillaBehaviour.BlockZillaDeath += CheckForNewBlockzilla;


	}

	void OnDisable()
	{
		EventHandler.BlockzillaSpawned -= BlockzillaSpawned;
		BlockZillaBehaviour.BlockZillaDeath -= CheckForNewBlockzilla;


	}

	// Use this for initialization
	protected override void Start () {
		base.Start ();

		Planet.PlaceObjectOnSurface (gameObject);
		Planet.RotateObjectToSurface (gameObject);
		startPosition = gameObject.transform.position + transform.up * 2;

		gameObject.transform.position = startPosition;

		currentOrbitDistance = Vector3.Distance (gameObject.transform.position, Planet.PlanetObject.transform.position);

		moveParticle = GetComponentInChildren<ParticleSystem> ();
		moveParticle.Play ();

		CheckForBlockzilla ();

	}
	
	// Update is called once per frame
	protected override void Update () {
		if (stats.Health <= 0)
		{
			Destroy(gameObject);
		}

		if (fighting) {
			return;
		}
		if (endingfight) {
			return;
		}


		if (moveIdle) {
			MoveIdle (CurrentMoveSpeed);

		} else if(!moveIdle && currentTarget != null) {
			MoveToBlockzilla (CurrentMoveSpeed);
		}
	}

	void MoveToBlockzilla(float speedModifier)
	{
		var blockzillaDestination = currentTarget.transform.position + currentTarget.transform.right;
		var dist = Vector2.Distance(gameObject.transform.position, blockzillaDestination);

		if (dist >= 3) {
			MoveIdle (CurrentMoveSpeed);
		} else {
			gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, blockzillaDestination,  Time.deltaTime);
			Planet.RotateObjectToSurface(gameObject);

			//Find threshold such that it fits with the target
			if (dist < 0.1f)
			{
				walkDirection = WalkDirection.Stationary;
				StartFight ();
			}
		}



	}

	protected void MoveIdle(float speedModifier)
	{
		currentOrbitDistance = Mathf.Clamp (currentOrbitDistance, 0.1f, Mathf.Infinity);
		var desiredPosition = gameObject.transform.position.normalized * currentOrbitDistance;

		gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, desiredPosition,  Time.deltaTime);
		gameObject.transform.RotateAround(Planet.PlanetObject.transform.position, Vector3.forward, (int)walkDirection * Time.deltaTime * speedModifier);
		Planet.RotateObjectToSurface(gameObject);
	}

	void CheckForNewBlockzilla(GameObject blockzilla)
	{
		CheckForBlockzilla ();
	}

	void BlockzillaSpawned()
	{
		if (fighting) {
			return;
		}

		CheckForBlockzilla ();



		moveIdle = false;
	}
	public BlockZillaBehaviour FindClosestBlockzilla(BlockZillaBehaviour[] targets, BlockZillaBehaviour currentTarget)
	{
		Vector3 currentPos = gameObject.transform.position;

		BlockZillaBehaviour tMin = null;
		float minDist = Mathf.Infinity;
		 
		foreach (var target in targets) {
			if (currentTarget == target) {
				continue;
			}

			float dist = Vector3.Distance (target.transform.position, currentPos);
			if (dist < minDist) {
				tMin = target;
 				minDist = dist;
			}
		}
		return tMin;
	}




	void CheckForBlockzilla()
	{
		if (currentTarget != null)
			return;
		
		if (EventHandler.instance.CheckForBlockzillaEvent ()) {
			moveIdle = false;
			currentTarget = FindClosestBlockzilla (GameObject.FindObjectsOfType<BlockZillaBehaviour> (), currentTarget);
			FindDirectionToTarget (currentTarget.transform.localPosition);
			CurrentMoveSpeed = RunningSpeed;

		} else {
			moveIdle = true;
			GetNewDirection ();
			currentTarget = null;
			CurrentMoveSpeed = WalkingSpeed;
		}


	}

	IEnumerator Fight()
	{
	 	
		while (fighting) {
			DealDamage ();
			anim.SetBool ("ExecutingAction", true);
			//print(gameObject.name + "Delt Damage");
			yield return new WaitForSeconds (AttackCooldown);

			anim.SetBool ("ExecutingAction", false);

			yield return new WaitForSeconds (0.5f);
		}


	}



	public void DealDamage()
	{
		try
		{
			currentTarget.TakeDamage (AttackDamage);
			//print (currentTarget.Health);
			if (currentTarget.Health <= 0) {
				StartCoroutine(StopFighting());
			}
			
		}
		catch(System.Exception) {
			StartCoroutine(StopFighting());
		}

	}

	IEnumerator StopFighting()
	{
		endingfight = true;
		yield return new WaitForSeconds (3f);
		anim.SetInteger("Movement", 0);
		anim.SetBool ("ExecutingAction", false);
		endingfight = false;
		moveParticle.Play ();
		fighting = false;

	}
	void StartFight()
	{
		if (!fighting) {
			moveParticle.Stop ();
			fighting = true;
			StartCoroutine (Fight());
			currentTarget.StartFighting ();
			currentTarget.SetBlockmanTarget (this);
			FindDirectionToTarget (currentTarget.transform.position);
		}
	}

	protected void FindDirectionToTarget(Vector2 targetPosition)
	{
		//print ("finding direction to blockzilla");
		Vector2 heading = targetPosition - (Vector2)transform.localPosition;
		var dirNum = AngleDir(transform.forward, heading, transform.up);

		var rotation = gameObject.transform.GetChild(0).localRotation;
		//print (dirNum);
		if (dirNum > 0)
		{
			//print ("going right");
			rotation.eulerAngles = new Vector2(0, 0);
			walkDirection = WalkDirection.Right;
		}
		else if(dirNum < 0)
		{
//			print ("going left");
			rotation.eulerAngles = new Vector2(0, 180);
			walkDirection = WalkDirection.Left;

		}

		//no need to get child every frame?
		gameObject.transform.GetChild(0).localRotation = rotation;
	}

	protected void GetNewDirection()
	{
		print ("finding random direction");
		var rand = Random.value;
		var rotation = gameObject.transform.GetChild(0).localRotation;

		if (rand >= 0.5)
		{
			rotation.eulerAngles = new Vector2(0, 180);

			walkDirection = WalkDirection.Left;
		}
		else
		{
			rotation.eulerAngles = new Vector2(0, 0);

			walkDirection = WalkDirection.Right;
		}
		gameObject.transform.GetChild(0).localRotation = rotation;

		int movement = (int)walkDirection;
		anim.SetInteger("Movement", movement);
	}

	float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
		Vector3 perp = Vector3.Cross(fwd, targetDir);
		float dir = Vector3.Dot(perp, up);

		if (dir > 0f) {
			return 1f;
		} else if (dir < 0f) {
			return -1f;
		} else {
			return 0f;
		}
	}

}
