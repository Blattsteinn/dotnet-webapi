using System.Security.Cryptography.X509Certificates;

namespace FoodOrderingApi.Dtos.Places;

// Join tables before using:
        // var place = _db.Places
            // .Include(p => p.ProductCategories)
            // .ThenInclude(p => p.Products)
            // .FirstOrDefault(p => p.Id == id);

public record GetPlaceDto(
    int Id,
    double Latitude,
    double Longtitude,
    string Name,
    string? Description,
    string? Address,
    bool Hidden,
    List <ProductCategoriesDetailDto> ProductCategories
);

public record ProductCategoriesDetailDto(
    int Id,
    string Name,
    bool Hidden,
    List<ProductSummaryDto> Products

);

public record ProductSummaryDto(
    int Id,
    string Name,
    bool Hidden,
    string? Description,
    decimal Price
);