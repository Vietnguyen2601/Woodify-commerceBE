using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Mappers;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Repositories.IRepositories;
using Shared.Events;
using Shared.Results;
using System.Threading.Tasks;
namespace IdentityService.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly AccountEventPublisher _eventPublisher;

    public AccountService(IAccountRepository accountRepository, IRoleRepository roleRepository, AccountEventPublisher eventPublisher)
    {
        _accountRepository = accountRepository;
        _roleRepository = roleRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<ServiceResult<AccountDto>> GetByIdAsync(Guid id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
            return ServiceResult<AccountDto>.NotFound("Account not found");

        return ServiceResult<AccountDto>.Success(account.ToDto());
    }

    public async Task<ServiceResult<AccountDto>> GetByUsernameAsync(string username)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            return ServiceResult<AccountDto>.NotFound("Account not found");

        return ServiceResult<AccountDto>.Success(account.ToDto());
    }

    public async Task<ServiceResult<IEnumerable<AccountDto>>> GetAllAsync()
    {
        var accounts = await _accountRepository.GetAllAsync();
        var accountDtos = accounts.Select(a => a.ToDto());

        return ServiceResult<IEnumerable<AccountDto>>.Success(accountDtos);
    }

    public async Task<ServiceResult<AccountDto>> CreateAsync(CreateAccountDto dto)
    {
        try
        {
            var account = dto.ToModel(dto.Password);
            await _accountRepository.CreateAsync(account);

            if (account.RoleId.HasValue)
                account.Role = await _roleRepository.GetByIdAsync(account.RoleId.Value);

            _eventPublisher.PublishAccountCreated(new AccountCreatedEvent
            {
                AccountId = account.AccountId,
                Username = account.Username,
                Name = string.IsNullOrWhiteSpace(account.Name) ? null : account.Name,
                Email = account.Email,
                CreatedAt = account.CreatedAt
            });

            return ServiceResult<AccountDto>.Created(account.ToDto(), "Account created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<AccountDto>.InternalServerError($"Error creating account: {ex.Message}");
        }
    }

    public async Task<ServiceResult<AccountDto>> UpdateAsync(Guid id, UpdateAccountDto dto)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
                return ServiceResult<AccountDto>.NotFound("Account not found");

            dto.MapToUpdate(account);
            await _accountRepository.UpdateAsync(account);

            var updatedAccount = await _accountRepository.GetByIdAsync(id);
            if (updatedAccount != null)
            {
                _eventPublisher.PublishAccountUpdated(new AccountUpdatedEvent
                {
                    AccountId = updatedAccount.AccountId,
                    Username = updatedAccount.Username,
                    Name = string.IsNullOrWhiteSpace(updatedAccount.Name) ? null : updatedAccount.Name,
                    Email = updatedAccount.Email ?? string.Empty,
                    IsActive = updatedAccount.IsActive,
                    UpdatedAt = updatedAccount.UpdatedAt
                });
            }
            return ServiceResult<AccountDto>.Success(updatedAccount!.ToDto(), "Account updated successfully");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ServiceResult<AccountDto>.InternalServerError($"Error updating account: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
                return ServiceResult.NotFound("Account not found");

            await _accountRepository.RemoveAsync(account);
            return ServiceResult.Success("Account deleted successfully");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error deleting account: {ex.Message}");
        }
    }

    public async Task<ServiceResult<AccountDto>> UpdateAccountStatusAsync(Guid id, UpdateAccountStatusDto dto)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
                return ServiceResult<AccountDto>.NotFound("Account not found");

            account.IsActive = dto.IsActive;
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepository.UpdateAsync(account);

            _eventPublisher.PublishAccountUpdated(new AccountUpdatedEvent
            {
                AccountId = account.AccountId,
                Username = account.Username,
                Name = string.IsNullOrWhiteSpace(account.Name) ? null : account.Name,
                Email = account.Email ?? string.Empty,
                IsActive = account.IsActive,
                UpdatedAt = account.UpdatedAt
            });

            var message = dto.IsActive ? "Account activated successfully" : "Account deactivated successfully";
            return ServiceResult<AccountDto>.Success(account.ToDto(), message);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ServiceResult<AccountDto>.InternalServerError($"Error updating account status: {ex.Message}");
        }
    }
}
