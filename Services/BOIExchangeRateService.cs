using System;
using System.Net.Http;
using System.Linq;
using CLSF_Compare.Models;

namespace CLSF_Compare.Services
{
    public class BOIExchangeRateService : IExchangeRateService
    {
        public decimal GetBOIRate(string sourceCurrency, string targetCurrency, DateTime date)
        {
            DateTime? validDate = FindValidDate(sourceCurrency, targetCurrency, date);

            if (!validDate.HasValue)
            {
                throw new Exception("No valid date found for fetching the exchange rate.");
            }

            string sURL = $"https://edge.boi.gov.il/FusionEdgeServer/sdmx/v2/data/dataflow/BOI.STATISTICS/EXR/1.0?startperiod={validDate:yyyy-MM-dd}&endperiod={validDate:yyyy-MM-dd}&format=csv";

            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, sURL);
                var response = httpClient.SendAsync(request).Result;    // Async call to the API - don't block the main thread

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string csvResult = response.Content.ReadAsStringAsync().Result;

                    // Parse the CSV response
                    var lines = csvResult.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(1); // Skip the header

                    foreach (var line in lines)
                    {
                        var rate = ParseBOIRateLine(line);

                        if (rate != null && rate.BASE_CURRENCY == sourceCurrency && rate.COUNTER_CURRENCY == targetCurrency)
                        {
                            return rate.OBS_VALUE; // Return the OBS_VALUE for the relevant currency pair
                        }
                    }

                    throw new Exception("No matching rate found for the specified currencies.");
                }

                throw new Exception($"Failed to retrieve data from BOI API. Status code: {response.StatusCode}");
            }
        }

        private DateTime? FindValidDate(string sourceCurrency, string targetCurrency, DateTime date)
        {
            // Define a range of dates to check (7 days forward and backward)
            const int range = 7;
            var datesToCheck = Enumerable.Range(-range, 2 * range + 1)
                                         .Select(offset => date.AddDays(offset))
                                         .OrderBy(d => Math.Abs((d - date).Days)); // Closest dates first

            using (var httpClient = new HttpClient())
            {
                foreach (var checkDate in datesToCheck)
                {
                    string sURL = $"https://edge.boi.gov.il/FusionEdgeServer/sdmx/v2/data/dataflow/BOI.STATISTICS/EXR/1.0?startperiod={checkDate:yyyy-MM-dd}&endperiod={checkDate:yyyy-MM-dd}&format=csv";
                    var request = new HttpRequestMessage(HttpMethod.Get, sURL);
                    var response = httpClient.SendAsync(request).Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string csvResult = response.Content.ReadAsStringAsync().Result;

                        // Check if data exists in the response
                        var lines = csvResult.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(1);
                        if (lines.Any(line => ParseBOIRateLine(line)?.BASE_CURRENCY == sourceCurrency &&
                                              ParseBOIRateLine(line)?.COUNTER_CURRENCY == targetCurrency))
                        {
                            return checkDate; // Found a valid date
                        }
                    }
                }
            }

            return null; // No valid date found in the range
        }

        private BOIRate ParseBOIRateLine(string line)
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
