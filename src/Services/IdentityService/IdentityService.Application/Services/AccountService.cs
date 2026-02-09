using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Mappers;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Repositories.IRepositories;
using Shared.Results;
using Shared.Caching;

namespace IdentityService.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICacheService _cacheService;
    private const string ACCOUNT_CACHE_PREFIX = "IdentityService:Account";
    private const string ALL_ACCOUNTS_CACHE_KEY = "IdentityService:AllAccounts";

    public AccountService(IAccountRepository accountRepository, IRoleRepository roleRepository, ICacheService cacheService)
    {
        _accountRepository = accountRepository;
        _roleRepository = roleRepository;
        _cacheService = cacheService;
    }

    public async Task<ServiceResult<AccountDto>> GetByIdAsync(Guid id)
    {
        // 1️⃣ Check cache trước
        var cacheKey = $"{ACCOUNT_CACHE_PREFIX}:Id:{id}";
        var cachedAccount = await _cacheService.GetAsync<AccountDto>(cacheKey);
        if (cachedAccount != null)
        {
            Console.WriteLine($"Cache hit for account {id}");
            return ServiceResult<AccountDto>.Success(cachedAccount);
        }

        // 2️⃣ Cache miss - query DB
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
            return ServiceResult<AccountDto>.NotFound("Account not found");
        
        var accountDto = account.ToDto();
        
        // 3️⃣ Store in cache
        await _cacheService.SetAsync(cacheKey, accountDto);
        
        return ServiceResult<AccountDto>.Success(accountDto);
    }

    public async Task<ServiceResult<AccountDto>> GetByUsernameAsync(string username)
    {
        // 1️⃣ Check cache trước
        var cacheKey = $"{ACCOUNT_CACHE_PREFIX}:Username:{username}";
        var cachedAccount = await _cacheService.GetAsync<AccountDto>(cacheKey);
        if (cachedAccount != null)
        {
            Console.WriteLine($"Cache hit for username {username}");
            return ServiceResult<AccountDto>.Success(cachedAccount);
        }

        // 2️⃣ Cache miss - query DB
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            return ServiceResult<AccountDto>.NotFound("Account not found");
        
        var accountDto = account.ToDto();
        
        // 3️⃣ Store in cache
        await _cacheService.SetAsync(cacheKey, accountDto);
        
        return ServiceResult<AccountDto>.Success(accountDto);
    }

    public async Task<ServiceResult<IEnumerable<AccountDto>>> GetAllAsync()
    {
        // 1️⃣ Check cache trước
        var cachedAccounts = await _cacheService.GetAsync<IEnumerable<AccountDto>>(ALL_ACCOUNTS_CACHE_KEY);
        if (cachedAccounts != null && cachedAccounts.Any())
        {
            Console.WriteLine($"Cache hit for all accounts");
            return ServiceResult<IEnumerable<AccountDto>>.Success(cachedAccounts);
        }

        // 2️⃣ Cache miss - query DB
        var accounts = await _accountRepository.GetAllAsync();
        var accountDtos = accounts.Select(a => a.ToDto()).ToList();
        
        // 3️⃣ Store in cache
        await _cacheService.SetAsync(ALL_ACCOUNTS_CACHE_KEY, accountDtos);
        
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

            // Invalidate all accounts cache
            await _cacheService.RemoveAsync(ALL_ACCOUNTS_CACHE_KEY);
            
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
            
            // Invalidate caches for this account
            var cacheKeyId = $"{ACCOUNT_CACHE_PREFIX}:Id:{id}";
            var cacheKeyUsername = $"{ACCOUNT_CACHE_PREFIX}:Username:{updatedAccount!.Username}";
            await _cacheService.RemoveAsync(cacheKeyId);
            await _cacheService.RemoveAsync(cacheKeyUsername);
            await _cacheService.RemoveAsync(ALL_ACCOUNTS_CACHE_KEY);
            
            return ServiceResult<AccountDto>.Success(updatedAccount.ToDto(), "Account updated successfully");
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
            
            // Invalidate caches for this account
            var cacheKeyId = $"{ACCOUNT_CACHE_PREFIX}:Id:{id}";
            var cacheKeyUsername = $"{ACCOUNT_CACHE_PREFIX}:Username:{account.Username}";
            await _cacheService.RemoveAsync(cacheKeyId);
            await _cacheService.RemoveAsync(cacheKeyUsername);
            await _cacheService.RemoveAsync(ALL_ACCOUNTS_CACHE_KEY);
            
            return ServiceResult.Success("Account deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error deleting account: {ex.Message}");
        }
    }
}
