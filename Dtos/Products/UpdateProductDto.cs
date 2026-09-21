namespace FoodOrderingApi.Dtos.Product;

public record UpdateProduct(
    string? Name,
    string? Description,
    bool? Hidden,
    decimal? Price
);