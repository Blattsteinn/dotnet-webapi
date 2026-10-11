using FoodOrderingApi.Dtos.Places;

namespace FoodOrderingApi.Dtos.Accounts.Owner;

// Deliberately flat: an owner sees their own places, including hidden ones,
// but never the identity graph behind them. isOpen is computed on read.
public record OwnedPlaceDto(
    int Id,
    string Name,
    string? Description,
    string? Address,
    double Latitude,
    double Longtitude,
    bool Hidden,
    bool AcceptingOrders,
    bool IsOpen,
    List<OpeningHourDto> OpeningHours
);
