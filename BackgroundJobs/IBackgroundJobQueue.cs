using System.Diagnostics.CodeAnalysis;

namespace ECommerceBackend.BackgroundJobs;

public interface IBackgroundJobQueue
{
    void Enqueue(BackgroundJob job);
    bool TryDequeue(out BackgroundJob job);
}

