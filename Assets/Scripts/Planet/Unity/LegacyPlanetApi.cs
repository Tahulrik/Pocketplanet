using System;
using UnityEngine;

namespace PocketPlanet.Planet
{
    // Keeps the old static Planet API working on top of the procedural planet.
    public static class LegacyPlanetApi
    {
        const string SpawnpointName = "BlockzillaSpawnpoint";

        static PlanetHost Host => PlanetHost.Active != null
            ? PlanetHost.Active
            : throw new InvalidOperationException("No PlanetHost in scene.");

        static PlanetFrame Frame => Host.Planet.Frame;

        public static float Radius => Frame.Radius;
        public static GameObject PlanetObject => Host.gameObject;

        public static Vector2 FindPointOnSurface(float radius, Vector2 position)
        {
            var frame = Frame;
            return frame.ToWorld(new PlanetCoord(frame.ToCoord(position).Arc, radius - frame.Radius));
        }

        public static void PlaceObjectOnSurface(GameObject target)
        {
            var position = target.transform.position;
            Vector3 surface = FindPointOnSurface(Radius, position);
            target.transform.position = new Vector3(surface.x, surface.y, position.z);
        }

        public static void RotateObjectToSurface(GameObject target)
        {
            var frame = Frame;
            target.transform.rotation = frame.UprightRotation(frame.ToCoord(target.transform.position).Arc);
        }

        public static Transform GetBlockzillaSpawnpoint(Vector2 position)
        {
            var planet = Host.Planet;
            var arc = planet.Frame.ToCoord(position).Arc;
            var spawnArc = planet.Queries.FindNearestDeepWater(arc) ?? arc;
            var spawnpoint = SpawnpointTransform();
            spawnpoint.position = planet.Frame.ToWorld(new PlanetCoord(spawnArc, 0f));
            spawnpoint.rotation = planet.Frame.UprightRotation(spawnArc);
            return spawnpoint;
        }

        static Transform SpawnpointTransform()
        {
            var host = Host.transform;
            var existing = host.Find(SpawnpointName);
            if (existing != null)
                return existing;
            var spawnpoint = new GameObject(SpawnpointName).transform;
            spawnpoint.SetParent(host, false);
            return spawnpoint;
        }
    }
}
