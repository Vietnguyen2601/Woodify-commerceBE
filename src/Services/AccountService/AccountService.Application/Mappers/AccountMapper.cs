using AccountService.Application.DTOs;
using AccountService.Domain.Entities;

namespace AccountService.Application.Mappers;

public static class AccountMapper
{
    public static AccountDto ToDto(this Account account)
    {
        if (account == null) throw new ArgumentNullException(nameof(account), "Account cannot be null");
        
        return new AccountDto
        {
            AccountId = account.AccountId,
            Username = account.Username,
            Email = account.Email,
            Name = account.Name,
            PhoneNumber = account.PhoneNumber,
            RoleId = account.RoleId,
            RoleName = account.Role?.RoleName,
            CreatedAt = account.CreatedAt,
            IsActive = account.IsActive
        };
    }

    public static Account ToModel(this CreateAccountDto dto, string passwordHash)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "CreateAccountDto cannot be null.");
        }

        if (string.IsNullOrEmpty(dto.Username) ||
            string.IsNullOrEmpty(dto.Email) ||
            string.IsNullOrEmpty(dto.Password))
        {
            throw new ArgumentException("Required fields (Username, Email, Password) cannot be null or empty.", nameof(dto));
        }

        return new Account
        {
            Username = dto.Username,
            PasswordHash = passwordHash,
            Email = dto.Email,
            Name = dto.Name,
            PhoneNumber = dto.PhoneNumber,
            RoleId = dto.RoleId
        };
    }

    public static void MapToUpdate(this UpdateAccountDto dto, Account account)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto), "UpdateAccountDto cannot be null");
        if (account == null) throw new ArgumentNullException(nameof(account), "Account cannot be null");

        if (!string.IsNullOrEmpty(dto.Name))
        {
            account.Name = dto.Name;
        }

        if (!string.IsNullOrEmpty(dto.PhoneNumber))
        {
            account.PhoneNumber = dto.PhoneNumber;
        }

        if (!string.IsNullOrEmpty(dto.Email))
        {
            account.Email = dto.Email;
        }

        if (dto.RoleId.HasValue)
        {
            account.RoleId = dto.RoleId;
        }

        if (dto.IsActive.HasValue)
        {
            account.IsActive = dto.IsActive.Value;
        }
    }
}
