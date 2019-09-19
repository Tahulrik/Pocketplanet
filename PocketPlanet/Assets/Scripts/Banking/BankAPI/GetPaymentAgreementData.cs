[System.Serializable]
public class GetPaymentAgreementData
{
		public string Text;
		public string AgreementNumber;
		public string PbsDebitGroup;
		public string PbsCreditorGroup;
		public string PbsCustomerNumber;
		public string Status;

	public GetPaymentAgreementData(	string agreementNumber, string text, string pbsDebitGroup, string pbsCreditorGroup, string pbsCustomerNumber, string status)
	{
		Text = text;
		AgreementNumber = agreementNumber;
		PbsDebitGroup = pbsDebitGroup;
		PbsCreditorGroup = pbsCreditorGroup;
		PbsCustomerNumber = pbsCustomerNumber;
		Status = status;
	}
}