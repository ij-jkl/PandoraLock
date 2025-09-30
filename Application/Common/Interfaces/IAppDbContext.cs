using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<UserEntity> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}