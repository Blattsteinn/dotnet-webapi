using System.Security.Claims;

using FoodOrderingApi.Controllers;
using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers.Accounts;


[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly OrderService _orderService;

    public UsersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    private string CallerId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public IActionResult GetMyOrders()
    {
        return Ok(_orderService.GetForUser(CallerId));
    }

    // 404 for both "no such order" and "not yours", so this cannot be used to
    // probe which order ids exist.
    [HttpGet("{orderId}")]
    public IActionResult GetMyOrder(int orderId)
    {
        GetOrderDto? order = _orderService.GetForUser(CallerId, orderId);
        if (order == null)
            return this.NotFoundProblem("Order does not exist.");

        return Ok(order);
    }

    // Cancels the caller's own order while it is still Pending. A non-pending
    // order cannot be cancelled here; 404 also covers orders that are not the
    // caller's, so the endpoint cannot be used to probe order ids.
    [HttpPost("{orderId}/cancel")]
    public IActionResult CancelMyOrder(int orderId)
    {
        (GetOrderDto? cancelled, CancelOrderResult result) =
            _orderService.CancelForUser(CallerId, orderId);

        return result switch
        {
            CancelOrderResult.OrderNotFound => this.NotFoundProblem("Order does not exist."),
            CancelOrderResult.NotPending => this.ConflictProblem("Only pending orders can be cancelled."),
            CancelOrderResult.Success => Ok(cancelled),
            _ => StatusCode(500)
        };
    }
}
