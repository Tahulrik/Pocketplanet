using PocketPlanet.Planet;
using UnityEngine;

public static class Planet
{
	public static float Radius => LegacyPlanetApi.Radius;

	public static GameObject PlanetObject => LegacyPlanetApi.PlanetObject;

	public static Vector2 FindPointOnSurface(float radius, Vector2 position) =>
		LegacyPlanetApi.FindPointOnSurface(radius, position);

	public static void PlaceObjectOnSurface(GameObject target) => LegacyPlanetApi.PlaceObjectOnSurface(target);

	public static void RotateObjectToSurface(GameObject target) => LegacyPlanetApi.RotateObjectToSurface(target);

	public static Transform GetBlockzillaSpawnpoint(Vector2 position) => LegacyPlanetApi.GetBlockzillaSpawnpoint(position);
}
