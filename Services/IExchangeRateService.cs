namespace CLSF_Compare.Services
{
    public interface IExchangeRateService
    {
        decimal GetBOIRate(string sourceCurrency, string targetCurrency, DateTime date);
    }
}
