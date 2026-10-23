using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;
using Citizens;

public class Building_Normal_Service : Building_Normal {

	public float moneyAmount = 100;
	HostileCitizenTarget targetScript;

    //add sound and particle when character purchases
   // AudioSource purchaseSound;
    //ParticleSystem purchaseParticle;

	void OnEnable()
	{
		
	}

	void OnDisable()
	{

	}

	protected override void Start()
	{
		base.Start ();

		targetScript = GetComponent<HostileCitizenTarget> ();

		buildingSize = BuildingSize.Special;

		targetScript.SetValidTarget (true);


	}

    public virtual void BuyItem(CitizenBehaviour customer)
    {
        var boughtItemPrice = (customer.stats.MoneyAmount / 100f) * Random.Range(1f, 15f);
        if (customer.stats.MoneyAmount >= boughtItemPrice)
        {
            moneyAmount += boughtItemPrice;
            customer.stats.MoneyAmount -= boughtItemPrice;
            //add happiness to costumer
        }

    }

    protected override void Update()
	{
		base.Update();

		if (targetScript.GetValidTarget ()) {
			if (moneyAmount <= 0) {
				targetScript.SetValidTarget (false);
			} else if (targetScript.isTargeted) {
				targetScript.SetValidTarget (false);
			}
			else if(targetScript.actionInProgress)
			{
				targetScript.SetValidTarget (false);
			}
		} else {
			if (moneyAmount > 0) {
				if (!targetScript.actionInProgress && !targetScript.isTargeted) {
					targetScript.SetValidTarget (true);
				}
			} 
		}
	}
		

}
