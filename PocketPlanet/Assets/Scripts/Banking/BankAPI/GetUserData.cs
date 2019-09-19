[System.Serializable]
public class GetUserData
{
	public string Firstname;
	public string Lastname;
	public string CustomerNumber;
	public string AccountNumber;
	public string Coowners;
	public string AccountStatus;
	public float Balance;
	public float CreditMax;
	public string TransactionsPage;
	public string PaymentAgreementsPage;
	public GetTransactionData[] CustomerTransactions;
	public GetPaymentAgreementData[] PaymentAgreements;

	public GetUserData(string accountNumber, string transactionsPage)
	{
		AccountNumber = accountNumber;
		TransactionsPage = transactionsPage;
	}

	public GetUserData(string firstname, string lastname, string customerNumber, string accountNumber, string coowners, string accountStatus, 
						float balance, float creditMax, string transactionsPage, string paymentAgreementsPage)
	{
		Firstname = firstname;
		Lastname = lastname;
		CustomerNumber = customerNumber;
		AccountNumber = accountNumber;
		Coowners = coowners;
		AccountStatus = accountStatus;
		Balance = balance;
		CreditMax = creditMax;
		TransactionsPage = transactionsPage;
		PaymentAgreementsPage = paymentAgreementsPage;
	}
}
