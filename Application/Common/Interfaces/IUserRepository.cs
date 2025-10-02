using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(int id);
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<UserEntity?> GetByUsernameAsync(string username);
    Task<UserEntity> CreateAsync(UserEntity user);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
}
