using IdentityService.Application.DTOs;
using Shared.Results;

namespace IdentityService.Application.Interfaces;

/// <summary>
/// Interface cho Account Business Service
/// </summary>
public interface IAccountService
{
    Task<ServiceResult<AccountDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<AccountDto>> GetByUsernameAsync(string username);
    Task<ServiceResult<IEnumerable<AccountDto>>> GetAllAsync();
    Task<ServiceResult<AccountDto>> CreateAsync(CreateAccountDto dto);
    Task<ServiceResult<AccountDto>> UpdateAsync(Guid id, UpdateAccountDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
}
