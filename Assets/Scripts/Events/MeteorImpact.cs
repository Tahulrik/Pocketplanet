using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;
using Citizens.Actions;

public class MeteorImpact : MonoBehaviour {

	public float HitRadius;
	public float BaseDamage = 10;

	[HideInInspector]
	public bool isDestroyed = false;

	bool dealtDamage = false;

	// Use this for initialization
	void Start () {
		dealtDamage = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		//apparently meteor is hitting more than just the ground
		if (!dealtDamage) {
			MeteorHit (col.contacts [0].point);
		}	
	}

	void MeteorHit(Vector2 hitPos)
	{
		dealtDamage = true;

		//Meteor Explosion
		gameObject.GetComponent<Collider2D> ().enabled = false;
		gameObject.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
		gameObject.GetComponent<AudioSource> ().Play ();
		Transform fire = transform.GetChild (0);
		for (int i = 0; i < fire.childCount; i++) {
			fire.GetChild (i).GetComponent<ParticleSystem> ().Stop ();
		}

		Transform explosion = transform.GetChild (1);

		for (int i = 0; i < explosion.childCount; i++) {
			explosion.GetChild (i).GetComponent<ParticleSystem> ().Play ();
		}


		StartCoroutine (WaitDestroy ());

		var hits = Physics2D.OverlapCircleAll (hitPos, HitRadius);
		for (int i = 0; i < hits.Length; i++) {
            //print(hits[i].name);
			var damageable = hits [i].transform.root.GetComponentInChildren<Damageable> ();

			if (damageable != null) {
				damageable.TakeDamage(BaseDamage);
				//Dont do this with a string comparison?
				if (damageable.tag == "Person") {

					CitizenBehaviour person = damageable.GetComponent<CitizenBehaviour>();
					person.CurrentAction = new StunAction (person, person.gameObject, Random.Range (5f, 10f), hitPos, person.CurrentAction);
				}

				if (damageable.tag == "Building") {
					Buildings.Building building = damageable.GetComponent<Buildings.Building> ();

					building.StartFire ();
				}
			}
		}
	}

	IEnumerator WaitDestroy()
	{
		isDestroyed = true;
		gameObject.GetComponent<SpriteRenderer> ().enabled = false;

		yield return new WaitForSeconds (5);

		Destroy (gameObject);
	}
}
