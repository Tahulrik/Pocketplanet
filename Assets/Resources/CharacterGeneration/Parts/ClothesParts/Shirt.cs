using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shirt : Clothes 
{
	public Color[] colorList;

	public enum ShirtType
	{
		open, closed
	};

	public ShirtType shirtType;

	public void SetShirt(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, clothColors);
	}
}
