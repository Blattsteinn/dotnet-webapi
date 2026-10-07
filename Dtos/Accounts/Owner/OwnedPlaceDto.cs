namespace FoodOrderingApi.Dtos.Accounts.Owner;

// Deliberately flat: an owner sees their own places, including hidden ones,
// but never the identity graph behind them.
public record OwnedPlaceDto(
    int Id,
    string Name,
    string? Description,
    string? Address,
    double Latitude,
    double Longtitude,
    bool Hidden
);
