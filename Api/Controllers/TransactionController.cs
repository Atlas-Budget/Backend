using Api.Dtos.Contracts.Transactions;
using Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Api.Service.Utils;

[Authorize]
[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;

    public TransactionsController(ITransactionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTransactionRequest request)
    {
        var userId = User.GetUserId();

        await _service.CreateAsync(userId, request);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var userId = User.GetUserId();

        var result = await _service.GetAsync(userId, start, end);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreateTransactionRequest request)
    {
        var userId = User.GetUserId();

        await _service.UpdateAsync(userId, id, request);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();

        await _service.DeleteAsync(userId, id);
        return Ok();
    }
}
