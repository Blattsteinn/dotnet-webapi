
namespace FoodOrderingApi.Dtos.Places;

public record UpdatePlaceDto(
    double? Latitude,
    double? Longtitude,
    string? Name,
    string? Description
);