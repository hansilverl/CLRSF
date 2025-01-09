namespace CLSF_Compare.Models
{
    public class InputModel
    {
        public DateTime Date { get; set; }
        public string SourceCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal BankRate { get; set; }
        public decimal BankFees { get; set; }
    }
}
    /* TODO:
    * ask mentor how different fees work, some may very by amount, 
    * Are we calculating by net or sale gross . 
    * Clearshift fee is a percentage of the amount
    * Should we consider transactions under 3600 ILS? - What's the minimum amount for a transaction?
    * Is the clearshift fee (over 3600 a percentage or a flat fee(180)) 
    * also, will sometimes have to multiply to exchange rate, sometimes divide
    * Bank formula (with hidden fees) - need to find out HOW to take fees - there may be a few fees to consider for the bank
    * Clearshift formula is amount * rate - fees
    */