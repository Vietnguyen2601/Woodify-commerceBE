using AccountService.Repositories.Base;
using AccountService.Repositories.Models;

namespace AccountService.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho Role
/// </summary>
public interface IRoleRepository : IBaseRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName);
}
