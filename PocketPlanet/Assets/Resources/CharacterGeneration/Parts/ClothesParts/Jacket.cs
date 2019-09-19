using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jacket : Clothes 
{
	public Color[] colorList;
	public void SetJacket(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, clothColors);
	}
}
