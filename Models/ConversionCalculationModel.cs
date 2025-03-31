
namespace CLSF_Compare.Models
{
    public class ConversionCalculationModel
    {
        private const decimal CS_fee = 0.5m; // differs between personal and business accounts, but leaving it abstract for now
        public decimal BankConvertedAmount { get; set; }
        public decimal ClearShiftConvertedAmount { get; set; }
        public decimal Savings { get; set; }

        public static ConversionCalculationModel Calculate(decimal amount, decimal bankRate, decimal bankFees, decimal clearShiftRate)
        {
            // default bank fees to 1.25% if not provided
            bankFees = bankFees == 0 ? 1.25m : bankFees;
            // we'll remove the fees BEFORE applying the rate(so we're calculating how much you get in your account at the end)
            // check if bank fee is in
            var bankAmount = amount - (amount * (bankFees / 100));
            var bankCalc = bankRate > 1 ? bankAmount * bankRate : bankAmount / (1m / bankRate);
            
            var csAmount = amount - (amount * (CS_fee / 100));
            var csCalc = clearShiftRate > 1 ? csAmount * clearShiftRate : csAmount / (1m / clearShiftRate);

            return new ConversionCalculationModel
            {
                BankConvertedAmount = bankCalc,
                ClearShiftConvertedAmount = csCalc, // you'd save more by using the ClearShift, cs_conv - bank_conv
                Savings = bankCalc - csCalc
            };
        }
    }
}