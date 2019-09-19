using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;

public class Citizen_Criminal : CitizenBehaviour {

	// Use this for initialization
	protected override void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update ();
	}

	void OnEnable()
	{
		HostileCitizenTarget.BecameTargeted += LostTarget;
	}

	void OnDisable()
	{
		HostileCitizenTarget.BecameTargeted -= LostTarget;
	}

	void LostTarget(GameObject GO)
	{
		if (CurrentAction == null) {
			return;
		}

		if (GO == CurrentAction.CurrentActionTarget) {
			Destroy (gameObject);
		}
	}
}
