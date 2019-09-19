using UnityEngine;

public class FaceAccessory : Accessories 
{
	public Color[] colorList;

	public bool isBeard = false;
	
	public void SetFaceAccessory(SpriteRenderer go)
	{
		CitizenManager.instance.SetSprite(gameObject, go, colorList, accessoryColors);
	}
}
