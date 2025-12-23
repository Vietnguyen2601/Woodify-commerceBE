using AccountService.Infrastructure.Data.Context;
using AccountService.Infrastructure.Repositories.Base;
using AccountService.Infrastructure.Repositories.IRepositories;
using AccountService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Role
/// </summary>
public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    public RoleRepository(AccountDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.RoleName == roleName);
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(r => r.RoleId == id);
    }
}
