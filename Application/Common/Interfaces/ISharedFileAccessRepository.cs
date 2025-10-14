using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ISharedFileAccessRepository
{
    Task<SharedFileAccessEntity?> GetByFileIdAndUserIdAsync(int fileId, int userId);
    Task<IEnumerable<SharedFileAccessEntity>> GetByFileIdAsync(int fileId);
    Task<SharedFileAccessEntity> CreateAsync(SharedFileAccessEntity sharedFileAccess);
    Task DeleteAsync(SharedFileAccessEntity sharedFileAccess);
    Task<bool> HasAccessAsync(int fileId, int userId);
}
