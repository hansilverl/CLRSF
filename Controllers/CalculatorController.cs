using Microsoft.AspNetCore.Mvc;
using CLSF_Compare.Models;
using CLSF_Compare.Services;
using System.Diagnostics;

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
    public IActionResult ManualInput(ManualInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Get BOI rate
        decimal boiRate = _exchangeRateService.GetBOIRate(model.SourceCurrency, model.TargetCurrency, model.TransactionDate);

        // Calculate costs
        decimal clearShiftTotalCost = model.Amount * boiRate; // ClearShift uses BOI rate with no commission
        decimal bankTotalCost = (model.Amount * model.BankRate) + model.BankCommission;
        var result = new CalculationResultModel
        {
            ClearShiftRate = boiRate,
            BankTotalCost = bankTotalCost,
            ClearShiftTotalCost = clearShiftTotalCost,
            PotentialSavings = bankTotalCost - clearShiftTotalCost
        };

        return View("Result", result);
    }
}