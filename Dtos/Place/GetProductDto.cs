
namespace FoodOrderingApi.Dtos.Places;
public record GetProductDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    bool Hidden,
    List <AddOnCategoriesDetailDto> ProductAddOnCategories
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
    decimal Price,
    bool Hidden
);