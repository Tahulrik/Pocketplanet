using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoe : Clothes 
{
	public Color[] colorList;
	
	public void SetShoe(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, clothColors);
	}
}
