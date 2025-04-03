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
            // Try to find a valid date or fallback to an average rate if none exists
            var rates = FindValidRatesInRange(sourceCurrency, targetCurrency, date); 

            if (!rates.Any())
            {
                throw new Exception("No valid date or rates found for fetching the exchange rate.");
            }

            // If we found valid rates, calculate and return the closest date’s rate
            var closestRate = rates.OrderBy(rate => Math.Abs((rate.Key - date).Days)).First();
            return closestRate.Value;
        }

        private Dictionary<DateTime, decimal> FindValidRatesInRange(string sourceCurrency, string targetCurrency, DateTime date)
        {
            int range = 7; // Initial range of 7 days
            var allRates = new Dictionary<DateTime, decimal>();

            using (var httpClient = new HttpClient())
            {
                while (range <= 30) // Dynamically expand the range if needed, up to 30 days
                {
                    DateTime startDate = date.AddDays(-range);
                    DateTime endDate = date.AddDays(range);

                    // Construct a single API request for the entire range
                    string sURL = $"https://edge.boi.gov.il/FusionEdgeServer/sdmx/v2/data/dataflow/BOI.STATISTICS/EXR/1.0?c%5BDATA_TYPE%5D=OF00&startperiod={startDate:yyyy-MM-dd}&endperiod={endDate:yyyy-MM-dd}&format=csv";
                    var request = new HttpRequestMessage(HttpMethod.Get, sURL);
                    var response = httpClient.SendAsync(request).Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string csvResult = response.Content.ReadAsStringAsync().Result;

                        // Parse all lines and collect valid rates for the given currency pair
                        var lines = csvResult.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(1);
                        foreach (var line in lines)
                        {
                            var rate = ParseBOIRateLine(line);
                            if (rate != null && rate.BASE_CURRENCY == sourceCurrency && rate.COUNTER_CURRENCY == targetCurrency)
                            {
                                allRates[rate.TIME_PERIOD] = rate.OBS_VALUE;
                            }
                        }

                        // If we found any valid rates, return them
                        if (allRates.Any())
                        {
                            return allRates;
                        }
                    }

                    // Increase the range and try again
                    range += 7;
                }
            }

            // Return all collected rates (could be empty if no data is found)
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
            catch (Exception)
            {
                return null; // Return null if parsing fails
            }
        }
    }
}
