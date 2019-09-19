using UnityEngine;

public class Suit : Clothes 
{
	public Color[] colorList;

	public void SetSuit(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, clothColors);
	}
}
