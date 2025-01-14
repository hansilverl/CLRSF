using Microsoft.AspNetCore.Mvc;
using CLSF_Compare.Models;
using CLSF_Compare.Services;

namespace CLSF_Compare.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly IExchangeRateService _exchangeRateService;

        public CalculatorController(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService;
        }

        [HttpGet]
        public IActionResult ManualInput()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ManualInput(InputModel input)
        {
            try
            {
                var clearShiftRate = _exchangeRateService.GetBOIRate(input.SourceCurrency, input.TargetCurrency, input.Date);

                var result = ConversionCalculationModel.Calculate(input.Amount, input.BankRate, input.BankFees, clearShiftRate);

                ViewBag.Result = result;
            }
            catch (Exception ex)
            {
                // Handle errors (e.g., API failures)
                ModelState.AddModelError("", $"Error: {ex.Message}");
                ViewBag.Result = null;
            }

            return View(input);
        }
    }
}