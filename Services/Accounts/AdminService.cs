using FoodOrderingApi.Data;

using FoodOrderingApi.Dtos.Accounts.Admin;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Models.Users;

using FoodOrderingApi.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FoodOrderingApi.Dtos.Places;


namespace FoodOrderingApi.Services.Accounts;

public enum AdminRoleResult
{
    Success,
    UserNotFound,
    UnknownRole,
    NotInRole,
    Failed
}

public enum AdminVisibilityResult
{
    Success,
    PlaceNotFound
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

    public bool DeletePlace(int id)
    {
        Place? place = _db.Places.Find(id);
        if (place == null)
            return false;
        
        _db.Places.Remove(place);
        _db.SaveChanges();
        return true;

    }

    // -------------------------------------------------------------------
    // -- Roles ----------------------------------------------------------


    // --------------------------------------
    // -- Granting owner role ---------------

    // Only promotes an User to Owner role. Does not have side affects
    public async Task<AdminRoleResult> AssignRoleAsync(string userId, string role)
    {
        var known = MatchKnownRole(role);
        if (known == null)
            return AdminRoleResult.UnknownRole;

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
    // -- Revoking owner role ---------------

    // Side effects:
    // 1. Demotes Owner to a regular User
    // 2. Hides all the places that the owner used to own. (sets hidden = true)
    // 3. Clears those places' manager list.
    // 4. Keeps Ownership rows if was ever regranted.

    public async Task<AdminRoleResult> RemoveRoleAsync(string userId, string role)
    {
        var known = MatchKnownRole(role);
        if (known == null)
            return AdminRoleResult.UnknownRole;

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

    private static string? MatchKnownRole(string role) =>
        Roles.All.FirstOrDefault(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
}