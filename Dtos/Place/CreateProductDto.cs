namespace FoodOrderingApi.Dtos.Places;

public record CreateProductDto(
    int Id,
    string Name,
    string? Description,
    bool Hidden,  // Remove ig later
    decimal Price
);