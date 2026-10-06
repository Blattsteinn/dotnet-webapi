using System.Security.Claims;

using FoodOrderingApi.Authorization;
using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Services.Orders;
using FoodOrderingApi.Services.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingApi.Dtos.Accounts.Admin;
using FoodOrderingApi.Dtos.Accounts.Owner;

namespace FoodOrderingApi.Controllers.Accounts;


// Conditions for callers:
// 1) the caller must hold the Owner role (they are allowed to run places).
// 2) every operation below is then scoped to the places they actually own.
[ApiController]
[Route("api/v1/owner")]
[Authorize(Roles = Roles.Owner)]
public class OwnerController : ControllerBase
{
    private readonly OwnerService _ownerService;
    private readonly OrderService _orderService;

    public OwnerController(OwnerService ownerService, OrderService orderService)
    {
        _ownerService = ownerService;
        _orderService = orderService;
    }

    // This finds an user from auth token.
    private string CallerId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // The only role this controller delegates. Manager is not an Identity role —
    // it is a per-place row — so the value is a label, not a claim.
    private const string ManagerRole = "Manager";

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // ADDING USER AS A MANAGER
    // ---------------------------------------------------------------------------
    [HttpPatch("places/{placeId}/managers/{userId}/roles")]
    public IActionResult AssignRole(int placeId, string userId, AssignRoleDto dto)
    {
        // Manager is the only role this endpoint hands out; anything else would
        // be an attempt to escalate through a place the caller merely owns.
        if (!string.Equals(dto.Role, ManagerRole, StringComparison.OrdinalIgnoreCase))
            return BadRequest($"Only the '{ManagerRole}' role can be assigned here.");

        ManagerRoleResult result = _ownerService.AddManager(CallerId, placeId, userId);
        return result switch
        {
            ManagerRoleResult.Success       => Ok(new { placeId, userId, role = ManagerRole }),
            ManagerRoleResult.PlaceNotFound => NotFound("Place does not exist."),
            ManagerRoleResult.NotTheOwner   => Forbid(),
            ManagerRoleResult.UserNotFound  => NotFound("User does not exist."),
            _ => StatusCode(500)
        };
    }

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // PLACE CONTROL ACTIONS
    // ---------------------------------------------------------------------------
    // uses OwnerService.cs

    [HttpGet("places")]
    public IActionResult GetMyPlaces()
    {
        return Ok(_ownerService.GetOwnedPlaces(CallerId));
    }

    [HttpGet("places/{id}")]
    public IActionResult GetPlace(int id)
    {
        GetPlaceDto? place = _ownerService.GetPlace(CallerId, id);
        if (place == null)
            return NotFound("Place does not exist or is not yours.");

        return Ok(place);
    }

    [HttpPost("places")]
    public IActionResult CreatePlace(CreatePlaceDto dto)
    {
        OwnedPlaceDto place = _ownerService.CreatePlace(CallerId, dto);
        return CreatedAtAction(nameof(GetPlace), new { id = place.Id }, place);
    }

    [HttpPatch("places/{id}")]
    public IActionResult UpdatePlace(int id, OwnerUpdatePlaceDto dto)
    {
        var (result, updated) = _ownerService.UpdatePlace(CallerId, id, dto);
        return result switch
        {
            OwnerPlaceResult.Success       => Ok(updated),
            OwnerPlaceResult.PlaceNotFound => NotFound("Place does not exist."),
            OwnerPlaceResult.NotTheOwner   => Forbid(),
            OwnerPlaceResult.UnknownChildId =>
                BadRequest("JSON included a child id that does not exist under this place."),
            _ => StatusCode(500)
        };
    }
    
    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // ORDER CONTROL ACTIONS
    // ---------------------------------------------------------------------------
    // Uses OrderService.cs

    [HttpGet("places/{placeId}/orders")]
    public IActionResult GetOrders(int placeId)
    {
        List<GetOrderDto>? orders = _orderService.GetForOwner(CallerId, placeId);

        if (orders == null)
            return NotFound("Place does not exist or is not yours.");

        return Ok(orders);
    }

    [HttpGet("places/{placeId}/orders/{orderId}")]
    public IActionResult GetOrder(int placeId, int orderId)
    {
        GetOrderDto? order = _orderService.GetForOwner(CallerId, placeId, orderId);
        if (order == null)
            return NotFound("Order does not exist for this place.");

        return Ok(order);
    }

}
