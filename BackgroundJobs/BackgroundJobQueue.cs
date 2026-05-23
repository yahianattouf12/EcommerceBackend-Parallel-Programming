using System.Collections.Concurrent;

namespace ECommerceBackend.BackgroundJobs;

public class BackgroundJobQueue : IBackgroundJobQueue
{
    private readonly ConcurrentQueue<BackgroundJob> _jobs = new();

    public void Enqueue(BackgroundJob job)
    {
        _jobs.Enqueue(job);
    }

    public bool TryDequeue(out BackgroundJob job)
    {
        return _jobs.TryDequeue(out job);
    }
}

