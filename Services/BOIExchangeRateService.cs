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
            // TODO: Add helper function that goes back to previous business day if the requested date is a weekend or holiday
            string sURL = $"https://edge.boi.gov.il/FusionEdgeServer/sdmx/v2/data/dataflow/BOI.STATISTICS/EXR/1.0?startperiod={date:yyyy-MM-dd}&endperiod={date:yyyy-MM-dd}&format=csv";

            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, sURL);
                var response = httpClient.SendAsync(request).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string csvResult = response.Content.ReadAsStringAsync().Result;

                    // Parse the CSV response
                    var lines = csvResult.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(1);

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
