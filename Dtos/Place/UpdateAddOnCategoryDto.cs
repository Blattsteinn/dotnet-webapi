namespace FoodOrderingApi.Dtos.Places;

public record UpdateAddOnCategory(
    string? Name,
    bool? Required,
    bool? Multichoice
);