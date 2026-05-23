using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(int id, User updated)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new Exception("User not found");

        user.UserName = updated.UserName;
        user.Email = updated.Email;

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
