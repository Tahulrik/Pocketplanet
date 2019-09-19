using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

public class MissileBehaviour : MonoBehaviour {

	[HideInInspector]
	public Building HomeBuilding;
	
	private GameObject target;

	private ParticleSystem[] trailParticles;
	private ParticleSystem[] explosionParticles;

	private bool foundFirstTarget = false;
	bool launched = false;

	public float rotationSpeed = 2;
	public float speed = 1;
	public float maxSpeed = 10;
	public float speedIncrease = 2;
	public float destroyAfter = 10;


	SpriteRenderer rend;

	// Use this for initialization
	void Start () 
	{
		rend = GetComponent<SpriteRenderer> ();
		trailParticles = transform.GetChild(0).GetComponentsInChildren<ParticleSystem>();
		explosionParticles = transform.GetChild(1).GetComponentsInChildren<ParticleSystem>();

		SetLayerSortingOrder (-4);
		StartCoroutine (StartLaunch (2));

	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		if (!launched) {
			return;
		}

		if (foundFirstTarget && target == null)
		{
			GetNewTarget();
		}
			

		if (target != null && !target.GetComponent<MeteorImpact>().isDestroyed)
		{
			var dir = target.transform.position - transform.position;
			var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;

			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotationSpeed * Time.deltaTime);
			transform.position = Vector3.MoveTowards(transform.position, transform.up * 100, Time.deltaTime* speed);
			speed = Mathf.Clamp(speed + Time.deltaTime * speedIncrease, 1, maxSpeed);
		}

		else
		{
			target = null;
			transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.up * 100, Time.deltaTime * speed);
			speed = Mathf.Clamp(speed + Time.deltaTime * speedIncrease, 1, maxSpeed);
		}
	}

	IEnumerator LookForTarget()
	{
		if (target == null || target.GetComponent<MeteorImpact>().isDestroyed)
		{ 
			yield return new WaitForSeconds(1f);
			try
			{
				target = FindObjectOfType<MeteorImpact>().gameObject;
				foundFirstTarget = true;
				SetLayerSortingOrder(1);
			}
			catch (System.Exception)
			{
				foundFirstTarget = false;
				StopAllCoroutines();
				StartCoroutine(LookForTarget());
			}
			yield break;
		}
	}

	void GetNewTarget()
	{ 
		try
		{
			target = FindObjectOfType<MeteorImpact>().gameObject;
		}
		catch (System.Exception e)
		{
		}
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		if (col.gameObject.layer == LayerMask.NameToLayer("Meteor"))
		{
			rend.enabled = false;
			GetComponent<Collider2D> ().enabled = false;
			foreach (ParticleSystem p in trailParticles)
			{
				p.Stop();
			}
			foreach (ParticleSystem p in explosionParticles)
			{
				p.Play();
			}

			Destroy(gameObject, 1);
		}
	}

	void OnDestroy()
	{
		try
		{
			HomeBuilding.GetComponent<Building_PublicService_MissileSilo>().numberOfMissiles--;
			HomeBuilding.GetComponent<Building_PublicService_MissileSilo>().ActiveMissiles.Remove(gameObject);
		}
		catch (System.Exception)
		{
			return;
		}
	}

	void SetLayerSortingOrder(int layerval)
	{
		rend.sortingOrder = layerval;
		trailParticles[0].GetComponent<Renderer> ().sortingOrder = layerval;
		trailParticles[1].GetComponent<Renderer> ().sortingOrder = layerval;
	}

	IEnumerator StartLaunch(float waitTime)
	{

		Planet.RotateObjectToSurface (gameObject);


		yield return new WaitForSeconds (waitTime);

		StartCoroutine(LookForTarget());
		Destroy(gameObject, destroyAfter);
		launched = true;
	}
}
