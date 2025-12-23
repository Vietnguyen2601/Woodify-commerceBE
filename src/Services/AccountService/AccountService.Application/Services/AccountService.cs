using AccountService.Application.DTOs;
using AccountService.Application.Mappers;
using AccountService.Domain.Entities;
using AccountService.Infrastructure.Repositories.IRepositories;

namespace AccountService.Application.Services;

/// <summary>
/// Account Business Service
/// Xử lý business logic cho Account
/// </summary>
public class AccountService : Interfaces.IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRoleRepository _roleRepository;

    public AccountService(IAccountRepository accountRepository, IRoleRepository roleRepository)
    {
        _accountRepository = accountRepository;
        _roleRepository = roleRepository;
    }

    public async Task<AccountDto?> GetByIdAsync(Guid id)
        => (await _accountRepository.GetByIdAsync(id))?.ToDto();

    public async Task<AccountDto?> GetByUsernameAsync(string username)
        => (await _accountRepository.GetByUsernameAsync(username))?.ToDto();

    public async Task<IEnumerable<AccountDto>> GetAllAsync()
        => (await _accountRepository.GetAllAsync()).Select(a => a.ToDto());

    public async Task<AccountDto> CreateAsync(CreateAccountDto dto)
    {
        var account = dto.ToModel(dto.Password);
        var created = await _accountRepository.AddAsync(account);
        
        if (created.RoleId.HasValue)
            created.Role = await _roleRepository.GetByIdAsync(created.RoleId.Value);

        return created.ToDto();
    }

    public async Task<AccountDto?> UpdateAsync(Guid id, UpdateAccountDto dto)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null) return null;

        dto.MapToUpdate(account);
        await _accountRepository.UpdateAsync(account);
        
        return (await _accountRepository.GetByIdAsync(id))?.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await _accountRepository.ExistsAsync(id)) return false;
        await _accountRepository.DeleteAsync(id);
        return true;
    }
}
