using Buildings;
using Citizens;
using PocketPlanet.Planet;
using UnityEngine;
using UnityEngine.Rendering;

// Picks by sprite bounds, since runtime-spawned characters have no dependable colliders.
public static class WorldPicker
{
	const int CitizenPriority = 1000;

	public static GameObject Pick(Vector2 worldPoint, float margin)
	{
		GameObject best = null;
		var bestPriority = int.MinValue;

		foreach (var citizen in Object.FindObjectsByType<CitizenBehaviour>(FindObjectsSortMode.None))
			Consider(citizen.gameObject, CitizenPriority, worldPoint, margin, ref best, ref bestPriority);

		foreach (var building in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
			if (IsStanding(building.gameObject) && RowDepthStyle.IsInteractiveObject(building.gameObject))
				Consider(building.gameObject, SortingOrderOf(building.gameObject), worldPoint, margin, ref best, ref bestPriority);

		return best;
	}

	public static Bounds VisibleBounds(GameObject target)
	{
		var bounds = new Bounds(target.transform.position, Vector3.zero);
		foreach (var sprite in target.GetComponentsInChildren<SpriteRenderer>())
			if (sprite.enabled && sprite.sprite != null)
				bounds.Encapsulate(sprite.bounds);
		return bounds;
	}

	static void Consider(GameObject target, int priority, Vector2 point, float margin, ref GameObject best, ref int bestPriority)
	{
		if (priority <= bestPriority || !IsHit(target, point, margin))
			return;
		best = target;
		bestPriority = priority;
	}

	static bool IsHit(GameObject target, Vector2 point, float margin)
	{
		foreach (var sprite in target.GetComponentsInChildren<SpriteRenderer>())
		{
			if (!sprite.enabled || sprite.sprite == null)
				continue;
			var bounds = sprite.bounds;
			bounds.Expand(margin * 2f);
			if (bounds.Contains(new Vector3(point.x, point.y, bounds.center.z)))
				return true;
		}
		return false;
	}

	// Demolished buildings release their spot right away but linger for their collapse animation.
	static bool IsStanding(GameObject building)
	{
		var occupant = building.GetComponent<PlanetOccupant>();
		return occupant == null || occupant.IsReserved;
	}

	static int SortingOrderOf(GameObject target)
	{
		var group = target.GetComponent<SortingGroup>();
		return group != null ? group.sortingOrder : 0;
	}
}
