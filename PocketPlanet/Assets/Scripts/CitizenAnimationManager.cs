using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;

public class CitizenAnimationManager : MonoBehaviour {

	CitizenBehaviour behaviour;
	// Use this for initialization
	void Start () {
		behaviour = GetComponent<CitizenBehaviour> ();	
	}
	
	// Update is called once per frame
	void Update () {
		if (behaviour.stats.CurrentStatus == Status.Alive) {

			if (behaviour.anim.isInitialized) {
				behaviour.anim.SetBool ("IsIncapacitated", false);
				behaviour.anim.SetBool ("IsDead", false);

			}

			switch (behaviour.CurrentSubState) {
			case SubState.Moving:
				//how the f do you modify playback speed
				//var state = animator.GetCurrentAnimatorStateInfo (0);
				//state.speed = Self.CurrentMoveSpeed;
				int movement = (int)behaviour.CurrentMoveSpeed;
				behaviour.anim.SetInteger ("Movement", movement);
				break;
			case SubState.Waiting:
				//wait anim play
				break;
			case SubState.ExecutingAction:
				//executing action play
				break;
			}

		} 
		else if(behaviour.stats.CurrentStatus == Status.Incapacitated)
		{
			if (behaviour.anim.isInitialized) {
				behaviour.anim.SetBool ("IsIncapacitated", true);
			}
		}
		else if(behaviour.stats.CurrentStatus == Status.Dead)
		{
			if (behaviour.anim.isInitialized) {
				behaviour.anim.SetBool ("IsDead", true);

			}
		}
	}
}
