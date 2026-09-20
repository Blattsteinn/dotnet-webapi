using FoodOrderingApi.Models;

namespace FoodOrderingApi.DTOs;

public record ProductCategoryResponse(
    int Id,
    string Name,
    string? Description,
    bool Hidden, // Remove ig later
    List<ProductResponse> Products
);