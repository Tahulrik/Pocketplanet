using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Citizens
{
	using Actions;

	public enum Status
	{
		Alive = 4,
		Arrested = 3,
		Injured = 1,
		Incapacitated = 0,
		Dead = 2
	}

	public class CitizenStats : Damageable {

		public delegate void CallParamedic(GameObject GO);

		public static event CallParamedic Injured;
		public static event CallParamedic Incapacitated;
		public static event CallParamedic Dead;



		public Status CurrentStatus = Status.Alive;

		public float DeathLimit = -25;

		public float MoneyAmount = 100;


		float timer = 0;
		float deathTimer = 1;
		float deathTimerDamage = 1;


		protected override void Start()
		{
			base.Start ();

		}

		void Update()
		{
			if (Input.GetKeyDown (KeyCode.Space)) {
				TakeDamage (10);
			}
			if (CurrentStatus == Status.Incapacitated) {
				timer += Time.deltaTime;
				if (timer >= deathTimer) {
					Health -= deathTimerDamage;
					if (Health == DeathLimit) {
						if (Dead != null) {
							Dead (gameObject);
						}
						CurrentStatus = Status.Dead;
					}
					timer = 0;
				}
			} else {
				Health = Mathf.Clamp (Health, DeathLimit, MaxHealth);
			}
		}

		public override void TakeDamage(float amount)
		{
			Health = Mathf.Clamp (Health - amount, 0, MaxHealth);
			var injuredLimit = (MaxHealth / 100f) * 30;
			if (Health == 0)  {
				if (Incapacitated != null) 
				{
					//print (gameObject.name + " Incapacitated");
					Incapacitated (gameObject);
				}
				CurrentStatus = Status.Incapacitated;

			}
			else if(Health <= injuredLimit){
				if (Injured != null) {
					//print (gameObject.name + " Injured");
					Injured (gameObject);
				} 
				CurrentStatus = Status.Injured;


			}
			else 
			{
				CurrentStatus = Status.Alive;
			}
		}

		public void CheckHealthStatus()
		{
			var injuredLimit = (MaxHealth / 100f) * 30;
			if (Health == 0) {
				//print (gameObject.name + " Injured");
				CurrentStatus = Status.Incapacitated;
			} else if (Health <= injuredLimit) {
				//print (gameObject.name + " Incapacitated");
				CurrentStatus = Status.Injured;
			} else if (Health == DeathLimit) {
				//print (gameObject.name + " Dead");
				CurrentStatus = Status.Dead;	
			} else {
				//print (gameObject.name + " Is Fine");
				CurrentStatus = Status.Alive;
			}
		}

	}
}