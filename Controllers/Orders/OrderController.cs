
using System.Security.Claims;

using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [Authorize]
    [HttpPost]
    public IActionResult Add(CreateOrderDto createOrderDto)
    {
        (GetOrderDto? newOrder, AddOrderResult result) =
            _orderService.Add(createOrderDto, User.FindFirstValue(ClaimTypes.NameIdentifier));

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
}