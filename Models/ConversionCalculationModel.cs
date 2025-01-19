namespace CLSF_Compare.Models
{
    public class ConversionCalculationModel
    {
        public decimal BankConvertedAmount { get; set; }
        public decimal ClearShiftConvertedAmount { get; set; }
        public decimal Savings { get; set; }

        public static ConversionCalculationModel Calculate(decimal amount, decimal bankRate, decimal bankFees, decimal clearShiftRate)
        {
            bankFees = bankFees == 0 ? 1.25m : bankFees;
            var b_ConvertedAmount = amount * bankRate;
            var cs_ConvertedAmount = amount * clearShiftRate;

            /* Because rate exchange will usually be higher than bank rate,
             So by using ClearShift we "gain"/save cs_ConvertedAmount - b_ConvertedAmount
             we also need to take into account the fees, so we need to subtract the fees from the "gain"  */
            var savings = cs_ConvertedAmount - b_ConvertedAmount - bankFees;

            return new ConversionCalculationModel
            {
                BankConvertedAmount = b_ConvertedAmount,
                ClearShiftConvertedAmount = cs_ConvertedAmount,
                Savings = savings
            };
        }
    }
}