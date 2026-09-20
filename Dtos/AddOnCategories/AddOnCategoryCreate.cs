namespace FoodOrderingApi.DTOs;

public record AddOnCategoryCreate(
    int Id,
    int ProductId,
    bool Required,
    bool Multichoice,
    string Name
);