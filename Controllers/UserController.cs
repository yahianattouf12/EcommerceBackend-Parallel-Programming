using ECommerceBackend.Models;
using ECommerceBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBackend.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _service.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        return Ok(await _service.CreateAsync(user));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, User user)
    {
        return Ok(await _service.UpdateAsync(id, user));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await _service.DeleteAsync(id));
    }
}
