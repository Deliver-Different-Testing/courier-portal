namespace CourierPortal.Core.Domain.Anz
{
    public class AnzDomesticLine
    {
        public decimal TransactionAmount { get; set; }
        public string AccountNo { get; set; }
        public string OtherPartyName { get; set; }
        public string OriginatorReference { get; set; }
        public string OriginatorAnalysisCode { get; set; }
        public string OriginatorParticulators { get; set; }
        public string OtherPartyReference { get; set; }
        public string OtherPartyAnalysisCode { get; set; }
        public string OtherPartyParticulars { get; set; }
    }
}
