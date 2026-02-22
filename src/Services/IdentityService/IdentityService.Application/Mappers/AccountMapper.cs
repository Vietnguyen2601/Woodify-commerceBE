using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Mappers;

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
            Address = account.Address,
            Dob = account.Dob,
            Gender = account.Gender,
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
            Password = passwordHash,
            Email = dto.Email,
            Name = dto.Name,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address ?? string.Empty,
            Dob = dto.Dob,
            Gender = dto.Gender,
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

        if (!string.IsNullOrEmpty(dto.Address))
        {
            account.Address = dto.Address;
        }

        if (dto.Dob.HasValue)
        {
            account.Dob = dto.Dob;
        }

        if (!string.IsNullOrEmpty(dto.Gender))
        {
            account.Gender = dto.Gender;
        }
    }
}
