using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour{

	public delegate void GamePointsChanged(int value);

	public static event GamePointsChanged PointsSpent;
	public static event GamePointsChanged PointsRecieved;

	public static GameHandler Instance;

	GameObject InputHandler;

	[HideInInspector]
	public ConstructionHandler ConstructionHandler;
	[HideInInspector]
	public CitizenManager CitizenManager;
	[HideInInspector]
	public CameraControls CameraControls;
    [HideInInspector]
    public AudioManager AudioManager;

    //Rename that goddamn script!!
    [HideInInspector]
	public MeteorHandler MeteorHandler;

	[HideInInspector]
	public CPRAccountData CPRData;


	[HideInInspector]
	public GameObject Blockzilla, Blockman;
	[HideInInspector]
	public GameObject[] SmallHouses, MediumHouses, LargeHouses, Specials;

	public GameObject UpgradeUI, DemolishUI, IncreaseWorkersUI, ContextMenuPanel;
	public Sprite UpgradeSprite, SpawnSprite;

	public Text pointsText, MoneyText, DailyMoneyText, WeekDayText;

	public int gamePoints = 0;

	[HideInInspector]
	public float dailyAmount = 0;

	public Color moneyGreen, moneyRed;

	public Color selectionAvailable, selectionUnavailable;

	// Use this for initialization
	void Awake () {
		Application.targetFrameRate = 60;

		Instance = this;

		pointsText.text = gamePoints.ToString();

		UpgradeUI.SetActive(false);
		DemolishUI.SetActive(false);
		IncreaseWorkersUI.SetActive(false);

		ConstructionHandler = FindObjectOfType<ConstructionHandler>();
        CitizenManager = FindObjectOfType<CitizenManager>();
        AudioManager = FindObjectOfType<AudioManager>();
		CPRData = FindObjectOfType<CPRAccountData>();

        CameraControls = FindObjectOfType<CameraControls> ();
		MeteorHandler = FindObjectOfType<MeteorHandler> ();

		Blockzilla = Resources.Load<GameObject>("Events/Blockzilla");
		Blockman = Resources.Load<GameObject>("Events/Blockman");
		SmallHouses = Resources.LoadAll<GameObject>("Buildings/SmallHouses");
		MediumHouses = Resources.LoadAll<GameObject>("Buildings/MediumHouses");
		LargeHouses = Resources.LoadAll<GameObject>("Buildings/LargeHouses");
		Specials = Resources.LoadAll<GameObject>("Buildings/Special");

		MoneyText.text = "Money: " + CPRData.currentBalance.ToString("N2");
		ResetDailyMoney();

	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			AddGamePoints (10);
		}
	}

	public void AddGamePoints(int points)
	{
		var gainedPoints = Mathf.Abs(points);

		gamePoints += gainedPoints;
		pointsText.text = gamePoints.ToString();

		if (PointsRecieved != null)
		{
			PointsRecieved(gainedPoints);
		}
	}

	public void SpendGamePoint(int points)
	{
		var usedPoints = Mathf.Abs(points);

		gamePoints = (int)Mathf.Clamp(gamePoints - usedPoints, 0, Mathf.Infinity);
		pointsText.text = gamePoints.ToString();;

		if (PointsSpent != null)
		{
			PointsSpent(usedPoints);
		}
	}

	//public void AddMoney(float money)
	//{
	//	CPRData.currentBalance += money;
	//	if (CPRData.currentBalance< 0)
	//		MoneyText.color = Color.red;
	//	else 
	//		MoneyText.color = Color.green;

	//	MoneyText.text = "Money: " + CPRData.currentBalance.ToString("N2");
	//}

	//public void AddDailyMoney(float money)
	//{
	//	dailyAmount += money;

	//	if (dailyAmount < 0)
	//		DailyMoneyText.color = Color.red;
	//	else
	//		DailyMoneyText.color = Color.green;

	//	DailyMoneyText.text = "Daily Money: " + dailyAmount.ToString("N2");
	//}

	//public void ResetDailyMoney()
	//{
	//	dailyAmount = 0;
	//	DailyMoneyText.color = Color.grey;
	//	DailyMoneyText.text = "Daily Money: " + dailyAmount.ToString("N2");
	//}

	public IEnumerator AddMoney(float money)
	{
		float tempVar = CPRData.currentBalance + money;
	
		if (money > 0)
		{ 
			while (CPRData.currentBalance < tempVar)
			{ 
				CPRData.currentBalance += money / 1 * Time.deltaTime;
	
				if (CPRData.currentBalance < 0)
					MoneyText.color = moneyRed;
				else 
					MoneyText.color = moneyGreen;
	
				MoneyText.text = "Money: " + CPRData.currentBalance.ToString("N2");
				yield return new WaitForEndOfFrame();
			}
			CPRData.currentBalance = tempVar;
			MoneyText.text = "Money: " + CPRData.currentBalance.ToString("N2");
		}
		if (money < 0)
		{ 
			while (CPRData.currentBalance > tempVar)
			{ 
				CPRData.currentBalance += money / 1 * Time.deltaTime;
	
				if (CPRData.currentBalance < 0)
					MoneyText.color = moneyRed;
				else 
					MoneyText.color = moneyGreen;
	
				MoneyText.text = "Money: " + CPRData.currentBalance.ToString("N2");
				yield return new WaitForEndOfFrame();
			}
			CPRData.currentBalance = tempVar;
			MoneyText.text = "Money: " + CPRData.currentBalance.ToString("N2");
		}
	}

	public IEnumerator AddDailyMoney(float money)
	{
		float tempVar = dailyAmount + money;
	
		if (money > 0)
		{ 
			while (dailyAmount < tempVar)
			{ 
				dailyAmount += money / 1 * Time.deltaTime;
	
				if (dailyAmount < 0)
					DailyMoneyText.color = moneyRed;
				else 
					DailyMoneyText.color = moneyGreen;
	
				DailyMoneyText.text = "Weekly Disposable: " + dailyAmount.ToString("N2");
				yield return new WaitForEndOfFrame();
			}
			dailyAmount = tempVar;
			DailyMoneyText.text = "Weekly Disposable: " + dailyAmount.ToString("N2");
		}
		if (money < 0)
		{ 
			while (dailyAmount > tempVar)
			{ 
				dailyAmount += money / 1 * Time.deltaTime;
	
				if (dailyAmount < 0)
					DailyMoneyText.color = moneyRed;
				else 
					DailyMoneyText.color = moneyGreen;
	
				DailyMoneyText.text = "Weekly Disposable: " + dailyAmount.ToString("N2");
				yield return new WaitForEndOfFrame();
			}
			dailyAmount = tempVar;
			DailyMoneyText.text = "Weekly Disposable: " + dailyAmount.ToString("N2");
		}
	}

	public IEnumerator ResetDailyMoney()
	{
		float tempVar = dailyAmount;
		if (dailyAmount > 0)
		{
			while (dailyAmount > 0)
			{
				dailyAmount -= tempVar / 1 * Time.deltaTime;
				DailyMoneyText.color = Color.black;
				DailyMoneyText.text = "Weekly Disposable: " + dailyAmount.ToString("N2");
				yield return new WaitForEndOfFrame();
			}
			dailyAmount = 0;
			DailyMoneyText.text = "Weekly Disposable: " + dailyAmount.ToString("N2");
		}
		if (dailyAmount < 0)
		{
			while (dailyAmount < 0)
			{
				dailyAmount -= tempVar / 1 * Time.deltaTime;
				DailyMoneyText.color = Color.black;
				DailyMoneyText.text = "Weekly Disposable: " + dailyAmount.ToString("N2");
				yield return new WaitForEndOfFrame();
			}
			dailyAmount = 0;
			DailyMoneyText.text = "Weekly Disposable: " + dailyAmount.ToString("N2");
		}
	}
}