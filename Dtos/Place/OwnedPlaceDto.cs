namespace FoodOrderingApi.Dtos.Places;

// Deliberately flat: an owner sees their own places, including hidden ones,
// but never the identity graph behind them.
public record OwnedPlaceDto(
    int Id,
    string Name,
    string? Description,
    double Latitude,
    double Longtitude,
    bool Hidden
);
