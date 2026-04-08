namespace CourierPortal.Core.Domain.Bnz
{
    public class BnzPayrollTransaction
    {
        public string AccountNo { get; set; }
        public decimal TransactionAmount { get; set; }
        public string OtherPartyName { get; set; }
        public string OtherPartyReference { get; set; }
        public string OtherPartyCode { get; set; }
        public string OtherPartyParticulars { get; set; }
        public string YourName { get; set; }
        public string YourCode { get; set; }
        public string YourReference { get; set; }
        public string YourParticulars { get; set; }
    }
}
