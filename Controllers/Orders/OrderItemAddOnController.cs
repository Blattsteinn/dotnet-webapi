
using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Services.Orders;
using FoodOrderingApi.Dtos.Orders;
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


    [HttpPost]
    public IActionResult Add(CreateOrderItemAddOnDto createOrderItemAddOnDto, int orderItemId)
    {
        var (result, newOrderItem) = _orderItemAddOnService.Add(createOrderItemAddOnDto, orderItemId);

        if(result == AddOrderItemAddOnResult.AddOnNotFound)
            return NotFound("Add on does not exist.");

        if(result == AddOrderItemAddOnResult.OrderItemNotFound)
            return NotFound("Order item does not exist.");
        
        return Ok(newOrderItem);
    }
}