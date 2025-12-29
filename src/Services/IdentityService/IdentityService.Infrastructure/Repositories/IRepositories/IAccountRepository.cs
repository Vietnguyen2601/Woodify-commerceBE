using IdentityService.Infrastructure.Repositories.Base;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho Account
/// </summary>
public interface IAccountRepository : IGenericRepository<Account>
{
    Task<Account?> GetByUsernameAsync(string username);
    Task<Account?> GetByEmailAsync(string email);
}
