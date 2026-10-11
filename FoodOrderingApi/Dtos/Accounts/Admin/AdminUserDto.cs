namespace FoodOrderingApi.Dtos.Accounts.Admin;

// One entry of GET /api/v1/admin/users.
//
// Deliberately two fields: the admin picks a person to promote, and the id is
// what the role endpoints take. Nothing else about the account is exposed.
public record AdminUserDto(
    string Id,
    string? Email
);
