
namespace FoodOrderingApi.Dtos.Orders;

using FoodOrderingApi.Models.Orders;
public record UpdateOrderDto(
    OrderStatus? Status
);