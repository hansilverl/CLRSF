using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CurrencyComparisonTool.Models;

namespace CurrencyComparisonTool.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new CurrencyComparisonModel());
        }

        [HttpPost]
        public IActionResult Calculate(CurrencyComparisonModel model)
        {
            if (ModelState.IsValid)
            {
                model = CurrencyComparisonModel.Calculate(model);
            }
            return View("Index", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}