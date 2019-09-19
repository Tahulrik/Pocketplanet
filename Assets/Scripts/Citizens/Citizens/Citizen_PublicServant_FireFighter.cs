using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens.Actions;
using Buildings;

namespace Citizens
{
	public class Citizen_PublicServant_FireFighter : Citizen_PublicServant {

		public ParticleSystem waterParticle;


		void OnEnable()
		{
			Building.OnDestroyed += CheckIfHomeWasDestroyed;
		}

		void Disable()
		{
			Building.OnDestroyed -= CheckIfHomeWasDestroyed;

		}

		protected override void Start ()
		{
			base.Start ();
		}

		void CheckIfHomeWasDestroyed(GameObject GO)
		{
			//print ("building destroyed event");
			try
			{
				
			if (GO == null) {
				if(GO != HomeBuilding.gameObject)
					return;
			}
			//print ("building destroyed was " + GO.name);

			StartCoroutine(GetNewHome ());
			
			}
			catch(MissingReferenceException) {
			}
		}

		IEnumerator GetNewHome()
		{

			yield return new WaitForEndOfFrame ();
			var newHome = GameObject.FindObjectOfType<Building_PublicService_FireDepartment> ();
			//print ("getting new home");
			if (newHome != null) {
				//print ("new home found");
				HomeBuilding = newHome;
			} 
		}

		protected override void Update ()
		{
			base.Update ();
		}

	}	
}

