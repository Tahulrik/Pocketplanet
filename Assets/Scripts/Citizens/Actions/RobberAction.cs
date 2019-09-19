using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;
using Buildings;

namespace Citizens.Actions
{
	public abstract class RobberAction : CitizenAction {

		public delegate void RobberCrime (GameObject GO);

		public float RobAmount = 50f;
		public float RobbedMoney = 0f;

		void OnEnable()
		{
			Building.OnDestroyed += CheckIfTargetWasDestroyed;
		}

		void Disable()
		{
			Building.OnDestroyed -= CheckIfTargetWasDestroyed;

		}

		void CheckIfTargetWasDestroyed(GameObject GO)
		{
			if (GO == Self.HomeBuilding) {
				MonoBehaviour.Destroy (Self.gameObject);
			}
		}


	}

}
