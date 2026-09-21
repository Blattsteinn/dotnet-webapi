
using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Services.Orders;
using FoodOrderingApi.Dtos.OrderItemAddOn;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers.Orders;

[ApiController]
[Route("api/v1/order-items/{OrderItemId}/add-ons")]
public class OrderItemAddOnController : ControllerBase
{
    private readonly OrderItemAddOnService _orderItemAddOnService;
    public OrderItemAddOnController(OrderItemAddOnService orderItemAddOnService)
    {
        _orderItemAddOnService = orderItemAddOnService;
    }

    [HttpGet]
    public IActionResult GetAll(int orderItemId)
    {
        List<OrderItemAddOn>? orderItemAddOns = _orderItemAddOnService.GetAll(orderItemId);
        if(orderItemAddOns == null)
            return NotFound();

        return Ok(orderItemAddOns);

    }

    [HttpGet("{id}")]
    public IActionResult Get(int id, int orderItemId)
    {
        OrderItemAddOn? orderItemAddOn = _orderItemAddOnService.Get(id, orderItemId);
        if(orderItemAddOn == null)
            return NotFound();

        return Ok(orderItemAddOn);
    }


    [HttpPost]
    public IActionResult Add(CreateOrderItemAddOnDto createOrderItemAddOnDto, int orderItemId)
    {
        var newOrderItem = _orderItemAddOnService.Add(createOrderItemAddOnDto, orderItemId);

        if(newOrderItem == null)
            return BadRequest();
        
        return Ok(newOrderItem);
    }
}