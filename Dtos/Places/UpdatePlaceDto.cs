

using Microsoft.AspNetCore.Routing.Constraints;

namespace FoodOrderingApi.Dtos.Places;

public record UpdatePlaceDto(
    double? Latitude,
    double? Longtitude,
    string? Name,
    string? Description
);

// ├── Dtos/
// │   ├── Places/
// │   │   ├── PlaceDto.cs
// │   │   ├── CreatePlaceDto.cs
// │   │   └── UpdatePlaceDto.cs
// │   ├── ProductCategories/
// │   │   ├── ProductCategoryDto.cs
// │   │   ├── CreateProductCategoryDto.cs
// │   │   └── UpdateProductCategoryDto.cs
// │   ├── Products/
// │   │   ├── ProductDto.cs
// │   │   ├── CreateProductDto.cs
// │   │   └── UpdateProductDto.cs
// │   ├── AddOnCategories/
// │   │   └── ...
// │   └── AddOns/
// │       └── ...