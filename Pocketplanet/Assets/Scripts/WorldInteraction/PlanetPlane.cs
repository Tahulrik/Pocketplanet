using UnityEngine;

public static class PlanetPlane
{
	public static Vector3 FromScreen(Vector2 screenPosition)
	{
		var ray = Camera.main.ScreenPointToRay(screenPosition);
		var plane = new Plane(Vector3.forward, Planet.PlanetObject.transform.position);
		return plane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : ray.origin;
	}
}
