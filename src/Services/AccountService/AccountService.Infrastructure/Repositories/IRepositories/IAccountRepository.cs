using AccountService.Infrastructure.Repositories.Base;
using AccountService.Domain.Entities;

namespace AccountService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho Account
/// </summary>
public interface IAccountRepository : IBaseRepository<Account>
{
    Task<Account?> GetByUsernameAsync(string username);
    Task<Account?> GetByEmailAsync(string email);
}
