using AccountService.Repositories.Base;
using AccountService.Repositories.Models;

namespace AccountService.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho Account
/// </summary>
public interface IAccountRepository : IBaseRepository<Account>
{
    Task<Account?> GetByUsernameAsync(string username);
    Task<Account?> GetByEmailAsync(string email);
}
