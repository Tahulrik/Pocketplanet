using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Citizens
{
	public class Citizen_PublicServant : CitizenBehaviour {

		void Start()
		{
			base.Start ();
		}

		void Update()
		{
			base.Update ();
		}

		protected IEnumerator GetNewHome()
		{

			yield return new WaitForEndOfFrame ();
			var newHome = GameObject.FindObjectOfType<Building_PublicService_FireDepartment> ();
			print ("getting new home");
			if (newHome != null) {
				print ("new home found");
				HomeBuilding = newHome;
			} else {
				print ("no home found");
				Destroy (gameObject);
			}
			//what if there is no more fire departments??
		}
	}	
}

