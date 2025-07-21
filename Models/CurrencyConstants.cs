namespace CurrencyComparisonTool.Models
{
    public static class CurrencyConstants
    {
        public static readonly Dictionary<string, string> CurrencySymbols = new()
        {
            { "USD", "$" },
            { "ILS", "₪" },
            { "EUR", "€" },
            { "GBP", "£" },
            { "CHF", "₣" },
            { "CAD", "C$" },
            { "AUD", "A$" },
            { "PLN", "zł" }
        };

        // Add AllowedCurrencies as a list of currency codes
        public static readonly List<string> AllowedCurrencies = CurrencySymbols.Keys.ToList();

        // ClearShift minimum fees by currency (target currency)
        public static readonly Dictionary<string, decimal> ClearShiftMinimumFees = new()
        {
            { "ILS", 50m },    // 50 ₪
            { "USD", 25m },    // $25 (general) / $13 (within US - not implemented yet)
            { "EUR", 25m },    // €25 (general) / €12 (within SEPA - not implemented yet)
            { "GBP", 25m },    // £25
            { "CHF", 25m },    // ₣25 (using same as GBP/EUR pattern)
            { "CAD", 25m },    // C$25 (using same pattern)
            { "AUD", 25m },    // A$25 (using same pattern)
            { "PLN", 100m }    // zł100 (estimated based on currency value)
        };
    }
}