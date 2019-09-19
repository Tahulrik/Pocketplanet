using UnityEngine;
using System.Collections;

public class Bodies : MonoBehaviour 
{
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

	int timesRun;

	void Start()
	{
		AssignLayers();
		timesRun = 0;
	}

	public IEnumerator AssignBodyType()
	{
		int randomBodyType = Random.Range(0, CitizenManager.instance.allBodytypes.Length);
		int chance = Random.Range(0, 101);
		if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allBodytypes[randomBodyType].gender.ToString() || CitizenManager.instance.allBodytypes[randomBodyType].gender.ToString() == "all")
		{
			if(chance <= CitizenManager.instance.allBodytypes[randomBodyType].spawnRateInPercent)
			{
				CitizenManager.instance.allBodytypes[randomBodyType].SetBodytype(transform.GetChild(0).GetComponent<SpriteRenderer>());
				StopAllCoroutines();
				yield break;
			}
			else
			{
				StartCoroutine(AssignBodyType());
				yield return null;
			}
		}
		else
		{
			StartCoroutine(AssignBodyType());
			yield return null;
		}
	}

	public IEnumerator AssignEyes()
	{
		int randomEyes = Random.Range(0, CitizenManager.instance.allEyes.Length);
		int chance = Random.Range(0, 101);
		if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allEyes[randomEyes].gender.ToString() || CitizenManager.instance.allEyes[randomEyes].gender.ToString() == "all")
		{
			if(chance <= CitizenManager.instance.allEyes[randomEyes].spawnRateInPercent)
			{
				CitizenManager.instance.allEyes[randomEyes].SetEyes(transform.GetChild(1).GetComponent<SpriteRenderer>());
				StopAllCoroutines();
				yield break;
			}
			else
			{
				StartCoroutine(AssignEyes());
				yield return null;
			}
		}
		else
		{
			StartCoroutine(AssignEyes());
			yield return null;
		}
	}

	public IEnumerator AssignHair()
	{
		int randomHair = Random.Range(0, CitizenManager.instance.allHair.Length);
		int chance = Random.Range(0, 101);
		if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allHair[randomHair].gender.ToString() || CitizenManager.instance.allHair[randomHair].gender.ToString() == "all")
		{
			if(chance <= CitizenManager.instance.allHair[randomHair].spawnRateInPercent)
			{
				CitizenManager.instance.allHair[randomHair].SetHair(transform.GetChild(2).GetComponent<SpriteRenderer>());
				if(!CitizenManager.instance.allHair[randomHair].hatPossible)
				{
					transform.parent.GetChild(2).GetChild(0).gameObject.SetActive(false);
				}
				StopAllCoroutines();
				yield break;
			}
			else
			{
				StartCoroutine(AssignHair());
				yield return null;
			}
		}
		else
		{
			if(timesRun > 3)
			{
				transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = null;
				StopAllCoroutines();
				yield break;
			}
			timesRun++;
			StartCoroutine(AssignHair());
			yield return null;
		}
	}

	public IEnumerator AssignSpecial(Special.SpecialType type)
	{
		int matchCount = 0;
		foreach(Special s in CitizenManager.instance.allSpecials)
		{
			if(type == s.type)
			{
				matchCount++;
			}
		}
		if(matchCount <= 0)
		{
			Debug.LogError("No specials have the type: " + type.ToString() + ". Please add a character of that type to the Specials folder.\nGOD WILL NOW DESTROY " 
				+ transform.GetComponentInParent<Citizen>().name + "!");
			Destroy(transform.GetComponentInParent<Citizen>().gameObject);
			StopAllCoroutines();
			yield break;
		}

		int randomSpecial = Random.Range(0, CitizenManager.instance.allSpecials.Length);
		if(type == CitizenManager.instance.allSpecials[randomSpecial].type)
		{
			if(type == Special.SpecialType.BonusCitizen)
			{
				if(CitizenManager.instance.spawnedBonusCitizens.Count >= CitizenManager.instance.allBonusCitizens.Count)
				{
					Debug.LogWarning("No more bonus characters to spawn.\nSpawning normal Citizen!");
					transform.parent.GetChild(1).gameObject.SetActive(true);
					transform.parent.GetChild(2).gameObject.SetActive(true);
					GetComponentInParent<Citizen>().CreateCitizen();
					yield break;
				}
				GameObject objInScene = GameObject.Find(CitizenManager.instance.allSpecials[randomSpecial].name);
				if(objInScene != null)
				{
					StartCoroutine(AssignSpecial(type));
					yield return null;
				}
				GetComponentInParent<Citizen>().gameObject.name = CitizenManager.instance.allSpecials[randomSpecial].name;
				CitizenManager.instance.spawnedBonusCitizens.Add(GetComponentInParent<Citizen>().gameObject);
			}
			CitizenManager.instance.allSpecials[randomSpecial].SetSpecial(transform.GetChild(2).GetComponent<SpriteRenderer>());
			StopAllCoroutines();
			yield break;
		}
		else
		{
			StartCoroutine(AssignSpecial(type));
			yield return null;
		}
	}

	void AssignLayers()
	{
		CitizenManager.instance.SetLayer(transform.GetChild(0).GetComponent<SpriteRenderer>());
		CitizenManager.instance.SetLayer(transform.GetChild(1).GetComponent<SpriteRenderer>());
		CitizenManager.instance.SetLayer(transform.GetChild(2).GetComponent<SpriteRenderer>());
	}
}
