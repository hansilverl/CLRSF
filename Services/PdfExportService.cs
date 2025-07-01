using CurrencyComparisonTool.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace CurrencyComparisonTool.Services
{
    public class PdfExportService : IExportService
    {
        private readonly IWebHostEnvironment _environment;
        
        public PdfExportService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public byte[] GeneratePdfReport(ExportReportModel reportModel)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 50, 50);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();
            
            // Add content
            AddHeader(document, reportModel);
            AddSeparator(document);
            AddTransactionDetails(document, reportModel);
            AddSeparator(document);
            AddComparisonAnalysis(document, reportModel);
            AddSeparator(document);
            AddSummary(document, reportModel);
            AddFooter(document, reportModel);
            
            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> GeneratePdfReportAsync(ExportReportModel reportModel)
        {
            return await Task.Run(() => GeneratePdfReport(reportModel));
        }

        public string GetReportFileName(ExportReportModel reportModel)
        {
            var timestamp = reportModel.GeneratedDate.ToString("yyyyMMdd_HHmmss");
            return $"ClearShift_Report_{reportModel.ReportId}_{timestamp}.pdf";
        }

        private void AddHeader(Document document, ExportReportModel reportModel)
        {
            // Company branding section
            var headerTable = new PdfPTable(2) { WidthPercentage = 100 };
            headerTable.SetWidths(new float[] { 2, 1 });
            
            // Left side - Company info with logo
            var companyCell = new PdfPCell();
            companyCell.Border = Rectangle.NO_BORDER;
            
            // Try to add logo
            var logoPath = Path.Combine(_environment.WebRootPath, "images", "logo.png");
            
            if (File.Exists(logoPath))
            {
                try
                {
                    var logo = Image.GetInstance(logoPath);
                    logo.ScaleToFit(120f, 40f);
                    logo.Alignment = Element.ALIGN_LEFT;
                    companyCell.AddElement(logo);
                    companyCell.AddElement(new Paragraph(" ")); // Space after logo
                }
                catch
                {
                    // If logo fails to load, fall back to text
                    companyCell.AddElement(new Paragraph("ClearShift", GetHeaderFont()));
                }
            }
            else
            {
                companyCell.AddElement(new Paragraph("ClearShift", GetHeaderFont()));
            }
            
            companyCell.AddElement(new Paragraph("Currency Conversion Analysis", GetSubHeaderFont()));
            headerTable.AddCell(companyCell);
            
            // Right side - Report info
            var reportInfoCell = new PdfPCell();
            reportInfoCell.Border = Rectangle.NO_BORDER;
            reportInfoCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            reportInfoCell.AddElement(new Paragraph($"Report ID: {reportModel.ReportId}", GetBodyFont()));
            reportInfoCell.AddElement(new Paragraph($"Generated: {reportModel.GeneratedDate:dd/MM/yyyy HH:mm}", GetBodyFont()));
            headerTable.AddCell(reportInfoCell);
            
            document.Add(headerTable);
            document.Add(new Paragraph(" ")); // Space
        }

        private void AddTransactionDetails(Document document, ExportReportModel reportModel)
        {
            document.Add(new Paragraph("Transaction Details", GetSectionHeaderFont()));
            document.Add(new Paragraph(" "));
            
            var table = new PdfPTable(2) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 1, 1 });
            
            AddTableRow(table, "Transaction Date:", reportModel.TransactionDate.ToString("dd/MM/yyyy"));
            AddTableRow(table, "Amount:", $"{reportModel.Amount:N2} {reportModel.SourceCurrency}");
            AddTableRow(table, "Currency Pair:", $"{reportModel.SourceCurrency} → {reportModel.TargetCurrency}");
            
            document.Add(table);
            document.Add(new Paragraph(" "));
        }

        private void AddComparisonAnalysis(Document document, ExportReportModel reportModel)
        {
            document.Add(new Paragraph("Conversion Comparison", GetSectionHeaderFont()));
            document.Add(new Paragraph(" "));
            
            var table = new PdfPTable(3) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 1, 1, 1 });
            
            // Header row
            AddTableHeaderCell(table, "");
            AddTableHeaderCell(table, "Traditional Bank");
            AddTableHeaderCell(table, "ClearShift");
            
            // Exchange rate
            AddTableRow(table, "Exchange Rate:", 
                reportModel.BankRate.ToString("N4"), 
                reportModel.ClearShiftRate.ToString("N4"));
            
            // Fees
            AddTableRow(table, "Fees (%):", 
                reportModel.BankFees.ToString("N2") + "%", 
                reportModel.ClearShiftFees.ToString("N2") + "%");
            
            // Final amount
            AddTableRow(table, $"Final Amount ({reportModel.TargetCurrency}):", 
                reportModel.BankConvertedAmount.ToString("N2"), 
                reportModel.ClearShiftConvertedAmount.ToString("N2"));
            
            document.Add(table);
            document.Add(new Paragraph(" "));
        }

        private void AddSummary(Document document, ExportReportModel reportModel)
        {
            document.Add(new Paragraph("Summary", GetSectionHeaderFont()));
            document.Add(new Paragraph(" "));
            
            var summaryTable = new PdfPTable(1) { WidthPercentage = 100 };
            
            var savingsText = reportModel.TotalSavings >= 0 
                ? $"Potential Savings with ClearShift: {reportModel.TotalSavings:N2} {reportModel.TargetCurrency}"
                : $"Additional Cost with ClearShift: {Math.Abs(reportModel.TotalSavings):N2} {reportModel.TargetCurrency}";
            
            var percentageText = reportModel.SavingsPercentage >= 0 
                ? $"Savings Percentage: {reportModel.SavingsPercentage:N2}%"
                : $"Additional Cost Percentage: {Math.Abs(reportModel.SavingsPercentage):N2}%";
            
            var summaryCell = new PdfPCell();
            summaryCell.BackgroundColor = new BaseColor(248, 249, 250);
            summaryCell.Padding = 15;
            summaryCell.AddElement(new Paragraph(savingsText, GetHighlightFont()));
            summaryCell.AddElement(new Paragraph(percentageText, GetBodyFont()));
            summaryTable.AddCell(summaryCell);
            
            document.Add(summaryTable);
            document.Add(new Paragraph(" "));
        }

        private void AddFooter(Document document, ExportReportModel reportModel)
        {
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));
            
            var footerParagraph = new Paragraph();
            footerParagraph.Add(new Chunk("This report is generated for informational purposes only. ", GetFooterFont()));
            footerParagraph.Add(new Chunk("Exchange rates are based on Bank of Israel published rates. ", GetFooterFont()));
            footerParagraph.Add(new Chunk("Actual rates may vary based on market conditions and transaction specifics.", GetFooterFont()));
            footerParagraph.Alignment = Element.ALIGN_CENTER;
            
            document.Add(footerParagraph);
        }

        private void AddSeparator(Document document)
        {
            var line = new Paragraph("_____________________________________________________________________");
            line.Font = new Font(Font.HELVETICA, 8, Font.NORMAL, new BaseColor(211, 211, 211));
            line.Alignment = Element.ALIGN_CENTER;
            document.Add(line);
            document.Add(new Paragraph(" "));
        }

        private void AddTableRow(PdfPTable table, string label, string value)
        {
            var labelCell = new PdfPCell(new Phrase(label, GetBodyFont()));
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.PaddingBottom = 5;
            table.AddCell(labelCell);
            
            var valueCell = new PdfPCell(new Phrase(value, GetBodyFont()));
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.PaddingBottom = 5;
            table.AddCell(valueCell);
        }

        private void AddTableRow(PdfPTable table, string label, string value1, string value2)
        {
            var labelCell = new PdfPCell(new Phrase(label, GetBodyFont()));
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.PaddingBottom = 5;
            table.AddCell(labelCell);
            
            var value1Cell = new PdfPCell(new Phrase(value1, GetBodyFont()));
            value1Cell.Border = Rectangle.NO_BORDER;
            value1Cell.PaddingBottom = 5;
            value1Cell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(value1Cell);
            
            var value2Cell = new PdfPCell(new Phrase(value2, GetBodyFont()));
            value2Cell.Border = Rectangle.NO_BORDER;
            value2Cell.PaddingBottom = 5;
            value2Cell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(value2Cell);
        }

        private void AddTableHeaderCell(PdfPTable table, string text)
        {
            var cell = new PdfPCell(new Phrase(text, GetSubHeaderFont()));
            cell.BackgroundColor = new BaseColor(52, 58, 64);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 8;
            table.AddCell(cell);
        }

        // Font definitions for professional appearance
        private Font GetHeaderFont() => new Font(Font.HELVETICA, 24, Font.BOLD, new BaseColor(52, 58, 64));
        private Font GetSubHeaderFont() => new Font(Font.HELVETICA, 14, Font.BOLD, new BaseColor(255, 255, 255));
        private Font GetSectionHeaderFont() => new Font(Font.HELVETICA, 16, Font.BOLD, new BaseColor(52, 58, 64));
        private Font GetBodyFont() => new Font(Font.HELVETICA, 11, Font.NORMAL, new BaseColor(73, 80, 87));
        private Font GetHighlightFont() => new Font(Font.HELVETICA, 14, Font.BOLD, new BaseColor(40, 167, 69));
        private Font GetFooterFont() => new Font(Font.HELVETICA, 9, Font.ITALIC, new BaseColor(108, 117, 125));
    }
}
