
namespace AgreementsSpace
{
	[System.Serializable]
	public class CustEmb
	{
		
	}
	[System.Serializable]
	public class CustLinks
	{

	}
	[System.Serializable]
	public class PACustomer
	{
		public string customerNumber;
		public string firstname;
		public string lastname;
		public CustLinks _links;
		public CustEmb _embedded;
	}
	[System.Serializable]
	public class OwnerEmbedded
	{

	}
	[System.Serializable]
	public class OwnerLinks
	{

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
	public class AccLinks
	{

	}
	[System.Serializable]
	public class PAAccount
	{
		public string accountNumber;
		public string balance;
		public string coowners;
		public string accountStatus;
		public string accountMax;
		public AccLinks _links;
		public AccEmbedded _embedded;
	}
	[System.Serializable]
	public class PAEmbedded
	{
		public PAAccount account;
		public PACustomer customer;
	}
	[System.Serializable]
	public class PALinks
	{

	}

	[System.Serializable]
	public class PaymentAgreements
	{
		public string agreementNumber;
		public string text;
		public string pbsDebitGroup;
		public string pbsCreditorGroup;
		public string pbsCustomerNumber;
		public string status;
		public PALinks _links;

	}
	[System.Serializable]
	public class Embedded
	{
		public PaymentAgreements[] paymentAgreements;
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
	public class Account
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
		public Account account;
		public First first;
	}
	[System.Serializable]
	public class AccountPaymentAgreements
	{
		public int count;
		public int total;
		public Links _links;
		public Embedded _embedded;
	}
}
