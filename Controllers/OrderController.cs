
using FoodOrderingApi.Models;
using FoodOrderingApi.Services;
using FoodOrderingApi.Dtos.Order;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        List<Order> orders = _orderService.GetAll();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        Order? order = _orderService.Get(id);
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public IActionResult Add(Order order)
    {
        Order? newOrder = _orderService.Add(order);
        if (newOrder == null)
            return NotFound("Place does not exist.");

        return Ok(newOrder);
    }

    [HttpPatch("{id}")]
    public IActionResult Update(int id, UpdateOrderDto order)
    {
        Order? updatedOrder = _orderService.Update(id, order);
        if (updatedOrder == null)
            return NotFound();

        return Ok(updatedOrder);
    }
}