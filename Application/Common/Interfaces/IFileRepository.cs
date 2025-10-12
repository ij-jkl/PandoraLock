using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IFileRepository
{
    Task<FileEntity> CreateAsync(FileEntity file);
    Task<FileEntity> UpdateAsync(FileEntity file);
    Task<FileEntity?> GetByNameAndUserIdAsync(string name, int userId);
    Task<List<FileEntity>> GetAllByUserIdAsync(int userId);
}
