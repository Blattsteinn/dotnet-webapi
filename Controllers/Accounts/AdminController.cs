
using FoodOrderingApi.Controllers;
using FoodOrderingApi.Services.Accounts;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search)
    {
        List<AdminUserDto> users = await _adminService.GetUsersAsync(search);
        return Ok(users);
    }

    // ---------------------------------------------------
    // --- Roles -----------------------------------------

    // The only role that can be granted/taken out here is Owner. 
    // Admin role is not grantable.
    
    // ---------------------------------------------------
    // -- Granting owner role --

    // unlocks the /api/v1/owner endpoints for that user.

    [HttpPost("users/{userId}/roles")]
    public async Task<IActionResult> AssignRole(string userId, AssignRoleDto dto)
    {
        AdminRoleResult result = await _adminService.AssignRoleAsync(userId, dto.Role);
        return result switch
        {
            AdminRoleResult.Success      => Ok(new { userId, role = Roles.Owner }),
            AdminRoleResult.UserNotFound => this.NotFoundProblem("User does not exist."),
            AdminRoleResult.RoleNotAllowed => this.BadRequestProblem($"Only the '{Roles.Owner}' role can be granted here."),
            _ => StatusCode(500)
        };
    }

    // ---------------------------------------------------
    // -- Revoking owner role --

    // This also hides every place that user owned &&
    // clears those places' manager list.

    [HttpDelete("users/{userId}/roles/{role}")]
    public async Task<IActionResult> RemoveRole(string userId, string role)
    {
        AdminRoleResult result = await _adminService.RemoveRoleAsync(userId, role);
        return result switch
        {
            AdminRoleResult.Success      => NoContent(),
            AdminRoleResult.UserNotFound => this.NotFoundProblem("User does not exist."),
            AdminRoleResult.RoleNotAllowed => this.BadRequestProblem($"Only the '{Roles.Owner}' role can be revoked here."),
            AdminRoleResult.NotInRole    => this.BadRequestProblem("User does not have that role."),
            _ => StatusCode(500)
        };
    }

}