using System.Security.Claims;

using FoodOrderingApi.Authorization;
using FoodOrderingApi.Dtos.Places;
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

    public OwnerController(OwnerService ownerService)
    {
        _ownerService = ownerService;
    }

    // This finds an user from auth token.
    private string CallerId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private const string ManagerRole = "Manager";

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // ADDING / REMOVING / LISTING USERS AS MANAGERS
    // ---------------------------------------------------------------------------
    // uses OwnerService.cs

    [HttpGet("places/{placeId}/managers")]
    public IActionResult GetManagers(int placeId)
    {
        List<ManagerDto>? managers = _ownerService.GetManagers(CallerId, placeId);
        if (managers == null)
            return NotFound("Place does not exist or is not yours.");

        return Ok(managers);
    }

    [HttpPatch("places/{placeId}/managers/{userId}/roles")]
    public IActionResult AssignRole(int placeId, string userId, AssignRoleDto dto)
    {
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

    [HttpDelete("places/{placeId}/managers/{userId}/roles")]
    public IActionResult RemoveRole(int placeId, string userId, AssignRoleDto dto)
    {
        if (!string.Equals(dto.Role, ManagerRole, StringComparison.OrdinalIgnoreCase))
            return BadRequest($"Only the '{ManagerRole}' role can be removed here.");

        ManagerRoleResult result = _ownerService.RemoveManager(CallerId, placeId, userId);
        return result switch
        {
            ManagerRoleResult.Success       => NoContent(),
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
        PrivatePlaceDto? place = _ownerService.GetPlace(CallerId, id);
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

    // Fully replaces the place's weekly schedule. An empty list clears it.
    [HttpPut("places/{placeId}/hours")]
    public IActionResult ReplaceHours(int placeId, SetOpeningHoursDto dto)
    {
        var (result, hours, error) = _ownerService.ReplaceHours(CallerId, placeId, dto);
        return result switch
        {
            OwnerHoursResult.Success       => Ok(new { placeId, openingHours = hours }),
            OwnerHoursResult.PlaceNotFound => NotFound("Place does not exist or is not yours."),
            OwnerHoursResult.InvalidSchedule => BadRequest(error),
            _ => StatusCode(500)
        };
    }
}
