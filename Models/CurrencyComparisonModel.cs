using System.ComponentModel.DataAnnotations;

namespace CurrencyComparisonTool.Models
{
    public class CurrencyComparisonModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Bank Rate must be positive")]
        public decimal BankRate { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Bank Fees must be non-negative")]
        public decimal BankFees { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }

        [Required]
        public string SourceCurrency { get; set; } = "USD";

        [Required]
        public string TargetCurrency { get; set; } = "ILS";

        public decimal BankCost { get; set; }
        public decimal ClearshiftCost { get; set; }
        public decimal Savings { get; set; }

        public static CurrencyComparisonModel Calculate(CurrencyComparisonModel model, decimal clearshiftRate)
        {

            // default bank fees to 1.25% if not provided
            model.BankFees = model.BankFees == 0 ? 1.25m : model.BankFees;
            // we'll remove the fees BEFORE applying the rate(so we're calculating how much you get in your account at the end)
            var bankAmount = model.Amount - (model.Amount * (model.BankFees / 100));
            var bankCalc = model.BankRate > 1 ? bankAmount * model.BankRate : bankAmount / (1m / model.BankRate);

            var csAmount = model.Amount - (model.Amount * (0.5m / 100)); // CS fee is 0.5%
            var csCalc = clearshiftRate > 1 ? csAmount * clearshiftRate : csAmount / (1m / clearshiftRate);

            model.BankCost = bankCalc;
            model.ClearshiftCost = csCalc; // you'd save more by using the ClearShift, cs_conv - bank_conv
            model.Savings = bankCalc - csCalc;

            return model;
        }
    }
}
