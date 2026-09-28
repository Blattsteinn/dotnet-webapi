
using FoodOrderingApi.Models.Orders;

namespace FoodOrderingApi.Dtos.Orders;
public record OrderDto(
    int? PlaceId,
    OrderStatus Status,
    DateTime CreatedAt,
    List<OrderItemDto> OrderItems
);

public record OrderItemDto(
    int? ProductId,
    string Name,
    decimal Price,
    int Quantity,
    List<OrderItemAddOnDto> OrderItemAddOns
);

public record  OrderItemAddOnDto(
    int? AddOnId,
    string Name,
    decimal Price,
    int Quantity
);