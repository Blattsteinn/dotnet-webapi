namespace FoodOrderingApi.Dtos.Accounts.User;

public record LoginDto(
    string Email,
    string Password
);