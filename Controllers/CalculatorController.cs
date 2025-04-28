using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CLSF_Compare.Controllers
{
    public class CalculatorController : Controller
    {
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file selected.");
            }

            try
            {
                List<string[]> csvData = new List<string[]>();

                using (var stream = new StreamReader(file.OpenReadStream()))
                {
                    while (!stream.EndOfStream)
                    {
                        var line = await stream.ReadLineAsync();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var values = line.Split(',');
                            csvData.Add(values);
                        }
                    }
                }

                ViewBag.CsvData = csvData;

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return BadRequest("Error processing file.");
            }
        }
    }
}

        
    


