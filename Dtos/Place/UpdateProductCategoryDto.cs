namespace FoodOrderingApi.Dtos.Places;

public record UpdateProductCategory(
    string? Name,
    string? Description,
    bool? Hidden
);