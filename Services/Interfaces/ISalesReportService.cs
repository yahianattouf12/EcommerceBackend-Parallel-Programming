public interface ISalesReportService
{
    Task<DailySalesReport> GenerateDailyReportSync(DateTime date);
}
