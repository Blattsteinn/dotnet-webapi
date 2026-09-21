namespace FoodOrderingApi.Dtos.Places;

public record UpdateAddOn(
    string? Name,
    decimal? Price,
    bool? Hidden
);