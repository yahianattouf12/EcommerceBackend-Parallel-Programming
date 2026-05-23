public class DailySalesReport
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int TotalOrders { get; set; }
    public int TotalItemsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}
