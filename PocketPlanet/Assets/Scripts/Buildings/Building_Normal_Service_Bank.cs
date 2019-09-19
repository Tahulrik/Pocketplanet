using System.Collections;
using System.Collections.Generic;
using Citizens;
using UnityEngine;
namespace Buildings
{
	public class Building_Normal_Service_Bank : Building_Normal_Service {

        float MaxMoney = 1000f;

        protected override void Update()
        {
            base.Update();

            if (moneyAmount < MaxMoney)
            {

                moneyAmount += Time.deltaTime * 0.05f;
            }

            moneyAmount = Mathf.Clamp(moneyAmount, 0, MaxMoney);
        }

        public override void BuyItem(CitizenBehaviour customer)
        {
            var boughtItemPrice = (moneyAmount / 100f) * Random.Range(1f, 10f);

            boughtItemPrice = Mathf.Clamp(boughtItemPrice, 30, 100);

            customer.stats.MoneyAmount += boughtItemPrice;
            moneyAmount -= boughtItemPrice;
        }
    }
}
