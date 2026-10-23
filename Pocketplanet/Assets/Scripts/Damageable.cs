using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Damageable : MonoBehaviour {

	//These should be changed to ints?
	public float MaxHealth = 100;
	public float Health;

	protected virtual void Start()
	{
		Health = MaxHealth;
	}
		
	public abstract void TakeDamage (float amount);



}
