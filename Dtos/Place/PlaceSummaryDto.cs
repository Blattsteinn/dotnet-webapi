namespace FoodOrderingApi.Dtos.Places;

// Flat read model for the public place list. Intentionally omits the
// ownership row and the product graph carried by the Place entity.
public record PlaceSummaryDto(
    int Id,
    string Name,
    string? Description,
    double Latitude,
    double Longtitude,
    bool Hidden
);
