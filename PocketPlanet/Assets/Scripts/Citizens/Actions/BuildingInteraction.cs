using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

namespace Citizens.Actions
{
	public interface BuildingInteraction {
		
		Building GetCitizenScript (GameObject target);
	}
	
}