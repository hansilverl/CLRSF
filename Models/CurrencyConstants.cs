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
    }
}