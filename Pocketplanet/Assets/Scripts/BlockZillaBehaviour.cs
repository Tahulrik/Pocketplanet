using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens.Actions;
using Citizens;
using Buildings;

public class BlockZillaBehaviour : Damageable {
    public delegate void BlockZilla(GameObject BlockZilla);

	public static event BlockZilla BlockZillaSpawned;
    public static event BlockZilla BlockZillaDeath;
	public static event BlockZilla BlockZillaDespawn;


    WalkDirection walkDirection = WalkDirection.Stationary;

    GameObject target;


	public float BreathAttackRange = 2f;
	public float BreathAttackDamage = 10;
	public float AttackCooldown = 2f;

    bool isDead = false;

	bool fighting = false;
	[HideInInspector]
	public bool isSpawning = true;
	[HideInInspector]
	public bool isDespawning = false;


	public Citizen_Blockman blockmanTarget;

	[HideInInspector]
	public GameObject SpawnPoint;
    public AudioSource[] audioSources;

	Animator anim;
	Vector2 colliderSize;
	// Use this for initialization
	protected void Start()
	{
		Planet.PlaceObjectOnSurface(gameObject);
		Planet.RotateObjectToSurface(gameObject);

		base.Start();

		Spawn ();

		colliderSize = gameObject.GetComponentInChildren<Collider2D> ().bounds.extents;
		anim = GetComponentInChildren<Animator> ();
		var ZillaSprites = Resources.LoadAll<Sprite>("Graphics/BlockZilla");
		var ZillaSprite = GetComponentInChildren<SpriteRenderer>();

		ZillaSprite.sprite = ZillaSprites[Random.Range(0, ZillaSprites.Length)];


		GetNewDirection();




		audioSources = GetComponents<AudioSource>();
	}

    // Update is called once per frame
    void Update () {

		if (isSpawning || isDespawning) {
			return;
		}

		if (Health <= 0)
		{
			if (!isDead) {
				Die();
			}	
		}

        if(!isDead)
        {
			if (fighting && blockmanTarget == null)
			{
				StopFighting();
			}

			if (fighting) 
			{
				return;
			}
            MoveToTarget(2.5f);
        }
	}

	void Spawn()
	{
		StartCoroutine (ChangeSortingOrder());
		if (BlockZillaSpawned != null) {
			BlockZillaSpawned (gameObject);
		}

	}

	IEnumerator ChangeSortingOrder()
	{
		isSpawning = true;
		var rend = GetComponentInChildren<SpriteRenderer> ();
		rend.sortingOrder = -1;

		yield return new WaitForSeconds (3f);

		isSpawning = false;

		rend.sortingOrder = 1;
	}

    void Die()
    {
        isDead = true;

        if (BlockZillaDeath != null)
        {
            BlockZillaDeath(gameObject);
        }

        audioSources[0].Play();
        anim.SetBool("Dead", isDead);
        Destroy(gameObject, 5f);
    }

	public void Despawn()
	{
		isDespawning = true;
		GetComponentInChildren<SpriteRenderer> ().sortingOrder = -1;

		if (BlockZillaDespawn != null) {
			BlockZillaDespawn (gameObject);
		}

		anim.SetTrigger ("Despawn");

		Destroy (gameObject, 5f);
	}

    public override void TakeDamage(float amount)
    {
        Health -= amount / 5;
    }

   /* private void TakeDamageOverTime()
    {
        Health -= Time.deltaTime * 0.8f;
    }*/



    public GameObject FindClosestTarget(List<GameObject> targets)
    {
        Vector3 currentPos = gameObject.transform.position;

        GameObject tMin = null;
        float minDist = Mathf.Infinity;

        foreach (var target in targets)
        {
            float dist = Vector3.Distance(target.transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = target;
                minDist = dist;
            }
        }

        return tMin;
    }



    protected void FindDirectionToTarget(Vector2 targetPosition)
    {
        float dot = Vector2.Dot(targetPosition, gameObject.transform.right);
        var rotation = gameObject.transform.GetChild(0).localRotation;

        if (dot > 0)
        {
            rotation.eulerAngles = new Vector2(0, 180);
            walkDirection = WalkDirection.Left;
        }
        else
        {
            rotation.eulerAngles = new Vector2(0, 0);

            walkDirection = WalkDirection.Right;
        }

        //no need to get child every frame?
        gameObject.transform.GetChild(0).localRotation = rotation;
    }

    protected void MoveToTarget(float speedModifier)
    {
        //FindDirectionToTarget(target.transform.position);
		anim.SetInteger ("Movement", (int)walkDirection);
        gameObject.transform.RotateAround(Planet.PlanetObject.transform.position, Vector3.forward, (int)walkDirection * Time.deltaTime * speedModifier);
		Planet.RotateObjectToSurface (gameObject);
    }

    protected void GetNewDirection()
    {
		var dot = Vector2.Dot(SpawnPoint.transform.position, transform.right);

		var rotation = gameObject.transform.GetChild(0).localRotation;
		if (dot >= 0)
        {
			rotation.eulerAngles = new Vector2(0, 0);

            walkDirection = WalkDirection.Left;
        }
        else
        {
			rotation.eulerAngles = new Vector2(0, 180f);

            walkDirection = WalkDirection.Right;
        }

		gameObject.transform.GetChild(0).localRotation = rotation;

    }

	public void StartFighting()
	{
		anim.SetInteger ("Movement", 0);
		fighting = true;
		StartCoroutine (Fight ());

	}

	public void StopFighting()
	{
		fighting = false;

	}

	IEnumerator Fight()
	{
		while (fighting) {

			DealDamage ();
			//play animation
			try{
				if (blockmanTarget.stats.Health <= 0) {
					StopFighting ();
				}
			}
			catch(System.Exception){
				//StopFighting ();
			}


			yield return new WaitForSeconds (AttackCooldown);
		}
	}

	void DealDamage()
	{
		anim.SetTrigger("Fighting");
		var attackPos = new Vector2(transform.position.x, transform.position.y + colliderSize.y);

		var hits = Physics2D.OverlapCircleAll (attackPos, BreathAttackRange);
		foreach (var hit in hits) {
			if (!hit.transform.root.GetComponentInChildren<Damageable> ()) {
				continue;
			}
			var damageable = hit.transform.root.GetComponent<Damageable> ();

			if (damageable.GetComponent<BlockZillaBehaviour>()) {

				continue;
			}
			var dot = Vector2.Dot (gameObject.transform.right, hit.gameObject.transform.position);


			var tempDamage = BreathAttackDamage;
			if (damageable.GetComponent<CitizenStats> ()) {
				var citizen = damageable.GetComponent<CitizenBehaviour> ();
				if (citizen.GetType () == typeof(Citizen_Blockman)) {

					tempDamage = tempDamage / 2;
					damageable.TakeDamage (tempDamage);
				}
			} else {
				if (dot < 0) {
					continue;
				}
				damageable.TakeDamage (tempDamage);
			}
			//print (damageable.GetType ().Name);
		}
	}

	public void SetBlockmanTarget(Citizen_Blockman blockman)
	{
		blockmanTarget = blockman;
		FindDirectionToTarget (blockmanTarget.transform.position);
	}
}
