using System.Collections;
using UnityEngine;

public class AccountDataTest : MonoBehaviour 
{
	string APIPage = "api.futurefinance.io/api/";
	string username = "user186";
	string password = "bmGe5TEdFFVL";
	
	long accountNrAsLong;

	//public string[] allUsers;



	public GetUserData[] userData;
	
	void Start()
	{
		StartCoroutine(GetAccountsData());
		print(GetServerTime.GetNistTime());
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			for(int i = 0; i < userData.Length; i++)
			{
				if(userData[i].Balance >= -50000 && userData[i].Balance != 0)
				{
					print(userData[i].Balance);
					print(userData[i].Firstname + " " + userData[i].Lastname);
				}
			}
		}
	}

	IEnumerator GetAccountsData()
	{
		int page = 1;
		int maxPages;
		int currentIndex = 0;
		string accountPage = "http://"+username+":"+password+"@"+"api.futurefinance.io/api/accounts/?page="+page+"&sort=DESCENDING";
		//string accountPage = "http://"+username+":"+password+"@"+APIPage+"accounts/";
		WWW accountData = new WWW(accountPage);
		yield return accountData;
		
		string jsonAccountData = accountData.text;
		AccountsSpace.BankAccounts allAcountData = JsonUtility.FromJson<AccountsSpace.BankAccounts>(jsonAccountData);

		userData = new GetUserData[allAcountData.total];
		maxPages = Mathf.CeilToInt((float)allAcountData.total / allAcountData.count);

		for(int i = 0; i < maxPages; i++)
		{
			accountPage = "http://"+username+":"+password+"@"+"api.futurefinance.io/api/accounts/?page="+page+"&sort=DESCENDING";
			accountData = new WWW(accountPage);
			yield return accountData;
		
			jsonAccountData = accountData.text;
			allAcountData = JsonUtility.FromJson<AccountsSpace.BankAccounts>(jsonAccountData);

			for(int x = 0; x < allAcountData.count; x++)
			{
				userData[currentIndex] = new GetUserData(allAcountData._embedded.accounts[x].accountNumber, allAcountData._embedded.accounts[x]._links.transactions.href);
			}

			for(int x = 0; x < allAcountData.count; x++)
			{
			
				string transactionsPage = "http://"+username+":"+password+"@"+APIPage+"accounts/"+userData[x].AccountNumber+"/transactions/";
				WWW transactionsData = new WWW(transactionsPage);
				yield return transactionsData;
				
				string jsonTransactionData = transactionsData.text;
				TransactionsSpace.AccountTransactions allAcountTransactions = JsonUtility.FromJson<TransactionsSpace.AccountTransactions>(jsonTransactionData);

				string paymentAgreementPage = "http://"+username+":"+password+"@"+APIPage+"accounts/"+userData[x].AccountNumber+"/paymentagreements/";
				WWW paymentAgreementData = new WWW(paymentAgreementPage);
				yield return paymentAgreementData;
				
				string jsonPaymentAgreementData = paymentAgreementData.text;
				AgreementsSpace.AccountPaymentAgreements allPaymentAgreeements = JsonUtility.FromJson<AgreementsSpace.AccountPaymentAgreements>(jsonPaymentAgreementData);

				userData[currentIndex] = new GetUserData(
					allAcountData._embedded.accounts[x]._embedded.owner.firstname, 
					allAcountData._embedded.accounts[x]._embedded.owner.lastname, 
					allAcountData._embedded.accounts[x]._embedded.owner.customerNumber, 
					allAcountData._embedded.accounts[x].accountNumber, 
					allAcountData._embedded.accounts[x].coowners, 
					allAcountData._embedded.accounts[x].accountStatus, 
					allAcountData._embedded.accounts[x].balance, 
					allAcountData._embedded.accounts[x].creditMax, 
					allAcountData._embedded.accounts[x]._links.transactions.href, 
					allAcountData._embedded.accounts[x]._links.paymentagreements.href);			
					//userData[currentIndex].CustomerTransactions = new GetTransactionData[allAcountTransactions.count];
					//userData[currentIndex].PaymentAgreements = new GetPaymentAgreementData[allPaymentAgreeements.count];

				/*for(int y = 0; y < allAcountTransactions.count; y++)
				{
					userData[currentIndex].CustomerTransactions[y] = new GetTransactionData(					
						allAcountTransactions._embedded.transactions[y].id,
						allAcountTransactions._embedded.transactions[y].transactionDate,
						allAcountTransactions._embedded.transactions[y].transactionDateTimestamp,
						allAcountTransactions._embedded.transactions[y].merchantCategoryCode,
						allAcountTransactions._embedded.transactions[y].amount,
						allAcountTransactions._embedded.transactions[y].text,
						allAcountTransactions._embedded.transactions[y].alternativeText,
						allAcountTransactions._embedded.transactions[y].reconciled,
						allAcountTransactions._embedded.transactions[y].paymentMedia,
						allAcountTransactions._embedded.transactions[y].reserved);
				}*/
				/*for(int z = 0; z < allPaymentAgreeements.count; z++)
				{
					userData[currentIndex].PaymentAgreements[z] = new GetPaymentAgreementData(
						allPaymentAgreeements._embedded.paymentAgreements[z].agreementNumber,
						allPaymentAgreeements._embedded.paymentAgreements[z].text,
						allPaymentAgreeements._embedded.paymentAgreements[z].pbsDebitGroup,
						allPaymentAgreeements._embedded.paymentAgreements[z].pbsCreditorGroup,
						allPaymentAgreeements._embedded.paymentAgreements[z].pbsCustomerNumber,
						allPaymentAgreeements._embedded.paymentAgreements[z].status);
				}*/
				currentIndex++;
			}
			page++;
		}
	}
}


