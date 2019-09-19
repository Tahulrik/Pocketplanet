using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Citizens.Actions
{
	public interface CitizenInteraction {

		CitizenBehaviour GetCitizenScript (GameObject target);
	}
}
