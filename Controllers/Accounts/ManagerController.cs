using System.Security.Claims;

using FoodOrderingApi.Authorization;
using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Services.Orders;
using FoodOrderingApi.Services.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers.Accounts;


// Conditions for callers:
// 1) any authenticated user may reach these endpoints; "manager" is not an
//    Identity role but a per-place row in the Managers table.
// 2) every operation is then scoped to the places the caller owns *while still
//    holding the Owner role* or the places they were added to as a manager.
[ApiController]
[Route("api/v1/manager")]
[Authorize]
public class ManagerController : ControllerBase
{
    private readonly ManagerService _managerService;
    private readonly OrderService _orderService;

    public ManagerController(ManagerService managerService, OrderService orderService)
    {
        _managerService = managerService;
        _orderService = orderService;
    }

    // This finds an user from auth token.
    private string CallerId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // CHANGING ORDER STATUS
    // ---------------------------------------------------------------------------
    // The owner counts as a manager of their place, so both the owner and anyone
    // the owner added to the Managers table can reach this endpoint.

    // The contents of a placed order are immutable; only the OrderStatus can change.
    [HttpPatch("places/{placeId}/orders/{orderId}/status")]
    public IActionResult UpdateOrderStatus(int placeId, int orderId, UpdateOrderDto dto)
    {
        if (dto.Status == null)
            return BadRequest("A status is required.");

        GetOrderDto? updated = _orderService.SetStatusForPlace(
            CallerId, User.IsOwner(), placeId, orderId, dto.Status.Value);

        if (updated == null)
            return NotFound("Order does not exist for this place.");

        return Ok(updated);
    }
}
