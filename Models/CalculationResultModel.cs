namespace CLSF_Compare.Models
{
    public class CalculationResultModel
    {
        public decimal BankConversionCost { get; set; }
        public decimal ClearShiftConversionCost { get; set; }
        public decimal Savings { get; set; }

        public static CalculationResultModel Calculate(decimal amount, decimal bankRate, decimal bankFees, decimal clearShiftRate)
        {
            var bankCost = amount * bankRate + bankFees;
            var clearShiftCost = amount * clearShiftRate;

            return new CalculationResultModel
            {
                BankConversionCost = bankCost,
                ClearShiftConversionCost = clearShiftCost,
                Savings = bankCost - clearShiftCost
            };
        }
    }
}
