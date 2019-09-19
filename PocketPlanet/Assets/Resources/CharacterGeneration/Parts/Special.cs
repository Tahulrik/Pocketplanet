using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Special : MonoBehaviour 
{
	public enum SpecialType
	{
		BonusCitizen = 1, Police = 2, Firefighter = 3, Doctor = 4, Mafia = 5, Robber = 6, Blockman = 7
	};
	
	Color[] specialColorList = new Color[]
	{
		new Color(1,1,1,1)
	};

	public SpecialType type;

	public void SetSpecial(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, specialColorList, specialColorList);
	}
}
