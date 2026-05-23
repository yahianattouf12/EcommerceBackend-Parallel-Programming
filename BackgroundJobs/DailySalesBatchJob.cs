namespace ECommerceBackend.BackgroundJobs;

public class DailySalesBatchJob : BackgroundJob
{
    public DateTime Date { get; }

    public DailySalesBatchJob(DateTime date)
    {
        Date = date;
    }
}

