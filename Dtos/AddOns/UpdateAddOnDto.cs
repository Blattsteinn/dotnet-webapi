namespace FoodOrderingApi.Dtos.AddOn;

public record UpdateAddOn(
    string? Name,
    decimal? Price,
    bool? Hidden
);