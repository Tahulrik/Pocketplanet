using UnityEngine;
using System.Collections;

public class Accessories : MonoBehaviour 
{
	protected Color[] accessoryColors = new Color[] 
	{
		new Color(74,121,247),
		new Color(246,74,74),
		new Color(158,215,127),
		new Color(57,62,80),
		new Color(42,45,58),
		new Color(187,214,226),
		new Color(181,181,181),
		new Color(255,253,230),
		new Color(70,70,70),
		new Color(132,152,172),
		new Color(109,125,141),
		new Color(231,205,162),
		new Color(154,154,154),
	};

	public enum Gender
	{
		male, female, all
	};
	public enum Age
	{
		infant, young, adult, old, all
	};

	[RangeAttribute(0, 100)]
	public float spawnRateInPercent;

	public Gender gender;

	int bodyAccTries = 0;

	void Start()
	{
		AssignLayers();
	}
	public IEnumerator AssignHat()
	{
		int randomHat = Random.Range(0, CitizenManager.instance.allHats.Length);
		int chance = Random.Range(0, 101);
		if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allHats[randomHat].gender.ToString() || CitizenManager.instance.allHats[randomHat].gender.ToString() == "all")
		{
			if(chance <= CitizenManager.instance.allHats[randomHat].spawnRateInPercent)
			{
				CitizenManager.instance.allHats[randomHat].SetHat(transform.GetChild(0).GetComponent<SpriteRenderer>());		
				StopAllCoroutines();
				yield break;	
			}
			else
			{
				transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
				StopAllCoroutines();
				yield break;
			}
		}
		else
		{
			StartCoroutine(AssignHat());
			yield return null;
		}
	}
	public IEnumerator AssignGlasses()
	{
		int randomGlasses = Random.Range(0, CitizenManager.instance.allGlasses.Length);
		int chance = Random.Range(0, 101);
		if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allGlasses[randomGlasses].gender.ToString() || CitizenManager.instance.allGlasses[randomGlasses].gender.ToString() == "all")
		{
			if(chance <= CitizenManager.instance.allGlasses[randomGlasses].spawnRateInPercent)
			{
				CitizenManager.instance.allGlasses[randomGlasses].SetGlasses(transform.GetChild(1).GetComponent<SpriteRenderer>());
				StopAllCoroutines();
				yield break;
			}
			else
			{
				transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
				StopAllCoroutines();
				yield break;
			}
		}
		else
		{
			StartCoroutine(AssignGlasses());
			yield return null;
		}
	}
	public IEnumerator AssignBodyAccessory()
	{
		if(transform.parent.GetChild(1).GetComponent<Clothes>().shirtAssigned)
		{
			int randomBodyAccessory = Random.Range(0, CitizenManager.instance.allBodyAccessories.Length);
			int chance = Random.Range(0, 101);
			if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allBodyAccessories[randomBodyAccessory].gender.ToString() || CitizenManager.instance.allBodyAccessories[randomBodyAccessory].gender.ToString() == "all")
			{
				if(chance <= CitizenManager.instance.allBodyAccessories[randomBodyAccessory].spawnRateInPercent)
				{
					if(CitizenManager.instance.allBodyAccessories[randomBodyAccessory].needSuit && transform.parent.GetChild(1).GetComponent<Clothes>().suitOn)
					{
						transform.GetChild(2).GetComponent<Renderer>().sortingOrder += 6;
						CitizenManager.instance.allBodyAccessories[randomBodyAccessory].SetBodyAccessory(transform.GetChild(2).GetComponent<SpriteRenderer>());
						StopAllCoroutines();
						yield break;
					}
					else if(CitizenManager.instance.allBodyAccessories[randomBodyAccessory].needClosedShirt && transform.parent.GetChild(1).GetComponent<Clothes>().shirtTypeString == "closed" && !CitizenManager.instance.allBodyAccessories[randomBodyAccessory].needSuit
					|| !CitizenManager.instance.allBodyAccessories[randomBodyAccessory].needClosedShirt && transform.parent.GetChild(1).GetComponent<Clothes>().shirtTypeString == "open" && !CitizenManager.instance.allBodyAccessories[randomBodyAccessory].needSuit)
					{
						CitizenManager.instance.allBodyAccessories[randomBodyAccessory].SetBodyAccessory(transform.GetChild(2).GetComponent<SpriteRenderer>());
						StopAllCoroutines();
						yield break;
					}
					else
					{
						bodyAccTries++;
						StartCoroutine(AssignBodyAccessory());
						yield return null;
					}
				}
				else
				{
					bodyAccTries++;
					if(bodyAccTries >= 3)
					{
						transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = null;
						StopAllCoroutines();
						yield break;
					}
					StartCoroutine(AssignBodyAccessory());
					yield return null;
				}
			}
			else
			{
				StartCoroutine(AssignBodyAccessory());
				yield return null;
			}
		}
		else
		{
			StartCoroutine(AssignBodyAccessory());
			yield return null;
		}
	}
	public IEnumerator AssignFaceAccessory()
	{
		int randomFaceAccessory = Random.Range(0, CitizenManager.instance.allFaceAccessories.Length);
		int chance = Random.Range(0, 101);
		if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allFaceAccessories[randomFaceAccessory].gender.ToString() || CitizenManager.instance.allFaceAccessories[randomFaceAccessory].gender.ToString() == "all")
		{
			if(chance <= CitizenManager.instance.allFaceAccessories[randomFaceAccessory].spawnRateInPercent)
			{
				CitizenManager.instance.allFaceAccessories[randomFaceAccessory].SetFaceAccessory(transform.GetChild(3).GetComponent<SpriteRenderer>());
				if(CitizenManager.instance.allFaceAccessories[randomFaceAccessory].isBeard)
				{
					if(transform.parent.GetChild(0).GetChild(2).gameObject.GetComponent<SpriteRenderer>().sprite != null)
						transform.GetChild(3).GetComponent<SpriteRenderer>().color = transform.parent.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>().color;
					else
						transform.GetChild(3).GetComponent<SpriteRenderer>().color = Color.black;
				}
				StopAllCoroutines();
				yield break;
			}
			else
			{
				transform.GetChild(3).GetComponent<SpriteRenderer>().sprite = null;
				StopAllCoroutines();
				yield break;
			}
		}
		else
		{
			StartCoroutine(AssignFaceAccessory());
			yield return null;
		}
	}

	void AssignLayers()
	{
		CitizenManager.instance.SetLayer(transform.GetChild(0).GetComponent<SpriteRenderer>());
		CitizenManager.instance.SetLayer(transform.GetChild(1).GetComponent<SpriteRenderer>());
		CitizenManager.instance.SetLayer(transform.GetChild(2).GetComponent<SpriteRenderer>());
		CitizenManager.instance.SetLayer(transform.GetChild(3).GetComponent<SpriteRenderer>());
	}
}
