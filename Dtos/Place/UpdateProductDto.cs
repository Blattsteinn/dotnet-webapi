namespace FoodOrderingApi.Dtos.Places;

public record UpdateProduct(
    string? Name,
    string? Description,
    bool? Hidden,
    decimal? Price
);