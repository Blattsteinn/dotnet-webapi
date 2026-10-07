using System.Security.Cryptography.X509Certificates;

namespace FoodOrderingApi.Dtos.Places;

// Join tables before using:
        // var place = _db.Places
            // .Include(p => p.ProductCategories)
            // .ThenInclude(pc => pc.Products)
            // .FirstOrDefault(p => p.Id == id);

// Flat read model for the public place.
public record PlaceSummaryDto(
    int Id,
    string Name,
    string? Description,
    string? Address,
    double Latitude,
    double Longtitude,
    bool AcceptingOrders,
    bool IsOpen,
    List<OpeningHourDto> OpeningHours
);

// Owner/admin read model for a single place.
public record PrivatePlaceDto(
    int Id,
    double Latitude,
    double Longtitude,
    string Name,
    string? Description,
    string? Address,
    bool Hidden,
    bool AcceptingOrders,
    bool IsOpen,
    List<OpeningHourDto> OpeningHours,
    List<ProductCategoriesDetailDto> ProductCategories
);

// Public read model for a single place.
public record PublicPlaceDto(
    int Id,
    double Latitude,
    double Longtitude,
    string Name,
    string? Description,
    string? Address,
    bool AcceptingOrders,
    bool IsOpen,
    List<OpeningHourDto> OpeningHours,
    List<ProductCategoriesDetailDto> ProductCategories
);

public record ProductCategoriesDetailDto(
    int Id,
    string Name,
    bool Hidden,
    List<PlaceProductDto> Products

);

// A product as it appears inside a place read model. Unlike the standalone
// public ProductSummaryDto it keeps Hidden: the private (owner/admin) place
// response shows the flag so an owner can see what they have hidden.
public record PlaceProductDto(
    int Id,
    string Name,
    bool Hidden,
    string? Description,
    decimal Price
);
