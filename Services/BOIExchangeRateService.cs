using System;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using CurrencyComparisonTool.Models;

namespace CurrencyComparisonTool.Services
{
    public class BOIExchangeRateService : IExchangeRateService
    {
        public decimal GetBOIRate(string sourceCurrency, string targetCurrency, DateTime date)
        {
            if (sourceCurrency == targetCurrency)
                return 1.0m;

            if (string.Equals(targetCurrency, "ILS", StringComparison.OrdinalIgnoreCase))
            {
                return GetDirectRate(sourceCurrency, "ILS", date);
            }

            if (string.Equals(sourceCurrency, "ILS", StringComparison.OrdinalIgnoreCase))
            {
                var targetToILSRate = GetDirectRate(targetCurrency, "ILS", date);
                if (targetToILSRate == 0) throw new DivideByZeroException("Target rate is zero");
                return 1 / targetToILSRate;
            }

            // Both currencies are non-ILS – calculate cross rate
            var sourceToILS = GetDirectRate(sourceCurrency, "ILS", date);
            var targetToILS = GetDirectRate(targetCurrency, "ILS", date);

            if (targetToILS == 0) throw new DivideByZeroException("Cross conversion failed due to zero target rate");

            return Decimal.Round(sourceToILS / targetToILS, 6);
        }

        private decimal GetDirectRate(string fromCurrency, string toCurrency, DateTime date)
        {
            var rates = FindValidRatesInRange(fromCurrency, toCurrency, date);

            if (!rates.Any())
            {
                throw new Exception($"No valid exchange rate found for {fromCurrency} to {toCurrency}");
            }

            // Return the rate closest to the requested date
            var closestRate = rates.OrderBy(rate => Math.Abs((rate.Key - date).Days)).First();
            return closestRate.Value;
        }

        private Dictionary<DateTime, decimal> FindValidRatesInRange(string sourceCurrency, string targetCurrency, DateTime date)
        {
            int range = 7;
            var allRates = new Dictionary<DateTime, decimal>();

            using var httpClient = new HttpClient();

            while (range <= 30)
            {
                DateTime startDate = date.AddDays(-range);
                DateTime endDate = date.AddDays(range);

                string url = $"https://edge.boi.gov.il/FusionEdgeServer/sdmx/v2/data/dataflow/BOI.STATISTICS/EXR/1.0?" +
                             $"RER_{sourceCurrency}_{targetCurrency}&c%5BDATA_TYPE%5D=OF00" +
                             $"&startperiod={startDate:yyyy-MM-dd}&endperiod={endDate:yyyy-MM-dd}&format=csv";

                var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url)).Result;

                if (response.IsSuccessStatusCode)
                {
                    string csvResult = response.Content.ReadAsStringAsync().Result;
                    var lines = csvResult.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1);

                    foreach (var line in lines)
                    {
                        var rate = ParseBOIRateLine(line);
                        if (rate != null &&
                            rate.BASE_CURRENCY.Equals(sourceCurrency, StringComparison.OrdinalIgnoreCase) &&
                            rate.COUNTER_CURRENCY.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase))
                        {
                            allRates[rate.TIME_PERIOD] = rate.OBS_VALUE;
                        }
                    }

                    if (allRates.Any())
                        return allRates;
                }

                range += 7;
            }

            return allRates;
        }

        private BOIRate? ParseBOIRateLine(string line)
        {
            try
            {
                var values = line.Split(',');

                return new BOIRate
                {
                    SERIES_CODE = values[0],
                    FREQ = values[1],
                    BASE_CURRENCY = values[2],
                    COUNTER_CURRENCY = values[3],
                    UNIT_MEASURE = values[4],
                    DATA_TYPE = values[5],
                    DATA_SOURCE = values[6],
                    TIME_COLLECT = values[7],
                    CONF_STATUS = values[8],
                    PUB_WEBSITE = values[9],
                    UNIT_MULT = int.TryParse(values[10], out int unitMult) ? unitMult : 0,
                    COMMENTS = values[11],
                    TIME_PERIOD = DateTime.TryParse(values[12], out var timePeriod) ? timePeriod : default,
                    OBS_VALUE = decimal.TryParse(values[13], out decimal obsValue) ? obsValue : 0
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
