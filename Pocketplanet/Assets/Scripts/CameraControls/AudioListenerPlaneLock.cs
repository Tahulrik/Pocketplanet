using UnityEngine;

// 3D sounds are placed on the planet's plane, so the listener must sit on that plane
// rather than at the camera, whose distance from the plane depends on the camera rig.
[RequireComponent(typeof(AudioListener))]
public class AudioListenerPlaneLock : MonoBehaviour
{
	void LateUpdate()
	{
		var planet = Planet.PlanetObject;
		if (planet == null)
			return;

		var position = transform.position;
		position.z = planet.transform.position.z;
		transform.position = position;
	}
}
