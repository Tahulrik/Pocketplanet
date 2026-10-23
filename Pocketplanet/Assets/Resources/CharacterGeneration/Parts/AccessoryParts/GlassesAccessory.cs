using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassesAccessory : Accessories 
{
	public Color[] colorList;
	public void SetGlasses(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, accessoryColors);
	}
}
