namespace CLSF_Compare.Models
{
    public class ConversionCalculationModel
    {
        public decimal BankConversionCost { get; set; }
        public decimal ClearShiftConversionCost { get; set; }
        public decimal Savings { get; set; }

        public static ConversionCalculationModel Calculate(decimal amount, decimal bankRate, decimal bankFees, decimal clearShiftRate)
        {
            var bankCost = amount * bankRate + bankFees;
            var clearShiftCost = amount * clearShiftRate;

            return new ConversionCalculationModel
            {
                BankConversionCost = bankCost,
                ClearShiftConversionCost = clearShiftCost,
                Savings = bankCost - clearShiftCost
            };
        }
    }
}
