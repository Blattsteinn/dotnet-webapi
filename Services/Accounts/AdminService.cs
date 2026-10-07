using FoodOrderingApi.Data;

using FoodOrderingApi.Dtos.Accounts.Admin;
using FoodOrderingApi.Models.Users;

using FoodOrderingApi.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace FoodOrderingApi.Services.Accounts;

public enum AdminRoleResult
{
    Success,
    UserNotFound,
    RoleNotAllowed,
    NotInRole,
    Failed
}

public class AdminService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // -------------------------------------------------------------------
    // -- Finding a user -------------------------------------------------

    // Returns id + email only: enough to pick the person to promote, and
    // nothing else about the account.
    public async Task<List<AdminUserDto>> GetUsersAsync(string? search)
    {
        IQueryable<ApplicationUser> users = _db.Users;

        if (!string.IsNullOrWhiteSpace(search))
        {
            // SQLite's LIKE is case-insensitive (unlike Contains, which becomes a
            // case-sensitive instr), and the escaped pattern keeps a literal % or
            // _ in the search from behaving as a wildcard.
            var pattern = $"%{EscapeLikePattern(search.Trim())}%";
            users = users.Where(u => u.Email != null && EF.Functions.Like(u.Email, pattern, "\\"));
        }

        return await users
            .OrderBy(u => u.Email)
            .ThenBy(u => u.Id)
            .Select(u => new AdminUserDto(u.Id, u.Email))
            .ToListAsync();
    }

    // ---------------------------------------------------
    // --- Roles -----------------------------------------

    // --------------------------------------
    // --- Granting owner role ---

    // Side effects:
    // 1. unlocks the /api/v1/owner endpoints for that user

    public async Task<AdminRoleResult> AssignRoleAsync(string userId, string role)
    {
        var known = MatchGrantableRole(role);
        if (known == null)
            return AdminRoleResult.RoleNotAllowed;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return AdminRoleResult.UserNotFound;

        if (await _userManager.IsInRoleAsync(user, known))
            return AdminRoleResult.Success;

        var result = await _userManager.AddToRoleAsync(user, known);
        if (!result.Succeeded)
            return AdminRoleResult.Failed;

        // Rotate the stamp so tokens issued before this moment stop working.
        await _userManager.UpdateSecurityStampAsync(user);
        return AdminRoleResult.Success;
    }

    // --------------------------------------
    // --- Revoking owner role ---

    // Side effects:
    // 1. Demotes Owner to a regular User
    // 2. Hides all the places that the owner used to own. (sets hidden = true)
    // 3. Clears those places' manager list.
    // 4. Keeps Ownership rows if was ever regranted.

    public async Task<AdminRoleResult> RemoveRoleAsync(string userId, string role)
    {
        var known = MatchGrantableRole(role);
        if (known == null)
            return AdminRoleResult.RoleNotAllowed;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return AdminRoleResult.UserNotFound;

        if (!await _userManager.IsInRoleAsync(user, known))
            return AdminRoleResult.NotInRole;

        var result = await _userManager.RemoveFromRoleAsync(user, known); //1st side effect
        if (!result.Succeeded)
            return AdminRoleResult.Failed;

        // Doesn't wait for the JWT token to expire.
        // Removes the existing token on their very next request, 
        await _userManager.UpdateSecurityStampAsync(user);

        if (known == Roles.Owner)
        {
            SetVisibilityForOwner(user.Id, hidden: true); // 2nd side effect
            ClearManagersForOwner(user.Id); // 3rd side effect
        }

        return AdminRoleResult.Success;
    }

    // ------------------------------------------------------
    // -- Helper functions for revoking / adding an owner role 
    private void SetVisibilityForOwner(string ownerId, bool hidden)
    {
        var owned = _db.Places
            .Where(p => p.Ownership != null && p.Ownership.OwnerId == ownerId)
            .ToList();

        foreach (var place in owned)
            place.Hidden = hidden;

        _db.SaveChanges();
    }
    private void ClearManagersForOwner(string ownerId)
    {
        var ownedPlaceIds = _db.PlaceOwnerships
            .Where(o => o.OwnerId == ownerId)
            .Select(o => o.PlaceId);

        var grants = _db.Managers
            .Where(m => ownedPlaceIds.Contains(m.PlaceId));

        _db.Managers.RemoveRange(grants);
        _db.SaveChanges();
    }

    private static string? MatchGrantableRole(string role) =>
        string.Equals(role, Roles.Owner, StringComparison.OrdinalIgnoreCase) ? Roles.Owner : null;

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}