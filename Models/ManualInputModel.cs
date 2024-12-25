namespace CLSF_Compare.Models
{
    public class ManualInputModel
    {
        public DateTime TransactionDate { get; set; }
        public decimal BankRate { get; set; }
        public decimal BankCommission { get; set; }
        public string SourceCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public decimal Amount { get; set; }
    }
    
    
}