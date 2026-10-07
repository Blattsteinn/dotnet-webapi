namespace FoodOrderingApi.Dtos.Accounts.Owner;

// Flat by design: an owner sees who manages their place, not the identity graph.
public record ManagerDto(
    string UserId,
    string? UserName,
    string? Email
);
