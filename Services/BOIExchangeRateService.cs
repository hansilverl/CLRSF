using System.Net.Http;
using System;
using CLSF_Compare.Models;

namespace CLSF_Compare.Services
{
    public class BOIExchangeRateService : IExchangeRateService
    {
        public decimal GetBOIRate(string sourceCurrency, string targetCurrency, DateTime date)
        {
            string sURL = $"https://edge.boi.gov.il/FusionEdgeServer/sdmx/v2/data/dataflow/BOI.STATISTICS/EXR/1.0?startperiod={date:yyyy-MM-dd}&endperiod={date:yyyy-MM-dd}&format=csv";

            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(new HttpMethod("GET"), sURL);
                var response = httpClient.SendAsync(request).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var lines = result.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(1);   // Skip the header line

                    foreach (var item in lines)
                    {
                        var values = item.Split(',');
                        if (decimal.TryParse(values[13], out decimal rateValue) && rateValue > 0)
                        {
                            return rateValue;
                        }
                    }
                }
            }

            throw new Exception("Failed to fetch BOI rate.");
        }
    }
}
