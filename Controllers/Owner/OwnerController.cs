using System.Security.Claims;

using FoodOrderingApi.Authorization;
using FoodOrderingApi.Dtos.Owner;
using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Services.Owners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers.Owners;


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
}
