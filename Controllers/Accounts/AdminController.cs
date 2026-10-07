
using FoodOrderingApi.Services.Accounts;
using Microsoft.AspNetCore.Mvc;

using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Dtos.Accounts.Admin;

using Microsoft.AspNetCore.Authorization;
using FoodOrderingApi.Authorization;
using FoodOrderingApi.Dtos.Places;

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

    // Let admin delete a place
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

    // # WARNING: do NOT revoke the Admin role from Admin account.
    // # Can only be re-grant directly in the database then.
    
    // ----------------------------------------------
    // -- Granting owner role -----------------------

    // Promoting a user to Owner unlocks the /api/v1/owner endpoints for that user.

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

    // ----------------------------------------------
    // -- Revoking owner role -----------------------

    // This also hides every place that user owned &&
    // clears those places' manager list.

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

}