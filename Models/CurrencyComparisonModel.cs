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

   
        public string BankFeesCurrency { get; set; } = "USD";

        [Range(0, double.MaxValue, ErrorMessage = "ClearShift Fees must be non-negative")]
        public decimal? CSFees { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
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

            // Ensure BankFeesCurrency is set
            model.BankFeesCurrency ??= model.SourceCurrency;

            // Calculate default bank fee as 1.25% of amount
            var defaultBankFeeAmount = model.Amount * 0.0125m; // 1.25% of amount

            // Calculate ClearShift fee: 0.59% or minimum fee (whichever is higher)
            var percentageFee = model.Amount * 0.0059m; // 0.59% of amount
            var minimumFee = CurrencyConstants.ClearShiftMinimumFees.TryGetValue(model.TargetCurrency, out var minFee) 
                ? minFee 
                : 25m; // Default minimum if currency not found
            
            // Convert minimum fee to source currency for comparison and calculation
            var minimumFeeInSourceCurrency = minimumFee;
            if (model.TargetCurrency != model.SourceCurrency)
            {
                // Convert minimum fee from target currency to source currency
                minimumFeeInSourceCurrency = minimumFee / clearshiftRate;
            }
            
            var defaultCSFeeAmount = Math.Max(percentageFee, minimumFeeInSourceCurrency);

            // Default fees if not supplied (as amounts in source currency)
            model.BankFees ??= defaultBankFeeAmount;
            model.CSFees ??= defaultCSFeeAmount;

            // Ensure valid rates
            if (model.BankRate <= 0 || clearshiftRate <= 0)
                throw new ArgumentException("Conversion rates must be positive.");

            // Convert bank fees to source currency if they're in target currency
            decimal bankFeesInSourceCurrency = model.BankFees.Value;
            if (model.BankFeesCurrency == model.TargetCurrency)
            {
                // Convert from target currency to source currency
                bankFeesInSourceCurrency = model.BankFees.Value / model.BankRate;
            }

            // Bank conversion - subtract fee from source amount before conversion
            var bankAmountAfterFees = model.Amount - bankFeesInSourceCurrency;
            model.b_convertedAmount = Decimal.Round(
                bankAmountAfterFees * model.BankRate, 4);

            // ClearShift conversion - subtract fee from source amount before conversion
            var csAmountAfterFees = model.Amount - model.CSFees.Value;
            model.cs_convertedAmount = Decimal.Round(
                csAmountAfterFees * clearshiftRate, 4);
            
            // Savings
            model.Savings = Decimal.Round(
                model.cs_convertedAmount - model.b_convertedAmount, 4);

            return model;
        }
    }
}
