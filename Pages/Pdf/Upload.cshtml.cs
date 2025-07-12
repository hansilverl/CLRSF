using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UglyToad.PdfPig;
using System.Text.Json;

namespace CurrencyComparisonTool.Pages.Pdf
{
    public class UploadModel : PageModel
    {
        [BindProperty]
        public IFormFile? PdfFile { get; set; }
        public string? PdfData { get; set; }
        public string? WordDataJson { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (PdfFile == null || PdfFile.Length == 0 || Path.GetExtension(PdfFile.FileName).ToLower() != ".pdf")
            {
                ErrorMessage = "Please upload a valid PDF file.";
                return Page();
            }
            using var ms = new MemoryStream();
            await PdfFile.CopyToAsync(ms);
            var bytes = ms.ToArray();
            PdfData = Convert.ToBase64String(bytes);

            ms.Position = 0;
            var words = new List<PdfWord>();
            using (var document = PdfDocument.Open(ms))
            {
                int pageNum = 1;
                foreach (var page in document.GetPages())
                {
                    foreach (var word in page.GetWords())
                    {
                        var rect = word.BoundingBox;
                        words.Add(new PdfWord
                        {
                            Text = word.Text,
                            Page = pageNum,
                            X = rect.Left,
                            Y = page.Height - rect.Top,
                            Width = rect.Width,
                            Height = rect.Height
                        });
                    }
                    pageNum++;
                }
            }
            WordDataJson = JsonSerializer.Serialize(words);
            return Page();
        }

        public class PdfWord
        {
            public string Text { get; set; } = string.Empty;
            public int Page { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }
    }
}
