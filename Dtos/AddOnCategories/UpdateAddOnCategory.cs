namespace FoodOrderingApi.Dtos.AddOnCategory;

public record UpdateAddOnCategory(
    string? Name,
    bool? Required,
    bool? Multichoice
);