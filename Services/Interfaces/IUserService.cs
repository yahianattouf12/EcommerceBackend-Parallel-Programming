using ECommerceBackend.Models;

namespace ECommerceBackend.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(int id, User user);
    Task<bool> DeleteAsync(int id);
}
