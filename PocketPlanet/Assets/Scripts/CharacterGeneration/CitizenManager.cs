using UnityEngine;
using System.Collections.Generic;

public class CitizenManager : MonoBehaviour
{
	public static CitizenManager instance = null;

    public static int initialLayer = -10;

    [HideInInspector]
    public BodyType[] allBodytypes;
    [HideInInspector]
	public Eyes[] allEyes;
    [HideInInspector]
	public Hair[] allHair;

    [HideInInspector]
    public Shirt[] allShirts;
    [HideInInspector]
	public Pants[] allPants;
    [HideInInspector]
	public Shoe[] allShoes;
    [HideInInspector]
	public Jacket[] allJackets;
    [HideInInspector]
	public Suit[] allSuits;

    [HideInInspector]
    public HatAccessory[] allHats;
    [HideInInspector]
	public GlassesAccessory[] allGlasses;
    [HideInInspector]
	public BodyAccessory[] allBodyAccessories;
    [HideInInspector]
	public FaceAccessory[] allFaceAccessories;

    [HideInInspector]
    public Special[] allSpecials;
    [HideInInspector]
    public List<Special> allBonusCitizens = new List<Special>();
    [HideInInspector]
    public List<GameObject> spawnedBonusCitizens = new List<GameObject>();
  
    [HideInInspector]
    public GameObject waterParticle, policeFireParticle, mafiaFireParticle, blockmanParticle;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        waterParticle = Resources.Load<GameObject>("Particles/WaterSpray");
        policeFireParticle = Resources.Load<GameObject>("Particles/PoliceFire");
        mafiaFireParticle = Resources.Load<GameObject>("Particles/MafiaFire");
		blockmanParticle = Resources.Load<GameObject>("Particles/BlockmanTrail");

        allBodytypes = Resources.LoadAll<BodyType>("CitizenParts/Bodypart");
		allEyes = Resources.LoadAll<Eyes>("CitizenParts/Bodypart");
		allHair = Resources.LoadAll<Hair>("CitizenParts/Bodypart");

        allPants = Resources.LoadAll<Pants>("CitizenParts/Clothes");
		allShirts = Resources.LoadAll<Shirt>("CitizenParts/Clothes");
		allShoes = Resources.LoadAll<Shoe>("CitizenParts/Clothes");
		allJackets = Resources.LoadAll<Jacket>("CitizenParts/Clothes");
		allSuits = Resources.LoadAll<Suit>("CitizenParts/Clothes");

        allHats = Resources.LoadAll<HatAccessory>("CitizenParts/Accessories");
		allGlasses = Resources.LoadAll<GlassesAccessory>("CitizenParts/Accessories");
		allBodyAccessories = Resources.LoadAll<BodyAccessory>("CitizenParts/Accessories");
		allFaceAccessories = Resources.LoadAll<FaceAccessory>("CitizenParts/Accessories");

        allSpecials = Resources.LoadAll<Special>("CitizenParts/Special");

        foreach(Special s in allSpecials)
        {
            if(s.type == Special.SpecialType.BonusCitizen)
            {
                allBonusCitizens.Add(s);
            }
        }
    }

	public Color GetRandomColor(Color[] colorArray, bool userdefinedArray)
	{
		Color tempColor = colorArray[Random.Range(0, colorArray.Length)];
		if(userdefinedArray)
        {
            return tempColor;
        }
        else
        {
            tempColor.r /= 255;
            tempColor.g /= 255;
            tempColor.b /= 255;
            return tempColor;
        }
	}
    public void SetSprite(GameObject sprite, SpriteRenderer go, Color[] userdefinedColorList, Color[] predefinedColorList)
    {
        SpriteRenderer rend;
		rend = sprite.GetComponent<SpriteRenderer>();
		go.sprite = rend.sprite;
		if(userdefinedColorList.Length == 0)
		{
			go.color = CitizenManager.instance.GetRandomColor(predefinedColorList, false);
		}
		else
		{
			go.color = CitizenManager.instance.GetRandomColor(userdefinedColorList, true);
		}	
    }
    public void SetLayer(SpriteRenderer rend)
    {
        Citizen thisCitizen = rend.GetComponentInParent<Citizen>();
        rend.GetComponent<Renderer>().sortingOrder += thisCitizen.layerIndex;
    }
    public void SetLayer(Citizen citizen)
    {
        Citizen thisCitizen = citizen;
        foreach(Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.sortingOrder += thisCitizen.layerIndex;
        }
    }
}
