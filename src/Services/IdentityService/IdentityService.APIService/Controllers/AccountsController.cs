using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("GetAllAccounts")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAll()
    {
        var accounts = await _accountService.GetAllAsync();
        return Ok(accounts);
    }

    [HttpGet("GetAccountById/{id:guid}")]
    public async Task<ActionResult<AccountDto>> GetById(Guid id)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return NotFound();
        return Ok(account);
    }

    [HttpGet("GetAccountByUsername/{username}")]
    public async Task<ActionResult<AccountDto>> GetByUsername(string username)
    {
        var account = await _accountService.GetByUsernameAsync(username);
        if (account == null) return NotFound();
        return Ok(account);
    }

    [HttpPost("CreateAccount")]
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountDto dto)
    {
        var account = await _accountService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = account.AccountId }, account);
    }

    [HttpPut("UpdateAccount/{id:guid}")]
    public async Task<ActionResult<AccountDto>> Update(Guid id, [FromBody] UpdateAccountDto dto)
    {
        var account = await _accountService.UpdateAsync(id, dto);
        if (account == null) return NotFound();
        return Ok(account);
    }

    [HttpDelete("DeleteAccount/{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _accountService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
