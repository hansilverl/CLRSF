using System;
using System.ComponentModel.DataAnnotations;

namespace CurrencyComparisonTool.Models
{
    public class ExportReportModel
    {
        public string ReportId { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public string CompanyName { get; set; } = "ClearShift";
        public string ReportTitle { get; set; } = "Currency Conversion Analysis Report";
        
        // Original comparison data
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string SourceCurrency { get; set; } = string.Empty;
        public string TargetCurrency { get; set; } = string.Empty;
        
        // Bank details
        public decimal BankRate { get; set; }
        public decimal BankFees { get; set; }
        public decimal BankConvertedAmount { get; set; }
        
        // ClearShift details
        public decimal ClearShiftRate { get; set; }
        public decimal ClearShiftFees { get; set; }
        public decimal ClearShiftConvertedAmount { get; set; }
        
        // Analysis
        public decimal TotalSavings { get; set; }
        public decimal SavingsPercentage { get; set; }
        
        // Additional metadata for future extension
        public string? ClientReference { get; set; }
        public string? Notes { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
        
        public static ExportReportModel FromComparisonModel(CurrencyComparisonModel model, decimal clearShiftRate)
        {
            var savingsPercentage = model.b_convertedAmount > 0 
                ? Math.Round((model.Savings / model.b_convertedAmount) * 100, 2) 
                : 0;
                
            return new ExportReportModel
            {
                TransactionDate = model.Date,
                Amount = model.Amount,
                SourceCurrency = model.SourceCurrency,
                TargetCurrency = model.TargetCurrency,
                BankRate = model.BankRate,
                BankFees = model.BankFees ?? 0,
                BankConvertedAmount = model.b_convertedAmount,
                ClearShiftRate = clearShiftRate,
                ClearShiftFees = model.CSFees ?? 0,
                ClearShiftConvertedAmount = model.cs_convertedAmount,
                TotalSavings = model.Savings,
                SavingsPercentage = savingsPercentage
            };
        }
    }
}
