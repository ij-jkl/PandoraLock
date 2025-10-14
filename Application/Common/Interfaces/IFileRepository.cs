using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IFileRepository
{
    Task<FileEntity> CreateAsync(FileEntity file);
    Task<FileEntity> UpdateAsync(FileEntity file);
    Task<FileEntity?> GetByIdAsync(int id);
    Task<FileEntity?> GetByNameAndUserIdAsync(string name, int userId);
    Task<List<FileEntity>> GetAllByUserIdAsync(int userId);
    Task<List<FileEntity>> GetAccessibleFilesAsync(int userId);
    Task<List<FileEntity>> GetAllPublicFilesAsync();
    Task<List<FileEntity>> GetFilesSharedWithUserAsync(int userId);
}
