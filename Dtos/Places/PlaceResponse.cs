using FoodOrderingApi.Models;

namespace FoodOrderingApi.DTOs;

public record PlaceResponse(
    int Id,
    double Latitude,
    double Longtitude,
    string Name,
    string? Description,
    List<ProductCategoryResponse> ProductCategories
);