namespace FoodOrderingApi.Authorization;

public static class AppClaimTypes
{
    // The claim type ASP.NET Core Identity uses for the security stamp
    // (IdentityOptions.ClaimsIdentity.SecurityStampClaimType).
    //
    // We re-check this claim on every request: a role change rotates the stamp,
    // which is what makes a previously issued token stop working immediately.
    // Without it, a revoked Owner keeps their Owner role until the token expires.
    public const string SecurityStamp = "AspNet.Identity.SecurityStamp";
}
