using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IFileRepository
{
    Task<FileEntity> CreateAsync(FileEntity file);
    Task<FileEntity> UpdateAsync(FileEntity file);
    Task<FileEntity?> GetByNameAsync(string name);
    Task<List<FileEntity>> GetAllAsync();
}
