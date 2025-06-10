using System;
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
        public decimal? BankFees { get; set; }

        public decimal? CSFees { get; set; }

        [Required]
        [Range(30000.01, double.MaxValue, ErrorMessage = "Enter an amount over 30K")]
        public decimal Amount { get; set; }

        [Required]
        public string SourceCurrency { get; set; } = "USD";

        [Required]
        public string TargetCurrency { get; set; } = "ILS";

        public decimal b_convertedAmount { get; set; }
        public decimal cs_convertedAmount { get; set; }
        public decimal Savings { get; set; }

        public static CurrencyComparisonModel Calculate(CurrencyComparisonModel model, decimal clearshiftRate)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            const decimal DefaultBankFee = 1.25m;
            const decimal DefaultCSFee = 0.59m; // Default fees in percent

            // Default fees if not supplied
            model.BankFees ??= DefaultBankFee;
            model.CSFees ??= DefaultCSFee;

            // Ensure valid rates
            if (model.BankRate <= 0 || clearshiftRate <= 0)
                throw new ArgumentException("Conversion rates must be positive.");

            // Bank conversion
            model.b_convertedAmount = Decimal.Round(
                (model.Amount * model.BankRate) * (1 - (model.BankFees.Value / 100)), 4);

            // ClearShift conversion
            model.cs_convertedAmount = Decimal.Round(
                (model.Amount * clearshiftRate) * (1 - (model.CSFees.Value / 100)), 4);

            // Savings
            model.Savings = Decimal.Round(
                model.cs_convertedAmount - model.b_convertedAmount, 4);

            return model;
        }
    }
}
