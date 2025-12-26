using IdentityService.Infrastructure.Data.Context;
using IdentityService.Infrastructure.Repositories.Base;
using IdentityService.Infrastructure.Repositories.IRepositories;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Account
/// </summary>
public class AccountRepository : GenericRepository<Account>, IAccountRepository
{
    public AccountRepository(AccountDbContext context) : base(context)
    {
    }

    public override async Task<Account?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.AccountId == id);
    }

    public async Task<Account?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.Username == username);
    }

    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    public override async Task<List<Account>> GetAllAsync()
    {
        return await _dbSet
            .Include(a => a.Role)
            .ToListAsync();
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(a => a.AccountId == id);
    }
}
