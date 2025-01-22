// Controllers/CalculatorController.cs
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Calculate(InputModel input)
        {
            try
            {
                var clearShiftRate = _exchangeRateService.GetBOIRate(input.SourceCurrency, input.TargetCurrency, input.Date);
                var result = ConversionCalculationModel.Calculate(input.Amount, input.BankRate, input.BankFees, clearShiftRate);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}