using IdentityService.Infrastructure.Data.Context;
using IdentityService.Infrastructure.Repositories.Base;
using IdentityService.Infrastructure.Repositories.IRepositories;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
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
