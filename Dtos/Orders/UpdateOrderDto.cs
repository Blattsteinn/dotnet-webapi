
namespace FoodOrderingApi.Dtos.Order;

using FoodOrderingApi.Models.Orders;
public record UpdateOrderDto(
    OrderStatus Status
);