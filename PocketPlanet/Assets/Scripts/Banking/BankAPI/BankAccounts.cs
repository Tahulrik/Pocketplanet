namespace AccountsSpace
{
	[System.Serializable]
	public class OwnerEmbedded
	{
		
	}
	[System.Serializable]
	public class OwnerSelf
	{
		public string href;
	}
	[System.Serializable]
	public class OwnerLinks
	{
		public OwnerSelf self;
	}
	[System.Serializable]
	public class AccOwner
	{
		public string customerNumber;
		public string firstname;
		public string lastname;
		public OwnerLinks _links;
		public OwnerEmbedded _embedded;
	}
	[System.Serializable]
	public class AccEmbedded
	{
		public AccOwner owner;
	}
	[System.Serializable]
	public class AccPaymentAgreements
	{
		public string href;
	}
	[System.Serializable]
	public class AccTransactions
	{
		public string href;
	}
	[System.Serializable]
	public class AccSelf
	{
		public string href;
	}
	[System.Serializable]
	public class AccLinks
	{
		public AccSelf self;
		public AccTransactions transactions;
		public AccPaymentAgreements paymentagreements;
	}
	[System.Serializable]
	public class Accounts
	{
		public string accountNumber;
		public float balance;
		public string coowners;
		public string accountStatus;
		public float creditMax;
		public AccLinks _links;
		public AccEmbedded _embedded;
	}
	[System.Serializable]
	public class Embedded
	{
		public Accounts[] accounts;
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
	public class BankAccounts
	{
		public int count;
		public int total;
		public Links _links;
		public Embedded _embedded;
	}
}
