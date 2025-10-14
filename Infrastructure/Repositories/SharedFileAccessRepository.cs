using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SharedFileAccessRepository : ISharedFileAccessRepository
{
    private readonly AppDbContext _context;

    public SharedFileAccessRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SharedFileAccessEntity?> GetByFileIdAndUserIdAsync(int fileId, int userId)
    {
        return await _context.SharedFileAccess
            .FirstOrDefaultAsync(s => s.FileId == fileId && s.SharedWithUserId == userId);
    }

    public async Task<IEnumerable<SharedFileAccessEntity>> GetByFileIdAsync(int fileId)
    {
        return await _context.SharedFileAccess
            .Include(s => s.SharedWithUser)
            .Where(s => s.FileId == fileId)
            .ToListAsync();
    }

    public async Task<SharedFileAccessEntity> CreateAsync(SharedFileAccessEntity sharedFileAccess)
    {
        await _context.SharedFileAccess.AddAsync(sharedFileAccess);
        await _context.SaveChangesAsync();
        return sharedFileAccess;
    }

    public async Task DeleteAsync(SharedFileAccessEntity sharedFileAccess)
    {
        _context.SharedFileAccess.Remove(sharedFileAccess);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasAccessAsync(int fileId, int userId)
    {
        return await _context.SharedFileAccess
            .AnyAsync(s => s.FileId == fileId && s.SharedWithUserId == userId);
    }
}
