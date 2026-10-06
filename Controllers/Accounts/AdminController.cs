
using FoodOrderingApi.Services.Accounts;
using Microsoft.AspNetCore.Mvc;

using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Dtos.Accounts.Admin;

using Microsoft.AspNetCore.Authorization;
using FoodOrderingApi.Authorization;

namespace FoodOrderingApi.Controllers.Accounts;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;
    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("places")]
    public IActionResult GetAllPlaces()
    {
        List<Place> places = _adminService.GetAllPlaces();
        return Ok(places);
    }

    [HttpGet("places/{id}")]
    public IActionResult GetPlace(int id)
    {
        Place? place = _adminService.GetPlace(id);
        if (place == null)
            return NotFound("Place does not exist.");

        return Ok(place);
    }
    
    [HttpPost("places")]
    public IActionResult AddPlace(Place place)
    {
        Place newPlace = _adminService.AddPlace(place);
        return Ok(newPlace);
    }



    [HttpDelete("places/{id}")]
    public IActionResult DeletePlace(int id)
    {
        bool exists = _adminService.DeletePlace(id);
        if(!exists)
            return NotFound();

        return NoContent();
    }

    // -------------------------------------------------------------------
    // -- Roles ----------------------------------------------------------

    // # NOTE: do NOT revoke the Admin role from Admin account.
    // # can only re-grant Admin directly in the database then.

    // This request grants an Owner / Admin role.
    // It's the only way a user becomes able to create places.
    [HttpPost("users/{userId}/roles")]
    public async Task<IActionResult> AssignRole(string userId, AssignRoleDto dto)
    {
        AdminRoleResult result = await _adminService.AssignRoleAsync(userId, dto.Role);
        return result switch
        {
            AdminRoleResult.Success      => Ok(new { userId, role = dto.Role }),
            AdminRoleResult.UserNotFound => NotFound("User does not exist."),
            AdminRoleResult.UnknownRole  => BadRequest("Unknown role. Allowed: Admin, Owner."),
            _ => StatusCode(500)
        };
    }

    // Revoking Owner's role
    // Side effect: it also hides every place that user owned.
    [HttpDelete("users/{userId}/roles/{role}")]
    public async Task<IActionResult> RemoveRole(string userId, string role)
    {
        AdminRoleResult result = await _adminService.RemoveRoleAsync(userId, role);
        return result switch
        {
            AdminRoleResult.Success      => NoContent(),
            AdminRoleResult.UserNotFound => NotFound("User does not exist."),
            AdminRoleResult.UnknownRole  => BadRequest("Unknown role. Allowed: Admin, Owner."),
            AdminRoleResult.NotInRole    => BadRequest("User does not have that role."),
            _ => StatusCode(500)
        };
    }

    // -------------------------------------------------------------------
    // -- Visibility -----------------------------------------------------

    // This REQUEST is for ownerless places or just for general admin control.
    // Hides or re-publishes a single place.
    [HttpPatch("places/{id}/visibility")]
    public IActionResult SetVisibility(int id, SetPlaceVisibilityDto dto)
    {
        AdminVisibilityResult result = _adminService.SetVisibility(id, dto.Hidden);
        return result switch
        {
            AdminVisibilityResult.Success       => Ok(new { placeId = id, hidden = dto.Hidden }),
            AdminVisibilityResult.PlaceNotFound => NotFound("Place does not exist."),
            _ => StatusCode(500)
        };
    }
}