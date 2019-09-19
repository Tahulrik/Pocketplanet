using UnityEngine;

public class HatAccessory : Accessories 
{
	public Color[] colorList;

	public void SetHat(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, accessoryColors);
	}
}
