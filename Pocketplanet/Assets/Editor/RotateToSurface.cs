using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RotateToSurface : Editor
{
	[MenuItem("Homemades/Place On Surface &p")]
	public static void PlaceObjectOnSurface()
	{
		GameObject PlanetObject = GameObject.Find("Planet");
		float radius = PlanetObject.GetComponent<CircleCollider2D>().radius;

		GameObject[] allSelected = Selection.gameObjects;
		Undo.RegisterCompleteObjectUndo(allSelected, "Undo Place On Surface");

		foreach (GameObject g in allSelected)
		{
			g.transform.position = FindPointOnSurface(radius, g.transform.position);
		}
	}

	[MenuItem("Homemades/Rotate To Surface &r")]
	public static void RotateObjectToSurface()
	{
		GameObject PlanetObject = GameObject.Find("Planet");
		float radius = PlanetObject.GetComponent<CircleCollider2D>().radius;

		GameObject[] allSelected = Selection.gameObjects;
		Undo.RegisterCompleteObjectUndo(allSelected, "Undo Rotate To Surface");

		foreach (GameObject g in allSelected)
		{
			var direction = PlanetObject.transform.position - g.transform.position;
			var lookDir = Quaternion.LookRotation(Vector3.forward, -direction);
			g.transform.rotation = lookDir;
		}
	}

	[MenuItem("Homemades/Enable or Disable Selection &d")]
	public static void EnableDisableGO()
	{
		GameObject[] allSelected = Selection.gameObjects;
		Undo.RegisterCompleteObjectUndo(allSelected, "Undo Enable/Disable");

		foreach (GameObject g in allSelected)
		{
			g.SetActive(!g.activeInHierarchy);
		}
	}

	static Vector2 FindPointOnSurface(float radius, Vector2 position)
	{
		GameObject PlanetObject = GameObject.Find("Planet");

		var planetVec = new Vector2(PlanetObject.transform.position.x, PlanetObject.transform.position.y);
		var toVector = (position.normalized * radius) - planetVec;

		var newRes = planetVec + toVector;

		return newRes;
	}

}
