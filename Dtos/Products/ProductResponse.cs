using FoodOrderingApi.Models;

namespace FoodOrderingApi.DTOs;

public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    bool Hidden,  // Remove ig later
    decimal Price
);