using System.Security.Claims;

using FoodOrderingApi.Authorization;
using FoodOrderingApi.Controllers;
using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Services.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingApi.Dtos.Accounts.Owner;

namespace FoodOrderingApi.Controllers.Accounts;


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

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // ADDING / REMOVING / LISTING USERS AS MANAGERS
    // ---------------------------------------------------------------------------


    [HttpGet("places/{placeId}/managers")]
    public IActionResult GetManagers(int placeId)
    {
        List<ManagerDto>? managers = _ownerService.GetManagers(CallerId, placeId);
        if (managers == null)
            return this.NotFoundProblem("Place does not exist or is not yours.");

        return Ok(managers);
    }

    // Assignment is by email
    [HttpPatch("places/{placeId}/managers/roles")]
    public async Task<IActionResult> AssignManager(int placeId, AssignManagerDto dto)
    {
        ManagerResult result = await _ownerService.AssignManagerAsync(CallerId, placeId, dto.Email);
        return result switch
        {
            ManagerResult.Success       => Ok(new { placeId, email = dto.Email }),
            ManagerResult.PlaceNotFound => this.NotFoundProblem("Place does not exist."),
            ManagerResult.NotTheOwner   => Forbid(),
            ManagerResult.UserNotFound  => this.NotFoundProblem("User does not exist."),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("places/{placeId}/managers/{userId}/roles")]
    public IActionResult RemoveManager(int placeId, string userId)
    {
        ManagerResult result = _ownerService.RemoveManager(CallerId, placeId, userId);
        return result switch
        {
            ManagerResult.Success       => NoContent(),
            ManagerResult.PlaceNotFound => this.NotFoundProblem("Place does not exist."),
            ManagerResult.NotTheOwner   => Forbid(),
            ManagerResult.UserNotFound  => this.NotFoundProblem("User does not exist."),
            _ => StatusCode(500)
        };
    }

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // PLACE CONTROL ACTIONS
    // ---------------------------------------------------------------------------

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
            return this.NotFoundProblem("Place does not exist or is not yours.");

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
            OwnerPlaceResult.PlaceNotFound => this.NotFoundProblem("Place does not exist."),
            OwnerPlaceResult.NotTheOwner   => Forbid(),
            OwnerPlaceResult.UnknownChildId =>
                this.BadRequestProblem("JSON included a child id that does not exist under this place."),
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
            OwnerHoursResult.PlaceNotFound => this.NotFoundProblem("Place does not exist or is not yours."),
            OwnerHoursResult.InvalidSchedule => this.BadRequestProblem(error),
            _ => StatusCode(500)
        };
    }
}
