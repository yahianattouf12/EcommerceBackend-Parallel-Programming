using ECommerceBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBackend.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // مؤقتًا: userId ثابت
    private const int UserId = 3;

    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        var items = await _cartService.GetItemsAsync(UserId);
        return Ok(items);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddItem(int productId, int quantity = 1)
    {
        var item = await _cartService.AddItemAsync(UserId, productId, quantity);
        return Ok(item);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
    {
        var item = await _cartService.UpdateQuantityAsync(UserId, productId, quantity);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        var result = await _cartService.RemoveItemAsync(UserId, productId);
        return Ok(result);
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var result = await _cartService.ClearCartAsync(UserId);
        return Ok(result);
    }
}
