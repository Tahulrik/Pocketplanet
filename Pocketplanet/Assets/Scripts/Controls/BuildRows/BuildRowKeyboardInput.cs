using PocketPlanet.Planet;
using UnityEngine;

[RequireComponent(typeof(BuildRowSelection))]
public class BuildRowKeyboardInput : MonoBehaviour
{
	[SerializeField] KeyCode frontKey = KeyCode.Alpha1;
	[SerializeField] KeyCode middleKey = KeyCode.Alpha2;
	[SerializeField] KeyCode backKey = KeyCode.Alpha3;
	[SerializeField] KeyCode cycleKey = KeyCode.Tab;

	BuildRowSelection selection;

	void Awake() => selection = GetComponent<BuildRowSelection>();

	void Update()
	{
		if (Input.GetKeyDown(frontKey))
			selection.Select(SurfaceRow.Front);
		else if (Input.GetKeyDown(middleKey))
			selection.Select(SurfaceRow.Middle);
		else if (Input.GetKeyDown(backKey))
			selection.Select(SurfaceRow.Back);
		else if (Input.GetKeyDown(cycleKey))
			selection.CycleBackward();
	}
}
