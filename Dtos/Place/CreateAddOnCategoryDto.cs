namespace FoodOrderingApi.Dtos.Places;

public record CreateAddOnCategoryDto(
    int Id,
    int ProductId,
    bool Required,
    bool Multichoice,
    string Name
);