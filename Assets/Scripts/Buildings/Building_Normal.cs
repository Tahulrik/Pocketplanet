using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

namespace Buildings{
	[RequireComponent(typeof(HostileCitizenTarget))]

	public class Building_Normal : Building {

		protected override void Start()
		{
			base.Start ();
		}

		protected override void Update()
		{
			base.Update ();
		}

		protected override void SpawnCitizens()
		{
			
		}
	}

}
