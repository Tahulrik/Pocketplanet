using UnityEngine;
using System.Collections;

public class Clothes : MonoBehaviour 
{
	protected Color[] clothColors = new Color[] 
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
	[HideInInspector]
	public bool shirtAssigned = false;
	[HideInInspector]
	public string shirtTypeString = ""; 
	[HideInInspector]
	public bool suitOn = false;

	public Gender gender;

	void Awake()
	{
		NothingSuitsYouLikeASuit();
	}

	void Start()
	{
		AssignLayers();
	}

	public IEnumerator AssignShirt()
	{
		int randomShirt = Random.Range(0, CitizenManager.instance.allShirts.Length);
		int chance = Random.Range(0, 101);
		if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allShirts[randomShirt].gender.ToString() || CitizenManager.instance.allShirts[randomShirt].gender.ToString() == "all")
		{
			if(chance <= CitizenManager.instance.allShirts[randomShirt].spawnRateInPercent)
			{
				shirtAssigned = true;
				shirtTypeString = CitizenManager.instance.allShirts[randomShirt].shirtType.ToString();
				CitizenManager.instance.allShirts[randomShirt].SetShirt(transform.GetChild(0).GetComponent<SpriteRenderer>());
				StopAllCoroutines();
				yield break;
			}
			else
			{
				StartCoroutine(AssignShirt());
				yield return null;
			}
		}
		else
		{
			StartCoroutine(AssignShirt());
			yield return null;
		}
	}
	public IEnumerator AssignPants()
	{
		int randomPants = Random.Range(0, CitizenManager.instance.allPants.Length);
		int chance = Random.Range(0, 101);
		if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allPants[randomPants].gender.ToString() || CitizenManager.instance.allPants[randomPants].gender.ToString() == "all")
		{
			if(chance <= CitizenManager.instance.allPants[randomPants].spawnRateInPercent)
			{
				CitizenManager.instance.allPants[randomPants].SetPants(transform.GetChild(1).GetComponent<SpriteRenderer>());
				StopAllCoroutines();
				yield break;
			}
			else
			{
				StartCoroutine(AssignPants());
				yield return null;
			}
		}
		else
		{
			StartCoroutine(AssignPants());
			yield return null;
		}
	}
	public IEnumerator AssignShoes()
	{
		int randomShoes = Random.Range(0, CitizenManager.instance.allShoes.Length);
		int chance = Random.Range(0, 101);
		if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allShoes[randomShoes].gender.ToString() || CitizenManager.instance.allShoes[randomShoes].gender.ToString() == "all")
		{
			if(chance <= CitizenManager.instance.allShoes[randomShoes].spawnRateInPercent)
			{
				CitizenManager.instance.allShoes[randomShoes].SetShoe(transform.GetChild(2).GetComponent<SpriteRenderer>());
				StopAllCoroutines();
				yield break;
			}
			else
			{
				StartCoroutine(AssignShoes());
				yield return null;
			}
		}
		else
		{
			StartCoroutine(AssignShoes());
			yield return null;
		}
	}
	public IEnumerator AssignJacket()
	{
		int randomJacket = Random.Range(0, CitizenManager.instance.allJackets.Length);
		int chance = Random.Range(0, 101);
		if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allJackets[randomJacket].gender.ToString() || CitizenManager.instance.allJackets[randomJacket].gender.ToString() == "all")
		{
			if(chance <= CitizenManager.instance.allJackets[randomJacket].spawnRateInPercent)
			{
				CitizenManager.instance.allJackets[randomJacket].SetJacket(transform.GetChild(3).GetComponent<SpriteRenderer>());
				StopAllCoroutines();
				yield break;
			}
			else
			{
				StartCoroutine(AssignJacket());
				yield return null;
			}
		}
		else
		{
			transform.GetChild(3).GetComponent<SpriteRenderer>().sprite = null;
			StopAllCoroutines();
			yield break;
		}
	}
	public IEnumerator AssignSuit()
	{
		int randomSuit = Random.Range(0, CitizenManager.instance.allSuits.Length);
		int chance = Random.Range(0, 101);
		if(GetComponentInParent<Citizen>().gender.ToString() == CitizenManager.instance.allSuits[randomSuit].gender.ToString() || CitizenManager.instance.allSuits[randomSuit].gender.ToString() == "all")
		{
			if(chance <= CitizenManager.instance.allSuits[randomSuit].spawnRateInPercent)
			{
				CitizenManager.instance.allSuits[randomSuit].SetSuit(transform.GetChild(4).GetComponent<SpriteRenderer>());
				StopAllCoroutines();
				yield break;
			}
			else
			{
				StartCoroutine(AssignSuit());
				yield return null;
			}
		}
		else
		{
			StartCoroutine(AssignSuit());
			yield return null;
		}
	}

	void NothingSuitsYouLikeASuit()
	{
		float randVal;
		transform.GetChild(3).gameObject.SetActive(false);
		transform.GetChild(4).gameObject.SetActive(false);
		randVal = Random.Range(0.0f, 1.0f);
		if(randVal >= 0.7f)
		{
			transform.GetChild(1).gameObject.SetActive(false);
			transform.GetChild(4).gameObject.SetActive(true);
			suitOn = true;
		}
		else
		{
			transform.GetChild(3).gameObject.SetActive(false);
			suitOn = false;
		}
	}

	void AssignLayers()
	{
		CitizenManager.instance.SetLayer(transform.GetChild(0).GetComponent<SpriteRenderer>());
		CitizenManager.instance.SetLayer(transform.GetChild(1).GetComponent<SpriteRenderer>());
		CitizenManager.instance.SetLayer(transform.GetChild(2).GetComponent<SpriteRenderer>());
		CitizenManager.instance.SetLayer(transform.GetChild(3).GetComponent<SpriteRenderer>());
		CitizenManager.instance.SetLayer(transform.GetChild(4).GetComponent<SpriteRenderer>());
	}
}
