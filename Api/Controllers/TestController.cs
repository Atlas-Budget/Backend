using Microsoft.AspNetCore.Mvc;
using Api.Data;
using Api.Entities;
using Api.Dtos; // <--- novo

namespace Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly AppDbContext _context;

    public TestController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_context.TestItems.ToList());
    }

    [HttpPost]
    public IActionResult Post([FromBody] TestItemDto dto)
    {
        var item = new TestItem { Name = dto.Name };
        _context.TestItems.Add(item);
        _context.SaveChanges();

        return Ok(item);
    }
}
