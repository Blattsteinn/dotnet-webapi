
using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Services.Orders;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers.Orders;

[ApiController]
[Route("api/v1/orders/{orderId}/order-items")]
public class OrderItemController : ControllerBase
{
    private readonly OrderItemService _orderItemService;
    public OrderItemController(OrderItemService orderItemService)
    {
        _orderItemService = orderItemService;
    }

    // Only has GET requests. I don't see the point in others. 
    // I assume we could add PATCH if there's a need later.
    // Order handles all the nesting anyway, keeping for debugging purposes.

    [HttpGet]
    public IActionResult GetAll(int orderId)
    {
        List<OrderItem>? orderItems = _orderItemService.GetAll(orderId);
        if (orderItems == null)
            return NotFound("Order does not exist.");

        return Ok(orderItems);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id, int orderId)
    {
        OrderItem? orderItem = _orderItemService.Get(id, orderId);
        if (orderItem == null)
            return NotFound("Order does not exist.");

        return Ok(orderItem);
    }

}