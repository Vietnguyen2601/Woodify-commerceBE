using AccountService.Common.DTOs;
using AccountService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.APIService.Controllers;

/// <summary>
/// Controller quản lý Accounts
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Lấy tất cả accounts
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAll()
    {
        var accounts = await _accountService.GetAllAsync();
        return Ok(accounts);
    }

    /// <summary>
    /// Lấy account theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountDto>> GetById(Guid id)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return NotFound();
        return Ok(account);
    }

    /// <summary>
    /// Lấy account theo username
    /// </summary>
    [HttpGet("username/{username}")]
    public async Task<ActionResult<AccountDto>> GetByUsername(string username)
    {
        var account = await _accountService.GetByUsernameAsync(username);
        if (account == null) return NotFound();
        return Ok(account);
    }

    /// <summary>
    /// Tạo account mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountDto dto)
    {
        var account = await _accountService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = account.AccountId }, account);
    }

    /// <summary>
    /// Cập nhật account
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AccountDto>> Update(Guid id, [FromBody] UpdateAccountDto dto)
    {
        var account = await _accountService.UpdateAsync(id, dto);
        if (account == null) return NotFound();
        return Ok(account);
    }

    /// <summary>
    /// Xóa account
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _accountService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
