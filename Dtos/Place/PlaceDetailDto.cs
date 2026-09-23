using System.Security.Cryptography.X509Certificates;

namespace FoodOrderingApi.Dtos.Places;


public record PlaceDetailDto(
    int Id,
    double Latitude,
    double Longtitude,
    string Name,
    string? Description,
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