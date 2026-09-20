namespace FoodOrderingApi.Dtos.ProductCategory;

public record UpdateProductCategory(
    string? Name,
    string? Description,
    bool? Hidden
);