using CurrencyComparisonTool.Models;

namespace CurrencyComparisonTool.Services
{
    public interface IExportService
    {
        byte[] GeneratePdfReport(ExportReportModel reportModel);
        Task<byte[]> GeneratePdfReportAsync(ExportReportModel reportModel);
        string GetReportFileName(ExportReportModel reportModel);
    }
}
