using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostileCitizenTarget : MonoBehaviour {

	public delegate void CriminalTarget(GameObject GO);

	public static event CriminalTarget BecameTargeted;

	public bool isTargeted = false;
	public bool actionInProgress = false;
	bool isValidTarget;

	public void StartTargeting(bool value)
	{
		isTargeted = value;
	}

	public void StartAction(bool value)
	{
		actionInProgress = value;
	}

	public void SetValidTarget(bool value)
	{
		if (value) {

			if (!isValidTarget) {
				WorldObjects.ValidTargets.Add (gameObject);
				isValidTarget = true;
				if (BecameTargeted != null) {
					BecameTargeted (gameObject);
				}
			}

		} 
		if (!value) {
			if (isValidTarget) {
				WorldObjects.ValidTargets.Remove (gameObject);
				isValidTarget = false;
			}
		}

	}

	public bool GetValidTarget()
	{
		return isValidTarget;
	}
}
