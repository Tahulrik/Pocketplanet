using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class CPRAccountData : MonoBehaviour 
{
	public string CPRNumber;
	public string accountNumber;

	public float currentBalance = 0;
	public float minWaitTime = 0.4f;
	public float maxWaitTime = 1.0f;

	public bool dataSaved = false;

	private GetAccountFromUserInput userData;

	private GetAccountFromUserInput tempData;

	public GetTransactionData[] transactions;

	GameObject CPRText;
	GameObject PINText;
	GameObject ErrorImage;
	Text ErrorText;
	string JSONData;

	public delegate void TransactionAction(float val);
	public static event TransactionAction TransactionDone;

	/*
	void Start()
	{
		DontDestroyOnLoad(this.gameObject);
		JSONData = PlayerPrefs.GetString("PlayerInfo");
		if(PlayerPrefs.GetInt("UserCreated", 0) == 1)
		{
			userData = JsonUtility.FromJson<GetAccountFromUserInput>(JSONData);
			tempData = JsonUtility.FromJson<GetAccountFromUserInput>(JSONData);
			//StartCoroutine(CheckForNewData());
			SceneManager.LoadScene("Main");
			StartCoroutine(IterateOverAllTransactions(minWaitTime, maxWaitTime));
		}
		CPRText = GameObject.Find("CPRText");
		PINText = GameObject.Find("PINText");
		ErrorImage = GameObject.Find("ErrorImage");
		ErrorText = ErrorImage.GetComponentInChildren<Text>();
		ErrorImage.SetActive(false);
	}*/

	void Start()
	{
		dataSaved = true;
	}

	IEnumerator FillTransactions()
	{
		int page = 1;
		int maxPages;
		int currentIndex = 0;
		string accountPage = "http://user186:bmGe5TEdFFVL@api.futurefinance.io/api/accounts/" + accountNumber + "/transactions/?page=" + page + "&sort=DESCENDING";
		WWW accountData = new WWW(accountPage);
		yield return accountData;

		string jsonAccountData = accountData.text;
		TransactionsSpace.AccountTransactions allAccountData = JsonUtility.FromJson<TransactionsSpace.AccountTransactions>(jsonAccountData);
		transactions = new GetTransactionData[allAccountData.total];

		maxPages = Mathf.CeilToInt((float)allAccountData.total / allAccountData.count);
		//print(maxPages);

		for (int i = 0; i < maxPages; i++)
		{
			accountPage = "http://user186:bmGe5TEdFFVL@api.futurefinance.io/api/accounts/" + accountNumber + "/transactions/?page=" + page + "&sort=DESCENDING";
			accountData = new WWW(accountPage);
			yield return accountData;

			jsonAccountData = accountData.text;
			allAccountData = JsonUtility.FromJson<TransactionsSpace.AccountTransactions>(jsonAccountData);
			for (int x = 0; x < allAccountData.count; x++)
			{
				transactions[currentIndex] = new GetTransactionData
				(
					allAccountData._embedded.transactions[x].transactionDateTimestamp,
					allAccountData._embedded.transactions[x].amount,
					allAccountData._embedded.transactions[x].transactionDate
				);
				currentIndex++;
			}
			page++;
			//print(page);
		}
		yield return new WaitForSeconds(0.5f);
		dataSaved = true;
		string allTransaction = JsonUtility.ToJson(allAccountData);
		print(allTransaction);
	}

	IEnumerator DataFiller()
	{
		StartCoroutine(GetUserDataToolkit.GetAccountData(CPRNumber, (var) => userData = var));
		yield return new WaitForSeconds(1f);
		//if (CheckUserData())
		{
			StartCoroutine(GetUserDataToolkit.GetBankAccountData(CPRNumber, (var) => userData = var));
			StartCoroutine(FillTransactionData(1f));
			StartCoroutine(FillPaymentAgreementsData(1f));
			StartCoroutine(SaveToJSON(2f));
			//StartCoroutine(IterateOverAllTransactions(minWaitTime, maxWaitTime));
			yield break;
		}
	}


	void Update()
	{
		if(Input.GetKeyDown(KeyCode.L))
		{
			PlayerPrefs.SetInt("UserCreated", 0);
		}
	}

	public void Login()
	{
		CPRNumber = "";
		userData.FullName = "";
		StartCoroutine(CheckCredentials());
	}

	IEnumerator IterateOverAllTransactions(float minWaitTime, float maxWaitTime)
	{
		yield return new WaitForSeconds(2.0f);
		for(int i = 0; i < userData.Transactions.Length; i++)
		{
			currentBalance += userData.Transactions[i].Amount;
			if(TransactionDone != null)
				TransactionDone(userData.Transactions[i].Amount);
			yield return new WaitForSeconds(UnityEngine.Random.Range(minWaitTime, maxWaitTime));
		}
	}

	IEnumerator FillTransactionData(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		for(int x = 0; x < userData.BankAccounts.Length; x++)
		{
			StartCoroutine(GetUserDataToolkit.GetTransactionData(userData.BankAccounts[x].accountNumber, (var)=>userData=var));
			print(userData.BankAccounts[x].accountNumber);
		}
	}
	IEnumerator FillPaymentAgreementsData(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		for(int x = 0; x < userData.BankAccounts.Length; x++)
			StartCoroutine(GetUserDataToolkit.GetPaymentAgreementsData(userData.BankAccounts[x].accountNumber, (var)=>userData=var));
	}
	IEnumerator SaveToJSON(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		PlayerPrefs.SetString("PlayerInfo", JsonUtility.ToJson(userData));
		PlayerPrefs.SetInt("UserCreated", 1);
		print("data saved");
		dataSaved = true;
	}
	IEnumerator CheckForNewData()
	{
		yield return new WaitForSeconds(1.0f);
		StartCoroutine(GetUserDataToolkit.GetAccountData(userData.BankAccounts[0]._embedded.owner.customerNumber, (var)=>userData=var));
		StartCoroutine(GetUserDataToolkit.GetBankAccountData(userData.BankAccounts[0]._embedded.owner.customerNumber, (var)=>userData=var));
		StartCoroutine(FillTransactionData(1f));
		StartCoroutine(FillPaymentAgreementsData(1f));
		yield return new WaitForSeconds(2.0f);
		if(!string.Equals(JsonUtility.ToJson(userData), JsonUtility.ToJson(tempData)))
		{
			PlayerPrefs.SetString("PlayerInfo", JsonUtility.ToJson(userData));
			print("Updated PlayerPrefs");
		}
	}
	IEnumerator CheckCredentials()
	{
		if(CheckCPRLength())
		{
			if(CheckPinLength())
			{
				StartCoroutine(GetUserDataToolkit.GetAccountData(CPRNumber, (var)=>userData=var));
				yield return new WaitForSeconds(1f);
				if(CheckUserData())
				{
					StartCoroutine(GetUserDataToolkit.GetBankAccountData(CPRNumber, (var)=>userData=var));
					StartCoroutine(FillTransactionData(1f));
					StartCoroutine(FillPaymentAgreementsData(1f));
					StartCoroutine(SaveToJSON(2f));
					SceneManager.LoadScene("Main");
					StartCoroutine(IterateOverAllTransactions(minWaitTime, maxWaitTime));
					yield break;
				}
			}
		}
		yield return new WaitForSeconds(1.0f);
		ErrorImage.SetActive(false);
	}
	bool CheckCPRLength()
	{
		CPRNumber = CPRText.GetComponent<Text>().text;
		if(CPRNumber.Length != CPRText.GetComponentInParent<InputField>().characterLimit)
		{
			ErrorImage.SetActive(true);
			ErrorText.text = "NOT A CORRECT CPR!";
			return false;
		}
		return true;
	}
	bool CheckPinLength()
	{
		if(PINText.GetComponent<Text>().text.Length != PINText.GetComponentInParent<InputField>().characterLimit)
		{
			ErrorImage.SetActive(true);
			ErrorText.text = "PLEASE INPUT A FOUR DIGIT PIN";
			return false;
		}
		return true;
	}
	bool CheckUserData()
	{
		if(userData.FullName == "" || userData.FullName == " ")
		{
			ErrorImage.SetActive(true);
			ErrorText.text = "NO VALID USER FOUND!";
			return false;
		}
		return true;
	}
}
