using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly AppDbContext _context;

    public FileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FileEntity> CreateAsync(FileEntity file)
    {
        _context.Files.Add(file);

        await _context.SaveChangesAsync();

        return file;
    }

    public async Task<FileEntity> UpdateAsync(FileEntity file)
    {
        _context.Files.Update(file);

        await _context.SaveChangesAsync();

        return file;
    }

    public async Task<FileEntity?> GetByNameAndUserIdAsync(string name, int userId)
    {
        return await _context.Files.FirstOrDefaultAsync(f => f.Name == name && f.UserId == userId);
    }

    public async Task<List<FileEntity>> GetAllByUserIdAsync(int userId)
    {
        return await _context.Files.Where(f => f.UserId == userId).ToListAsync();
    }
}
