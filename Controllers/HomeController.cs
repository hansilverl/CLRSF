using Microsoft.AspNetCore.Mvc;
using CLSF_Compare.Models;
using CLSF_Compare.Services;
using CurrencyComparisonTool.Services;

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
                ModelState.AddModelError("", $"Error: {ex.Message}");
                ViewBag.Result = null;
            }

            return View(input);
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file selected.");
            }

            List<string[]> csvData = new List<string[]>();

            using (var stream = new StreamReader(file.OpenReadStream()))
            {
                while (!stream.EndOfStream)
                {
                    var line = await stream.ReadLineAsync();
                    var values = line.Split(',');
                    csvData.Add(values);
                }
            }

            ViewBag.CsvData = csvData;

            return View("Upload");
        }

    } 
}
