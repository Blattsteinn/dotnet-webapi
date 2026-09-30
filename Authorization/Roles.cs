using System.Security.Claims;

namespace FoodOrderingApi.Authorization;

// Single source of truth for Identity role names.
// Use these constants everywhere instead of raw strings so renames stay safe.
public static class Roles
{
    public const string Admin = "Admin";
    public const string Owner = "Owner";

    // Every role this application knows about. Used for seeding and validation.
    public static readonly string[] All = { Admin, Owner };

    // Uniform checks. The Admin role is still a role (not a special-cased user)
    // so every call site asks the same question the same way.
    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(Admin);

    // Global capability: "may this person create places at all?"
    // It says nothing about *which* places — that is what Ownership answers.
    public static bool IsOwner(this ClaimsPrincipal user) => user.IsInRole(Owner);
}
