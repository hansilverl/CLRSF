using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CurrencyComparisonTool.Models;
using CurrencyComparisonTool.Services;



namespace CurrencyComparisonTool.Controllers
{
    public class HomeController : Controller
    {
        private readonly IExchangeRateService _exchangeRateService;
        public HomeController(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService;
        }
        public IActionResult Index()
        {
            return View(new CurrencyComparisonModel());
        }

        [HttpPost]
        public IActionResult Calculate(CurrencyComparisonModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine($"ModelState is valid: {model.Date} {model.SourceCurrency} {model.TargetCurrency} {model.Amount} {model.BankRate} {model.BankFees}");
                    var clearShiftRate = _exchangeRateService.GetBOIRate(model.SourceCurrency, model.TargetCurrency, model.Date);
                    Console.WriteLine($"ClearShift Rate: {clearShiftRate}");
                    model = CurrencyComparisonModel.Calculate(model, clearShiftRate);
                }
                catch (Exception ex)
                {
                    // Set specific error message based on the exception
                    string errorMessage = ex.Message.Contains("No valid exchange rate found") 
                        ? $"Exchange rate not available for {model.SourceCurrency} to {model.TargetCurrency} on {model.Date:dd/MM/yyyy}. Please try a different date or currency pair."
                        : ex.Message.Contains("404") || ex.Message.Contains("Not Found")
                        ? "Bank of Israel exchange rate service is temporarily unavailable. Please try again in a few minutes."
                        : "Unable to fetch current exchange rates. Please check your connection and try again.";
                    
                    ViewData["ErrorMessage"] = errorMessage;
                    Console.WriteLine($"Error getting exchange rate: {ex.Message}");
                }
            }
            return View("Index", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult IndexWithNonce()
        {
            // Generate nonce for CSP
            ViewData["ScriptNonce"] = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            var model = new CurrencyComparisonModel();
            return View("Index", model);
        }
    }
}