using System.Security.Claims;

using FoodOrderingApi.Authorization;
using FoodOrderingApi.Controllers;
using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Services.Orders;
using FoodOrderingApi.Services.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers.Accounts;


[ApiController]
[Route("api/v1/manager")]
[Authorize]
public class ManagerController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly ManagerService _managerService;

    // reuses existing services
    public ManagerController(ManagerService managerService, OrderService orderService)
    {
        _orderService = orderService;
        _managerService = managerService;
    }

    // This finds an user from auth token.
    private string CallerId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // NOTE:
    // All of these controller actions are reachable by:
    // 1. Place owner
    // 2. Manager

    // The places this caller manages: their own (while they hold the Owner
    // role) plus any place they were assigned to manage.
    [HttpGet("places")]
    public IActionResult GetMyPlaces()
    {
        return Ok(_managerService.GetManagedPlaces(CallerId, User.IsOwner()));
    }

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // CHANGING ORDER STATUS
    // ---------------------------------------------------------------------------

    // The contents of a placed order are immutable - only the OrderStatus can change.
    [HttpPatch("places/{placeId}/orders/{orderId}/status")]
    public IActionResult UpdateOrderStatus(int placeId, int orderId, UpdateOrderDto dto)
    {
        if (dto.Status == null)
            return this.BadRequestProblem("A status is required.");

        (GetOrderDto? updated, SetOrderStatusResult result) = _orderService.SetStatusForPlace(
            CallerId, User.IsOwner(), placeId, orderId, dto.Status.Value);

        return result switch
        {
            SetOrderStatusResult.OrderNotFound => this.NotFoundProblem("Order does not exist for this place."),
            SetOrderStatusResult.InvalidTransition => this.ConflictProblem("The requested status transition is not allowed."),
            SetOrderStatusResult.Success => Ok(updated),
            _ => StatusCode(500)
        };
    }

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // ORDER VIEW
    // ---------------------------------------------------------------------------

    [HttpGet("places/{placeId}/orders")]
    public IActionResult GetOrders(int placeId)
    {
        List<GetOrderDto>? orders = _orderService.GetForPlace(CallerId, User.IsOwner(), placeId);

        if (orders == null)
            return this.NotFoundProblem("Place does not exist or is not yours.");

        return Ok(orders);
    }

    [HttpGet("places/{placeId}/orders/{orderId}")]
    public IActionResult GetOrder(int placeId, int orderId)
    {
        GetOrderDto? order = _orderService.GetForPlace(CallerId, User.IsOwner(), placeId, orderId);
        if (order == null)
            return this.NotFoundProblem("Order does not exist for this place.");

        return Ok(order);
    }
}
