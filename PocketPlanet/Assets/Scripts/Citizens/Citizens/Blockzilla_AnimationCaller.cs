using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Citizens;
using Citizens.Actions;

public class Blockzilla_AnimationCaller : MonoBehaviour {

	public float StompAttackDamage = 10;
	public float StompAttackRange = 1f;

	ParticleSystem[] particles;


	void Start()
	{
		particles = transform.GetComponentsInChildren<ParticleSystem>();
	}

	public void WalkImpact()
	{
		var hits = Physics2D.OverlapCircleAll(transform.position, StompAttackRange);
		foreach (Collider2D hit in hits)
		{
			var damageable = hit.transform.root.GetComponent<Damageable>();

			if (damageable == null)
			{
				continue;
			}
			if (damageable.GetType() == typeof(BlockZillaBehaviour)) {
				continue;
			}

			if (damageable.GetComponent<CitizenStats>())
			{
				var citizen = damageable.GetComponent<CitizenBehaviour>();
				if (citizen.GetType() == typeof(Citizen_Blockman)) {
					continue;
				}
				var action = citizen.CurrentAction;

				citizen.CurrentAction = new StunAction(citizen, citizen.gameObject, Random.Range(4f, 8f), gameObject.transform.position, action);
			}

			damageable.TakeDamage(StompAttackDamage);
		}

		particles[0].Play();
	}

	public void AttackParticle ()
	{
		particles [1].Play ();
	}
}
