
using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Services.Orders;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers.Orders;

[ApiController]
[Route("api/v1/order-items/{orderItemId}/add-ons")]
public class OrderItemAddOnController : ControllerBase
{
    private readonly OrderItemAddOnService _orderItemAddOnService;
    public OrderItemAddOnController(OrderItemAddOnService orderItemAddOnService)
    {
        _orderItemAddOnService = orderItemAddOnService;
    }

    // SAME AS IN mentioned OrderItemController:
    // Only has GET requests. I don't see the point in others. 
    // I assume we could add PATCH if there's a need later.
    // Order handles all the nesting anyway, keeping for debugging purposes.

    [HttpGet]
    public IActionResult GetAll(int orderItemId)
    {
        List<OrderItemAddOn>? orderItemAddOns = _orderItemAddOnService.GetAll(orderItemId);
        if(orderItemAddOns == null)
            return NotFound("OrderItem does not exist.");

        return Ok(orderItemAddOns);

    }

    [HttpGet("{id}")]
    public IActionResult Get(int id, int orderItemId)
    {
        OrderItemAddOn? orderItemAddOn = _orderItemAddOnService.Get(id, orderItemId);
        if(orderItemAddOn == null)
            return NotFound("OrderItem does not exist.");

        return Ok(orderItemAddOn);
    }

}