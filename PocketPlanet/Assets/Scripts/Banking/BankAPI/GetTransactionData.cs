[System.Serializable]
public class GetTransactionData
{
	string Text;
	string TransactionID;
	public string TransactionDate;
	public string TransactionDateTimestamp;
	int MerchantCategoryCode;
	public float Amount;
	string AlternativeText;
	bool Reconciled;
	string PaymentMedia;
	bool Reserved;

	public GetTransactionData(	string transactionID, string transactionDate, string transactionDateTimestamp, int merchantCategoryCode, float amount, string text, 
								string alternativeText, bool reconciled, string paymentMedia, bool reserved)
	{
		Text = text;
		TransactionID = transactionID;
		TransactionDate = transactionDate;
		TransactionDateTimestamp = transactionDateTimestamp;
		MerchantCategoryCode = merchantCategoryCode;
		Amount = amount;
		AlternativeText = alternativeText;
		Reconciled = reconciled;
		PaymentMedia = paymentMedia;
		Reserved = reserved;
	}

	public GetTransactionData(string transactionDateTimestamp, float amount)
	{
		TransactionDateTimestamp = transactionDateTimestamp;
		Amount = amount;	
	}

	public GetTransactionData(string transactionDateTimestamp, float amount, string transactionDate)
	{
		TransactionDateTimestamp = transactionDateTimestamp;
		Amount = amount;
		TransactionDate = transactionDate;
	}
}