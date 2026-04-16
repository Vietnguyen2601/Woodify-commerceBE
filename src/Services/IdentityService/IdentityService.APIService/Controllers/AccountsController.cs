using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

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
    public async Task<ActionResult<ServiceResult<IEnumerable<AccountDto>>>> GetAll()
    {
        var result = await _accountService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("GetAccountById/{id:guid}")]
    public async Task<ActionResult<ServiceResult<AccountDto>>> GetById(Guid id)
    {
        var result = await _accountService.GetByIdAsync(id);

        if (result.Status == 404)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("GetAccountByUsername/{username}")]
    public async Task<ActionResult<ServiceResult<AccountDto>>> GetByUsername(string username)
    {
        var result = await _accountService.GetByUsernameAsync(username);

        if (result.Status == 404)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("CreateAccount")]
    public async Task<ActionResult<ServiceResult<AccountDto>>> Create([FromBody] CreateAccountDto dto)
    {
        var result = await _accountService.CreateAsync(dto);

        if (result.Status == 201)
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.AccountId }, result);

        return BadRequest(result);
    }

    [HttpPut("UpdateAccount/{id:guid}")]
    public async Task<ActionResult<ServiceResult<AccountDto>>> Update(Guid id, [FromBody] UpdateAccountDto dto)
    {
        var result = await _accountService.UpdateAsync(id, dto);

        if (result.Status == 404)
            return NotFound(result);

        if (result.Status != 200)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("DeleteAccount/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Delete(Guid id)
    {
        var result = await _accountService.DeleteAsync(id);

        if (result.Status == 404)
            return NotFound(result);

        if (result.Status != 200)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Admin: Kích hoạt hoặc khóa tài khoản (ban/unban)
    /// </summary>
    [HttpPatch("UpdateAccountStatus/{id:guid}")]
    public async Task<ActionResult<ServiceResult<AccountDto>>> UpdateAccountStatus(Guid id, [FromBody] UpdateAccountStatusDto dto)
    {
        var result = await _accountService.UpdateAccountStatusAsync(id, dto);

        if (result.Status == 404)
            return NotFound(result);

        if (result.Status != 200)
            return BadRequest(result);

        return Ok(result);
    }
}
