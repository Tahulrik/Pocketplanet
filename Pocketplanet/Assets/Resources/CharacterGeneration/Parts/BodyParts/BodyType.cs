using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyType : Bodies 
{
	public Color[] colorList;
	Color[] skinColors = new Color[]
	{
		new Color(255,236,218),
		new Color(251,224,202),
		new Color(227,201,180),
		new Color(179,159,144),
		new Color(239,206,150),
		new Color(136,108,81),
		new Color(240,180,101)
	};
	
	public void SetBodytype(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, skinColors);
			
	}
}
