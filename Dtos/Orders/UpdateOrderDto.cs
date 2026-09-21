
namespace FoodOrderingApi.Dtos.Order;

using FoodOrderingApi.Models;
public record UpdateOrderDto(
    OrderStatus Status
);