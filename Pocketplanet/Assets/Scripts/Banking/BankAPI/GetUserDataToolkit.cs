using System.Collections;
using UnityEngine;

public class GetUserDataToolkit : MonoBehaviour {


	static string APIPage = "api.futurefinance.io/api/";
	static string username = "user186";
	static string password = "bmGe5TEdFFVL";

	public static GetAccountFromUserInput thisUserData;

	public static IEnumerator GetAccountData(string CPR, System.Action<GetAccountFromUserInput> userData)
	{
		string accountPage = "http://"+username+":"+password+"@"+APIPage+"customers/"+CPR;
		WWW accountData = new WWW(accountPage);
		yield return accountData;
		
		string jsonAccountData = accountData.text;
		CPRSpace.UserByCPR allAccountData = JsonUtility.FromJson<CPRSpace.UserByCPR>(jsonAccountData);
		thisUserData = new GetAccountFromUserInput(allAccountData.firstname, allAccountData.lastname);
		userData(thisUserData);
	}
	public static IEnumerator GetBankAccountData(string CPR, System.Action<GetAccountFromUserInput> userData)
	{
		string accountPage = "http://"+username+":"+password+"@"+APIPage+"customers/"+CPR+"/accounts/";
		WWW accountData = new WWW(accountPage);
		yield return accountData;
		
		string jsonAccountData = accountData.text;
		AccountCPRSpace.AccountByCPR allAccountData = JsonUtility.FromJson<AccountCPRSpace.AccountByCPR>(jsonAccountData);
		thisUserData.BankAccounts = new AccountCPRSpace.Accounts[allAccountData.count];
		for(int x = 0; x < allAccountData.count; x++)
		{
			thisUserData.BankAccounts[x] = allAccountData._embedded.accounts[x];
		}
		userData(thisUserData);
	}
	public static IEnumerator GetTransactionData(string accountNumber, System.Action<GetAccountFromUserInput> userData)
	{
		int page = 1;
		int maxPages;
		int currentIndex = 0;
		string accountPage = "http://"+username+":"+password+"@"+"api.futurefinance.io/api/accounts/"+accountNumber+"/transactions/?page="+page+"&sort=DESCENDING";
		WWW accountData = new WWW(accountPage);
		yield return accountData;

		string jsonAccountData = accountData.text;
		TransactionsSpace.AccountTransactions allAccountData = JsonUtility.FromJson<TransactionsSpace.AccountTransactions>(jsonAccountData);
		thisUserData.Transactions = new GetTransactionData[allAccountData.total];

		maxPages = Mathf.CeilToInt((float)allAccountData.total / allAccountData.count);

		for(int i = 0; i < maxPages; i++)
		{
			accountPage = "http://"+username+":"+password+"@"+"api.futurefinance.io/api/accounts/"+accountNumber+"/transactions/?page="+page+"&sort=DESCENDING";
			accountData = new WWW(accountPage);
			yield return accountData;
		
			jsonAccountData = accountData.text;
			allAccountData = JsonUtility.FromJson<TransactionsSpace.AccountTransactions>(jsonAccountData);
			for(int x = 0; x < allAccountData.count; x++)
			{
				thisUserData.Transactions[currentIndex] = new GetTransactionData
				(
					allAccountData._embedded.transactions[x].id,
					allAccountData._embedded.transactions[x].transactionDate,
					allAccountData._embedded.transactions[x].transactionDateTimestamp,
					allAccountData._embedded.transactions[x].merchantCategoryCode,
					allAccountData._embedded.transactions[x].amount,
					allAccountData._embedded.transactions[x].text,
					allAccountData._embedded.transactions[x].alternativeText,
					allAccountData._embedded.transactions[x].reconciled,
					allAccountData._embedded.transactions[x].paymentMedia,
					allAccountData._embedded.transactions[x].reserved
				);
				currentIndex++;
			}
			page++;
		}
		userData(thisUserData);
	}

	public static IEnumerator GetPaymentAgreementsData(string accountNumber, System.Action<GetAccountFromUserInput> userData)
	{
		string accountPage = "http://"+username+":"+password+"@"+APIPage+"accounts/"+accountNumber+"/paymentagreements/";
		WWW accountData = new WWW(accountPage);
		yield return accountData;
		
		string jsonAccountData = accountData.text;
		AgreementsSpace.AccountPaymentAgreements allAccountData = JsonUtility.FromJson<AgreementsSpace.AccountPaymentAgreements>(jsonAccountData);
		thisUserData.PaymentAgreements = new GetPaymentAgreementData[allAccountData.count];
		for(int x = 0; x < allAccountData.count; x++)
		{
			thisUserData.PaymentAgreements[x] = new GetPaymentAgreementData
			(
				allAccountData._embedded.paymentAgreements[x].agreementNumber,
				allAccountData._embedded.paymentAgreements[x].text,
				allAccountData._embedded.paymentAgreements[x].pbsDebitGroup,
				allAccountData._embedded.paymentAgreements[x].pbsCreditorGroup,
				allAccountData._embedded.paymentAgreements[x].pbsCustomerNumber,
				allAccountData._embedded.paymentAgreements[x].status
			);
		}
		userData(thisUserData);
	}
}
