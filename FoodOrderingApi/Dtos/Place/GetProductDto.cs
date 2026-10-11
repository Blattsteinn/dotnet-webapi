
namespace FoodOrderingApi.Dtos.Places;

// Public product read models.
//
// These are served by unauthenticated endpoints, so neither carries a Hidden
// flag: hidden products are left out of the response rather than reported, and
// the flag could therefore only ever be false. Hidden add-ons are filtered out
// the same way, which is why AddOnDetailDto has no flag either.

// Full read model for a single product, add-on tree included.
public record GetProductDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    List<AddOnCategoriesDetailDto> ProductAddOnCategories
);

// Flat read model for a product list entry. There is no add-on tree here — the
// list endpoint is a browse view, not a product page.
public record ProductSummaryDto(
    int Id,
    string Name,
    string? Description,
    decimal Price
);

public record AddOnCategoriesDetailDto(
    int Id,
    string Name,
    bool Required,
    bool Multichoice,
    List<AddOnDetailDto> AddOns
);

public record AddOnDetailDto(
    int Id,
    string Name,
    decimal Price
);