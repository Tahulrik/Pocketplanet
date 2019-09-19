using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;
using Buildings;

namespace Citizens.Actions
{
	public abstract class MafiaAction : CitizenAction {

		public delegate void MafiaCrime (GameObject GO);

		protected float ExtortionWaitTime = 5f;
		protected float RobbedMoney = 0f;

		protected ParticleSystem Gunparticle;


	}
}
