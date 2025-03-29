using System.ComponentModel.DataAnnotations;

namespace CurrencyComparisonTool.Models
{
    public class CurrencyComparisonModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Bank Rate must be positive")]
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

        public static CurrencyComparisonModel Calculate(CurrencyComparisonModel model)
        {
            decimal clearshiftRate = model.BankRate * 0.995m;
            model.BankCost = (model.Amount * model.BankRate) + model.BankFees;
            model.ClearshiftCost = model.Amount * clearshiftRate;
            model.Savings = model.BankCost - model.ClearshiftCost;
            
            return model;
        }
    }
}