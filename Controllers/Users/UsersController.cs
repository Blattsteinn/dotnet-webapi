using System.Security.Claims;

using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers.Users;

// "My orders" — every route here is scoped to the caller's own id, which comes
// from the token and never from the URL, so a customer can only ever read their
// own history.
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
            return NotFound("Order does not exist.");

        return Ok(order);
    }
}
