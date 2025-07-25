using CurrencyComparisonTool.Models;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
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
            var document = CreateDocument(reportModel);
            var renderer = new PdfDocumentRenderer(true) { Document = document };
            renderer.RenderDocument();

            using var stream = new MemoryStream();
            renderer.PdfDocument.Save(stream);
            return stream.ToArray();
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

        private Document CreateDocument(ExportReportModel reportModel)
        {
            var doc = new Document();
            DefineStyles(doc);

            var section = doc.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.LeftMargin = Unit.FromPoint(50);
            section.PageSetup.RightMargin = Unit.FromPoint(50);
            section.PageSetup.TopMargin = Unit.FromPoint(50);
            section.PageSetup.BottomMargin = Unit.FromPoint(50);

            AddHeader(section, reportModel);
            AddSeparator(section);
            AddTransactionDetails(section, reportModel);
            AddSeparator(section);
            AddComparisonAnalysis(section, reportModel);
            AddSeparator(section);
            AddSummary(section, reportModel);
            AddFooter(section);

            return doc;
        }

        private void DefineStyles(Document doc)
        {
            var normal = doc.Styles["Normal"];
            normal.Font.Name = "Helvetica";
            normal.Font.Size = 11;
            normal.Font.Color = Color.FromRgb(73, 80, 87);

            var header = doc.Styles.AddStyle("Header", "Normal");
            header.Font.Size = 24;
            header.Font.Bold = true;
            header.Font.Color = Color.FromRgb(52, 58, 64);

            var subHeader = doc.Styles.AddStyle("SubHeader", "Normal");
            subHeader.Font.Size = 14;
            subHeader.Font.Bold = true;
            subHeader.Font.Color = Color.FromRgb(255, 255, 255);

            var sectionHeader = doc.Styles.AddStyle("SectionHeader", "Normal");
            sectionHeader.Font.Size = 16;
            sectionHeader.Font.Bold = true;
            sectionHeader.Font.Color = Color.FromRgb(52, 58, 64);

            var body = doc.Styles.AddStyle("Body", "Normal");
            body.Font.Size = 11;
            body.Font.Color = Color.FromRgb(73, 80, 87);

            var highlight = doc.Styles.AddStyle("Highlight", "Normal");
            highlight.Font.Size = 14;
            highlight.Font.Bold = true;
            highlight.Font.Color = Color.FromRgb(40, 167, 69);

            var footer = doc.Styles.AddStyle("Footer", "Normal");
            footer.Font.Size = 9;
            footer.Font.Italic = true;
            footer.Font.Color = Color.FromRgb(108, 117, 125);
        }

        private void AddHeader(Section section, ExportReportModel reportModel)
        {
            var table = section.AddTable();
            table.Borders.Width = 0;
            table.Rows.LeftIndent = 0;
            table.AddColumn(Unit.FromCentimeter(12));
            table.AddColumn(Unit.FromCentimeter(6));
            var row = table.AddRow();
            row.TopPadding = 0;
            row.BottomPadding = 0;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Top;
            row.Cells[1].VerticalAlignment = VerticalAlignment.Top;

            // Left: Logo and company info
            var companyCell = row.Cells[0];
            companyCell.Borders.Width = 0;
            var logoPath = Path.Combine(_environment.WebRootPath, "images", "logo.png");
            if (File.Exists(logoPath))
            {
                try
                {
                    var logoPara = companyCell.AddParagraph();
                    var image = logoPara.AddImage(logoPath);
                    image.LockAspectRatio = true;
                    image.Width = Unit.FromPoint(120);
                    logoPara.Format.SpaceAfter = Unit.FromPoint(0);
                }
                catch
                {
                    var companyPara = companyCell.AddParagraph("ClearShift");
                    companyPara.Style = "Header";
                }
            }
            else
            {
                var companyPara = companyCell.AddParagraph("ClearShift");
                companyPara.Style = "Header";
            }
            var subHeaderPara = companyCell.AddParagraph("Currency Conversion Analysis");
            subHeaderPara.Style = "SubHeader";
            subHeaderPara.Format.SpaceBefore = Unit.FromPoint(0);
            // Add blank line after logo/subheader
            var blank = companyCell.AddParagraph("");
            blank.Format.SpaceBefore = Unit.FromPoint(2);
            blank.Format.SpaceAfter = Unit.FromPoint(2);

            // Right: Report info (Report ID and Generated Date)
            var infoCell = row.Cells[1];
            infoCell.Borders.Width = 0;
            infoCell.Format.Alignment = ParagraphAlignment.Right;
            infoCell.VerticalAlignment = VerticalAlignment.Top;
            var infoTable = infoCell.Elements.AddTable();
            infoTable.Borders.Width = 0;
            infoTable.AddColumn(Unit.FromCentimeter(6));
            var infoRow1 = infoTable.AddRow();
            var infoRow2 = infoTable.AddRow();
            infoRow1.Cells[0].AddParagraph($"Report ID: {reportModel.ReportId}").Style = "Body";
            infoRow2.Cells[0].AddParagraph($"Generated: {reportModel.GeneratedDate:dd/MM/yyyy HH:mm}").Style = "Body";
            foreach (Row r in infoTable.Rows)
            {
                r.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                r.Cells[0].VerticalAlignment = VerticalAlignment.Center;
                r.Cells[0].Borders.Width = 0;
                r.TopPadding = 0;
                r.BottomPadding = 0;
            }
            // Add blank line after header
            var headerSpace = section.AddParagraph("");
            headerSpace.Format.SpaceBefore = Unit.FromPoint(2);
            headerSpace.Format.SpaceAfter = Unit.FromPoint(2);
        }

        private void AddTransactionDetails(Section section, ExportReportModel reportModel)
        {
            section.AddParagraph("Transaction Details", "SectionHeader");
            var space = section.AddParagraph("");
            space.Format.SpaceBefore = Unit.FromPoint(2);
            space.Format.SpaceAfter = Unit.FromPoint(2);
            var table = section.AddTable();
            table.Borders.Width = 0;
            table.AddColumn(Unit.FromCentimeter(8));
            table.AddColumn(Unit.FromCentimeter(10));

            AddTableRow(table, "Transaction Date:", reportModel.TransactionDate.ToString("dd/MM/yyyy"));
            AddTableRow(table, "Amount:", $"{reportModel.Amount:N2} {reportModel.SourceCurrency}");
            AddTableRow(table, "Currency Pair:", $"{reportModel.SourceCurrency} → {reportModel.TargetCurrency}");
            // Add blank line after section
            var after = section.AddParagraph("");
            after.Format.SpaceBefore = Unit.FromPoint(2);
            after.Format.SpaceAfter = Unit.FromPoint(2);
        }

        private void AddComparisonAnalysis(Section section, ExportReportModel reportModel)
        {
            section.AddParagraph("Conversion Comparison", "SectionHeader");
            var space = section.AddParagraph("");
            space.Format.SpaceBefore = Unit.FromPoint(2);
            space.Format.SpaceAfter = Unit.FromPoint(2);
            var table = section.AddTable();
            table.Borders.Width = 0;
            table.AddColumn(Unit.FromCentimeter(6));
            table.AddColumn(Unit.FromCentimeter(6));
            table.AddColumn(Unit.FromCentimeter(6));

            // Header row
            var headerRow = table.AddRow();
            headerRow.Shading.Color = Color.FromRgb(52, 58, 64);
            headerRow.Cells[0].AddParagraph("");
            headerRow.Cells[1].AddParagraph("Traditional Bank").Style = "SubHeader";
            headerRow.Cells[2].AddParagraph("ClearShift").Style = "SubHeader";
            foreach (Cell cell in headerRow.Cells)
            {
                cell.Format.Alignment = ParagraphAlignment.Center;
                cell.VerticalAlignment = VerticalAlignment.Center;
                cell.Borders.Width = 0;
                cell.Format.Font.Color = Color.FromRgb(255, 255, 255);
                cell.Format.Font.Bold = true;
                cell.Shading.Color = Color.FromRgb(52, 58, 64);
                cell.Format.SpaceBefore = Unit.FromPoint(8);
                cell.Format.SpaceAfter = Unit.FromPoint(8);
            }

            // Data rows
            AddComparisonTableRow(table, "Exchange Rate:",
                reportModel.BankRate.ToString("N4"),
                reportModel.ClearShiftRate.ToString("N4"));
            
            // Get currency symbols for fees
            var bankFeeSymbol = CurrencyConstants.CurrencySymbols.TryGetValue(reportModel.BankFeesCurrency, out var bankSymbol) ? bankSymbol : "";
            var clearShiftFeeSymbol = CurrencyConstants.CurrencySymbols.TryGetValue(reportModel.SourceCurrency, out var csSymbol) ? csSymbol : "";
            
            AddComparisonTableRow(table, "Fees:",
                $"{bankFeeSymbol}{reportModel.BankFees:N2}",
                $"{clearShiftFeeSymbol}{reportModel.ClearShiftFees:N2}");
            
            // Get currency symbol for final amounts
            var targetSymbol = CurrencyConstants.CurrencySymbols.TryGetValue(reportModel.TargetCurrency, out var tSymbol) ? tSymbol : "";
            
            AddComparisonTableRow(table, $"Final Amount:",
                $"{targetSymbol}{reportModel.BankConvertedAmount:N2}",
                $"{targetSymbol}{reportModel.ClearShiftConvertedAmount:N2}");
            // Add blank line after section
            var after = section.AddParagraph("");
            after.Format.SpaceBefore = Unit.FromPoint(2);
            after.Format.SpaceAfter = Unit.FromPoint(2);
        }

        private void AddSummary(Section section, ExportReportModel reportModel)
        {
            section.AddParagraph("Summary", "SectionHeader");
            var space = section.AddParagraph("");
            space.Format.SpaceBefore = Unit.FromPoint(2);
            space.Format.SpaceAfter = Unit.FromPoint(2);
            var table = section.AddTable();
            table.Borders.Width = 0;
            table.AddColumn(Unit.FromCentimeter(18));
            var row = table.AddRow();
            var cell = row.Cells[0];
            cell.Shading.Color = Color.FromRgb(248, 249, 250);
            cell.Format.LeftIndent = Unit.FromPoint(15);
            cell.Format.RightIndent = Unit.FromPoint(15);
            cell.Format.SpaceBefore = Unit.FromPoint(10);
            cell.Format.SpaceAfter = Unit.FromPoint(10);
            cell.Borders.Width = 0;

            // Get currency symbol for target currency
            var targetSymbol = CurrencyConstants.CurrencySymbols.TryGetValue(reportModel.TargetCurrency, out var tSymbol) ? tSymbol : "";
            
            var savingsText = reportModel.TotalSavings >= 0
                ? $"Potential Savings with ClearShift: {targetSymbol}{reportModel.TotalSavings:N2}"
                : $"Additional Cost with ClearShift: {targetSymbol}{Math.Abs(reportModel.TotalSavings):N2}";
            var percentageText = reportModel.SavingsPercentage >= 0
                ? $"Savings Percentage: {reportModel.SavingsPercentage:N2}%"
                : $"Additional Cost Percentage: {Math.Abs(reportModel.SavingsPercentage):N2}%";

            var savingsPara = cell.AddParagraph(savingsText);
            savingsPara.Style = "Highlight";
            var percentagePara = cell.AddParagraph(percentageText);
            percentagePara.Style = "Body";
            // Add blank line after section
            var after = section.AddParagraph("");
            after.Format.SpaceBefore = Unit.FromPoint(2);
            after.Format.SpaceAfter = Unit.FromPoint(2);
        }

        private void AddFooter(Section section)
        {
            // Add blank line before footer
            var space = section.AddParagraph("");
            space.Format.SpaceBefore = Unit.FromPoint(6);
            space.Format.SpaceAfter = Unit.FromPoint(6);
            var footerPara = section.AddParagraph();
            footerPara.AddText("This report is generated for informational purposes only. ");
            footerPara.AddText("Exchange rates are based on Bank of Israel published rates. ");
            footerPara.AddText("Actual rates may vary based on market conditions and transaction specifics.");
            footerPara.Style = "Footer";
            footerPara.Format.Alignment = ParagraphAlignment.Center;
        }

        private void AddSeparator(Section section)
        {
            var separatorPara = section.AddParagraph("_____________________________________________________________________\n");
            separatorPara.Format.Font.Size = 8;
            separatorPara.Format.Font.Color = Color.FromRgb(211, 211, 211);
            separatorPara.Format.Alignment = ParagraphAlignment.Center;
            // Add blank line after separator
            var space = section.AddParagraph("");
            space.Format.SpaceBefore = Unit.FromPoint(2);
            space.Format.SpaceAfter = Unit.FromPoint(2);
        }

        private void AddTableRow(Table table, string label, string value)
        {
            var row = table.AddRow();
            row.Format.SpaceAfter = Unit.FromPoint(0);
            row.TopPadding = 5;
            row.BottomPadding = 5;

            var labelCell = row.Cells[0];
            labelCell.AddParagraph(label).Style = "Body";
            labelCell.Borders.Width = 0;

            var valueCell = row.Cells[1];
            valueCell.AddParagraph(value).Style = "Body";
            valueCell.Borders.Width = 0;
        }

        private void AddComparisonTableRow(Table table, string label, string value1, string value2)
        {
            var row = table.AddRow();
            row.Height = Unit.FromPoint(25);
            row.TopPadding = 5;
            row.BottomPadding = 5;

            var labelCell = row.Cells[0];
            labelCell.Format.LeftIndent = Unit.FromPoint(10);
            labelCell.VerticalAlignment = VerticalAlignment.Center;
            labelCell.Borders.Width = 0;
            labelCell.AddParagraph(label).Style = "Body";

            var value1Cell = row.Cells[1];
            value1Cell.Format.Alignment = ParagraphAlignment.Center;
            value1Cell.VerticalAlignment = VerticalAlignment.Center;
            value1Cell.Borders.Width = 0;
            value1Cell.AddParagraph(value1).Style = "Body";

            var value2Cell = row.Cells[2];
            value2Cell.Format.Alignment = ParagraphAlignment.Center;
            value2Cell.VerticalAlignment = VerticalAlignment.Center;
            value2Cell.Borders.Width = 0;
            value2Cell.AddParagraph(value2).Style = "Body";
        }
    }
}