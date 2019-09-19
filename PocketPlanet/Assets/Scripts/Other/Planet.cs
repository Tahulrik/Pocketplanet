using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Planet : MonoBehaviour {

	public static float Radius;
	public static GameObject PlanetObject;

	// Use this for initialization
	void Awake() {
		Radius = GetComponentInChildren<CircleCollider2D> ().radius;
		PlanetObject = gameObject;


	}

	public static Vector2 FindPointOnSurface(float radius, Vector2 position)
	{
		var planetVec = new Vector2 (PlanetObject.transform.position.x, PlanetObject.transform.position.y);
		var toVector = (position.normalized*radius) - planetVec;

		var newRes = planetVec + toVector;

		return newRes;
	}

    public static void PlaceObjectOnSurface(GameObject GO)
    {
        GO.transform.position = Planet.FindPointOnSurface(Planet.Radius, GO.transform.position);
    }

    public static void RotateObjectToSurface(GameObject GO)
    {
		var direction = PlanetObject.transform.position - GO.transform.position;
        var lookDir = Quaternion.LookRotation(Vector3.forward, -direction);
        GO.transform.rotation = lookDir;
    }

	public static Transform GetBlockzillaSpawnpoint(Vector2 position)
	{
		var spawnpoints = PlanetObject.transform.GetChild (1).GetComponentsInChildren<BlockzillaSpawnpoint>();
		Vector3 currentPos = position;

		Transform tMin = null;
		float minDist = Mathf.Infinity;

		foreach (var target in spawnpoints)
		{
			float dist = Vector3.Distance(target.transform.position, currentPos);
			if (dist < minDist)
			{
				tMin = target.transform;
				minDist = dist;
			}
		}

		print(tMin.name);

		return tMin;
	}

}
