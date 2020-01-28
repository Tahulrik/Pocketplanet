using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionTypes;
public class MeteorHandler : MonoBehaviour {

	public delegate void MeteorDisaster(GameObject go);

	public static event MeteorDisaster MeteorFired;

	[Tooltip("The meteor object to be spawned")]
	public GameObject meteor;

	float randomSize;
	AudioSource[] sirenSounds;

	void OnEnable()
	{
		ObjectSpawner.Released += RecieveMeteorEvent;
		ObjectSpawner.Released += RecieveMeteorShowerEvent;
	}

	void OnDisable()
	{
		ObjectSpawner.Released -= RecieveMeteorEvent;
		ObjectSpawner.Released -= RecieveMeteorShowerEvent;

	}

	void Start(){
		sirenSounds = GetComponents<AudioSource> ();

	}

	void Update(){

	}

	public void SpawnMeteor(Vector2 position)
	{
		sirenSounds [0].Play ();

		randomSize = Random.Range (0.3f, 2.5f);

		var hitPos = new Vector3(position.x, position.y, 0);
        //hitPos.Normalize ();

		var planetDirection = hitPos - Planet.PlanetObject.transform.position;

        GameObject currentMeteor = Instantiate (meteor) as GameObject;
		currentMeteor.transform.position = planetDirection.normalized * 35;
		currentMeteor.transform.localScale = new Vector3 (randomSize, randomSize, 1);

		Rigidbody2D rigid = currentMeteor.GetComponent<Rigidbody2D> ();
		rigid.AddForce(-planetDirection.normalized*Random.Range(200, 240));
		rigid.AddTorque (Random.Range(2, 10));

		MeteorImpact meteorScript = currentMeteor.GetComponent<MeteorImpact> ();
		meteorScript.BaseDamage = meteorScript.BaseDamage * (randomSize*4);
		meteorScript.HitRadius = 1.0f * randomSize;

		if(MeteorFired != null)
		{
			MeteorFired (currentMeteor);
		}
			
	}

	public void MeteorShower(Vector2 position)
	{
		sirenSounds [1].Play ();
//		Debug.LogWarning ("StartingShower");
		StartCoroutine(WaitForNextMeteor(position));
	}

	IEnumerator WaitForNextMeteor(Vector2 position)
	{
		int meteorAmount = Random.Range (2, 11);

		float showerArea = Random.Range (5f, 10f);

		Vector3 hitPos = new Vector3(position.x, position.y, 0);
		for (int i = 0; i < meteorAmount; i++) {

			Vector2 planetDirection = hitPos - Planet.PlanetObject.transform.position;
			var meteorPos = planetDirection+(Random.insideUnitCircle*showerArea);
			//Debug.DrawRay (Planet.PlanetObject.transform.position, meteorPos, Color.red, 10f);
			SpawnMeteor (meteorPos);

			yield return new WaitForSeconds (Random.Range(0.5f, 2f));
		}
	}

	void RecieveMeteorEvent(EventCategory type, Vector2 position, GameObject prefab, bool fromPlayer)
	{
		if (type == EventCategory.Meteor) {
			SpawnMeteor (position);
		}
	}

	void RecieveMeteorShowerEvent(EventCategory type, Vector2 position, GameObject prefab, bool fromPlayer)
	{
		if (type == EventCategory.MeteorShower) {
			MeteorShower (position);
		}
	}
		
}
