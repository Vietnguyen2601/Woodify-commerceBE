using IdentityService.Infrastructure.Repositories.Base;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho Role
/// </summary>
public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName);
}
