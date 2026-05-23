using ECommerceBackend.Models;
using ECommerceBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBackend.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    //! ////////////////////////////////////////////////// !//
    //?    ...this is hte 1st question in the project...   ?//
    //! ////////////////////////////////////////////////// !//
    [HttpPut("decrease-quantity-unsafe/{productId}/{quantity}")]
    public async Task<IActionResult> DecreaseQuantity(int productId, int quantity)
    {
        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_service.DecreaseQuantityAsync(productId, quantity));
        }
        await Task.WhenAll(tasks);
        return Ok();
    } 

    [HttpPut("decrease-quantity-safe/{productId}/{quantity}")]
    public async Task<IActionResult> DecreaseQuantitySafely(int productId, int quantity)
    {
        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_service.DecreaseQuantitySafeAsync(productId, quantity));
        }
        await Task.WhenAll(tasks);
        return Ok();
    }


    //! ////////////////////////////////////////////////// !//
    //?    ...this is hte 2cd question in the project...   ?//
    //! ////////////////////////////////////////////////// !//
    [HttpPut("decrease-quantity-controlled/{productId}/{qty}")]
    public async Task<IActionResult> DecreaseQuantityControlled(int productId, int qty)
    {
        var tasks = new List<Task>();

        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() => _service.DecreaseQuantityControlledAsync(productId, qty)));
        }


        await Task.WhenAll(tasks);

        return Ok("Done");
    }



    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _service.GetByIdAsync(id);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        return Ok(await _service.CreateAsync(product));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Product product)
    {
        return Ok(await _service.UpdateAsync(id, product));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await _service.DeleteAsync(id));
    }
}
