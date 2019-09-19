namespace CPRSpace
{
	[System.Serializable]
	public class Embedded
	{
		
	}
	[System.Serializable]
	public class Self
	{
		public string href;
	}
	[System.Serializable]
	public class Links
	{
		public Self self;
	}
	[System.Serializable]
	public class UserByCPR
	{
		public string customerNumber;
		public string firstname;
		public string lastname;
		public Links _links;
		public Embedded _embedded;
	}
}
