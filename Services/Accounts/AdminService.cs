using FoodOrderingApi.Data;

using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Models.Users;

using FoodOrderingApi.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


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

    // Not nested. Admin sees all places, hidden or not.
    public List<Place> GetAllPlaces()
    {
        List<Place> places = _db.Places
            .Include(p => p.Ownership)
            .ToList();
        return places;
    }

    public Place? GetPlace(int id)
    {   
        Place? place = _db.Places
            .Include(p => p.Ownership)
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Products)
            .ThenInclude(p => p.AddOnCategories)
            .ThenInclude(ac => ac.AddOns)
            .FirstOrDefault(p => p.Id == id);

            return place;
    }

    public Place AddPlace(Place place)
    {
        _db.Places.Add(place);
        _db.SaveChanges();

        return place; 
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
    // -- Role assignment ------------------------------------------------

    // Promoting a user to Owner is the admin's job, and it is what unlocks the
    // /api/v1/owner endpoints for that user.

    // Note: regranting a role does not make the place visible. Owners should do that 
    // manually. AssignRoleAsync never touches places
    public async Task<AdminRoleResult> AssignRoleAsync(string userId, string role)
    {
        var known = MatchKnownRole(role);
        if (known == null)
            return AdminRoleResult.UnknownRole;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return AdminRoleResult.UserNotFound;

        if (await _userManager.IsInRoleAsync(user, known))
            return AdminRoleResult.Success;             // idempotent

        var result = await _userManager.AddToRoleAsync(user, known);
        if (!result.Succeeded)
            return AdminRoleResult.Failed;

        // Rotate the stamp so tokens issued before this moment stop working.
        await _userManager.UpdateSecurityStampAsync(user);
        return AdminRoleResult.Success;
    }

    // Revoking Owner demotes user back to a regular AND
    // takes that person's places out of public view (sets hidden = true)
    //
    // The Ownership rows are kept on purpose if we ever re-grant them the role.
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

        var result = await _userManager.RemoveFromRoleAsync(user, known);
        if (!result.Succeeded)
            return AdminRoleResult.Failed;

        // Doesn't wait for the JWT token to expire.
        // Removes the existing token on their very next request, 
        await _userManager.UpdateSecurityStampAsync(user);

        if (known == Roles.Owner)
            // Here we make all of user owned places hidden
            SetVisibilityForOwner(user.Id, hidden: true);

        return AdminRoleResult.Success;
    }

    // -------------------------------------------------------------------
    // -- Visibility controls --------------------------------------------

    // Just in case admin needs to hide the place:
    public AdminVisibilityResult SetVisibility(int placeId, bool hidden)
    {
        Place? place = _db.Places.FirstOrDefault(p => p.Id == placeId);
        if (place == null)
            return AdminVisibilityResult.PlaceNotFound;

        place.Hidden = hidden;
        _db.SaveChanges();
        return AdminVisibilityResult.Success;
    }

    // Used when owner is demoted.
    // RemoveRoleAsync calls this: 
    private void SetVisibilityForOwner(string ownerId, bool hidden)
    {
        var owned = _db.Places
            .Where(p => p.Ownership != null && p.Ownership.OwnerId == ownerId)
            .ToList();

        foreach (var place in owned)
            place.Hidden = hidden;

        _db.SaveChanges();
    }

    private static string? MatchKnownRole(string role) =>
        Roles.All.FirstOrDefault(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
}