using UnityEngine;

public class BodyAccessory : Accessories 
{
	public bool needSuit;

	public bool needClosedShirt;

	public Color[] colorList;
	public void SetBodyAccessory(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, accessoryColors);
	}
}
