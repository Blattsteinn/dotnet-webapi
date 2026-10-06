using System.Security.Claims;

using FoodOrderingApi.Authorization;
using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Dtos.Accounts.Manager;
using FoodOrderingApi.Dtos.Accounts.Owner;
using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Services.Orders;
using FoodOrderingApi.Services.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    // The contents of a placed order are immutable; only the OrderStatus can change.
    [HttpPatch("places/{placeId}/orders/{orderId}/status")]
    public IActionResult UpdateOrderStatus(int placeId, int orderId, UpdateOrderDto dto)
    {
        if (dto.Status == null)
            return BadRequest("A status is required.");

        GetOrderDto? updated =
            _orderService.SetStatusForPlace(CallerId, placeId, orderId, dto.Status.Value);

        if (updated == null)
            return NotFound("Order does not exist for this place.");

        return Ok(updated);
    }
}
