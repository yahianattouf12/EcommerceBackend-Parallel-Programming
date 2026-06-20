using System;
using System.Threading.Tasks;

namespace ECommerceBackend.Services
{
    public interface ICacheService
    {
        Task<string?> GetStringAsync(string key);
        Task SetStringAsync(string key, string value, TimeSpan duration);
        Task RemoveAsync(string key);
    }
}
