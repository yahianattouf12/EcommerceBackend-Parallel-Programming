
namespace ECommerceBackend.AOP;

[AttributeUsage(AttributeTargets.Method)]
public class CacheAttribute : Attribute
{
    public int Duration { get; }
    public CacheAttribute(int durationInSeconds)
    {
        Duration = durationInSeconds;
    }
}
