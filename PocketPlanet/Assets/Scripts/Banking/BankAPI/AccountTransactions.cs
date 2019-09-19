
namespace TransactionsSpace
{

	[System.Serializable]
	public class Transactions
	{
		public string id;
		public string transactionDate;
		public string transactionDateTimestamp;
		public int merchantCategoryCode;
		public float amount;
		public string text;
		public string alternativeText;
		public bool reconciled;
		public string paymentMedia;
		public bool reserved;
	}
	[System.Serializable]
	public class Embedded
	{
		public Transactions[] transactions;
	}
	[System.Serializable]
	public class Next
	{
		public string href;
	}
	[System.Serializable]
	public class Last
	{
		public string href;
	}
	[System.Serializable]
	public class Prev
	{
		public string href;
	}
	[System.Serializable]
	public class Self
	{
		public string href;
	}
	[System.Serializable]
	public class First
	{
		public string href;
	}
	[System.Serializable]
	public class Links
	{
		public Next next;
		public Last last;
		public Prev prev;
		public Self self;
		public First first;
	}
	[System.Serializable]
	public class AccountTransactions
	{
		public int count;
		public int total;
		public Links _links;
		public Embedded _embedded;
	}
}