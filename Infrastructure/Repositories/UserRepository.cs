using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> GetByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
    }

    public async Task<UserEntity?> GetByUsernameAsync(string username)
    {
        var normalizedUsername = username.ToLowerInvariant();
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == normalizedUsername);
    }

    public async Task<UserEntity?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        var normalized = usernameOrEmail.ToLowerInvariant();
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == normalized || u.Email == normalized);
    }

    public async Task<UserEntity> CreateAsync(UserEntity user)
    {
        user.Username = user.Username.ToLowerInvariant();
        user.Email = user.Email.ToLowerInvariant();
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<UserEntity> UpdateAsync(UserEntity user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        var normalizedUsername = username.ToLowerInvariant();
        return await _context.Users.AnyAsync(u => u.Username == normalizedUsername);
    }
}
