namespace CLSF_Compare.Models
{
    public class ConversionCalculationModel
    {
        public decimal BankConvertedAmount { get; set; }
        public decimal ClearShiftConvertedAmount { get; set; }
        public decimal Savings { get; set; }

        public static ConversionCalculationModel Calculate(decimal amount, decimal bankRate, decimal bankFees, decimal clearShiftRate)
        {
            // default bank fees to 1.25% if not provided
            bankFees = bankFees == 0 ? 1.25m : bankFees;
            var b_ConvertedAmount = amount * bankRate;
            var cs_ConvertedAmount = amount * clearShiftRate;

            var b_Fees = amount * bankFees;
            // clearShift has one fee of 0.25%
            var cs_Fees = amount * (0.25m / 100);

            /* Because rate exchange will usually be higher than bank rate,
             So by using ClearShift we "gain"/save cs_ConvertedAmount - b_ConvertedAmount
             we also need to take into account the fees, so we need to subtract the fees from the "gain"  */
            var savings = (cs_ConvertedAmount - b_ConvertedAmount) - (b_Fees - cs_Fees);

            return new ConversionCalculationModel
            {
                BankConvertedAmount = b_ConvertedAmount,
                ClearShiftConvertedAmount = cs_ConvertedAmount,
                Savings = savings
            };
        }
    }
}