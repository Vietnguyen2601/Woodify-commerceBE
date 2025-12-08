using AccountService.Common.DTOs;

namespace AccountService.Services.Interfaces;

/// <summary>
/// Interface cho Account Business Service
/// </summary>
public interface IAccountService
{
    Task<AccountDto?> GetByIdAsync(Guid id);
    Task<AccountDto?> GetByUsernameAsync(string username);
    Task<IEnumerable<AccountDto>> GetAllAsync();
    Task<AccountDto> CreateAsync(CreateAccountDto dto);
    Task<AccountDto?> UpdateAsync(Guid id, UpdateAccountDto dto);
    Task<bool> DeleteAsync(Guid id);
}
