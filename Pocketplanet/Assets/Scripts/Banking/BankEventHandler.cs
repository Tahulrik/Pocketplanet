using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

/*public class BankEventHandler : MonoBehaviour {

	CPRAccountData data;

	[HideInInspector]
	public float averageMontlyIncome = 0, MontlyExpenses = 0;
	private float minWaitTime = 0.4f;
	private float maxWaitTime = 1.0f;
	public int pointsPerDay = 3;
	public float timescale = 1.0f;
	public float timeBetweenEvents = 1;

	public float weeklyUsableAmount;
	float thisDailyUsable;

	bool runOnce = false;

	int currentIndex = 0;

	GameObject nextDayButton;

	System.DateTime currentDay;

	System.DateTime currectDayText;

	public delegate void TransactionAction(float val);
	public static event TransactionAction TransactionDone;


	// Use this for initialization
	void Start () 
	{
		data = FindObjectOfType<CPRAccountData>();

		nextDayButton = GameObject.Find("NextDayButton");

		currentDay = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

		currectDayText = new System.DateTime(2017, 4, 3, 0, 0, 0, System.DateTimeKind.Utc);

		GameHandler.Instance.WeekDayText.text = currectDayText.DayOfWeek.ToString();

	}
	
	// Update is called once per frame
	void Update () 
	{
		Time.timeScale = timescale;
		if (!runOnce && data.dataSaved)
		{
            StartCoroutine(SetAverageMontly());
		}
	}

	IEnumerator SetAverageMontly()
	{
		runOnce = true;
        StartCoroutine(GameHandler.Instance.ResetDailyMoney());
		averageMontlyIncome = GetAverageMonthlyIncome();
		print("Expenses" + GetAverageMonthlyExpenses());
		yield return new WaitForEndOfFrame();

		StartCoroutine(NextDay());
	}	



	float GetAverageMonthlyIncome()
	{

		float amount = 0;
		int months = 0;
		int lastMonth = PrintDateTime(0).Month;

		for (int x = 0; x < data.transactions.Length; x++)
		{
			if (data.transactions[x].Amount > 0)
			{
				if (PrintDateTime(x).Month < lastMonth || PrintDateTime(x).Month == 12 && lastMonth == 1)
				{
					months++;
					lastMonth = PrintDateTime(x).Month;
				}
				amount += data.transactions[x].Amount;
				if (months > 0)
					break;
			}
		}
		print("Income: " + amount / months);
		return amount / months;
	}

	float GetAverageMonthlyExpenses()
	{

		float amount = 0;
		int months = 0;
		int lastMonth = PrintDateTime(0).Month;

		for (int x = 0; x < data.transactions.Length; x++)
		{
			if (data.transactions[x].Amount < 0)
			{
				if (PrintDateTime(x).Month < lastMonth || PrintDateTime(x).Month == 12 && lastMonth == 1)
				{
					months++;
					lastMonth = PrintDateTime(x).Month;
				}
				amount += data.transactions[x].Amount;
				if (months > 0)
					break;
			}
		}
		return amount / months;
	}

	System.DateTime PrintDateTime(int x)
	{
		var epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		long timeString = long.Parse(data.transactions[x].TransactionDateTimestamp);
		return epoch.AddMilliseconds(timeString);	
	}

	/*IEnumerator IterateOverAllTransactions()
	{
		float tempAmount = 0;
		yield return new WaitForSeconds(2.0f);
		for (int i = 0; i < data.transactions.Length; i++)
		{
			if (PrintDateTime(i).Date != currentDay.Date)
			{
				GameHandler.Instance.AddDailyMoney(dailyUsableAmount);
				tempAmount += data.transactions[i].Amount;
				GameHandler.Instance.AddDailyMoney(tempAmount);
				GameHandler.Instance.AddMoney(tempAmount);
				GameHandler.Instance.ResetDailyMoney();
				CalculateProperEvent(tempAmount / dailyUsableAmount * 100);
				currentDay = PrintDateTime(i).Date;
				tempAmount = 0;
			}
			else if (PrintDateTime(i).Date == currentDay.Date)
			{
				tempAmount += data.transactions[i].Amount + GameHandler.Instance.dailyAmount;
				GameHandler.Instance.AddMoney(tempAmount);
				GameHandler.Instance.AddDailyMoney(tempAmount);
				tempAmount = 0;
			}
			else
			{
				print("This should nok happen");
			}
			yield return new WaitForSeconds(Random.Range(minWaitTime / Time.timeScale, maxWaitTime / Time.timeScale));
		}	
	}*/

	/*public void NextDayFunction()
	{
		currectDayText = currectDayText.AddDays(1);
		GameHandler.Instance.WeekDayText.text = currectDayText.DayOfWeek.ToString();
		StartCoroutine(NextDay());
	}

	IEnumerator NextDay()
	{
		GameHandler.Instance.AddGamePoints(pointsPerDay);
		nextDayButton.SetActive(false);
		int transactionToday = 0;

		currentDay = PrintDateTime(currentIndex);
		for (int i = currentIndex; i < data.transactions.Length; i++)
		{
			if (PrintDateTime(i).Date == currentDay.Date)
			{
				transactionToday++;
			}
			else
			{
				currentIndex = i;
				break;
			}
		}


		if (currectDayText.DayOfWeek.ToString() == "Monday")
		{
			if(thisDailyUsable < 0)
				StartCoroutine(GameHandler.Instance.AddDailyMoney(weeklyUsableAmount + Mathf.Abs(thisDailyUsable)));
			else
                StartCoroutine(GameHandler.Instance.AddDailyMoney(weeklyUsableAmount - Mathf.Abs(thisDailyUsable)));

			thisDailyUsable = weeklyUsableAmount;
		}

		float dailyUsed = 0;

		for (int i = transactionToday; i > 0; i--)
		{ 
			float transactionAmount = data.transactions[currentIndex-i].Amount;
			thisDailyUsable += transactionAmount;
			dailyUsed = (thisDailyUsable / weeklyUsableAmount) * 100;
			yield return new WaitForSeconds(timeBetweenEvents / timescale);
			StartCoroutine(GameHandler.Instance.AddDailyMoney(transactionAmount));
			StartCoroutine(GameHandler.Instance.AddMoney(transactionAmount));
			if (transactionAmount < 0)
			{
				print(dailyUsed);
				CalculateNegativeEvent(dailyUsed);
			}
			else
			{
				float tempVal = (transactionAmount / weeklyUsableAmount) * 1000;
				CalculatePositiveEvent(tempVal);
			}
			//CalculateProperEvent(dailyUsed);
		}
		nextDayButton.SetActive(true);
	}

	void CalculateNegativeEvent(float percent)
	{ 
		if (percent > 0)
		{
			GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 1);
		}
		if (percent <= -10)
		{
			GameHandler.Instance.SpendGamePoint(1);
		}
		if (percent <= -15)
		{
			EventHandler.TriggerRobber();
		}
		if (percent <= -25)
		{
			EventHandler.TriggerMafia();
		}
		if (percent <= -50)
		{
			GameHandler.Instance.MeteorHandler.SpawnMeteor(Random.insideUnitCircle* Planet.Radius);
		}
		if (percent <= -250)
		{
			EventHandler.TriggerBlockZilla(EventCategory.Blockzilla, Random.insideUnitCircle* Planet.Radius, GameHandler.Instance.Blockzilla, false);
		}
		if (percent <= -300)
		{
			GameHandler.Instance.MeteorHandler.MeteorShower(Random.insideUnitCircle);
		}
	}
	void CalculatePositiveEvent(float percent)
	{ 
		if (percent > 0)
		{
			GameHandler.Instance.AddGamePoints(1);
		}
		if (percent > 5)
		{
			GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 1);
		}
		if (percent > 10)
		{
			int upgradedAmount = 0;
			GameHandler.Instance.ConstructionHandler.UpgradeBuilding(BuildingSize.Small, GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)], ref upgradedAmount, 1);
			if (upgradedAmount == 0)
			{
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 2);
			}
			GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.Specials[Random.Range(0, GameHandler.Instance.Specials.Length)], 1);
		}
		if (percent > 15)
		{
			int upgradedAmount = 0;
			GameHandler.Instance.ConstructionHandler.UpgradeBuilding(BuildingSize.Medium, GameHandler.Instance.LargeHouses[Random.Range(0, GameHandler.Instance.LargeHouses.Length)], ref upgradedAmount, 1);
			if (upgradedAmount == 0)
			{
				GameHandler.Instance.ConstructionHandler.UpgradeBuilding(BuildingSize.Small, GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)], ref upgradedAmount, 1);
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)], 1);
			}
		}
		if (percent > 25)
		{
			int upgradedAmount = 0;
			GameHandler.Instance.ConstructionHandler.UpgradeBuilding(BuildingSize.Medium, GameHandler.Instance.LargeHouses[Random.Range(0, GameHandler.Instance.LargeHouses.Length)], ref upgradedAmount, 1);
			if (upgradedAmount == 0)
			{
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 1);
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)], 1);
			}
		}
		if (percent > 50)
		{
			int upgradedAmount = 0;
			GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.Specials[Random.Range(0, GameHandler.Instance.Specials.Length)], 2);
			GameHandler.Instance.ConstructionHandler.UpgradeBuilding(BuildingSize.Small, GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)], ref upgradedAmount, 2);
			if (upgradedAmount == 0)
			{
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 2);
			}
		}
		if (percent > 75)
		{
			GameHandler.Instance.AddGamePoints(2);
			GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.LargeHouses[Random.Range(0, GameHandler.Instance.LargeHouses.Length)], 1);
			GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.Specials[Random.Range(0, GameHandler.Instance.Specials.Length)], 1);
		}
		if (percent > 150)
		{
			EventHandler.TriggerBlockman(EventCategory.BlockMan, Random.insideUnitCircle * 2, GameHandler.Instance.Blockman, false);
		}
	}*/
	/*void CalculateProperEvent(float percent)
	{
		if (percent < 0)
		{
			if (percent > -100)
			{
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 1);
			}
			else if (percent <= -100 && percent > -110)
			{
				EventHandler.TriggerRobber();
			}
			else if (percent <= -110 && percent > -125)
			{
				GameHandler.Instance.MeteorHandler.SpawnMeteor(Random.insideUnitCircle* Planet.Radius);
			}
			else if (percent <= -125 && percent > -150)
			{
				GameHandler.Instance.MeteorHandler.SpawnMeteor(Random.insideUnitCircle* Planet.Radius);
				EventHandler.TriggerRobber();
			}
			else if (percent <= -150 && percent > -200)
			{
				GameHandler.Instance.MeteorHandler.SpawnMeteor(Random.insideUnitCircle* Planet.Radius);
				EventHandler.TriggerRobber();
				EventHandler.TriggerMafia();
			}
			else if (percent <= -200 && percent > -400)
			{
				EventHandler.TriggerBlockZilla(EventCategory.Blockzilla, Random.insideUnitCircle* Planet.Radius, GameHandler.Instance.Blockzilla, false);
				EventHandler.TriggerMafia();
			}
			else if (percent <= -400)
			{
				GameHandler.Instance.MeteorHandler.MeteorShower(Random.insideUnitCircle);
			}
		}
		else
		{
			Building_Normal_Residence[] allHouses = FindObjectsOfType<Building_Normal_Residence>();
			if (percent < 50)
			{
				GameHandler.Instance.AddGamePoints(1);
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.Specials[Random.Range(0, GameHandler.Instance.Specials.Length)], 1);
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 2);

			}
			else if (percent >= 50 && percent < 75)
			{
				GameHandler.Instance.AddGamePoints(2);
				int upgradedAmount = 0;
				GameHandler.Instance.ConstructionHandler.UpgradeBuilding(allHouses, BuildingSize.Small, GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)], ref upgradedAmount, 1);
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 3);
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.Specials[Random.Range(0, GameHandler.Instance.Specials.Length)], 1);
			}
			else if (percent >= 75 && percent < 100)
			{
				GameHandler.Instance.AddGamePoints(3);
				int upgradedAmount = 0;
				GameHandler.Instance.ConstructionHandler.UpgradeBuilding(allHouses, BuildingSize.Medium, GameHandler.Instance.LargeHouses[Random.Range(0, GameHandler.Instance.LargeHouses.Length)], ref upgradedAmount, 1);
				GameHandler.Instance.ConstructionHandler.UpgradeBuilding(allHouses, BuildingSize.Small, GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)], ref upgradedAmount, 2);
				if (upgradedAmount == 0)
				{
					GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 2);
					GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)], 1);
				}
				else
				{
					GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 2);
				}
			}
			else if (percent >= 100)
			{
				GameHandler.Instance.AddGamePoints(4);
				int upgradedAmount = 0;
				GameHandler.Instance.ConstructionHandler.UpgradeBuilding(allHouses, BuildingSize.Medium, GameHandler.Instance.LargeHouses[Random.Range(0, GameHandler.Instance.LargeHouses.Length)], ref upgradedAmount, 2);
				GameHandler.Instance.ConstructionHandler.UpgradeBuilding(allHouses, BuildingSize.Small, GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)], ref upgradedAmount, 3);
				GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.Specials[Random.Range(0, GameHandler.Instance.Specials.Length)], 2);
				if (upgradedAmount == 0)
				{
					GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 2);
					GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)], 1);
					GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.LargeHouses[Random.Range(0, GameHandler.Instance.LargeHouses.Length)], 1);

				}
				else if (upgradedAmount == 1)
				{
					GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 2);
					GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.MediumHouses[Random.Range(0, GameHandler.Instance.MediumHouses.Length)], 1);
				}
				else
				{
					GameHandler.Instance.ConstructionHandler.CreateNewBuildingsAtRandomPosition(GameHandler.Instance.SmallHouses[Random.Range(0, GameHandler.Instance.SmallHouses.Length)], 2);
				}
			}
		}
	}*/
//}
