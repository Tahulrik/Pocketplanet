using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pants : Clothes 
{
	public Color[] colorList;
	public void SetPants(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, clothColors);
	}	
}
