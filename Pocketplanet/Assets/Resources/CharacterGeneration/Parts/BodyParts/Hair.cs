using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hair : Bodies 
{
	public Color[] colorList;
	Color[] hairColors = new Color[]
	{
		new Color(229,153,92),
		new Color(249,254,135),
		new Color(42,43,50),
		new Color(123,98,71)
	};

	public bool hatPossible;

	
	public void SetHair(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, hairColors);
	}
}
