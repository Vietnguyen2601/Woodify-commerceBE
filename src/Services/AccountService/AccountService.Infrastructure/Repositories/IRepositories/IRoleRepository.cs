using AccountService.Infrastructure.Repositories.Base;
using AccountService.Domain.Entities;

namespace AccountService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho Role
/// </summary>
public interface IRoleRepository : IBaseRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName);
}
