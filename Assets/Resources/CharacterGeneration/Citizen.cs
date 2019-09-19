using UnityEngine;
using UnityEngine.SceneManagement;
using Citizens;

public class Citizen : MonoBehaviour 
{
    string[] maleFirstNames = new string[]
    {
        "Noah", "Victor", "Oliver", "Oscar", "William", "Lucas", "Carl", "Emil", "Malthe", "Frederik", 
        "Magnus", "Alfred", "Elias", "Valdemar", "August", "Anton", "Villads", "Alexander", "Christian", 
        "Mikkel", "Nohr", "Johan", "Aksel", "Sebastian", "Adam", "Liam", "Storm", "Albert", "Elliot", "Laurits", 
        "Felix", "Conrad", "Theodor", "Viggo", "Arthur", "Benjamin", "Mathias", "Jakob", "Marius", "Otto", 
        "Theo", "Thor", "Mads", "Philip", "Marcus", "Jonathan", "Lauge", "Sander", "Bertram", "Loui", 
        "Ulrik", "Anders", "Niklas", "Kim", "Lasse"
    };

    string[] femaleFirstNames = new string[]
    {
        "Sofia", "Freja", "Alma", "Laura", "Ida", "Clara", "Ella", "Anna", "Emma", "Josefine", "Alberte", 
        "Agnes", "Isabella", "Maja", "Karla", "Lærke", "Sofie", "Victoria", "Olivia", "Liva", "Emilie", "Esther",
        "Ellen", "Mathilde", "Emily", "Caroline", "Sara", "Astrid", "Mille", "Aya", "Frida", "Lily", "Nora", 
        "Marie", "Liv", "Naja", "Vigga", "Luna", "Asta", "Rosa", "Saga", "Andrea", "Filippa", "Johanne", "Nanna",
        "Lea", "Tilde", "Molly", "Gry", "Signe", "Nadja Pauline", "Winnie Pauline", "Henny"
    };
    string[] surNames = new string[]
    {
        "Nielsen", "Jensen", "Hansen", "Pedersen", "Andersen", "Christensen", "Larsen", "Sørensen", "Rasmussen", 
        "Jørgensen"
    };

	Bodies body;
	Clothes clothes;
	Accessories accesories;
	
	public enum Gender
	{
		male, female
	};

	string firstName, lastName;
	int maxLayers = 10;
	[HideInInspector]
	public int layerIndex;
	public Gender gender;
	public bool isSpecialCharacter;
	public Special.SpecialType specialType;

	void Awake()
	{
		CitizenManager.initialLayer += maxLayers;
		layerIndex = CitizenManager.initialLayer;
		gender = (Gender)Random.Range(0,2);
		if(gender == Gender.female)
		{
			firstName = femaleFirstNames[Random.Range(0, femaleFirstNames.Length)];
		}
		else if(gender == Gender.male)
		{
			firstName = maleFirstNames[Random.Range(0, maleFirstNames.Length)];
		}
		lastName = surNames[Random.Range(0, surNames.Length)];

		gameObject.name = firstName + " " + lastName;
	
		body = GetComponentInChildren<Bodies>();
		clothes = GetComponentInChildren<Clothes>();
		accesories = GetComponentInChildren<Accessories>();
		body.gender = (Bodies.Gender)gender;
		clothes.gender = (Clothes.Gender)gender;
		accesories.gender = (Accessories.Gender)gender;
	}

	void Start()
	{
		if (SceneManager.GetActiveScene ().name == "CharacterGenerationTest") {
			if(isSpecialCharacter)
			{
				CreateSpecial(specialType);
			}
			else
				CreateCitizen();
		}

	}

	public void CreateCitizen()
	{
		StartCoroutine(body.AssignBodyType());
		StartCoroutine(body.AssignEyes());
		StartCoroutine(body.AssignHair());

		StartCoroutine(clothes.AssignShirt());
		StartCoroutine(clothes.AssignShoes());
		if(!clothes.suitOn)
		{
			StartCoroutine(clothes.AssignPants());
			StartCoroutine(clothes.AssignJacket());
		}
		else if(clothes.suitOn)
		{
			StartCoroutine(clothes.AssignSuit());
		}
		StartCoroutine(accesories.AssignHat());
		StartCoroutine(accesories.AssignGlasses());
		StartCoroutine(accesories.AssignBodyAccessory());
		StartCoroutine(accesories.AssignFaceAccessory());




        //add the rest of the stats through
       // InitializeCivilianStats(personScript);
    }



    public void CreateSpecial(Special.SpecialType type)
	{

		specialType = type;
		InstantiateCorrectParticlePrefab(type);
		StartCoroutine(body.AssignBodyType());
		StartCoroutine(body.AssignEyes());
		CitizenManager.instance.SetLayer(this);
		transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
		transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).gameObject.SetActive(false);
		StartCoroutine(body.AssignSpecial(type));

	}

    void InstantiateCorrectParticlePrefab(Special.SpecialType type)
	{
		Transform particleTrans = transform.GetChild(0).GetChild(0).GetChild(1);
		GameObject particlePrefab;
		if (type == Special.SpecialType.Firefighter)
		{
			particlePrefab = Instantiate(CitizenManager.instance.waterParticle, Vector3.zero, CitizenManager.instance.waterParticle.transform.rotation, particleTrans);
			particlePrefab.transform.position = particlePrefab.transform.parent.position;
		}
		else if (type == Special.SpecialType.Police)
		{
			particlePrefab = Instantiate(CitizenManager.instance.policeFireParticle, Vector3.zero, CitizenManager.instance.policeFireParticle.transform.rotation, particleTrans);
			particlePrefab.transform.position = particlePrefab.transform.parent.position;
		}
		else if (type == Special.SpecialType.Mafia)
		{
			particlePrefab = Instantiate(CitizenManager.instance.mafiaFireParticle, Vector3.zero, CitizenManager.instance.mafiaFireParticle.transform.rotation, particleTrans);
			particlePrefab.transform.position = particlePrefab.transform.parent.position;
		}
		else if (type == Special.SpecialType.Blockman)
		{
			particlePrefab = Instantiate(CitizenManager.instance.blockmanParticle, Vector3.zero, CitizenManager.instance.blockmanParticle.transform.rotation, particleTrans);
			particlePrefab.transform.position = particlePrefab.transform.parent.position;
		}
		else
			return;
	}
}
