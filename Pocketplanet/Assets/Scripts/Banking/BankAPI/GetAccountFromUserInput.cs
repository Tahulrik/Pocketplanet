[System.Serializable]
public class GetAccountFromUserInput
{
	public string FullName;
	public AccountCPRSpace.Accounts[] BankAccounts;
	public GetTransactionData[] Transactions;
	public GetPaymentAgreementData[] PaymentAgreements;

	public GetAccountFromUserInput(string firstName, string lastName)
	{
		FullName = firstName + " " + lastName;
	}

	public GetAccountFromUserInput(string firstName, string lastName, AccountCPRSpace.Accounts[] bankAccounts, GetTransactionData[] transactions, GetPaymentAgreementData[] paymentAgreements)
	{
		FullName = firstName + " " + lastName;
		BankAccounts = bankAccounts;
		Transactions = transactions;
		PaymentAgreements = paymentAgreements;
	}
}
