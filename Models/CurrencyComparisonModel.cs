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

        [Range(0, double.MaxValue, ErrorMessage = "Bank Fees must be non-negative")]
        public decimal BankFees { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }

        [Required]
        public string SourceCurrency { get; set; } = "USD";

        [Required]
        public string TargetCurrency { get; set; } = "ILS";

        private const decimal CS_fee = 0.5M; // differs between personal and business accounts, but leaving it abstract for now


        public decimal b_convertedAmount { get; set; }
        public decimal cs_convertedAmount { get; set; }
        public decimal Savings { get; set; }

        public static CurrencyComparisonModel Calculate(CurrencyComparisonModel model, decimal clearshiftRate)
        {

            // default bank fees to 1.25% if not provided
            model.BankFees = model.BankFees == 0 ? 1.25m : model.BankFees;

            // Calculate the converted amount for the bank
            model.b_convertedAmount = (model.Amount * model.BankRate) * (1 - (model.BankFees / 100));
            // Calculate the converted amount for ClearShift
            model.cs_convertedAmount = (model.Amount * clearshiftRate) * (1 - (CS_fee / 100));
            // Calculate the savings, with clearshift I should end up with more money than the bank
            model.Savings = model.cs_convertedAmount - model.b_convertedAmount;

            return model;
        }
    }
}
