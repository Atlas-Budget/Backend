using Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Service.Utils;
using Api.Dtos.Transactions.Set;

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

        var id = await _service.CreateTransaction(userId, request);
        return Ok(id); // opcional: retornar o id (recomendado)
    }

    [HttpGet("resume")]
    public async Task<IActionResult> Resume([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var userId = User.GetUserId();

        var result = await _service.GetTransactionsResume(userId, startDate, endDate);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var userId = User.GetUserId();

        var result = await _service.GetTransaction(userId, id);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CreateTransactionRequest request)
    {
        var userId = User.GetUserId();

        await _service.UpdateTransaction(userId, id, request);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();

        await _service.DeleteTransaction(userId, id);
        return Ok();
    }
}
