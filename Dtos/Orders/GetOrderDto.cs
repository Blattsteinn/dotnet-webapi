
using FoodOrderingApi.Models.Orders;

namespace FoodOrderingApi.Dtos.Orders;
public record GetOrderDto(
    int? PlaceId,
    OrderStatus Status,
    DateTime CreatedAt,
    List<GetOrderItemDto> OrderItems
);

public record GetOrderItemDto(
    int? ProductId,
    string Name,
    decimal Price,
    int Quantity,
    List<GetOrderItemAddOnDto> OrderItemAddOns
);

public record  GetOrderItemAddOnDto(
    int? AddOnId,
    string Name,
    decimal Price,
    int Quantity
);