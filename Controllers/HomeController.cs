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
                Console.WriteLine($"ModelState is valid: {model.Date} {model.SourceCurrency} {model.TargetCurrency} {model.Amount} {model.BankRate} {model.BankFees}");
                var clearShiftRate = _exchangeRateService.GetBOIRate(model.SourceCurrency, model.TargetCurrency, model.Date);
                Console.WriteLine($"ClearShift Rate: {clearShiftRate}");
                model = CurrencyComparisonModel.Calculate(model, clearShiftRate);
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