using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CitizenCreator : MonoBehaviour 
{
	int currentBodyType = 0, currentEyes = 0, currentHair = 0;

	int currentShirt = 0, currentPants = 0, currentShoe = 0, currentJacket = 0, currentSuit = 0;

	int currentHatSprite = 0, currentGlassesSprite = 0, currentBodyAccessorySprite = 0, currentFaceAccessorySprite = 0;

	Toggle bodyTypeToggle, eyesToggle, hairToggle;

	Toggle shirtToggle, pantsToggle, shoeToggle, jacketToggle, suitToggle;

	Toggle hatToggle, glassesToggle, bodyAccessoryToggle, faceAccessoryToggle;

	SpriteRenderer bodyType, eyes, hair;
	
	SpriteRenderer shirt, pants, shoe, jacket, suit;

	SpriteRenderer hat, glasses, bodyAccessory, faceAccessory;

	void Awake()
	{
		GetAllRefs();
	}

	void Start()
	{
		SetDefaultCitizen();
	}

	public void SwitchBodyTypeSprite(bool direction)
	{
		bodyType.sprite = SwitchSprite(direction, ref currentBodyType, CitizenManager.instance.allBodytypes.Length, CitizenManager.instance.allBodytypes[currentBodyType].GetComponent<SpriteRenderer>().sprite);
	}
	public void SwitchEyesSprite(bool direction)
	{
		eyes.sprite = SwitchSprite(direction, ref currentEyes, CitizenManager.instance.allEyes.Length, CitizenManager.instance.allEyes[currentEyes].GetComponent<SpriteRenderer>().sprite);
	}
	public void SwitchHairSprite(bool direction)
	{
		hair.sprite = SwitchSprite(direction, ref currentHair, CitizenManager.instance.allHair.Length, CitizenManager.instance.allHair[currentHair].GetComponent<SpriteRenderer>().sprite);
	}
	public void SwitchShirtSprite(bool direction)
	{
		shirt.sprite = SwitchSprite(direction, ref currentShirt, CitizenManager.instance.allShirts.Length, CitizenManager.instance.allShirts[currentShirt].GetComponent<SpriteRenderer>().sprite);
	}
	public void SwitchPantsSprite(bool direction)
	{
		pants.sprite = SwitchSprite(direction, ref currentPants, CitizenManager.instance.allPants.Length, CitizenManager.instance.allPants[currentPants].GetComponent<SpriteRenderer>().sprite);
	}
	public void SwitchShoeSprite(bool direction)
	{
		shoe.sprite = SwitchSprite(direction, ref currentShoe, CitizenManager.instance.allShoes.Length, CitizenManager.instance.allShoes[currentShoe].GetComponent<SpriteRenderer>().sprite);
	}
	public void SwitchJacketSprite(bool direction)
	{
		jacket.sprite = SwitchSprite(direction, ref currentJacket, CitizenManager.instance.allJackets.Length, CitizenManager.instance.allJackets[currentJacket].GetComponent<SpriteRenderer>().sprite);
	}
	public void SwitchSuitSprite(bool direction)
	{
		suit.sprite = SwitchSprite(direction, ref currentSuit, CitizenManager.instance.allSuits.Length, CitizenManager.instance.allSuits[currentSuit].GetComponent<SpriteRenderer>().sprite);
	}
	public void SwitchBodyAccessorySprite(bool direction)
	{
		bodyAccessory.sprite = SwitchSprite(direction, ref currentBodyAccessorySprite, CitizenManager.instance.allBodyAccessories.Length, CitizenManager.instance.allBodyAccessories[currentBodyAccessorySprite].GetComponent<SpriteRenderer>().sprite);
	}
	public void SwitchFaceAccessorySprite(bool direction)
	{
		faceAccessory.sprite = SwitchSprite(direction, ref currentFaceAccessorySprite, CitizenManager.instance.allFaceAccessories.Length, CitizenManager.instance.allFaceAccessories[currentFaceAccessorySprite].GetComponent<SpriteRenderer>().sprite);
	}
	public void SwitchHatSprite(bool direction)
	{
		hat.sprite = SwitchSprite(direction, ref currentHatSprite, CitizenManager.instance.allHats.Length, CitizenManager.instance.allHats[currentHatSprite].GetComponent<SpriteRenderer>().sprite);
	}
	public void SwitchGlassesSprite(bool direction)
	{
		bodyAccessory.sprite = SwitchSprite(direction, ref currentBodyAccessorySprite, CitizenManager.instance.allBodyAccessories.Length, CitizenManager.instance.allBodyAccessories[currentBodyAccessorySprite].GetComponent<SpriteRenderer>().sprite);
	}

	public void EnableHair(GameObject go)
	{
		ToggleSprite(go, hairToggle);
	}
	public void EnableShirt(GameObject go)
	{
		ToggleSprite(go, shirtToggle);
	}
	public void EnablePants(GameObject go)
	{
		ToggleSprite(go, pantsToggle);
	}
	public void EnableJacket(GameObject go)
	{
		ToggleSprite(go, jacketToggle);
	}
	public void EnableSuit(GameObject go)
	{
		ToggleSprite(go, suitToggle);
	}
	public void EnableBodyAccessory(GameObject go)
	{
		ToggleSprite(go, bodyAccessoryToggle);
	}
	public void EnableFaceAccessory(GameObject go)
	{
		ToggleSprite(go, faceAccessoryToggle);
	}
	public void EnableHat(GameObject go)
	{
		ToggleSprite(go, hatToggle);
	}
	public void EnableGlasses(GameObject go)
	{
		ToggleSprite(go, glassesToggle);
	}

	void GetAllRefs()
	{
		hairToggle = GameObject.Find("HairToggle").GetComponent<Toggle>();
		shirtToggle = GameObject.Find("ShirtToggle").GetComponent<Toggle>();
		pantsToggle = GameObject.Find("PantsToggle").GetComponent<Toggle>();
		jacketToggle = GameObject.Find("JacketToggle").GetComponent<Toggle>();
		suitToggle = GameObject.Find("SuitToggle").GetComponent<Toggle>();
		hatToggle = GameObject.Find("HatToggle").GetComponent<Toggle>();
		glassesToggle = GameObject.Find("GlassesToggle").GetComponent<Toggle>();
		bodyAccessoryToggle = GameObject.Find("BodyAccessoryToggle").GetComponent<Toggle>();
		faceAccessoryToggle = GameObject.Find("FaceAccessoryToggle").GetComponent<Toggle>();

		bodyType = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
		eyes = transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
		hair = transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();

		shirt = transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>();
		pants = transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();
		shoe = transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<SpriteRenderer>();
		jacket = transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<SpriteRenderer>();
		suit = transform.GetChild(0).GetChild(1).GetChild(4).GetComponent<SpriteRenderer>();

		hat = transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<SpriteRenderer>();
		glasses = transform.GetChild(0).GetChild(2).GetChild(1).GetComponent<SpriteRenderer>();
		bodyAccessory = transform.GetChild(0).GetChild(2).GetChild(2).GetComponent<SpriteRenderer>();
		faceAccessory = transform.GetChild(0).GetChild(2).GetChild(3).GetComponent<SpriteRenderer>();
	}

	void SetDefaultCitizen()
	{
		bodyType.sprite = CitizenManager.instance.allBodytypes[0].GetComponent<SpriteRenderer>().sprite;
		eyes.sprite = CitizenManager.instance.allEyes[0].GetComponent<SpriteRenderer>().sprite;
		hair.sprite = CitizenManager.instance.allHair[0].GetComponent<SpriteRenderer>().sprite;

		shirt.sprite = CitizenManager.instance.allShirts[0].GetComponent<SpriteRenderer>().sprite;
		pants.sprite = CitizenManager.instance.allPants[0].GetComponent<SpriteRenderer>().sprite;
		shoe.sprite = CitizenManager.instance.allShoes[0].GetComponent<SpriteRenderer>().sprite;
		jacket.sprite = CitizenManager.instance.allJackets[0].GetComponent<SpriteRenderer>().sprite;
		suit.sprite = CitizenManager.instance.allSuits[0].GetComponent<SpriteRenderer>().sprite;

		hat.sprite = CitizenManager.instance.allHats[0].GetComponent<SpriteRenderer>().sprite;
		glasses.sprite = CitizenManager.instance.allGlasses[0].GetComponent<SpriteRenderer>().sprite;
		bodyAccessory.sprite = CitizenManager.instance.allBodyAccessories[0].GetComponent<SpriteRenderer>().sprite;
		faceAccessory.sprite = CitizenManager.instance.allFaceAccessories[0].GetComponent<SpriteRenderer>().sprite;

		suit.gameObject.SetActive(false);
		suitToggle.isOn = false;
	}

	Sprite SwitchSprite(bool direction, ref int spriteInt, int spriteIntLength, Sprite spriteManager)
	{
		Sprite tempSprite;
		if(!direction)
		{
			spriteInt--;
			if(spriteInt < 0)
			{
				spriteInt = spriteIntLength - 1;
			}
			tempSprite = spriteManager;
		}
		else
		{
			spriteInt++;
			if(spriteInt >= spriteIntLength)
			{
				spriteInt = 0;
			}
			tempSprite = spriteManager;
		}
		return tempSprite;
	}

	void ToggleSprite(GameObject go, Toggle toggle)
	{
		if(toggle.isOn)
		{
			go.SetActive(true);
		}
		else
		{
			go.SetActive(false);
		}
	}
}
