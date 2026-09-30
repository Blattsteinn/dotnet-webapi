
using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Services.Orders;
using FoodOrderingApi.Dtos.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FoodOrderingApi.Controllers.Orders;

[ApiController]
[Route("api/v1/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    // Doesn't nest on purpose.
    [HttpGet]
    public IActionResult GetAll()
    {
        List<Order> orders = _orderService.GetAll();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        GetOrderDto? order = _orderService.Get(id);
        if (order == null)
            return NotFound("Order does not exist.");

        return Ok(order);
    }

    [Authorize]
    [HttpPost]
    public IActionResult Add(CreateOrderDto createOrderDto)
    {
        (GetOrderDto? newOrder, AddOrderResult result) = _orderService.Add(createOrderDto);

        return result switch
        {
            AddOrderResult.PlaceNotFound => NotFound("Specified place does not exist."),
            AddOrderResult.ZeroProducts => BadRequest("Your request didn't specify any produts."),
            AddOrderResult.ProductNotFound => NotFound("Request specified product ID/s that do not exist."),
            AddOrderResult.AddOnNotFound => NotFound("Request specified add on ID/s that do not exist."),
            AddOrderResult.Success => Ok(newOrder),
             _ =>  StatusCode(500)
        };
    }

    [HttpPatch("{id}")]
    public IActionResult Update(int id, UpdateOrderDto order)
    {
        GetOrderDto? updatedOrder = _orderService.Update(id, order);
        
        if (updatedOrder == null)
            return NotFound("Order does not exist.");

        return Ok(updatedOrder);
    }
}