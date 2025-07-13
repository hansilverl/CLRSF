using Microsoft.AspNetCore.Mvc;
using CurrencyComparisonTool.Models;

namespace CurrencyComparisonTool.Controllers
{
    public class PdfController : Controller
    {
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid file");
                return View();
            }

            var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
            var path = Path.Combine("wwwroot/uploads", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return RedirectToAction("Annotate", new { fileName });
        }

        [HttpGet]
        public IActionResult Annotate(string fileName)
        {
            ViewData["FileName"] = fileName;
            return View();
        }

        [HttpPost]
        public IActionResult SaveMapping([FromBody] List<PdfSelection> selections)
        {
            // TODO: map selections to your data model
            return Ok();
        }
    }
}
