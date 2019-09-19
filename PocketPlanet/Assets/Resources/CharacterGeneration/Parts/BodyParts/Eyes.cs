using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eyes : Bodies 
{
	public Color[] colorList;
	Color[] eyeColors = new Color[]
	{
		new Color(126,182,236),
		new Color(175,145,114),
		new Color(194,174,85),
		new Color(163,220,148),
		new Color(149,221,193)
	};
	
	public void SetEyes(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, eyeColors);	
	}
}
