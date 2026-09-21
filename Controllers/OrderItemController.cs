
using FoodOrderingApi.Models;
using FoodOrderingApi.Services;
using FoodOrderingApi.Dtos.OrderItem;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers;

[ApiController]
[Route("api/v1/orders/{orderId}/order-items")]
public class OrderItemController : ControllerBase
{
    private readonly OrderItemService _orderItemService;
    public OrderItemController(OrderItemService orderItemService)
    {
        _orderItemService = orderItemService;
    }

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
            return NotFound();

        return Ok(orderItem);
    }

    [HttpPost]
    public IActionResult Add(int orderId, CreateOrderItemDto orderItem)
    {
        var (result, newOrderItem) = _orderItemService.Add(orderItem, orderId);

        if (result == AddOrderItemResult.OrderNotFound)
            return NotFound("Order does not exist.");

        if (result == AddOrderItemResult.ProductNotFound)
            return BadRequest("Product does not exist.");

        return Ok(newOrderItem);
    }
}