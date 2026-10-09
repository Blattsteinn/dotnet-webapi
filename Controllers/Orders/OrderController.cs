
using System.Security.Claims;

using FoodOrderingApi.Controllers;
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
            AddOrderResult.PlaceNotFound => this.NotFoundProblem("Specified place does not exist."),
            AddOrderResult.PlaceClosed => this.ConflictProblem("The place is currently closed and is not accepting orders."),
            AddOrderResult.ZeroProducts => this.BadRequestProblem("Your request didn't specify any produts."),
            AddOrderResult.ProductNotFound => this.NotFoundProblem("Request specified product ID/s that do not exist."),
            AddOrderResult.AddOnNotFound => this.NotFoundProblem("Request specified add on ID/s that do not exist."),
            AddOrderResult.RequiredAddOnsNotSpecified => this.BadRequestProblem("Your order add ons do not have the required ones."),
            AddOrderResult.SingleChoiceAddOnsExceeded => this.BadRequestProblem("Your order add ons include more than one option from a single-choice category."),
            AddOrderResult.Success => Created($"/api/v1/orders/{newOrder!.Id}", newOrder),
             _ =>  StatusCode(500)
        };
    }
}