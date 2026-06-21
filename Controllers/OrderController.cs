using ECommerceBackend.BackgroundJobs;
using ECommerceBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IInvoiceService _invoiceService;
    private readonly INotificationService _notificationService;
    private readonly IBackgroundJobQueue _backgroundJobQueue;

    public OrderController
        (IOrderService orderService,
        IInvoiceService invoiceService,
        INotificationService notificationService,
        IBackgroundJobQueue backgroundJobQueue)
    {
        _orderService = orderService;
        _invoiceService = invoiceService;
        _notificationService = notificationService;
        _backgroundJobQueue = backgroundJobQueue;
    }

    //todo : hard code you should to replace it with some logic
    private const int UserId = 3;
    private const int userId8 = 3;


    //! ////////////////////////////////////////////////// !//
    //?    ...this is hte 3ed question in the project...   ?//
    //! ////////////////////////////////////////////////// !//
    [HttpPost("create-sync")]
    public async Task<IActionResult> CreateOrderSync()
    {
        var order = await _orderService.CreateOrderAsync(UserId);

        // this is the sync way.. this is the wrong way ...
        var invoice = await _invoiceService.GenerateInvoiceAsync(order.Id);
        await _notificationService.SendOrderCreatedAsync(order, invoice);

        return Ok(order);
    }

    [HttpPost("create-async")]
    public async Task<IActionResult> CreateOrderAsync()
    {
        var order = await _orderService.CreateOrderAsync(UserId);

        // the heavy work go to the background job.. this is the async way.. this is the right way..
        _backgroundJobQueue.Enqueue(new OrderBackgroundJob(order.Id));

       // then we just return the order to the user
        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserOrders()
    {
        return Ok(await _orderService.GetUserOrdersAsync(UserId));
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderById(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId, UserId);
        return order == null ? NotFound() : Ok(order);
    }

    //? الطلب الثامن ??????????????????????????????????????????????????????
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromQuery] bool simulatePaymentFailure = false)
    {
        try
        {
            var order = await _orderService.CheckoutAsync(userId8, simulatePaymentFailure);
            return Ok(order);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("Stock changed concurrently, please retry.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
