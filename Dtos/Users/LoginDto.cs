namespace FoodOrderingApi.Dtos.Users;

public record LoginDto(
    string Email,
    string Password
);